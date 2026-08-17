using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>
    /// Shipping presets. Prefers on-disk assets so a friend can tweak them;
    /// falls back to the same numbers if the asset is missing.
    /// </summary>
    public static class PersonalityCatalog
    {
        public static PhysicalPersonality Gummy() => Load("Gummy", PhysicalPersonality.GummyPreset);
        public static PhysicalPersonality Knight() => Load("Knight", PhysicalPersonality.KnightPreset);
        public static PhysicalPersonality Scout() => Load("Scout", PhysicalPersonality.ScoutPreset);
        public static PhysicalPersonality Marcher() => Load("Marcher", PhysicalPersonality.MarcherPreset);

        public static ProjectilePersonality Candy() => LoadProjectile("Candy", ProjectilePersonality.CandyPreset);
        public static ProjectilePersonality Gumdrop() => LoadProjectile("Gumdrop", ProjectilePersonality.GumdropPreset);
        public static ProjectilePersonality Jawbreaker() => LoadProjectile("Jawbreaker", ProjectilePersonality.JawbreakerPreset);

        public static GummyUnit Levy() => Resources.Load<GummyUnit>("Units/Levy");

        public static GameModeDefinition HoldWestMode() => Resources.Load<GameModeDefinition>("Modes/HoldWest");

        public static GameModeRules Rules()
        {
            var mode = HoldWestMode();
            if (mode == null)
                return GameModeRules.HoldWest();
            var label = mode.Objective != null ? mode.Objective.Label : "WEST";
            return GameModeRules.Create(mode.DisplayName, label, mode.Victory);
        }

        public static PhysicalPersonality FromUnit(GummyUnit unit)
        {
            if (unit != null && unit.Personality != null)
                return unit.Personality;
            return Gummy();
        }

        static PhysicalPersonality Load(string name, System.Func<PhysicalPersonality> fallback)
        {
            var asset = Resources.Load<PhysicalPersonality>("Personalities/" + name);
            return asset != null ? asset : fallback();
        }

        static ProjectilePersonality LoadProjectile(string name, System.Func<ProjectilePersonality> fallback)
        {
            var asset = Resources.Load<ProjectilePersonality>("Projectiles/" + name);
            return asset != null ? asset : fallback();
        }
    }
}
