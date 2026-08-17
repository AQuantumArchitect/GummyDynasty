using UnityEngine;

namespace GummyDynasty.Simulation
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ProjectileBall : MonoBehaviour
    {
        public string FirerId;
        public float Lifetime = 5f;
        public bool InFlight => !_settled && _rb != null && _rb.linearVelocity.sqrMagnitude > 1f;

        float _dieAt;
        bool _settled;
        bool _punched;
        Rigidbody _rb;

        void Awake() => _rb = GetComponent<Rigidbody>();

        void OnEnable() => _dieAt = Time.time + Lifetime;

        void Update()
        {
            if (Time.time >= _dieAt)
                Destroy(gameObject);
        }

        void FixedUpdate()
        {
            if (_settled || _rb == null)
                return;
            if (_punched && _rb.linearVelocity.sqrMagnitude < 1.44f)
                Settle();
        }

        void OnCollisionEnter(Collision collision)
        {
            var body = collision.rigidbody != null
                ? collision.rigidbody.GetComponentInParent<GummyBody>()
                : null;
            if (body != null)
            {
                _punched = true;
                var downFor = body.Personality != null ? body.Personality.RecoverySeconds * 0.45f : 0.55f;
                body.Slack(Mathf.Clamp(downFor, 0.35f, 1.6f));
                var agent = body.GetComponent<GummyAgent>();
                var self = !string.IsNullOrEmpty(FirerId) && body.AgentId == FirerId;
                var severity = collision.impulse.magnitude / 28f;
                agent?.ObserveWorldImpact(severity, self, "candy");
                return;
            }

            var other = collision.rigidbody;
            if (other == null || other.isKinematic)
                Settle();
        }

        void Settle()
        {
            if (_settled)
                return;
            _settled = true;
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();
            if (_rb != null)
            {
                _rb.linearVelocity *= 0.22f;
                _rb.angularVelocity *= 0.22f;
                _rb.linearDamping = CandyShot.SettleDamping;
                _rb.angularDamping = 6f;
            }
            _dieAt = Mathf.Min(_dieAt, Time.time + CandyShot.AfterHitLifetime);
        }
    }
}
