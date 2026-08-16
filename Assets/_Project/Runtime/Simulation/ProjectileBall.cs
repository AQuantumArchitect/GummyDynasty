using UnityEngine;

namespace GummyDynasty.Simulation
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ProjectileBall : MonoBehaviour
    {
        public string FirerId;
        public float Lifetime = 8f;

        float _dieAt;

        void OnEnable() => _dieAt = Time.time + Lifetime;

        void Update()
        {
            if (Time.time >= _dieAt)
                Destroy(gameObject);
        }

        void OnCollisionEnter(Collision collision)
        {
            var body = collision.rigidbody != null ? collision.rigidbody.GetComponentInParent<GummyBody>() : null;
            if (body == null)
                return;
            var agent = body.GetComponent<GummyAgent>();
            var self = !string.IsNullOrEmpty(FirerId) && body.AgentId == FirerId;
            agent?.ObserveWorldImpact(collision.impulse.magnitude / 12f, self);
        }
    }
}
