namespace GummyDynasty.Simulation
{
    public enum SessionPhase
    {
        Idle,
        Running,
        Paused
    }

    /// <summary>Authoritative session snapshot. Expand this; do not put Unity view types here.</summary>
    public sealed class SessionState
    {
        public SessionPhase Phase { get; private set; } = SessionPhase.Idle;
        public float ElapsedSeconds { get; private set; }
        public int Tick { get; private set; }

        public void Start()
        {
            Phase = SessionPhase.Running;
            ElapsedSeconds = 0f;
            Tick = 0;
        }

        public void Pause()
        {
            if (Phase == SessionPhase.Running)
                Phase = SessionPhase.Paused;
        }

        public void Resume()
        {
            if (Phase == SessionPhase.Paused)
                Phase = SessionPhase.Running;
        }

        public void Stop()
        {
            Phase = SessionPhase.Idle;
        }

        public void Advance(float deltaSeconds)
        {
            if (Phase != SessionPhase.Running)
                return;

            ElapsedSeconds += deltaSeconds;
            Tick++;
        }
    }
}
