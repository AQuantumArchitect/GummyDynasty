using GummyDynasty.Core;
using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>Drives SessionState from Unity's player loop. Presentation should read state, not mutate it.</summary>
    public sealed class SessionDirector : MonoBehaviour
    {
        public SessionState State { get; } = new SessionState();

        void Awake()
        {
            ServiceRegistry.Current?.Register(State);
            ServiceRegistry.Current?.Register(this);
        }

        void Start()
        {
            State.Start();
            GameEvents.RaiseSessionStarted();
            GameEvents.RaiseStatus("session running");
        }

        void Update()
        {
            State.Advance(Time.deltaTime);
        }

        void OnDestroy()
        {
            State.Stop();
            GameEvents.RaiseSessionEnded();
        }
    }
}
