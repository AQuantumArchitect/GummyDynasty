using GummyDynasty.Core;
using GummyDynasty.Simulation;
using UnityEngine;

namespace GummyDynasty.UI
{
    /// <summary>Temporary on-screen status until UI Toolkit screens exist.</summary>
    public sealed class HudController : MonoBehaviour
    {
        string _status = "GummyDynasty";
        SessionState _session;

        void OnEnable() => GameEvents.StatusChanged += OnStatus;
        void OnDisable() => GameEvents.StatusChanged -= OnStatus;

        void Start()
        {
            ServiceRegistry.Current?.TryGet(out _session);
        }

        void OnStatus(string message) => _status = message;

        void OnGUI()
        {
            const int pad = 12;
            var phase = _session != null ? _session.Phase.ToString() : "—";
            var tick = _session != null ? _session.Tick.ToString() : "0";
            GUI.Label(new Rect(pad, pad, 640, 24), $"GummyDynasty  |  {phase}  |  tick {tick}");
            GUI.Label(new Rect(pad, pad + 22, 640, 24), _status);
        }
    }
}
