using GummyDynasty.Cognition;
using GummyDynasty.Core;
using GummyDynasty.Simulation;
using UnityEngine;

namespace GummyDynasty.UI
{
    /// <summary>Friend strip + selected-mind inspector. Research hatch stays hidden.</summary>
    public sealed class HudController : MonoBehaviour
    {
        string _status = "click, yeet, fire, smash, march";
        ToySandboxDirector _toy;
        string _inspectId;
        float _mass = 2.8f;
        float _spring = 180f;
        float _recover = 1.4f;

        void OnEnable() => GameEvents.StatusChanged += OnStatus;
        void OnDisable() => GameEvents.StatusChanged -= OnStatus;

        void Start() => Bind();

        void Bind()
        {
            ServiceRegistry.Current?.TryGet(out _toy);
            if (_toy == null)
                _toy = FindFirstObjectByType<ToySandboxDirector>();
        }

        void OnStatus(string message) => _status = message;

        void OnGUI()
        {
            if (_toy == null)
                Bind();

            const int pad = 12;
            var research = _toy != null && _toy.ShowResearchHud;
            var boxW = research ? 760 : 620;
            var boxH = research ? 210 : 188;
            GUI.color = new Color(0f, 0f, 0f, 0.5f);
            GUI.Box(new Rect(8, 8, boxW, boxH), GUIContent.none);
            GUI.color = Color.white;

            var title = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
            var line = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            var n = _toy != null ? _toy.GummyCount : 0;
            var scene = _toy != null ? _toy.Showcase.ToString().ToUpperInvariant() : "PIT";
            GUI.Label(new Rect(pad, pad, boxW - 16, 22),
                "GUMMY " + scene + "   " + n + "   " + BuildStamp.HudMark, title);
            GUI.Label(new Rect(pad, pad + 22, boxW - 170, 20), _status, line);
            DrawQr(boxW - 148, pad);

            if (_toy == null)
                return;

            if (GUI.Button(new Rect(pad, pad + 46, 68, 26), "YEET"))
                _toy.YeetSelected();
            if (GUI.Button(new Rect(pad + 74, pad + 46, 68, 26), "FIRE"))
                _toy.FireProjectile();
            if (GUI.Button(new Rect(pad + 148, pad + 46, 68, 26), "LOB"))
                _toy.LobCatapult();
            if (GUI.Button(new Rect(pad + 222, pad + 46, 68, 26), "DROP"))
                _toy.DropCrate();
            if (GUI.Button(new Rect(pad + 296, pad + 46, 68, 26), "SMASH"))
                _toy.SmashWall();
            if (GUI.Button(new Rect(pad + 370, pad + 46, 76, 26), "MARCH"))
                _toy.BeginWestMarch();

            if (GUI.Button(new Rect(pad, pad + 76, 68, 24), "PIT"))
                _toy.LoadShowcase(ShowcaseKind.Pit);
            if (GUI.Button(new Rect(pad + 74, pad + 76, 80, 24), "CASTLE"))
                _toy.LoadShowcase(ShowcaseKind.Castle);
            if (GUI.Button(new Rect(pad + 160, pad + 76, 76, 24), "TRAIN"))
                _toy.LoadShowcase(ShowcaseKind.Train);

            var join = string.IsNullOrEmpty(_toy.JoinUrl) ? "phone host down" : _toy.JoinUrl;
            var mode = _toy.Mode != null ? _toy.Mode.DisplayName : "WEST";
            GUI.Label(new Rect(pad, pad + 104, boxW - 170, 18),
                join + (_toy.Victory ? "   HELD" : "   " + mode), line);

            var holding = HoldingLabel();
            if (!string.IsNullOrEmpty(holding))
                GUI.Label(new Rect(pad, pad + 124, boxW - 110, 18), holding, line);

            DrawInspector();

            if (!research)
                return;

            if (GUI.Button(new Rect(pad + 454, pad + 46, 70, 26), "HIDE"))
                _toy.ToggleResearchHud();
            if (GUI.Button(new Rect(pad + 530, pad + 46, 70, 26), "ARMY"))
                _toy.SeedLogicalArmy(1000);
            if (GUI.Button(new Rect(pad + 606, pad + 46, 80, 26), "EMBODY"))
                _toy.EmbodyLogicals(8);
            var pop = _toy.Logic != null ? _toy.Logic.Population : null;
            if (pop != null)
                GUI.Label(new Rect(pad, pad + 146, boxW - 16, 18),
                    "logical " + pop.Count + "   ghosts " + pop.DisembodiedCount, line);
        }

        void DrawQr(float x, float y)
        {
            var qr = _toy != null ? _toy.JoinQr : null;
            if (qr == null)
                return;
            var n = qr.GetLength(0);
            const float s = 4.6f;
            const float quiet = 8f;
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(x - quiet, y - quiet, n * s + quiet * 2f, n * s + quiet * 2f), Texture2D.whiteTexture);
            for (var j = 0; j < n; j++)
            for (var i = 0; i < n; i++)
            {
                if (!qr[i, j])
                    continue;
                GUI.color = Color.black;
                GUI.DrawTexture(new Rect(x + i * s, y + j * s, s, s), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;
        }

        void DrawInspector()
        {
            var body = _toy.Selected;
            if (body == null || body.Personality == null)
            {
                _inspectId = null;
                return;
            }

            var agent = _toy.SelectedAgent;
            if (body.AgentId != _inspectId)
            {
                _inspectId = body.AgentId;
                _mass = body.Personality.Mass;
                _spring = body.Personality.JointSpring;
                _recover = body.Personality.RecoverySeconds;
            }

            const float w = 280f;
            const float h = 232f;
            var x = Screen.width - w - 8f;
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.Box(new Rect(x, 8, w, h), GUIContent.none);
            GUI.color = Color.white;

            var title = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
            var line = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            var tiny = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            var left = x + 10f;
            GUI.Label(new Rect(left, 14, w - 20, 18), body.Personality.DisplayName, title);
            GUI.Label(new Rect(left, 34, w - 20, 16), body.MotorState.ToString(), line);

            if (agent != null)
            {
                var intent = agent.Intent;
                GUI.Label(new Rect(left, 52, w - 20, 16),
                    "file " + LabelOrDash(_toy.FormationTactic), line);
                GUI.Label(new Rect(left, 68, w - 20, 16),
                    "in " + LabelOrDash(intent.InheritedName) +
                    "  loc " + LabelOrDash(intent.LocalName) +
                    "  eff " + LabelOrDash(intent.EffectiveName), tiny);

                var mad = string.IsNullOrEmpty(agent.Memory.LastHitter)
                    ? "nobody"
                    : agent.Memory.LastHitter;
                var ally = string.IsNullOrEmpty(agent.Memory.LastDownedAlly)
                    ? "—"
                    : agent.Memory.LastDownedAlly;
                GUI.Label(new Rect(left, 86, w - 20, 16), "mad at " + mad + "   ally " + ally, tiny);

                var mems = agent.PeekMemories(4);
                var memLine = mems.Count == 0 ? "memory —" : "memory";
                for (var i = 0; i < mems.Count; i++)
                    memLine += (i == 0 ? "  " : ", ") + mems[i];
                GUI.Label(new Rect(left, 102, w - 20, 16), memLine, tiny);

                var pain = agent.ReadLocal(HierarchyRoles.Pain);
                var threat = agent.ReadLocal(HierarchyRoles.Threat);
                GUI.Label(new Rect(left, 118, w - 20, 16),
                    "pain " + pain.Value.ToString("0.00") + "   threat " + threat.Value.ToString("0.00"), tiny);
            }

            GUI.Label(new Rect(left, 138, 70, 16), "mass " + _mass.ToString("0.0"), tiny);
            var mass = GUI.HorizontalSlider(new Rect(left + 78, 140, w - 100, 16), _mass, 0.5f, 12f);
            GUI.Label(new Rect(left, 158, 70, 16), "spring " + _spring.ToString("0"), tiny);
            var spring = GUI.HorizontalSlider(new Rect(left + 78, 160, w - 100, 16), _spring, 40f, 400f);
            GUI.Label(new Rect(left, 178, 70, 16), "recover " + _recover.ToString("0.00"), tiny);
            var recover = GUI.HorizontalSlider(new Rect(left + 78, 180, w - 100, 16), _recover, 0.3f, 3.5f);

            if (Mathf.Abs(mass - _mass) > 0.01f || Mathf.Abs(spring - _spring) > 0.5f || Mathf.Abs(recover - _recover) > 0.01f)
            {
                _mass = mass;
                _spring = spring;
                _recover = recover;
                body.ApplyPlayTune(_mass, _spring, _recover);
            }
        }

        static string LabelOrDash(string s) => string.IsNullOrEmpty(s) ? "—" : s;

        string HoldingLabel()
        {
            if (_toy.SelectedBlock != null)
                return "crate  (FIRE / LOB / DROP aim here)";
            if (_toy.SelectedTossable != null)
                return _toy.SelectedTossable.Label;
            if (_toy.Selected != null && _toy.Selected.Personality != null)
                return _toy.Selected.Personality.DisplayName;
            return null;
        }
    }
}
