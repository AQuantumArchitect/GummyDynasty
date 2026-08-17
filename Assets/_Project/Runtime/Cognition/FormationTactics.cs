namespace GummyDynasty.Cognition
{
    /// <summary>
    /// Formation-level policy. Individuals still dodge incoming and go down locally.
    /// Congestion is the file's problem, not another per-hopper sidestep.
    /// </summary>
    public static class FormationTactics
    {
        public const string MarchWest = IntentResolver.MarchWest;
        public const string Spread = "spread";
        public const string ThroughBreach = "through-breach";
        public const string Hold = "hold";

        public const float WallAhead = 0.5f;
        public const float Hole = 0.5f;
        public const float HoldOrder = 0.55f;

        public static string Resolve(float objective, bool arrived, float wallAhead, float hole, float holdOrder = 0f)
        {
            if (objective < IntentResolver.MarchFloor)
                return IntentResolver.Idle;
            if (arrived || holdOrder >= HoldOrder)
                return Hold;
            if (hole >= Hole)
                return ThroughBreach;
            if (wallAhead >= WallAhead)
                return Spread;
            return MarchWest;
        }

        public static bool Travels(string tactic)
        {
            return tactic == MarchWest || tactic == Spread || tactic == ThroughBreach;
        }
    }
}
