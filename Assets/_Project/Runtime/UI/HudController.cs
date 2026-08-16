using System.Collections.Generic;
using System.Text;
using GummyDynasty.Cognition;
using GummyDynasty.Core;
using GummyDynasty.Simulation;
using UnityEngine;

namespace GummyDynasty.UI
{
    /// <summary>Host debug HUD. Player tactical UI will live on phones, not here.</summary>
    public sealed class HudController : MonoBehaviour
    {
        string _status = "GummyDynasty";
        SessionState _session;
        ToySandboxDirector _toy;
        readonly List<Belief> _beliefs = new List<Belief>(8);
        readonly StringBuilder _line = new StringBuilder(256);

        void OnEnable() => GameEvents.StatusChanged += OnStatus;
        void OnDisable() => GameEvents.StatusChanged -= OnStatus;

        void Start()
        {
            ServiceRegistry.Current?.TryGet(out _session);
            ServiceRegistry.Current?.TryGet(out _toy);
        }

        void OnStatus(string message) => _status = message;

        void OnGUI()
        {
            const int pad = 12;
            var phase = _session != null ? _session.Phase.ToString() : "—";
            var tick = _session != null ? _session.Tick.ToString() : "0";
            var n = _toy != null ? _toy.GummyCount : 0;
            GUI.Label(new Rect(pad, pad, 900, 22), $"GummyDynasty  |  {phase}  |  tick {tick}  |  gummies {n}");
            GUI.Label(new Rect(pad, pad + 20, 1100, 22), _status);
            GUI.Label(new Rect(pad, pad + 40, 1100, 22), "1 default   2 knight   3 scout   click select   space launch   K knock   F fire   B smash   R reset   F5-F8 bench   RMB orbit");

            if (_toy == null || _toy.Selected == null)
                return;

            var body = _toy.Selected;
            var agent = body.GetComponent<GummyAgent>();
            agent?.CopyBeliefs(_beliefs);
            _line.Clear();
            _line.Append(body.Personality != null ? body.Personality.DisplayName : "Gummy");
            _line.Append("  ").Append(body.AgentId);
            _line.Append("  ").Append(body.MotorState);
            _line.Append("  v=").Append(body.Velocity.magnitude.ToString("0.0"));
            GUI.Label(new Rect(pad, pad + 68, 1100, 22), _line.ToString());

            var y = pad + 90;
            for (var i = 0; i < _beliefs.Count; i++)
            {
                var b = _beliefs[i];
                GUI.Label(new Rect(pad, y, 1100, 20), $"{b.Role,-12}  value {b.Value:0.00}   η̂ {b.Confidence:0.00}");
                y += 18;
            }
        }
    }
}
