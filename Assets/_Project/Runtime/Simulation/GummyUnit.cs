using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>Creator stub: color + personality + default intent. Not an engine type.</summary>
    [CreateAssetMenu(menuName = "GummyDynasty/Gummy Unit", fileName = "GummyUnit")]
    public sealed class GummyUnit : ScriptableObject
    {
        public string DisplayName = "Levy";
        public FactionDefinition Faction;
        public PhysicalPersonality Personality;
        public Color Color = new Color(0.95f, 0.22f, 0.36f, 1f);
        public string DefaultIntent = "idle";
    }
}
