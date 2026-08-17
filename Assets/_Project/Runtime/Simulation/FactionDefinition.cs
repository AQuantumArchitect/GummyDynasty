using UnityEngine;

namespace GummyDynasty.Simulation
{
    [CreateAssetMenu(menuName = "GummyDynasty/Faction", fileName = "Faction")]
    public sealed class FactionDefinition : ScriptableObject
    {
        public string DisplayName = "Red";
        public Color Color = new Color(0.86f, 0.16f, 0.22f, 1f);
    }
}
