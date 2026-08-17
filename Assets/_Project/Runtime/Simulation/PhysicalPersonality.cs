using UnityEngine;

namespace GummyDynasty.Simulation
{
    [CreateAssetMenu(menuName = "GummyDynasty/Physical Personality", fileName = "PhysicalPersonality")]
    public sealed class PhysicalPersonality : ScriptableObject
    {
        [Header("Identity")]
        public string DisplayName = "Gummy";
        public Color Color = new Color(0.95f, 0.2f, 0.35f, 1f);

        [Header("Proportions")]
        [Min(0.2f)] public float Height = 1.1f;
        [Range(0.4f, 2.2f)] public float Width = 1f;
        [Range(0.5f, 2.5f)] public float HeadScale = 1.15f;
        [Range(0.4f, 1.6f)] public float ArmScale = 0.85f;

        [Header("Mass")]
        [Min(0.1f)] public float Mass = 2.4f;
        [Range(0f, 8f)] public float Drag = 0.6f;
        [Range(0f, 8f)] public float AngularDrag = 1.4f;

        [Header("Jelly")]
        [Min(0f)] public float JointSpring = 180f;
        [Min(0f)] public float JointDamper = 14f;
        [Range(0f, 1f)] public float Bounciness = 0.22f;
        [Range(0f, 1f)] public float DynamicFriction = 0.72f;
        [Range(0f, 1f)] public float StaticFriction = 0.85f;
        [Range(0f, 0.5f)] public float Squash = 0.22f;

        [Header("Combat feel")]
        [Min(0f)] public float LaunchMultiplier = 1f;
        [Min(0f)] public float KnockImpulse = 6.5f;
        [Min(0.05f)] public float RecoverySeconds = 1.4f;
        [Min(0f)] public float StandForce = 28f;
        [Min(0f)] public float WalkForce = 16f;

        public static PhysicalPersonality CreateRuntime(string displayName, Color color, float mass, float height, float launch, float knock)
        {
            var p = CreateInstance<PhysicalPersonality>();
            p.DisplayName = displayName;
            p.Color = color;
            p.Mass = mass;
            p.Height = height;
            p.LaunchMultiplier = launch;
            p.KnockImpulse = knock;
            return p;
        }

        public static PhysicalPersonality GummyPreset()
        {
            return CreateRuntime("Gummy", new Color(0.95f, 0.22f, 0.36f), 2.8f, 1.85f, 1f, 6.5f);
        }

        public static PhysicalPersonality KnightPreset()
        {
            var p = CreateRuntime("Knight", new Color(0.35f, 0.2f, 0.7f), 7.2f, 2.35f, 0.7f, 10f);
            p.JointSpring = 240f;
            p.RecoverySeconds = 2.1f;
            p.WalkForce = 10f;
            return p;
        }

        public static PhysicalPersonality ScoutPreset()
        {
            var p = CreateRuntime("Scout", new Color(0.2f, 0.85f, 0.45f), 1.3f, 1.15f, 1.45f, 4.2f);
            p.JointSpring = 120f;
            p.RecoverySeconds = 0.7f;
            p.WalkForce = 22f;
            return p;
        }

        public static PhysicalPersonality MarcherPreset()
        {
            var p = CreateRuntime("Marcher", new Color(0.98f, 0.35f, 0.18f), 2.2f, 1.65f, 1f, 6f);
            p.RecoverySeconds = 0.85f;
            p.WalkForce = 22f;
            return p;
        }
    }
}
