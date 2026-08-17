namespace GummyDynasty.Cognition
{
    public readonly struct ResolvedIntent
    {
        public readonly string ActorId;
        public readonly string InheritedName;
        public readonly string LocalName;
        public readonly string EffectiveName;
        public readonly float InheritedStrength;
        public readonly float LocalStrength;
        public readonly bool Overridden;

        public ResolvedIntent(
            string actorId,
            string inheritedName,
            string localName,
            string effectiveName,
            float inheritedStrength,
            float localStrength,
            bool overridden)
        {
            ActorId = actorId;
            InheritedName = inheritedName;
            LocalName = localName;
            EffectiveName = effectiveName;
            InheritedStrength = inheritedStrength;
            LocalStrength = localStrength;
            Overridden = overridden;
        }
    }

    /// <summary>
    /// Shadow-first policy: inherit the formation tactic unless local threat or pain
    /// demands a dodge or collapse. Congestion is a formation problem (see FormationTactics).
    /// </summary>
    public static class IntentResolver
    {
        public const string MarchWest = "march-west";
        public const string Dodge = "dodge";
        public const string Idle = "idle";
        public const string Down = "down";

        public const float ThreatOverride = 0.55f;
        public const float CongestionOverride = 0.62f;
        public const float PainDown = 0.7f;
        public const float MarchFloor = 0.55f;
        public const float ArriveBand = 1.6f;

        public static string InheritName(in Belief objective, string formationTactic)
        {
            if (objective.Value < MarchFloor)
                return Idle;
            if (string.IsNullOrEmpty(formationTactic) || formationTactic == Idle)
                return MarchWest;
            return formationTactic;
        }

        public static ResolvedIntent Resolve(
            string actorId,
            in Belief objective,
            in Belief threat,
            in Belief congestion,
            in Belief pain,
            bool upright,
            string formationTactic = null)
        {
            var inherited = InheritName(objective, formationTactic);
            var local = Idle;
            var localStr = 0f;

            if (!upright || pain.Value >= PainDown)
            {
                local = Down;
                localStr = upright ? pain.Value : 1f;
            }
            else if (threat.Value >= ThreatOverride)
            {
                local = Dodge;
                localStr = threat.Value;
            }

            var overridden = local == Dodge || local == Down;
            return new ResolvedIntent(
                actorId,
                inherited,
                local,
                overridden ? local : inherited,
                objective.Value,
                localStr,
                overridden);
        }
    }
}
