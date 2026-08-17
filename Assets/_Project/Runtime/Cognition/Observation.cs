namespace GummyDynasty.Cognition
{
    public enum NodeKind : byte
    {
        World = 0,
        Actor = 1,
        Faction = 2,
        Army = 3,
        Formation = 4
    }

    public enum RoleMode : byte
    {
        Dissipative = 0,
        Unitary = 1
    }

    public enum ReduceOp : byte
    {
        Mean = 0,
        Max = 1,
        Or = 2
    }

    /// <summary>η=0 is a no-op. Self-tagged reports do not update world nodes.</summary>
    public readonly struct Observation
    {
        public readonly string NodeId;
        public readonly string Role;
        public readonly float Value;
        public readonly float Eta;
        public readonly string ActorId;
        public readonly bool SelfTagged;

        public Observation(string nodeId, string role, float value, float eta, string actorId = null, bool selfTagged = false)
        {
            NodeId = nodeId;
            Role = role;
            Value = value < 0f ? 0f : value > 1f ? 1f : value;
            Eta = eta < 0f ? 0f : eta > 1f ? 1f : eta;
            ActorId = actorId;
            SelfTagged = selfTagged;
        }
    }

    public readonly struct Belief
    {
        public readonly string NodeId;
        public readonly string Role;
        public readonly float Value;
        public readonly float Confidence;

        public Belief(string nodeId, string role, float value, float confidence)
        {
            NodeId = nodeId;
            Role = role;
            Value = value;
            Confidence = confidence;
        }
    }

    public readonly struct Intent
    {
        public readonly string ActorId;
        public readonly string Name;
        public readonly float Strength;
        public readonly bool Shadow;

        public Intent(string actorId, string name, float strength = 1f, bool shadow = true)
        {
            ActorId = actorId;
            Name = name;
            Strength = strength;
            Shadow = shadow;
        }
    }
}
