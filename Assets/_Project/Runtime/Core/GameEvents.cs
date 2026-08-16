using System;

namespace GummyDynasty.Core
{
    /// <summary>Process-wide event hub. Simulation raises; Presentation/UI listen.</summary>
    public static class GameEvents
    {
        public static event Action SessionStarted;
        public static event Action SessionEnded;
        public static event Action<string> StatusChanged;

        public static void RaiseSessionStarted() => SessionStarted?.Invoke();
        public static void RaiseSessionEnded() => SessionEnded?.Invoke();
        public static void RaiseStatus(string message) => StatusChanged?.Invoke(message);

        public static void Clear()
        {
            SessionStarted = null;
            SessionEnded = null;
            StatusChanged = null;
        }
    }
}
