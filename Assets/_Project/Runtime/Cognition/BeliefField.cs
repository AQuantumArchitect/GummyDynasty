using System;
using System.Collections.Generic;

namespace GummyDynasty.Cognition
{
    /// <summary>
    /// Unity-native port of the Umwelt host contract: observe(η) → belief(value, confidence).
    /// Default update is an α-blend. Dissipative roles forget. Self-tag skips world nodes.
    /// </summary>
    public sealed class BeliefField
    {
        sealed class Leaf
        {
            public float Value = 0.5f;
            public float Confidence;
            public RoleMode Mode = RoleMode.Dissipative;
            public float GammaDiss = 0.35f;
            public ReduceOp Reduce = ReduceOp.Mean;
        }

        sealed class Node
        {
            public string Id;
            public string ParentId;
            public NodeKind Kind = NodeKind.World;
            public readonly Dictionary<string, Leaf> Roles = new Dictionary<string, Leaf>(8);
        }

        readonly Dictionary<string, Node> _nodes = new Dictionary<string, Node>(32);
        readonly List<string> _order = new List<string>(32);

        public int NodeCount => _nodes.Count;

        public void AddNode(string id, string parentId = null, NodeKind kind = NodeKind.World)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("node id required", nameof(id));

            if (!_nodes.TryGetValue(id, out var node))
            {
                node = new Node { Id = id };
                _nodes.Add(id, node);
                _order.Add(id);
            }

            node.ParentId = parentId;
            node.Kind = kind;
        }

        public void EnsureRole(string nodeId, string role, RoleMode mode = RoleMode.Dissipative, float gammaDiss = 0.35f, ReduceOp reduce = ReduceOp.Mean)
        {
            var node = Require(nodeId);
            if (!node.Roles.TryGetValue(role, out var leaf))
            {
                leaf = new Leaf();
                node.Roles.Add(role, leaf);
            }

            leaf.Mode = mode;
            leaf.GammaDiss = gammaDiss < 0f ? 0f : gammaDiss;
            leaf.Reduce = reduce;
        }

        public void Observe(in Observation obs)
        {
            if (obs.Eta <= 0f || string.IsNullOrEmpty(obs.NodeId) || string.IsNullOrEmpty(obs.Role))
                return;

            if (!_nodes.TryGetValue(obs.NodeId, out var node))
                return;

            if (obs.SelfTagged && node.Kind == NodeKind.World)
                return;

            if (!node.Roles.TryGetValue(obs.Role, out var leaf))
            {
                leaf = new Leaf();
                node.Roles.Add(obs.Role, leaf);
            }

            // α shrinks as the leaf becomes certain — Umwelt-compatible, not Belavkin.
            var alpha = obs.Eta * (1f - 0.5f * leaf.Confidence);
            leaf.Value += (obs.Value - leaf.Value) * alpha;
            leaf.Confidence += (1f - leaf.Confidence) * obs.Eta * 0.25f;
            if (leaf.Value < 0f) leaf.Value = 0f;
            else if (leaf.Value > 1f) leaf.Value = 1f;
            if (leaf.Confidence < 0f) leaf.Confidence = 0f;
            else if (leaf.Confidence > 1f) leaf.Confidence = 1f;
        }

        public void Tick(float deltaSeconds)
        {
            if (deltaSeconds <= 0f)
                return;

            for (var i = 0; i < _order.Count; i++)
            {
                var node = _nodes[_order[i]];
                foreach (var pair in node.Roles)
                {
                    var leaf = pair.Value;
                    if (leaf.Mode != RoleMode.Dissipative || leaf.GammaDiss <= 0f)
                        continue;

                    var k = 1f - ExpDecay(leaf.GammaDiss, deltaSeconds);
                    leaf.Value += (0.5f - leaf.Value) * k;
                    leaf.Confidence *= ExpDecay(leaf.GammaDiss, deltaSeconds);
                }
            }
        }

        public void Propagate(float inherit = 0.15f)
        {
            // Children take a weak prior from parents, then parents reduce from children.
            for (var i = 0; i < _order.Count; i++)
            {
                var node = _nodes[_order[i]];
                if (string.IsNullOrEmpty(node.ParentId) || !_nodes.TryGetValue(node.ParentId, out var parent))
                    continue;

                foreach (var pair in node.Roles)
                {
                    if (!parent.Roles.TryGetValue(pair.Key, out var parentLeaf))
                        continue;
                    pair.Value.Value += (parentLeaf.Value - pair.Value.Value) * inherit;
                }
            }

            for (var i = _order.Count - 1; i >= 0; i--)
            {
                var parent = _nodes[_order[i]];
                foreach (var pair in parent.Roles)
                    ReduceRole(parent, pair.Key, pair.Value);
            }
        }

        public Belief Get(string nodeId, string role)
        {
            if (_nodes.TryGetValue(nodeId, out var node) && node.Roles.TryGetValue(role, out var leaf))
                return new Belief(nodeId, role, leaf.Value, leaf.Confidence);
            return new Belief(nodeId, role, 0.5f, 0f);
        }

        public void CopyBeliefs(string nodeId, List<Belief> into)
        {
            into.Clear();
            if (!_nodes.TryGetValue(nodeId, out var node))
                return;
            foreach (var pair in node.Roles)
                into.Add(new Belief(nodeId, pair.Key, pair.Value.Value, pair.Value.Confidence));
        }

        void ReduceRole(Node parent, string role, Leaf dest)
        {
            var n = 0;
            var acc = 0f;
            var max = 0f;
            var any = 0f;
            for (var i = 0; i < _order.Count; i++)
            {
                var child = _nodes[_order[i]];
                if (child.ParentId != parent.Id)
                    continue;
                if (!child.Roles.TryGetValue(role, out var leaf))
                    continue;
                n++;
                acc += leaf.Value;
                if (leaf.Value > max) max = leaf.Value;
                if (leaf.Value > any) any = leaf.Value;
            }

            if (n == 0)
                return;

            switch (dest.Reduce)
            {
                case ReduceOp.Max:
                    dest.Value = max;
                    break;
                case ReduceOp.Or:
                    dest.Value = any > 0.55f ? any : dest.Value * 0.5f + any * 0.5f;
                    break;
                default:
                    dest.Value = acc / n;
                    break;
            }
        }

        Node Require(string id)
        {
            if (!_nodes.TryGetValue(id, out var node))
                throw new InvalidOperationException("unknown node " + id);
            return node;
        }

        static float ExpDecay(float gamma, float dt)
        {
            // e^{-γΔt} via 1-step Padé; fine at game dt.
            var x = gamma * dt;
            return 1f / (1f + x + 0.5f * x * x);
        }
    }
}
