using System.Collections.Generic;
using GummyDynasty.Cognition;
using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>One autonomous gummy: body + a private actor node on a shared field.</summary>
    public sealed class GummyAgent : MonoBehaviour
    {
        public const string RoleThreat = HierarchyRoles.Threat;
        public const string RolePain = HierarchyRoles.Pain;
        public const string RoleStand = HierarchyRoles.Stand;
        public const string RoleObjective = HierarchyRoles.Objective;

        public GummyBody Body { get; private set; }
        public string AgentId => Body != null ? Body.AgentId : name;
        public ResolvedIntent Intent { get; private set; }
        public int Slot { get; set; }
        public bool HoldsRank { get; set; }
        public AgentMemory Memory { get; } = new AgentMemory();
        public bool FlattenedPulse { get; set; }

        BeliefField _field;
        readonly List<Belief> _scratch = new List<Belief>(8);
        readonly List<MemoryEvent> _memScratch = new List<MemoryEvent>(4);
        bool _loggedDown;

        public void Bind(GummyBody body)
        {
            Body = body;
        }

        public void AttachField(BeliefField field, string formationId = null)
        {
            _field = field;
            if (field == null || Body == null)
                return;
            if (formationId == HierarchyIds.FormationRed || string.IsNullOrEmpty(formationId))
                BattleHierarchy.AttachActor(field, AgentId);
            else
            {
                field.AddNode(AgentId, formationId, NodeKind.Actor);
                field.EnsureRole(AgentId, RoleThreat, RoleMode.Dissipative, 0.5f);
                field.EnsureRole(AgentId, RolePain, RoleMode.Dissipative, 0.25f);
                field.EnsureRole(AgentId, RoleStand, RoleMode.Dissipative, 0.4f);
                field.EnsureRole(AgentId, RoleObjective, RoleMode.Unitary);
                field.EnsureRole(AgentId, HierarchyRoles.Congestion, RoleMode.Dissipative, 0.45f);
                field.EnsureRole(AgentId, HierarchyRoles.Cost, RoleMode.Dissipative, 0.3f);
            }
        }

        public void Sense()
        {
            if (_field == null || Body == null)
                return;

            Memory.Tick(Time.deltaTime);
            var upright = Body.MotorState == GummyMotorState.Locomoting;
            if (!upright && !_loggedDown)
            {
                Memory.Remember(MemoryKind.Flattened, 1f, Memory.LastHitter);
                _loggedDown = true;
                FlattenedPulse = true;
            }
            if (upright)
                _loggedDown = false;

            MemorySense.Write(_field, AgentId, Memory, upright);
            _field.Observe(new Observation(AgentId, RoleStand, upright ? 1f : 0.2f, 0.35f, AgentId));

            var inherited = _field.GetInherited(AgentId, RoleObjective);
            if (inherited.Value > 0.6f)
                _field.Observe(new Observation(AgentId, RoleObjective, inherited.Value, 0.28f, AgentId));
        }

        public void SenseNeighborhood(int nearby, float incomingThreat)
        {
            if (_field == null || Body == null)
                return;

            var crowd = Mathf.Clamp01((nearby - 1) / 4f);
            _field.Observe(new Observation(AgentId, HierarchyRoles.Congestion, crowd, 0.45f, AgentId));
            if (incomingThreat > 0f)
            {
                _field.Observe(new Observation(AgentId, RoleThreat, Mathf.Clamp01(incomingThreat), Mathf.Clamp01(incomingThreat), AgentId));
                if (incomingThreat > 0.25f)
                    Memory.Remember(MemoryKind.Incoming, incomingThreat);
            }

            var threat = _field.GetLocal(AgentId, RoleThreat);
            var congestion = _field.GetLocal(AgentId, HierarchyRoles.Congestion);
            var overrideW = Mathf.Max(threat.Value, congestion.Value);
            _field.SetOverride(AgentId, RoleObjective, overrideW >= IntentResolver.ThreatOverride ? overrideW : 0f);

            var cost = Mathf.Clamp01((threat.Value + congestion.Value) * 0.5f);
            _field.Observe(new Observation(AgentId, HierarchyRoles.Cost, cost, 0.4f, AgentId));
        }

        public void ObserveWorldImpact(float severity, bool selfCaused, string hitterId = null)
        {
            var who = selfCaused ? AgentId : (string.IsNullOrEmpty(hitterId) ? "candy" : hitterId);
            Memory.Remember(MemoryKind.Hit, Mathf.Clamp01(severity), who);
            if (_field == null)
                return;
            _field.Observe(new Observation(AgentId, RoleThreat, Mathf.Clamp01(severity), Mathf.Clamp01(severity), AgentId, selfTagged: selfCaused));
            if (!selfCaused)
                _field.Observe(new Observation(AgentId, RolePain, Mathf.Clamp01(severity), Mathf.Clamp01(severity * 0.8f), AgentId));
        }

        public void NoticeAllyDown(string allyId)
        {
            Memory.Remember(MemoryKind.SawAllyDown, 0.7f, allyId);
        }

        public string FormationTactic { get; private set; } = IntentResolver.Idle;

        public ResolvedIntent Resolve(string formationTactic = null)
        {
            if (_field == null || Body == null)
            {
                Intent = default;
                return Intent;
            }

            FormationTactic = string.IsNullOrEmpty(formationTactic) ? IntentResolver.Idle : formationTactic;
            Intent = IntentResolver.Resolve(
                AgentId,
                _field.Get(AgentId, RoleObjective),
                _field.GetLocal(AgentId, RoleThreat),
                _field.GetLocal(AgentId, HierarchyRoles.Congestion),
                _field.GetLocal(AgentId, RolePain),
                Body.MotorState == GummyMotorState.Locomoting,
                FormationTactic);
            return Intent;
        }

        public void Act(Vector3 westFlag, Vector3 incoming, Vector3 slot, bool hasSlot, string formationTactic = null)
        {
            if (Body == null)
                return;

            var intent = Resolve(formationTactic);
            if (intent.EffectiveName == IntentResolver.Down)
            {
                Body.Plant();
                return;
            }

            if (intent.EffectiveName == IntentResolver.Dodge)
            {
                var away = Body.Position - incoming;
                if (away.sqrMagnitude < 0.01f)
                    away = Quaternion.Euler(0f, Slot % 2 == 0 ? 75f : -75f, 0f) * Vector3.forward;
                away.y = 0f;
                var spread = Slot % 2 == 0 ? Vector3.right : Vector3.left;
                Body.Locomote((away.normalized + spread * 0.65f).normalized, Mathf.Max(0.7f, intent.LocalStrength));
                return;
            }

            if (intent.EffectiveName == FormationTactics.Hold)
            {
                if (hasSlot)
                {
                    var toHold = slot - Body.Position;
                    toHold.y = 0f;
                    if (toHold.sqrMagnitude <= MarchFormation.Arrive * MarchFormation.Arrive)
                    {
                        Body.Plant();
                        return;
                    }
                    var holdHeading = toHold.sqrMagnitude > 0.0001f ? toHold.normalized : Vector3.zero;
                    if (holdHeading.sqrMagnitude < 0.01f)
                    {
                        Body.Plant();
                        return;
                    }
                    Body.Locomote(holdHeading, 0.7f);
                    return;
                }
                Body.Plant();
                return;
            }

            if (FormationTactics.Travels(intent.EffectiveName))
            {
                if (intent.EffectiveName == IntentResolver.MarchWest
                    && Body.Position.x <= westFlag.x + IntentResolver.ArriveBand)
                {
                    if (hasSlot)
                    {
                        var toHold = slot - Body.Position;
                        toHold.y = 0f;
                        if (toHold.sqrMagnitude <= MarchFormation.Arrive * MarchFormation.Arrive)
                        {
                            Body.Plant();
                            return;
                        }
                    }
                    else
                    {
                        Body.Plant();
                        return;
                    }
                }

                var heading = MarchFormation.MarchHeading(Body.Position, slot, westFlag, hasSlot);
                if (heading.sqrMagnitude < 0.01f)
                {
                    Body.Plant();
                    return;
                }
                var strength = hasSlot
                    ? Mathf.Clamp01(0.55f + intent.InheritedStrength * 0.45f)
                    : intent.InheritedStrength;
                Body.Locomote(heading, strength);
            }
        }

        public Belief Read(string role) => _field != null ? _field.Get(AgentId, role) : default;

        public Belief ReadLocal(string role) => _field != null ? _field.GetLocal(AgentId, role) : default;

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

        public IReadOnlyList<MemoryEvent> PeekMemories(int n = 4)
        {
            Memory.CopyRecent(_memScratch, n);
            return _memScratch;
        }
    }
}
