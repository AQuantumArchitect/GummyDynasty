using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>Loose pit toy: gumdrop, jawbreaker, anything a friend can yeet.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Tossable : MonoBehaviour
    {
        public string Label = "Candy";
        public float LaunchMul = 1f;

        Rigidbody _rb;

        void Awake() => _rb = GetComponent<Rigidbody>();

        void FixedUpdate()
        {
            if (_rb == null)
                return;
            if (_rb.linearVelocity.y > 1.1f || _rb.position.y > 1.4f)
                return;
            var v = _rb.linearVelocity;
            var horiz = v.x * v.x + v.z * v.z;
            if (horiz < 0.05f)
            {
                v.x = 0f;
                v.z = 0f;
                _rb.linearVelocity = v;
                _rb.angularVelocity *= 0.35f;
                return;
            }
            v.x *= 0.84f;
            v.z *= 0.84f;
            _rb.linearVelocity = v;
            _rb.angularVelocity *= 0.86f;
        }

        public void Yeet(Vector3 direction)
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();
            if (_rb == null)
                return;
            _rb.isKinematic = false;
            _rb.WakeUp();
            var heading = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector3.forward;
            var mag = 10.5f * Mathf.Max(0.15f, LaunchMul);
            _rb.AddForce(heading * mag + Vector3.up * (mag * 0.38f), ForceMode.VelocityChange);
        }
    }
}
