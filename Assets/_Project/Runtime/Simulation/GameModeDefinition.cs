using UnityEngine;

namespace GummyDynasty.Simulation
{
    [CreateAssetMenu(menuName = "GummyDynasty/Game Mode", fileName = "GameMode")]
    public sealed class GameModeDefinition : ScriptableObject
    {
        public string DisplayName = "Hold WEST";
        public ObjectiveDefinition Objective;
        public string Victory = GameModeRules.HoldFlag;
        public string CommanderRole = PhoneCommand.RoleCommander;
        public string ArtilleryRole = PhoneCommand.RoleArtillery;
    }
}
