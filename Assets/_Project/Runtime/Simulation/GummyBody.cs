using UnityEngine;

namespace GummyDynasty.Simulation
{
    public enum GummyMotorState : byte
    {
        Locomoting = 0,
        Ragdoll = 1,
        Recovering = 2
    }

    /// <summary>
    /// Stand: Octodad-style one-way marionette string on the whole axis.
    /// Walk: bouncing-ball hop — same-phase VelocityChange on head/belly/hips,
    /// then gravity. Not a traveling wave (that is a caterpillar).
    /// </summary>
    public sealed class GummyBody : MonoBehaviour
    {
        public const float HopUp = 2.25f;
        public const float HopForward = 1.55f;
        public const float HopHz = 1.45f;
        public const float IdleHz = 1.12f;

        public PhysicalPersonality Personality { get; private set; }
        public GummyMotorState MotorState { get; private set; } = GummyMotorState.Ragdoll;
        public string AgentId { get; private set; }
        public Rigidbody Root { get; private set; }
        public Rigidbody Belly { get; private set; }
        public Rigidbody Head { get; private set; }
        public Rigidbody[] Parts { get; private set; } = System.Array.Empty<Rigidbody>();

        float _recoverAt = -1f;
        float _slackUntil;
        float _phase;
        Vector3 _walkHeading;
        float _walkStrength;
        bool _stoodUp;
        bool _hopped;
        bool _stiff;
        bool _clonedPersonality;
        int _getUpHops;
        MovingDeck _deck;
        readonly RaycastHit[] _groundHits = new RaycastHit[8];
        JointTune[] _joints = System.Array.Empty<JointTune>();

        public Vector3 Position => Root != null ? Root.worldCenterOfMass : transform.position;
        public Vector3 Velocity => Root != null ? Root.linearVelocity : Vector3.zero;
        public bool StringTaut => Time.time >= _slackUntil;

        public bool IsAirborne
        {
            get
            {
                if (Root == null) return false;
                return !TryGround(out _);
            }
        }

        public void Bind(string agentId, PhysicalPersonality personality, Rigidbody root, Rigidbody belly, Rigidbody head, Rigidbody[] parts)
        {
            AgentId = agentId;
            Personality = personality;
            Root = root;
            Belly = belly;
            Head = head;
            Parts = parts ?? System.Array.Empty<Rigidbody>();
            MotorState = GummyMotorState.Ragdoll;
            _phase = (agentId.GetHashCode() & 1023) * 0.001f;
            _slackUntil = Time.time;
            _stoodUp = false;
            _getUpHops = 0;
            CacheJoints();
            SetStiff(false);
        }

        public void ApplyImpulse(Vector3 impulse, Vector3? point = null)
        {
            if (Root == null) return;
            Slack(Personality != null ? Personality.RecoverySeconds : 1.2f);
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

        public void Slack(float seconds)
        {
            _slackUntil = Time.time + Mathf.Max(0.05f, seconds);
            _walkStrength = 0f;
            _stoodUp = false;
            _hopped = false;
            _getUpHops = 0;
            EnterRagdoll(seconds);
        }

        public void ForceRecoverSoon()
        {
            _slackUntil = Time.time;
            _recoverAt = Time.time;
            MotorState = GummyMotorState.Recovering;
        }

        public void Locomote(Vector3 worldHeading, float strength = 1f)
        {
            var heading = worldHeading;
            heading.y = 0f;
            if (heading.sqrMagnitude < 0.01f)
            {
                Halt();
                return;
            }

            _walkHeading = heading.normalized;
            _walkStrength = Mathf.Clamp01(strength);
        }

        public void Halt()
        {
            _walkStrength = 0f;
        }

        public void Plant()
        {
            Halt();
            BrakeHorizontal(0f, 0.12f);
        }

        public void ApplyPlayTune(float mass, float jointSpring, float recoverySeconds)
        {
            if (Personality == null)
                return;
            if (!_clonedPersonality)
            {
                Personality = Instantiate(Personality);
                _clonedPersonality = true;
            }

            Personality.Mass = Mathf.Max(0.1f, mass);
            Personality.JointSpring = Mathf.Max(0f, jointSpring);
            Personality.RecoverySeconds = Mathf.Max(0.05f, recoverySeconds);

            if (Root != null) Root.mass = Personality.Mass * 0.34f;
            if (Belly != null) Belly.mass = Personality.Mass * 0.30f;
            if (Head != null) Head.mass = Personality.Mass * 0.16f;
            var armMass = Personality.Mass * 0.10f;
            for (var i = 0; i < Parts.Length; i++)
            {
                var rb = Parts[i];
                if (rb == null || rb == Root || rb == Belly || rb == Head)
                    continue;
                rb.mass = armMass;
            }

            for (var i = 0; i < _joints.Length; i++)
            {
                var t = _joints[i];
                t.Linear.spring = Personality.JointSpring;
                t.Linear.damper = Personality.JointDamper;
                t.DriveX.positionSpring = Personality.JointSpring * 0.35f;
                t.DriveX.positionDamper = Personality.JointDamper;
                t.DriveYZ.positionSpring = Personality.JointSpring * 0.35f;
                t.DriveYZ.positionDamper = Personality.JointDamper;
                _joints[i] = t;
            }

            var was = _stiff;
            _stiff = !was;
            SetStiff(was);
        }

        public void EnterRagdoll(float recoverAfter)
        {
            MotorState = GummyMotorState.Ragdoll;
            _recoverAt = Time.time + Mathf.Max(0.05f, recoverAfter);
            SetStiff(false);
            SetDrag(false);
        }

        void FixedUpdate() => TickMotors(Time.fixedDeltaTime);

        public void TickMotors(float dt)
        {
            if (Root == null || Personality == null)
                return;

            var grounded = TryGround(out var groundY);
            RideDeck(grounded);

            if (!StringTaut)
            {
                if (MotorState == GummyMotorState.Ragdoll && Time.time >= _recoverAt && Root.linearVelocity.sqrMagnitude < 4f)
                    MotorState = GummyMotorState.Recovering;
                if (MotorState != GummyMotorState.Recovering || Time.time < _recoverAt)
                    return;
            }

            if (!_stoodUp)
            {
                var timedOut = Time.time >= _recoverAt + 0.8f;
                if ((grounded && Root.linearVelocity.y < 1.1f) ||
                    (timedOut && Root.linearVelocity.sqrMagnitude < 8f) ||
                    Time.time >= _recoverAt + 2.2f)
                {
                    _stoodUp = true;
                    _getUpHops = 2;
                    MotorState = GummyMotorState.Recovering;
                }
                else
                    return;
            }

            HoldUpright(groundY, grounded, dt);
            StackAxis();

            var walking = _walkStrength > 0.05f;
            if (walking)
                BounceHop(dt, grounded);
            else if (!walking && _getUpHops == 0 && grounded && _stoodUp)
                BrakeHorizontal(0.28f, 0.22f);
            else if (_getUpHops > 0 && grounded)
            {
                var savedHeading = _walkHeading;
                var savedStrength = _walkStrength;
                var wasHopped = _hopped;
                _walkHeading = Vector3.zero;
                _walkStrength = 0.42f;
                BounceHop(dt, grounded);
                if (_hopped && !wasHopped)
                    _getUpHops--;
                _walkHeading = savedHeading;
                _walkStrength = savedStrength;
            }

            FaceHeading(walking);
            DampSpin(Head, 0.86f);
            DampSpin(Belly, 0.88f);
            DampSpin(Root, 0.88f);

            MotorState = GummyMotorState.Locomoting;
            SetStiff(true);
            SetDrag(true);

            _walkStrength = Mathf.MoveTowards(_walkStrength, 0f, dt * 2.8f);
        }

        void HoldUpright(float groundY, bool grounded, float dt)
        {
            if (Head == null)
                return;

            _phase += dt * IdleHz * (Mathf.PI * 2f);
            var hang = (grounded ? groundY : 0f) + Personality.Height * 0.82f;
            hang += Mathf.Sin(_phase) * (Personality.Height * 0.035f);

            // One-way string: taut below hang, slack above (so a hop can leave the ground).
            var sag = hang - Head.position.y;
            if (sag <= 0f)
                return;

            var catchAccel = sag * (Personality.StandForce * 1.15f) - Head.linearVelocity.y * 7f;
            catchAccel = Mathf.Max(0f, catchAccel);
            // Same-phase lift. Hips stay planted when grounded so the string doesn't levitate them.
            LiftAxis(catchAccel, grounded);
        }

        void BounceHop(float dt, bool grounded)
        {
            var pace = HopHz * (0.85f + 0.15f * Mathf.Clamp01(Personality.WalkForce / 16f));
            _phase += dt * (pace - IdleHz) * (Mathf.PI * 2f);
            var cycle = Mathf.Repeat(_phase / (Mathf.PI * 2f), 1f);

            if (cycle < 0.34f)
            {
                _hopped = false;
                if (grounded)
                {
                    var squash = 5f * _walkStrength;
                    Head.AddForce(Vector3.down * squash, ForceMode.Acceleration);
                    if (Root != null)
                        Root.AddForce(Vector3.up * (squash * 0.55f), ForceMode.Acceleration);
                }
                return;
            }

            if (_hopped || cycle > 0.52f || !grounded)
                return;

            _hopped = true;
            var scale = _walkStrength * (0.8f + 0.2f * Mathf.Clamp01(Personality.WalkForce / 16f));
            var kick = Vector3.up * (HopUp * scale) + _walkHeading * (HopForward * scale);
            KickAxis(kick, 0.5f);
        }

        void LiftAxis(float accel, bool grounded)
        {
            if (accel <= 0.01f)
                return;
            var lift = Vector3.up * accel;
            AddAccel(Head, lift);
            AddAccel(Belly, lift * 0.92f);
            AddAccel(Root, lift * (grounded ? 0.22f : 0.88f));
        }

        void KickAxis(Vector3 deltaV, float armMul)
        {
            Kick(Head, deltaV);
            Kick(Belly, deltaV);
            Kick(Root, deltaV);
            for (var i = 0; i < Parts.Length; i++)
            {
                var p = Parts[i];
                if (p == null || p == Head || p == Belly || p == Root)
                    continue;
                Kick(p, deltaV * armMul);
            }
        }

        static void Kick(Rigidbody rb, Vector3 deltaV)
        {
            if (rb != null)
                rb.AddForce(deltaV, ForceMode.VelocityChange);
        }

        static void AddAccel(Rigidbody rb, Vector3 accel)
        {
            if (rb != null)
                rb.AddForce(accel, ForceMode.Acceleration);
        }

        void StackAxis()
        {
            if (Head == null || Root == null)
                return;

            var hips = Root.worldCenterOfMass;
            var lean = _walkStrength > 0.05f ? _walkHeading * (Personality.Height * 0.08f) : Vector3.zero;
            PullXz(Head, hips + lean, 14f);
            if (Belly != null)
            {
                var mid = Vector3.Lerp(hips, Head.worldCenterOfMass, 0.52f);
                PullXz(Belly, mid, 11f);
            }
        }

        static void PullXz(Rigidbody rb, Vector3 target, float spring)
        {
            var delta = target - rb.worldCenterOfMass;
            delta.y = 0f;
            rb.AddForce(delta * spring - Horizontal(rb.linearVelocity) * 3.2f, ForceMode.Acceleration);
        }

        void FaceHeading(bool walking)
        {
            if (!walking || Head == null)
                return;
            var fwd = Head.transform.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.01f)
                return;
            var align = Vector3.Cross(fwd.normalized, _walkHeading);
            Head.AddTorque(align * 10f - Head.angularVelocity * 0.35f, ForceMode.Acceleration);
        }

        bool TryGround(out float groundY)
        {
            groundY = 0f;
            if (Root == null || Personality == null)
            {
                _deck = null;
                return false;
            }

            var origin = Root.worldCenterOfMass + Vector3.up * 0.06f;
            var n = Physics.RaycastNonAlloc(origin, Vector3.down, _groundHits, Personality.Height * 1.15f, ~0, QueryTriggerInteraction.Ignore);
            var best = float.PositiveInfinity;
            var found = false;
            var hit = default(RaycastHit);
            for (var i = 0; i < n; i++)
            {
                if (IsOwn(_groundHits[i].rigidbody))
                    continue;
                if (_groundHits[i].distance >= best)
                    continue;
                best = _groundHits[i].distance;
                groundY = _groundHits[i].point.y;
                hit = _groundHits[i];
                found = true;
            }

            if (!found)
            {
                _deck = null;
                return false;
            }

            _deck = RideableDeck(hit);
            return Root.worldCenterOfMass.y - groundY < Personality.Height * 0.48f;
        }

        bool IsOwn(Rigidbody rb)
        {
            if (rb == null)
                return false;
            if (rb == Root || rb == Belly || rb == Head)
                return true;
            for (var i = 0; i < Parts.Length; i++)
            {
                if (Parts[i] == rb)
                    return true;
            }
            return false;
        }

        void RideDeck(bool grounded)
        {
            if (!grounded || _deck == null)
                return;
            var delta = _deck.Delta;
            delta.y = 0f;
            if (delta.sqrMagnitude < 1e-8f)
                return;
            for (var i = 0; i < Parts.Length; i++)
            {
                var rb = Parts[i];
                if (rb == null)
                    continue;
                rb.MovePosition(rb.position + delta);
            }
        }

        static MovingDeck RideableDeck(RaycastHit hit)
        {
            var col = hit.collider;
            if (col == null)
                return null;
            var part = col.GetComponentInParent<BreakablePart>();
            if (part != null && part.Detached)
                return null;
            return col.GetComponentInParent<MovingDeck>();
        }

        Vector3 DeckVelocity
        {
            get
            {
                if (_deck == null)
                    return Vector3.zero;
                var v = _deck.Velocity;
                v.y = 0f;
                return v;
            }
        }

        void BrakeHorizontal(float linearKeep, float angularKeep)
        {
            var ride = DeckVelocity;
            for (var i = 0; i < Parts.Length; i++)
            {
                var rb = Parts[i];
                if (rb == null)
                    continue;
                var v = rb.linearVelocity;
                v.x = v.x * linearKeep + ride.x * (1f - linearKeep);
                v.z = v.z * linearKeep + ride.z * (1f - linearKeep);
                rb.linearVelocity = v;
                rb.angularVelocity *= angularKeep;
            }
        }

        static Vector3 Horizontal(Vector3 v) => new Vector3(v.x, 0f, v.z);

        static void DampSpin(Rigidbody rb, float keep)
        {
            if (rb != null)
                rb.angularVelocity *= keep;
        }

        void SetDrag(bool standing)
        {
            var drag = standing ? Personality.Drag + 2.4f : Personality.Drag;
            var ang = standing ? Personality.AngularDrag + 6.2f : Personality.AngularDrag;
            for (var i = 0; i < Parts.Length; i++)
            {
                if (Parts[i] == null) continue;
                Parts[i].linearDamping = drag;
                Parts[i].angularDamping = ang;
            }
        }

        void CacheJoints()
        {
            var found = GetComponentsInChildren<ConfigurableJoint>();
            _joints = new JointTune[found.Length];
            for (var i = 0; i < found.Length; i++)
            {
                var j = found[i];
                _joints[i] = new JointTune
                {
                    Joint = j,
                    Linear = j.linearLimitSpring,
                    DriveX = j.angularXDrive,
                    DriveYZ = j.angularYZDrive,
                    LowX = j.lowAngularXLimit,
                    HighX = j.highAngularXLimit,
                    Y = j.angularYLimit,
                    Z = j.angularZLimit
                };
            }
        }

        void SetStiff(bool on)
        {
            if (_stiff == on || _joints.Length == 0)
            {
                _stiff = on;
                return;
            }
            _stiff = on;
            for (var i = 0; i < _joints.Length; i++)
            {
                var t = _joints[i];
                if (t.Joint == null)
                    continue;
                if (!on)
                {
                    t.Joint.linearLimitSpring = t.Linear;
                    t.Joint.angularXDrive = t.DriveX;
                    t.Joint.angularYZDrive = t.DriveYZ;
                    t.Joint.lowAngularXLimit = t.LowX;
                    t.Joint.highAngularXLimit = t.HighX;
                    t.Joint.angularYLimit = t.Y;
                    t.Joint.angularZLimit = t.Z;
                    continue;
                }

                var lin = t.Linear;
                lin.spring *= 2.6f;
                lin.damper *= 1.7f;
                t.Joint.linearLimitSpring = lin;

                var dx = t.DriveX;
                dx.positionSpring *= 3f;
                dx.maximumForce = 520f;
                t.Joint.angularXDrive = dx;
                var dyz = t.DriveYZ;
                dyz.positionSpring *= 3f;
                dyz.maximumForce = 520f;
                t.Joint.angularYZDrive = dyz;

                var tight = new SoftJointLimit { limit = 12f };
                t.Joint.lowAngularXLimit = new SoftJointLimit { limit = -12f };
                t.Joint.highAngularXLimit = tight;
                t.Joint.angularYLimit = tight;
                t.Joint.angularZLimit = tight;
            }
        }

        struct JointTune
        {
            public ConfigurableJoint Joint;
            public SoftJointLimitSpring Linear;
            public JointDrive DriveX;
            public JointDrive DriveYZ;
            public SoftJointLimit LowX;
            public SoftJointLimit HighX;
            public SoftJointLimit Y;
            public SoftJointLimit Z;
        }
    }
}
