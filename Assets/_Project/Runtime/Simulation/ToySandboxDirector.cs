using System.Collections.Generic;
using GummyDynasty.Cognition;
using GummyDynasty.Core;
using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>M1/M2 playable host: spawn, launch, smash, inspect beliefs, sample frame times.</summary>
    public sealed class ToySandboxDirector : MonoBehaviour
    {
        public BeliefField Field { get; } = new BeliefField();
        public FrameTimeSampler Sampler { get; } = new FrameTimeSampler();
        public GummyBody Selected { get; private set; }
        public int GummyCount { get; private set; }

        ToyArena _arena;
        PhysicalPersonality _default;
        PhysicalPersonality _knight;
        PhysicalPersonality _scout;
        float _benchUntil;
        int _benchTarget;
        readonly List<GummyAgent> _agents = new List<GummyAgent>(64);

        public PhysicalPersonality DefaultPersonality => _default;

        void Awake()
        {
            ServiceRegistry.Current?.Register(this);
            ServiceRegistry.Current?.Register(Field);
            Field.AddNode("world", null, NodeKind.World);
            Field.EnsureRole("world", "pressure", RoleMode.Dissipative, 0.2f, ReduceOp.Mean);
            Field.AddNode("formation-red", "world", NodeKind.World);
            Field.EnsureRole("formation-red", GummyAgent.RoleObjective, RoleMode.Unitary);
            Field.EnsureRole("formation-red", GummyAgent.RolePain, RoleMode.Dissipative, 0.3f, ReduceOp.Mean);
            Field.Observe(new Observation("formation-red", GummyAgent.RoleObjective, 0.8f, 0.6f));

            _default = PhysicalPersonality.CreateRuntime("Gummy", new Color(0.95f, 0.22f, 0.36f), 2.4f, 1.15f, 1f, 6.5f);
            _knight = PhysicalPersonality.CreateRuntime("Knight", new Color(0.35f, 0.2f, 0.7f), 6.2f, 1.55f, 0.7f, 10f);
            _knight.JointSpring = 240f;
            _knight.RecoverySeconds = 2.1f;
            _scout = PhysicalPersonality.CreateRuntime("Scout", new Color(0.2f, 0.85f, 0.45f), 1.1f, 0.75f, 1.45f, 4.2f);
            _scout.JointSpring = 120f;
            _scout.RecoverySeconds = 0.7f;
        }

        void Start()
        {
            _arena = GetComponent<ToyArena>();
            if (_arena == null)
                _arena = gameObject.AddComponent<ToyArena>();
            _arena.Build();
            Spawn(_default, new Vector3(-1.5f, 1.4f, 0f));
            Spawn(_default, new Vector3(0.4f, 1.6f, 0.6f));
            Spawn(_scout, new Vector3(-3f, 1.2f, 1.2f));
            GameEvents.RaiseStatus("toy sandbox — 1/2/3 spawn, click select, space launch, K knock, F fire, B smash");
        }

        void Update()
        {
            Sampler.Sample();
            if (Sampler.Running && Time.unscaledTime >= _benchUntil)
            {
                Sampler.End();
                GameEvents.RaiseStatus($"bench A N={_benchTarget}  p50={Sampler.P50:0.0}  p95={Sampler.P95:0.0}  p99={Sampler.P99:0.0} ms");
            }

            TickCognition();
        }

        void TickCognition()
        {
            for (var i = 0; i < _agents.Count; i++)
            {
                if (_agents[i] == null) continue;
                _agents[i].Sense();
            }
            Field.Tick(Time.deltaTime);
            Field.Propagate(0.08f);
        }

        public void SpawnDefault() => Spawn(_default, SpawnPoint());
        public void SpawnKnight() => Spawn(_knight, SpawnPoint());
        public void SpawnScout() => Spawn(_scout, SpawnPoint());

        public void LaunchSelected()
        {
            if (TryTarget(out var body))
                body.Launch(CameraForward());
        }

        public void KnockSelected()
        {
            if (TryTarget(out var body))
                body.KnockDown(CameraForward());
        }

        public void SmashWall() => _arena?.SmashWall();

        public void ResetArena() => ResetGummies();

        public void SelectFromScreen(Vector2 screenPosition)
        {
            var cam = Camera.main;
            if (cam == null) return;
            var ray = cam.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out var hit, 80f))
                return;
            var body = hit.rigidbody != null ? hit.rigidbody.GetComponentInParent<GummyBody>() : null;
            if (body != null)
                Selected = body;
        }

        public GummyBody Spawn(PhysicalPersonality personality, Vector3 position)
        {
            var body = GummyFactory.Spawn(personality, position, Quaternion.identity, _arena != null ? _arena.GummyRoot : transform);
            var agent = body.GetComponent<GummyAgent>();
            agent.AttachField(Field, "formation-red");
            _agents.Add(agent);
            GummyCount++;
            Selected = body;
            return body;
        }

        public void BeginBench(int target)
        {
            while (GummyCount < target)
                Spawn(_default, SpawnPoint() + Vector3.up * Random.Range(0f, 2f));
            _benchTarget = target;
            _benchUntil = Time.unscaledTime + 8f;
            Sampler.Begin();
            GameEvents.RaiseStatus("benchmark A sampling N=" + target);
        }

        void Fire()
        {
            var cam = Camera.main;
            var origin = cam != null ? cam.transform.position + cam.transform.forward * 1.4f : new Vector3(0f, 2f, -6f);
            var dir = cam != null ? cam.transform.forward : Vector3.forward;
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Projectile";
            ball.transform.position = origin;
            ball.transform.localScale = Vector3.one * 0.35f;
            var rb = ball.AddComponent<Rigidbody>();
            rb.mass = 2.2f;
            rb.AddForce(dir * 22f, ForceMode.VelocityChange);
            var proj = ball.AddComponent<ProjectileBall>();
            proj.FirerId = Selected != null ? Selected.AgentId : null;
        }

        public void FireProjectile() => Fire();

        void ResetGummies()
        {
            for (var i = 0; i < _agents.Count; i++)
            {
                if (_agents[i] != null)
                    Destroy(_agents[i].gameObject);
            }
            _agents.Clear();
            GummyCount = 0;
            Selected = null;
            _arena.ResetProps();
            Spawn(_default, new Vector3(-1.5f, 1.4f, 0f));
            Spawn(_default, new Vector3(0.4f, 1.6f, 0.6f));
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
            return new Vector3(Random.Range(-4f, 1f), 1.6f, Random.Range(-2f, 2f));
        }

        Vector3 CameraForward()
        {
            var cam = Camera.main;
            if (cam == null) return Vector3.forward;
            var f = cam.transform.forward;
            f.y = 0.15f;
            return f.sqrMagnitude > 0.01f ? f.normalized : Vector3.forward;
        }
    }
}
