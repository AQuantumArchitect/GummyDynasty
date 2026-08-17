namespace GummyDynasty.Cognition
{
    /// <summary>
    /// History writes the field. A just-flattened actor stays in pain after the
    /// impulse dies; an untouched neighbor does not. That is E2.
    /// </summary>
    public static class MemorySense
    {
        public static void Write(BeliefField field, string agentId, AgentMemory memory, bool upright)
        {
            if (field == null || memory == null || string.IsNullOrEmpty(agentId))
                return;

            var flattened = memory.Recency(MemoryKind.Flattened);
            var pain = upright ? flattened : 1f;
            field.Observe(new Observation(
                agentId,
                HierarchyRoles.Pain,
                pain,
                pain >= 0.5f ? 0.7f : 0.2f,
                agentId));

            var threat = memory.Recency(MemoryKind.Hit);
            var incoming = memory.Recency(MemoryKind.Incoming);
            var ally = memory.Recency(MemoryKind.SawAllyDown) * 0.65f;
            if (incoming > threat)
                threat = incoming;
            if (ally > threat)
                threat = ally;
            field.Observe(new Observation(
                agentId,
                HierarchyRoles.Threat,
                threat,
                threat > 0.05f ? 0.5f : 0.2f,
                agentId));
        }
    }
}
