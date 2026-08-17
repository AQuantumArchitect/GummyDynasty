using System.Collections.Generic;
using GummyDynasty.Cognition;
using GummyDynasty.Core;
using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>M1–M4 playable host: spawn, smash, march, logical army, inspect inherited vs local intent.</summary>
    public sealed class ToySandboxDirector : MonoBehaviour
    {
        public BeliefField Field { get; } = new BeliefField();
        public FrameTimeSampler Sampler { get; } = new FrameTimeSampler();
        public GummyBody Selected { get; private set; }
        public int GummyCount { get; private set; }
        public bool Marching { get; private set; }
        public Vector3 IncomingPoint { get; private set; }

        ToyArena _arena;
        LogicalDirector _logic;
        PhysicalPersonality _default;
        PhysicalPersonality _knight;
        PhysicalPersonality _scout;
        PhysicalPersonality _marcher;
        float _benchUntil;
        int _benchTarget;
        bool _collapseSampling;
        float _collapseUntil;
        int _collapseN;
        Transform _falling;
        float _fallUntil;
        string _lastTactic = IntentResolver.Idle;
        Vector3 _breachPoint;
        bool _issuedRanked;
        bool _orderedHold;
        bool _arrivedHold;
        Vector3 _artilleryAim = new Vector3(-2f, 1.1f, 0f);
        string _artilleryMachine = PhoneCommand.MachineCatapult;
        readonly PhoneSession _phones = new PhoneSession();
        PhoneHost _host;
        GameModeRules _mode = GameModeRules.HoldWest();
        readonly List<GummyAgent> _agents = new List<GummyAgent>(64);
        readonly List<string> _ancestry = new List<string>(8);
        readonly List<int> _marchIdx = new List<int>(16);
        Vector3[] _slots = new Vector3[64];
        bool[] _hasSlot = new bool[64];

        public PhysicalPersonality DefaultPersonality => _default;
        public LogicalDirector Logic => _logic;
        public IReadOnlyList<string> SelectedAncestry => _ancestry;
        public Tossable SelectedTossable { get; private set; }
        public BreakablePart SelectedBlock { get; private set; }
        public bool ShowResearchHud { get; private set; }
        public string FormationTactic { get; private set; } = IntentResolver.Idle;
        public Vector3 BreachPoint => _breachPoint;
        public PhoneSession Phones => _phones;
        public PhoneHost Host => _host;
        public GameModeRules Mode => _mode;
        public bool Victory { get; private set; }
        public ShowcaseKind Showcase => _arena != null ? _arena.Kind : ShowcaseKind.Pit;
        public string JoinUrl => _host != null ? _host.LanUrl : "";
        public bool[,] JoinQr => _host != null ? _host.Qr : null;

        void Awake()
        {
            ServiceRegistry.Current?.Register(this);
            ServiceRegistry.Current?.Register(Field);
            BattleHierarchy.Install(Field);

            var levy = PersonalityCatalog.Levy();
            _default = PersonalityCatalog.FromUnit(levy);
            _knight = PersonalityCatalog.Knight();
            _scout = PersonalityCatalog.Scout();
            _marcher = PersonalityCatalog.Marcher();
            _mode = PersonalityCatalog.Rules();
        }

        void Start()
        {
            _arena = GetComponent<ToyArena>();
            if (_arena == null)
                _arena = gameObject.AddComponent<ToyArena>();
            _logic = GetComponent<LogicalDirector>();
            if (_logic == null)
                _logic = gameObject.AddComponent<LogicalDirector>();
            _arena.Build();
            SpawnStarter();
            StartPhoneHost();
            GameEvents.RaiseStatus("scan the QR or open " + JoinUrl + "  ·  4 march  ·  7/8/9 pit/castle/train");
        }

        void OnDestroy()
        {
            _host?.Dispose();
            _host = null;
        }

        void StartPhoneHost()
        {
            _host = new PhoneHost(_phones);
            if (_host.TryStart())
            {
                ServiceRegistry.Current?.Register(_host);
                ServiceRegistry.Current?.Register(_phones);
            }
            else
                GameEvents.RaiseStatus("phone host failed — " + _host.LastError);
        }

        void Update()
        {
            Sampler.Sample();
            if (Sampler.Running && !_collapseSampling && Time.unscaledTime >= _benchUntil)
            {
                Sampler.End();
                GameEvents.RaiseStatus($"bench A N={_benchTarget}  p50={Sampler.P50:0.0}  p95={Sampler.P95:0.0}  p99={Sampler.P99:0.0} ms");
                BenchSink.Write("A", "active gummies", _benchTarget, Sampler.P50, Sampler.P95, Sampler.P99, "frame ms");
            }

            if (_collapseSampling && Time.unscaledTime >= _collapseUntil)
            {
                _collapseSampling = false;
                Sampler.End();
                GameEvents.RaiseStatus($"bench F collapse n={_collapseN}  p50={Sampler.P50:0.0}  p95={Sampler.P95:0.0} ms");
                BenchSink.Write("F", "wall collapse", _collapseN, Sampler.P50, Sampler.P95, Sampler.P99, "frame ms — seed");
            }

            DrainPhone();
            TickCognition();
            TickLogical();
            TickVictory();
            PublishPhoneState();
        }

        void TickCognition()
        {
            if (_falling != null && Time.time >= _fallUntil)
                _falling = null;

            RefreshIncoming();

            for (var i = 0; i < _agents.Count; i++)
            {
                var agent = _agents[i];
                if (agent == null) continue;
                agent.Sense();
                agent.SenseNeighborhood(CountNearby(agent), IncomingThreat(agent));
            }

            NotifyFlattenedAllies();

            var flag = FlagPosition;
            var com = RankedCom();
            var report = SmashWallQuery.Query(_arena != null ? _arena.SmashWallRoot : null, com);
            _breachPoint = report.BreachPoint;
            BattleHierarchy.ObserveRoad(Field, report.WallValue, report.HoleValue);

            Field.Tick(Time.deltaTime);
            Field.Propagate(0.22f);

            var objective = Field.Get(HierarchyIds.FormationRed, HierarchyRoles.Objective).Value;
            var arrived = Marching && com.x <= flag.x + IntentResolver.ArriveBand
                && com.x >= flag.x - IntentResolver.ArriveBand * 2f;
            if (arrived)
                _arrivedHold = true;
            var holdOrder = (_orderedHold || _arrivedHold)
                ? 1f
                : Field.Get(HierarchyIds.FormationRed, HierarchyRoles.Hold).Value;
            FormationTactic = FormationTactics.Resolve(objective, arrived || _arrivedHold, report.WallValue, report.HoleValue, holdOrder);
            if (Marching && FormationTactic != _lastTactic)
            {
                _lastTactic = FormationTactic;
                GameEvents.RaiseStatus("file " + FormationTactic
                    + (report.Hole ? "  hole at wall" : report.Ahead ? "  wall ahead" : ""));
            }

            AssignMarchSlots(flag, FormationTactic, _breachPoint, report.GapAir);
            for (var i = 0; i < _agents.Count; i++)
            {
                if (_agents[i] == null) continue;
                var slotted = i < _hasSlot.Length && _hasSlot[i];
                _agents[i].Act(flag, IncomingPoint, slotted ? _slots[i] : flag, slotted, FormationTactic);
            }

            RefreshAncestry();
        }

        public void SpawnDefault() => Spawn(_default, SpawnPoint());
        public void SpawnKnight() => Spawn(_knight, SpawnPoint());
        public void SpawnScout() => Spawn(_scout, SpawnPoint());

        public void LaunchSelected()
        {
            if (SelectedBlock != null)
            {
                SelectedBlock.Yeet(CameraForward());
                GameEvents.RaiseStatus("YEET crate");
                return;
            }
            if (SelectedTossable != null)
            {
                SelectedTossable.Yeet(CameraForward());
                GameEvents.RaiseStatus("YEET " + SelectedTossable.Label);
                return;
            }
            if (TryTarget(out var body))
            {
                body.Launch(CameraForward());
                GameEvents.RaiseStatus("YEET " + (body.Personality != null ? body.Personality.DisplayName : "gummy"));
            }
        }

        public void YeetSelected() => LaunchSelected();

        public void ToggleResearchHud()
        {
            ShowResearchHud = !ShowResearchHud;
            GameEvents.RaiseStatus(ShowResearchHud
                ? "research hatch on — 5 army / 6 embody / F9 bench (ghosts, no collision)"
                : "research hatch off");
        }

        public void KnockSelected()
        {
            if (!TryTarget(out var body))
                return;
            body.KnockDown(CameraForward());
            var agent = body.GetComponent<GummyAgent>();
            agent?.ObserveWorldImpact(0.75f, false, "host");
        }

        public void SmashWall()
        {
            var n = _arena != null ? _arena.SmashWall() : 0;
            if (n > 0 && !Sampler.Running)
            {
                _collapseN = n;
                _collapseUntil = Time.unscaledTime + 5f;
                _collapseSampling = true;
                Sampler.Begin();
            }
            GameEvents.RaiseStatus(n == 0 ? "no crates left — press R to rebuild" : "SMASH ×" + n + " — wall blasted");
        }

        public void LobCatapult()
        {
            var cat = _arena != null ? _arena.Catapult : FindFirstObjectByType<PitCatapult>();
            if (cat == null)
            {
                GameEvents.RaiseStatus("no catapult — press R to rebuild the pit");
                return;
            }
            cat.LoadStone();
            cat.SetTendril(MachineControls.Draw, 1f);
            cat.AimAtPoint(AimPoint());
            if (cat.TryFire())
                GameEvents.RaiseStatus("LOB — stone from the east catapult");
        }

        public void DropCrate()
        {
            var target = AimPoint();
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "SkyCrate";
            go.transform.position = target + Vector3.up * 7.2f;
            go.transform.localScale = Vector3.one * 0.9f;
            if (_arena != null && _arena.PropRoot != null)
                go.transform.SetParent(_arena.PropRoot, true);
            go.GetComponent<Renderer>().sharedMaterial = GummyLook.Material(new Color(0.62f, 0.34f, 0.16f));
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 6.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            go.AddComponent<BreakablePart>();
            ClearSelection();
            SelectedBlock = go.GetComponent<BreakablePart>();
            SelectedBlock.Mark(true);
            _falling = go.transform;
            _fallUntil = Time.time + 2.8f;
            GameEvents.RaiseStatus("DROP — crate from the sky");
        }

        public void ResetArena() => ResetGummies();

        public void BeginWestMarch()
        {
            EnsureRankedSquad();
            OrderWest();
            GameEvents.RaiseStatus("MARCH — ranks west, through the wall, plant at WEST");
        }

        public void OrderWest()
        {
            EnsureRankedSquad();
            _orderedHold = false;
            _arrivedHold = false;
            BattleHierarchy.IssueWestOrder(Field);
            Marching = true;
            _logic?.SetMarching(true);
        }

        public void OrderHold()
        {
            EnsureRankedSquad();
            _orderedHold = true;
            BattleHierarchy.IssueHoldOrder(Field);
            Marching = true;
            GameEvents.RaiseStatus("HOLD — plant in place");
        }

        public void LoadShowcase(ShowcaseKind kind)
        {
            ClearBodies();
            _arena.Rebuild(kind);
            SpawnStarter();
            StartPhoneHostIfDown();
            GameEvents.RaiseStatus(kind + " — " + JoinUrl);
        }

        public void FireMachine(string machine, Vector3 aim)
        {
            _artilleryAim = aim;
            _artilleryMachine = machine == PhoneCommand.MachineCannon
                ? PhoneCommand.MachineCannon
                : PhoneCommand.MachineCatapult;
            if (_artilleryMachine == PhoneCommand.MachineCannon)
            {
                var gun = _arena != null ? _arena.Cannon : FindFirstObjectByType<PitCannon>();
                if (gun == null)
                {
                    GameEvents.RaiseStatus("no cannon");
                    return;
                }
                gun.LoadStone();
                gun.SetTendril(MachineControls.Draw, 1f);
                gun.AimAtPoint(aim);
                if (gun.TryFire())
                    GameEvents.RaiseStatus("CANNON — stone");
                return;
            }

            var cat = _arena != null ? _arena.Catapult : FindFirstObjectByType<PitCatapult>();
            if (cat == null)
            {
                GameEvents.RaiseStatus("no catapult");
                return;
            }
            cat.LoadStone();
            cat.SetTendril(MachineControls.Draw, 1f);
            cat.AimAtPoint(aim);
            if (cat.TryFire())
                GameEvents.RaiseStatus("LOB — stone from the east catapult");
        }

        public void SelectFromScreen(Vector2 screenPosition)
        {
            var cam = Camera.main;
            if (cam == null) return;
            var ray = cam.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out var hit, 80f))
                return;
            var body = hit.rigidbody != null ? hit.rigidbody.GetComponentInParent<GummyBody>() : null;
            if (body != null)
            {
                ClearSelection();
                Selected = body;
                return;
            }
            var block = hit.collider != null ? hit.collider.GetComponentInParent<BreakablePart>() : null;
            if (block != null)
            {
                ClearSelection();
                SelectedBlock = block;
                block.Mark(true);
                return;
            }
            var toss = hit.collider != null ? hit.collider.GetComponentInParent<Tossable>() : null;
            if (toss != null)
            {
                ClearSelection();
                SelectedTossable = toss;
            }
        }

        void ClearSelection()
        {
            if (SelectedBlock != null)
                SelectedBlock.Mark(false);
            Selected = null;
            SelectedTossable = null;
            SelectedBlock = null;
        }

        public GummyBody Spawn(PhysicalPersonality personality, Vector3 position, int logicalId = -1)
        {
            var body = GummyFactory.Spawn(personality, position, Quaternion.identity, _arena != null ? _arena.GummyRoot : transform);
            var agent = body.GetComponent<GummyAgent>();
            agent.AttachField(Field, HierarchyIds.FormationRed);
            agent.Slot = GummyCount;
            _agents.Add(agent);
            GummyCount++;
            ClearSelection();
            Selected = body;
            if (_logic != null)
            {
                if (logicalId > 0)
                    _logic.BindExisting(body, logicalId);
                else
                    _logic.RegisterBody(body);
            }
            return body;
        }

        public int SeedLogicalArmy(int n = 1000)
        {
            if (_logic == null)
                return 0;
            var added = _logic.SeedArmy(n);
            if (Marching)
                _logic.SetMarching(true);
            _logic.ShowGhosts = true;
            ShowResearchHud = true;
            GameEvents.RaiseStatus("RESEARCH army +" + added + "  (ghosts, no collision — not the toy)");
            return added;
        }

        public int EmbodyLogicals(int n = 8)
        {
            if (_logic == null)
                return 0;
            var ids = new List<int>(n);
            _logic.FirstDisembodied(n, ids);
            var made = 0;
            for (var i = 0; i < ids.Count; i++)
            {
                if (!_logic.Population.TryGet(ids[i], out var soldier))
                    continue;
                Spawn(_default, soldier.Position + Vector3.up * 0.7f, ids[i]);
                made++;
            }
            GameEvents.RaiseStatus("embodied " + made + " logicals — they hop, the rest stay ghosts");
            return made;
        }

        public void BeginLogicalBench(int n = 1000) => _logic?.BeginBenchB(n);

        public void BeginBench(int target)
        {
            while (GummyCount < target)
                Spawn(_default, SpawnPoint() + Vector3.up * Random.Range(0f, 2f));
            _benchTarget = target;
            _benchUntil = Time.unscaledTime + 8f;
            Sampler.Begin();
            GameEvents.RaiseStatus("benchmark A sampling N=" + target);
        }

        public void FireProjectile()
        {
            var cam = Camera.main;
            var origin = cam != null
                ? cam.transform.position + cam.transform.forward * 2.1f + cam.transform.right * 0.35f
                : new Vector3(0f, 2.4f, -7f);
            var aim = AimPoint();
            CandyShot.Spawn(origin, CandyShot.BallisticDirection(origin, aim), Selected != null ? Selected.AgentId : null, PersonalityCatalog.Candy());
            GameEvents.RaiseStatus("FIRE — catapult stone (plants on the ground)");
        }

        public GummyAgent SelectedAgent => Selected != null ? Selected.GetComponent<GummyAgent>() : null;

        public ResolvedIntent SelectedIntent
        {
            get
            {
                if (Selected == null)
                    return default;
                var agent = Selected.GetComponent<GummyAgent>();
                return agent != null ? agent.Intent : default;
            }
        }

        public Vector3 FlagPosition => _arena != null ? _arena.FlagPosition : new Vector3(-10f, 0.2f, 0f);

        void TickLogical()
        {
            if (_logic == null)
                return;

            for (var i = 0; i < _agents.Count; i++)
            {
                var agent = _agents[i];
                if (agent == null || agent.Body == null)
                    continue;
                _logic.WriteBack(agent.Body, ToLogical(agent.Intent.EffectiveName), agent.Read(HierarchyRoles.Pain).Value);
            }

            _logic.Tick(Time.deltaTime, FlagPosition, new Vector3(80f, 4f, 0f));
            _logic.PollBench();
        }

        static LogicalIntent ToLogical(string name)
        {
            if (name == IntentResolver.MarchWest
                || name == FormationTactics.Spread
                || name == FormationTactics.ThroughBreach)
                return LogicalIntent.MarchWest;
            if (name == IntentResolver.Dodge) return LogicalIntent.Dodge;
            if (name == IntentResolver.Down) return LogicalIntent.Down;
            return LogicalIntent.Idle;
        }

        void ResetGummies()
        {
            for (var i = 0; i < _agents.Count; i++)
            {
                if (_agents[i] != null)
                    Destroy(_agents[i].gameObject);
            }
            _agents.Clear();
            GummyCount = 0;
            ClearSelection();
            _falling = null;
            Marching = false;
            FormationTactic = IntentResolver.Idle;
            _lastTactic = IntentResolver.Idle;
            _logic?.ResetPopulation();
            _logic?.SetMarching(false);

            foreach (var shot in FindObjectsByType<ProjectileBall>(FindObjectsSortMode.None))
                Destroy(shot.gameObject);

            _issuedRanked = false;
            _orderedHold = false;
            _arrivedHold = false;
            Victory = false;
            _phones.Reset();
            _arena.ResetProps();
            SpawnStarter();
            GameEvents.RaiseStatus("reset — " + JoinUrl);
        }

        void ClearBodies()
        {
            for (var i = 0; i < _agents.Count; i++)
            {
                if (_agents[i] != null)
                    Destroy(_agents[i].gameObject);
            }
            _agents.Clear();
            GummyCount = 0;
            ClearSelection();
            _falling = null;
            Marching = false;
            FormationTactic = IntentResolver.Idle;
            _lastTactic = IntentResolver.Idle;
            _issuedRanked = false;
            _orderedHold = false;
            _arrivedHold = false;
            Victory = false;
            _logic?.ResetPopulation();
            _logic?.SetMarching(false);
            foreach (var shot in FindObjectsByType<ProjectileBall>(FindObjectsSortMode.None))
                Destroy(shot.gameObject);
        }

        void SpawnStarter()
        {
            var a = Spawn(_default, _arena.DefaultSpawn(0));
            var b = Spawn(_default, _arena.DefaultSpawn(1));
            var c = Spawn(_scout, _arena.DefaultSpawn(2));
            if (_arena != null && _arena.Kind == ShowcaseKind.Train)
            {
                a.ForceRecoverSoon();
                b.ForceRecoverSoon();
                c.ForceRecoverSoon();
            }
        }

        void EnsureRankedSquad()
        {
            if (_issuedRanked)
                return;
            _issuedRanked = true;
            for (var i = 0; i < 5; i++)
            {
                var body = Spawn(_marcher, _arena != null ? _arena.RankedSpawn(i) : new Vector3(3.2f, 2.3f, (i - 2) * 1.05f));
                var agent = body.GetComponent<GummyAgent>();
                agent.Slot = i;
                agent.HoldsRank = true;
                body.ForceRecoverSoon();
            }
        }

        void DrainPhone()
        {
            if (_host == null)
                return;
            while (_host.TryDequeue(out var cmd))
                ApplyPhone(cmd);
        }

        public void ApplyPhone(PhoneCommand cmd)
        {
            if (cmd == null || cmd.Op != PhoneCommand.OpCmd)
                return;
            if (cmd.Role == PhoneCommand.RoleCommander)
            {
                if (cmd.Action == PhoneCommand.ActHold)
                    OrderHold();
                else
                    BeginWestMarch();
                return;
            }

            if (cmd.Action == PhoneCommand.ActAim)
            {
                _artilleryAim = new Vector3(cmd.X, 1.1f, cmd.Z);
                _artilleryMachine = cmd.Machine;
                var gun = NamedMachine(cmd.Machine);
                if (gun is PitCatapult cat)
                    cat.AimAtPoint(_artilleryAim);
                else if (gun is PitCannon can)
                    can.AimAtPoint(_artilleryAim);
                return;
            }

            if (cmd.Action == PhoneCommand.ActLoad)
            {
                var gun = NamedMachine(cmd.Machine);
                if (gun is PitCatapult cat)
                    cat.LoadStone();
                else if (gun is PitCannon can)
                    can.LoadStone();
                return;
            }

            if (cmd.Action == PhoneCommand.ActFire)
            {
                var aim = cmd.HasPoint ? new Vector3(cmd.X, 1.1f, cmd.Z) : _artilleryAim;
                FireMachine(cmd.Machine, aim);
            }
        }

        Machine NamedMachine(string name)
        {
            if (name == PhoneCommand.MachineCannon)
                return _arena != null ? _arena.Cannon : FindFirstObjectByType<PitCannon>();
            return _arena != null ? (Machine)_arena.Catapult : FindFirstObjectByType<PitCatapult>();
        }

        void TickVictory()
        {
            if (Victory || _mode == null)
                return;
            if (!_mode.CheckVictory(RankedCom().x, FlagPosition.x, Marching))
                return;
            Victory = true;
            GameEvents.RaiseStatus(_mode.DisplayName + " — HELD");
        }

        void PublishPhoneState()
        {
            if (_host == null)
                return;
            var flag = FlagPosition;
            var com = RankedCom();
            var wall = _arena != null ? _arena.SmashWallOrigin : new Vector3(-2f, 0.35f, 0f);
            var json =
                "{\"mode\":\"" + PhonePages.Esc(_mode != null ? _mode.DisplayName : "WEST") + "\""
                + ",\"showcase\":\"" + Showcase.ToString().ToLowerInvariant() + "\""
                + ",\"tactic\":\"" + PhonePages.Esc(FormationTactic) + "\""
                + ",\"victory\":" + (Victory ? "true" : "false")
                + ",\"url\":\"" + PhonePages.Esc(JoinUrl) + "\""
                + ",\"flag\":{\"x\":" + F(flag.x) + ",\"z\":" + F(flag.z) + "}"
                + ",\"com\":{\"x\":" + F(com.x) + ",\"z\":" + F(com.z) + "}"
                + ",\"wall\":{\"x\":" + F(wall.x) + ",\"z\":" + F(wall.z) + "}"
                + ",\"breach\":{\"x\":" + F(_breachPoint.x) + ",\"z\":" + F(_breachPoint.z) + "}"
                + ",\"commander\":\"" + PhonePages.Esc(_phones.CommanderId) + "\""
                + ",\"artillery\":\"" + PhonePages.Esc(_phones.ArtilleryId) + "\"}";
            _host.PublishState(json);
        }

        static string F(float v) => v.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

        void StartPhoneHostIfDown()
        {
            if (_host != null && _host.Listening)
                return;
            StartPhoneHost();
        }

        void NotifyFlattenedAllies()
        {
            for (var i = 0; i < _agents.Count; i++)
            {
                var down = _agents[i];
                if (down == null || !down.FlattenedPulse || down.Body == null)
                    continue;
                down.FlattenedPulse = false;
                var p = down.Body.Position;
                for (var j = 0; j < _agents.Count; j++)
                {
                    var other = _agents[j];
                    if (other == null || other == down || other.Body == null)
                        continue;
                    if ((other.Body.Position - p).sqrMagnitude < 16f)
                        other.NoticeAllyDown(down.AgentId);
                }
            }
        }

        bool TryTarget(out GummyBody body)
        {
            body = Selected;
            if (body != null) return true;
            body = FindFirstObjectByType<GummyBody>();
            return body != null;
        }

        Vector3 SpawnPoint()
        {
            if (_arena != null && _arena.Kind == ShowcaseKind.Train)
                return _arena.DefaultSpawn(Random.Range(0, 3)) + new Vector3(Random.Range(-0.3f, 0.3f), 0.2f, Random.Range(-0.2f, 0.2f));
            var east = _arena != null && _arena.Kind == ShowcaseKind.Castle ? 3.4f : 1.2f;
            return new Vector3(Random.Range(east, east + 3f), 2.4f, Random.Range(-2f, 2f));
        }

        Vector3 CameraForward()
        {
            var cam = Camera.main;
            if (cam == null) return Vector3.forward;
            var f = cam.transform.forward;
            f.y = 0.15f;
            return f.sqrMagnitude > 0.01f ? f.normalized : Vector3.forward;
        }

        Vector3 AimPoint()
        {
            if (Selected != null)
                return Selected.Position + Vector3.up * 0.3f;
            if (SelectedBlock != null)
                return SelectedBlock.transform.position;
            if (SelectedTossable != null)
                return SelectedTossable.transform.position;
            if (_arena != null)
                return _arena.SmashWallOrigin + Vector3.up * 0.75f;
            return new Vector3(-2f, 1.1f, 0f);
        }

        Vector3 RankedCom()
        {
            var sum = Vector3.zero;
            var n = 0;
            for (var i = 0; i < _agents.Count; i++)
            {
                var agent = _agents[i];
                if (agent == null || agent.Body == null)
                    continue;
                if (Marching && !agent.HoldsRank)
                    continue;
                sum += agent.Body.Position;
                n++;
            }
            if (n == 0)
                return new Vector3(3f, 0.8f, 0f);
            return sum / n;
        }

        void AssignMarchSlots(Vector3 flag, string tactic, Vector3 breach, float gapAir)
        {
            if (_agents.Count > _hasSlot.Length)
            {
                var cap = _agents.Count + 16;
                System.Array.Resize(ref _slots, cap);
                System.Array.Resize(ref _hasSlot, cap);
            }
            System.Array.Clear(_hasSlot, 0, _hasSlot.Length);
            _marchIdx.Clear();
            if (!Marching)
                return;

            for (var i = 0; i < _agents.Count; i++)
            {
                var agent = _agents[i];
                if (agent == null || agent.Body == null)
                    continue;
                var intent = agent.Intent.EffectiveName;
                if (intent == IntentResolver.Dodge || intent == IntentResolver.Down)
                    continue;
                var objective = agent.Read(HierarchyRoles.Objective).Value;
                if (objective < IntentResolver.MarchFloor
                    && !FormationTactics.Travels(intent)
                    && intent != FormationTactics.Hold)
                    continue;
                if (!agent.HoldsRank)
                    continue;
                _marchIdx.Add(i);
            }

            if (_marchIdx.Count == 0)
                return;

            _marchIdx.Sort((a, b) => _agents[a].Slot.CompareTo(_agents[b].Slot));

            var com = Vector3.zero;
            for (var k = 0; k < _marchIdx.Count; k++)
                com += _agents[_marchIdx[k]].Body.Position;
            com /= _marchIdx.Count;

            var n = _marchIdx.Count;
            for (var k = 0; k < n; k++)
            {
                var i = _marchIdx[k];
                if (i >= _slots.Length)
                    continue;
                _slots[i] = MarchFormation.SlotWorld(k, n, com, flag, tactic, breach, gapAir);
                _hasSlot[i] = true;
            }
        }

        int CountNearby(GummyAgent self)
        {
            var n = 0;
            var p = self.Body != null ? self.Body.Position : self.transform.position;
            for (var i = 0; i < _agents.Count; i++)
            {
                var other = _agents[i];
                if (other == null || other == self || other.Body == null) continue;
                if ((other.Body.Position - p).sqrMagnitude < 2.6f)
                    n++;
            }
            return n;
        }

        void RefreshIncoming()
        {
            Transform best = null;
            var bestDist = 8f;

            if (_falling != null)
            {
                best = _falling;
                bestDist = 99f;
            }

            var shots = FindObjectsByType<ProjectileBall>(FindObjectsSortMode.None);
            for (var s = 0; s < shots.Length; s++)
            {
                var shot = shots[s];
                if (shot == null || !shot.InFlight)
                    continue;
                var p = shot.transform.position;
                for (var i = 0; i < _agents.Count; i++)
                {
                    var agent = _agents[i];
                    if (agent == null || agent.Body == null)
                        continue;
                    var d = Vector3.Distance(agent.Body.Position, p);
                    if (d >= bestDist)
                        continue;
                    bestDist = d;
                    best = shot.transform;
                }
            }

            IncomingPoint = best != null
                ? best.position
                : FlagPosition + Vector3.right * 20f;
        }

        float IncomingThreat(GummyAgent agent)
        {
            if (agent.Body == null)
                return 0f;
            var incoming = IncomingPoint;
            if (incoming.x > FlagPosition.x + 18f)
                return 0f;
            var d = Vector3.Distance(agent.Body.Position, incoming);
            if (d > 7f)
                return 0f;
            return Mathf.Clamp01((7f - d) / 7f);
        }

        void RefreshAncestry()
        {
            if (Selected == null)
            {
                _ancestry.Clear();
                return;
            }
            Field.CopyAncestry(Selected.AgentId, _ancestry);
        }

        void OnDrawGizmos()
        {
            if (!Marching)
                return;
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.8f);
            Gizmos.DrawWireSphere(_breachPoint, 0.32f);
            Gizmos.color = new Color(0.25f, 0.85f, 1f, 0.45f);
            for (var i = 0; i < _agents.Count && i < _hasSlot.Length; i++)
            {
                if (_hasSlot[i])
                    Gizmos.DrawWireCube(_slots[i], Vector3.one * 0.28f);
            }
        }
    }
}
