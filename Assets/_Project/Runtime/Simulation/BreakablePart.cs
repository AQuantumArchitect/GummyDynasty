using UnityEngine;

namespace GummyDynasty.Simulation
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BreakablePart : MonoBehaviour
    {
        [SerializeField] float breakImpulse = 4.5f;
        [SerializeField] float restSeconds = 0.7f;

        Joint[] _joints;
        Rigidbody _rb;
        bool _detached;
        bool _settled;
        float _rest;

        public bool Detached => _detached;
        public bool Settled => _settled;
        public bool Marked { get; private set; }

        static readonly Color Wood = new Color(0.72f, 0.55f, 0.28f);
        static readonly Color Mark = new Color(1f, 0.92f, 0.28f);

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _joints = GetComponents<Joint>();
        }

        void FixedUpdate()
        {
            if (!_detached || _rb == null || _settled)
                return;
            if (_rb.linearVelocity.y > 1.2f)
                return;
            var v = _rb.linearVelocity;
            v.x *= 0.82f;
            v.z *= 0.82f;
            _rb.linearVelocity = v;
            _rb.angularVelocity *= 0.88f;

            if (_rb.linearVelocity.sqrMagnitude < 0.18f && _rb.position.y < 1.2f)
            {
                _rest += Time.fixedDeltaTime;
                if (_rest >= restSeconds)
                {
                    _rb.linearVelocity = Vector3.zero;
                    _rb.angularVelocity = Vector3.zero;
                    _rb.isKinematic = true;
                    _settled = true;
                }
            }
            else
                _rest = 0f;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (_detached || collision.impulse.magnitude < breakImpulse)
                return;
            Detach();
        }

        void OnJointBreak(float breakForce)
        {
            _joints = null;
            Detach();
        }

        public void Detach()
        {
            if (_detached)
                return;
            _detached = true;
            _settled = false;
            _rest = 0f;
            if (_joints != null)
            {
                for (var i = 0; i < _joints.Length; i++)
                {
                    if (_joints[i] != null)
                        Destroy(_joints[i]);
                }
            }
            _joints = null;
            if (_rb != null)
            {
                _rb.isKinematic = false;
                _rb.WakeUp();
            }
        }

        public void Blast(Vector3 origin, float force)
        {
            Detach();
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();
            if (_rb == null)
                return;

            var dir = transform.position - origin;
            dir.y = Mathf.Abs(dir.y) + 0.55f;
            if (dir.sqrMagnitude < 0.01f)
                dir = Vector3.up + UnityEngine.Random.insideUnitSphere * 0.2f;
            _rb.AddForce(dir.normalized * force + Vector3.up * (force * 0.4f), ForceMode.Impulse);
            _rb.AddTorque(UnityEngine.Random.insideUnitSphere * force, ForceMode.Impulse);
            Tint(new Color(1f, 0.45f, 0.12f));
        }

        public void Mark(bool on)
        {
            Marked = on;
            Tint(on ? Mark : Wood);
        }

        public void Yeet(Vector3 direction)
        {
            Detach();
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();
            if (_rb == null)
                return;
            var heading = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector3.forward;
            _rb.AddForce(heading * 11f + Vector3.up * 4.2f, ForceMode.VelocityChange);
        }

        void Tint(Color color)
        {
            var rend = GetComponent<Renderer>();
            if (rend != null)
                rend.sharedMaterial = GummyLook.Material(color);
        }
    }
}
