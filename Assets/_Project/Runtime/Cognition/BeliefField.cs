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
            public float LocalValue = 0.5f;
            public float InheritedValue = 0.5f;
            public float Confidence;
            public float Override;
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
            leaf.LocalValue = leaf.Value;
            leaf.Confidence += (1f - leaf.Confidence) * obs.Eta * 0.25f;
            if (leaf.Value < 0f) leaf.Value = 0f;
            else if (leaf.Value > 1f) leaf.Value = 1f;
            leaf.LocalValue = leaf.Value;
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
                    leaf.LocalValue += (0.5f - leaf.LocalValue) * k;
                    leaf.Confidence *= ExpDecay(leaf.GammaDiss, deltaSeconds);
                }
            }
        }

        public void Propagate(float inherit = 0.15f)
        {
            // Children take a weak prior from parents unless local override is high.
            // Unitary command roles are not reduced upward (faction order must survive the crowd).
            for (var i = 0; i < _order.Count; i++)
            {
                var node = _nodes[_order[i]];
                if (string.IsNullOrEmpty(node.ParentId) || !_nodes.TryGetValue(node.ParentId, out var parent))
                    continue;

                foreach (var pair in node.Roles)
                {
                    if (!parent.Roles.TryGetValue(pair.Key, out var parentLeaf))
                        continue;
                    var leaf = pair.Value;
                    leaf.InheritedValue = parentLeaf.Value;
                    var weight = inherit * (1f - Clamp01(leaf.Override));
                    if (weight <= 0f)
                    {
                        leaf.Value = leaf.LocalValue;
                        continue;
                    }

                    leaf.Value = leaf.LocalValue + (leaf.InheritedValue - leaf.LocalValue) * weight;
                }
            }

            for (var i = _order.Count - 1; i >= 0; i--)
            {
                var parent = _nodes[_order[i]];
                foreach (var pair in parent.Roles)
                {
                    if (pair.Value.Mode == RoleMode.Unitary)
                        continue;
                    ReduceRole(parent, pair.Key, pair.Value);
                }
            }
        }

        public Belief Get(string nodeId, string role)
        {
            if (_nodes.TryGetValue(nodeId, out var node) && node.Roles.TryGetValue(role, out var leaf))
                return new Belief(nodeId, role, leaf.Value, leaf.Confidence);
            return new Belief(nodeId, role, 0.5f, 0f);
        }

        public Belief GetLocal(string nodeId, string role)
        {
            if (_nodes.TryGetValue(nodeId, out var node) && node.Roles.TryGetValue(role, out var leaf))
                return new Belief(nodeId, role, leaf.LocalValue, leaf.Confidence);
            return new Belief(nodeId, role, 0.5f, 0f);
        }

        public Belief GetInherited(string nodeId, string role)
        {
            if (!_nodes.TryGetValue(nodeId, out var node) || string.IsNullOrEmpty(node.ParentId))
                return new Belief(nodeId, role, 0.5f, 0f);
            return Get(node.ParentId, role);
        }

        public void SetOverride(string nodeId, string role, float weight)
        {
            if (!_nodes.TryGetValue(nodeId, out var node))
                return;
            if (!node.Roles.TryGetValue(role, out var leaf))
                return;
            leaf.Override = Clamp01(weight);
        }

        public float GetOverride(string nodeId, string role)
        {
            if (_nodes.TryGetValue(nodeId, out var node) && node.Roles.TryGetValue(role, out var leaf))
                return leaf.Override;
            return 0f;
        }

        public bool TryGetParent(string nodeId, out string parentId)
        {
            parentId = null;
            if (!_nodes.TryGetValue(nodeId, out var node) || string.IsNullOrEmpty(node.ParentId))
                return false;
            parentId = node.ParentId;
            return true;
        }

        public NodeKind KindOf(string nodeId)
        {
            return _nodes.TryGetValue(nodeId, out var node) ? node.Kind : NodeKind.World;
        }

        public void CopyAncestry(string nodeId, List<string> into)
        {
            into.Clear();
            var id = nodeId;
            var guard = 0;
            while (!string.IsNullOrEmpty(id) && _nodes.ContainsKey(id) && guard++ < 16)
            {
                into.Add(id);
                id = _nodes[id].ParentId;
            }
        }

        public void CopyChildren(string parentId, List<string> into)
        {
            into.Clear();
            for (var i = 0; i < _order.Count; i++)
            {
                var node = _nodes[_order[i]];
                if (node.ParentId == parentId)
                    into.Add(node.Id);
            }
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

            dest.LocalValue = dest.Value;
        }

        static float Clamp01(float v)
        {
            if (v < 0f) return 0f;
            if (v > 1f) return 1f;
            return v;
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
