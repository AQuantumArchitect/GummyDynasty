using UnityEngine;

namespace GummyDynasty.Simulation
{
    [CreateAssetMenu(menuName = "GummyDynasty/Projectile Personality", fileName = "ProjectilePersonality")]
    public sealed class ProjectilePersonality : ScriptableObject
    {
        public string DisplayName = "Candy";
        public Color Color = new Color(1f, 0.92f, 0.12f, 1f);
        [Min(0.1f)] public float Mass = 5f;
        [Min(0.1f)] public float Speed = 10.77f;
        [Min(0.1f)] public float Scale = 0.9f;
        [Min(0.05f)] public float LaunchMul = 1f;

        public static ProjectilePersonality CreateRuntime(string displayName, Color color, float mass, float speed, float scale, float launchMul)
        {
            var p = CreateInstance<ProjectilePersonality>();
            p.DisplayName = displayName;
            p.Color = color;
            p.Mass = mass;
            p.Speed = speed;
            p.Scale = scale;
            p.LaunchMul = launchMul;
            return p;
        }

        public static ProjectilePersonality CandyPreset()
        {
            return CreateRuntime("Candy", new Color(1f, 0.92f, 0.12f), CandyShot.Mass, CandyShot.Speed, CandyShot.Scale, 1f);
        }

        public static ProjectilePersonality GumdropPreset()
        {
            return CreateRuntime("Gumdrop", new Color(1f, 0.45f, 0.72f), 1.35f, 9f, 0.52f, 1.35f);
        }

        public static ProjectilePersonality JawbreakerPreset()
        {
            return CreateRuntime("Jawbreaker", new Color(0.38f, 0.12f, 0.48f), 8f, 7.5f, 1.12f, 0.52f);
        }
    }
}
