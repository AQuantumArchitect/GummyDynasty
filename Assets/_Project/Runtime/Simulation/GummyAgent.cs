using System.Collections.Generic;
using GummyDynasty.Cognition;
using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>One autonomous gummy: body + a private actor node on a shared field.</summary>
    public sealed class GummyAgent : MonoBehaviour
    {
        public const string RoleThreat = "threat";
        public const string RolePain = "pain";
        public const string RoleStand = "stand";
        public const string RoleObjective = "objective";

        public GummyBody Body { get; private set; }
        public string AgentId => Body != null ? Body.AgentId : name;

        BeliefField _field;
        readonly List<Belief> _scratch = new List<Belief>(8);

        public void Bind(GummyBody body)
        {
            Body = body;
        }

        public void AttachField(BeliefField field, string formationId = null)
        {
            _field = field;
            if (field == null || Body == null)
                return;
            field.AddNode(AgentId, formationId, NodeKind.Actor);
            field.EnsureRole(AgentId, RoleThreat, RoleMode.Dissipative, 0.5f);
            field.EnsureRole(AgentId, RolePain, RoleMode.Dissipative, 0.25f);
            field.EnsureRole(AgentId, RoleStand, RoleMode.Dissipative, 0.4f);
            field.EnsureRole(AgentId, RoleObjective, RoleMode.Unitary, 0f);
        }

        public void Sense()
        {
            if (_field == null || Body == null)
                return;

            var ragdoll = Body.MotorState != GummyMotorState.Locomoting ? 1f : 0f;
            _field.Observe(new Observation(AgentId, RolePain, ragdoll, ragdoll > 0.5f ? 0.7f : 0.15f, AgentId));
            _field.Observe(new Observation(AgentId, RoleStand, Body.MotorState == GummyMotorState.Locomoting ? 1f : 0.2f, 0.35f, AgentId));
        }

        public void ObserveWorldImpact(float severity, bool selfCaused)
        {
            if (_field == null)
                return;
            _field.Observe(new Observation(AgentId, RoleThreat, Mathf.Clamp01(severity), Mathf.Clamp01(severity), AgentId, selfTagged: selfCaused));
            if (!selfCaused)
                _field.Observe(new Observation(AgentId, RolePain, Mathf.Clamp01(severity), Mathf.Clamp01(severity * 0.8f), AgentId));
        }

        public Belief Read(string role) => _field != null ? _field.Get(AgentId, role) : default;

        public void CopyBeliefs(List<Belief> into)
        {
            if (_field == null)
            {
                into.Clear();
                return;
            }
            _field.CopyBeliefs(AgentId, into);
        }

        public IReadOnlyList<Belief> PeekBeliefs()
        {
            CopyBeliefs(_scratch);
            return _scratch;
        }
    }
}
