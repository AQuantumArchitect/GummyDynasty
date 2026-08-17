using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>
    /// Host candy shot. Midpoint of the foam dart (0.92 @ 8, KE ≈ 29) and the
    /// bowling ball (3.4 @ 18, KE ≈ 551): a heavy catapult stone that punches
    /// through, then plants on the ground. X-button wall is still the boom.
    /// </summary>
    public static class CandyShot
    {
        public const float Mass = 5f;
        public const float Speed = 10.77f;
        public const float Scale = 0.9f;
        public const float Lifetime = 6f;
        public const float AfterHitLifetime = 2.2f;
        public const float SettleDamping = 4.2f;
        public const float FoamKe = 0.5f * 0.92f * 8f * 8f;
        public const float BowlingKe = 0.5f * 3.4f * 18f * 18f;

        public const float SmashImpulsePerCrate = 8.3f;
        public const int SmashCrateCount = 20;

        public static float KineticEnergy => 0.5f * Mass * Speed * Speed;
        public static float Momentum => Mass * Speed;
        public static float SmashWallImpulse => SmashImpulsePerCrate * SmashCrateCount;
        public static float MidpointKe => (FoamKe + BowlingKe) * 0.5f;

        static PhysicsMaterial _mat;

        public static PhysicsMaterial Material()
        {
            if (_mat != null)
                return _mat;
            _mat = new PhysicsMaterial("CandyShot")
            {
                bounciness = 0.05f,
                dynamicFriction = 0.95f,
                staticFriction = 1f,
                bounceCombine = PhysicsMaterialCombine.Minimum,
                frictionCombine = PhysicsMaterialCombine.Maximum
            };
            return _mat;
        }

        public static Vector3 BallisticDirection(Vector3 origin, Vector3 target)
        {
            var to = target - origin;
            var t = Mathf.Clamp(to.magnitude / Speed, 0.12f, 1.25f);
            to.y += 0.5f * 9.81f * t * t;
            return to.sqrMagnitude > 0.0001f ? to.normalized : Vector3.forward;
        }

        public static Vector3 DirectDirection(Vector3 origin, Vector3 target)
        {
            var to = target - origin;
            var t = Mathf.Clamp(to.magnitude / Speed, 0.08f, 1.1f);
            to.y += 0.5f * 9.81f * t * t * 0.86f;
            return to.sqrMagnitude > 0.0001f ? to.normalized : Vector3.forward;
        }

        public static GameObject Spawn(Vector3 origin, Vector3 direction, string firerId, ProjectilePersonality profile = null)
        {
            var mass = profile != null ? profile.Mass : Mass;
            var speed = profile != null ? profile.Speed : Speed;
            var scale = profile != null ? profile.Scale : Scale;
            var color = profile != null ? profile.Color : new Color(1f, 0.92f, 0.12f);
            var dir = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector3.forward;
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = profile != null ? profile.DisplayName : "CandyShot";
            ball.transform.position = origin;
            ball.transform.localScale = Vector3.one * scale;
            ball.GetComponent<Renderer>().sharedMaterial = GummyLook.Material(color);
            ball.GetComponent<Collider>().sharedMaterial = Material();

            var rb = ball.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.maxDepenetrationVelocity = 3.5f;
            rb.linearDamping = 0.22f;
            rb.angularDamping = 3.4f;
            rb.AddForce(dir * speed, ForceMode.VelocityChange);

            var proj = ball.AddComponent<ProjectileBall>();
            proj.FirerId = firerId;
            proj.Lifetime = Lifetime;

            var trail = ball.AddComponent<TrailRenderer>();
            trail.time = 0.42f;
            trail.startWidth = 0.28f;
            trail.endWidth = 0.03f;
            trail.material = GummyLook.Material(new Color(1f, 0.45f, 0.08f));
            trail.startColor = new Color(1f, 0.95f, 0.4f);
            trail.endColor = new Color(1f, 0.2f, 0.05f, 0f);
            return ball;
        }
    }
}
