using UnityEngine;

namespace GummyDynasty.Simulation
{
    public enum GummyMotorState : byte
    {
        Locomoting = 0,
        Ragdoll = 1,
        Recovering = 2
    }

    /// <summary>Physical gummy. Presentation may read pose; it must not own the rigidbodies.</summary>
    public sealed class GummyBody : MonoBehaviour
    {
        public PhysicalPersonality Personality { get; private set; }
        public GummyMotorState MotorState { get; private set; } = GummyMotorState.Ragdoll;
        public string AgentId { get; private set; }
        public Rigidbody Root { get; private set; }
        public Rigidbody[] Parts { get; private set; } = System.Array.Empty<Rigidbody>();

        float _recoverAt = -1f;
        Vector3 _spawnUp = Vector3.up;

        public Vector3 Position => Root != null ? Root.worldCenterOfMass : transform.position;
        public Vector3 Velocity => Root != null ? Root.linearVelocity : Vector3.zero;
        public bool IsAirborne
        {
            get
            {
                if (Root == null) return false;
                return !Physics.Raycast(Root.worldCenterOfMass, Vector3.down, Personality != null ? Personality.Height * 0.85f : 1f, ~0, QueryTriggerInteraction.Ignore);
            }
        }

        public void Bind(string agentId, PhysicalPersonality personality, Rigidbody root, Rigidbody[] parts)
        {
            AgentId = agentId;
            Personality = personality;
            Root = root;
            Parts = parts ?? System.Array.Empty<Rigidbody>();
            MotorState = GummyMotorState.Ragdoll;
        }

        public void ApplyImpulse(Vector3 impulse, Vector3? point = null)
        {
            if (Root == null) return;
            EnterRagdoll(Personality != null ? Personality.RecoverySeconds : 1.2f);
            if (point.HasValue)
                Root.AddForceAtPosition(impulse, point.Value, ForceMode.Impulse);
            else
                Root.AddForce(impulse, ForceMode.Impulse);
        }

        public void Launch(Vector3 direction, float extra = 1f)
        {
            var mul = Personality != null ? Personality.LaunchMultiplier : 1f;
            var mag = (8.5f + (Personality != null ? Personality.Mass : 2f)) * mul * extra;
            ApplyImpulse(direction.normalized * mag + Vector3.up * (mag * 0.35f));
        }

        public void KnockDown(Vector3 direction)
        {
            var knock = Personality != null ? Personality.KnockImpulse : 6f;
            ApplyImpulse(direction.normalized * knock + Vector3.up * (knock * 0.4f));
        }

        public void EnterRagdoll(float recoverAfter)
        {
            MotorState = GummyMotorState.Ragdoll;
            _recoverAt = Time.time + Mathf.Max(0.05f, recoverAfter);
            SetDrag(false);
        }

        void FixedUpdate()
        {
            if (Root == null || Personality == null)
                return;

            if (MotorState == GummyMotorState.Ragdoll && Time.time >= _recoverAt && Root.linearVelocity.sqrMagnitude < 1.8f)
                MotorState = GummyMotorState.Recovering;

            if (MotorState == GummyMotorState.Recovering)
            {
                var up = Root.transform.up;
                var torque = Vector3.Cross(up, _spawnUp) * Personality.StandForce;
                Root.AddTorque(torque, ForceMode.Acceleration);
                Root.AddForce(Vector3.up * (Personality.StandForce * 0.15f), ForceMode.Acceleration);
                if (Vector3.Dot(up, _spawnUp) > 0.85f && Root.linearVelocity.sqrMagnitude < 0.6f)
                {
                    MotorState = GummyMotorState.Locomoting;
                    SetDrag(true);
                }
            }

            if (MotorState == GummyMotorState.Locomoting)
            {
                var tilt = Vector3.Cross(Root.transform.up, _spawnUp);
                Root.AddTorque(tilt * (Personality.StandForce * 0.55f), ForceMode.Acceleration);
            }
        }

        void SetDrag(bool standing)
        {
            var drag = standing ? Personality.Drag + 1.2f : Personality.Drag;
            var ang = standing ? Personality.AngularDrag + 1.6f : Personality.AngularDrag;
            for (var i = 0; i < Parts.Length; i++)
            {
                if (Parts[i] == null) continue;
                Parts[i].linearDamping = drag;
                Parts[i].angularDamping = ang;
            }
        }
    }
}
