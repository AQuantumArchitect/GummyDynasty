using UnityEngine;

namespace GummyDynasty.Simulation
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BreakablePart : MonoBehaviour
    {
        [SerializeField] float breakImpulse = 4.5f;

        Joint _joint;
        Rigidbody _rb;
        bool _detached;

        public bool Detached => _detached;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _joint = GetComponent<Joint>();
        }

        void OnCollisionEnter(Collision collision)
        {
            if (_detached || collision.impulse.magnitude < breakImpulse)
                return;
            Detach();
        }

        public void Detach()
        {
            if (_detached)
                return;
            _detached = true;
            if (_joint != null)
                Destroy(_joint);
            if (_rb != null)
            {
                _rb.isKinematic = false;
                _rb.WakeUp();
            }
        }
    }
}
