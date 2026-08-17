namespace GummyDynasty.Simulation
{
    /// <summary>Data-driven victory. Unity-free so the harness can prove hold-WEST.</summary>
    public sealed class GameModeRules
    {
        public const string HoldFlag = "hold-flag";

        public string DisplayName = "Hold WEST";
        public string ObjectiveLabel = "WEST";
        public string Victory = HoldFlag;
        public string CommanderRole = PhoneCommand.RoleCommander;
        public string ArtilleryRole = PhoneCommand.RoleArtillery;
        public float ArriveBand = 1.6f;

        public static GameModeRules HoldWest()
        {
            return new GameModeRules();
        }

        public static GameModeRules Create(string displayName, string objectiveLabel, string victory)
        {
            var r = new GameModeRules();
            if (!string.IsNullOrEmpty(displayName))
                r.DisplayName = displayName;
            if (!string.IsNullOrEmpty(objectiveLabel))
                r.ObjectiveLabel = objectiveLabel;
            if (!string.IsNullOrEmpty(victory))
                r.Victory = victory;
            return r;
        }

        public bool CheckVictory(float comX, float flagX, bool inPlay)
        {
            if (!inPlay)
                return false;
            if (Victory != HoldFlag)
                return false;
            return comX <= flagX + ArriveBand && comX >= flagX - ArriveBand * 2f;
        }
    }
}
