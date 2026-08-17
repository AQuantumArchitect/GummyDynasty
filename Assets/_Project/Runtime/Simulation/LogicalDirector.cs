using System.Collections.Generic;
using System.Diagnostics;
using GummyDynasty.Core;
using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>M4 host: logical army, write-back from bodies, Benchmark B.</summary>
    public sealed class LogicalDirector : MonoBehaviour
    {
        public enum BenchPhase : byte
        {
            Idle = 0,
            Logical = 1,
            Naive = 2
        }

        public LogicalPopulation Population { get; } = new LogicalPopulation();
        public FrameTimeSampler TickSampler { get; } = new FrameTimeSampler();
        public FrameTimeSampler NaiveSampler { get; } = new FrameTimeSampler();
        public BenchPhase Phase { get; private set; }
        public float LastTickMs { get; private set; }
        public int LastSeeded { get; private set; }
        public bool ShowGhosts;

        readonly Dictionary<int, int> _bodyToLogical = new Dictionary<int, int>(64);
        readonly List<GameObject> _naive = new List<GameObject>(1024);
        readonly Stopwatch _watch = new Stopwatch();
        float _phaseUntil;
        int _benchN;

        void Awake()
        {
            ServiceRegistry.Current?.Register(this);
        }

        void OnDestroy()
        {
            TearDownNaive();
        }

        public int RegisterBody(GummyBody body)
        {
            if (body == null)
                return -1;
            var key = body.GetInstanceID();
            if (_bodyToLogical.TryGetValue(key, out var existing))
                return existing;
            var id = Population.Spawn(body.Position);
            Population.SetEmbodied(id, true);
            _bodyToLogical[key] = id;
            return id;
        }

        public void BindExisting(GummyBody body, int logicalId)
        {
            if (body == null || logicalId <= 0)
                return;
            Population.SetEmbodied(logicalId, true);
            _bodyToLogical[body.GetInstanceID()] = logicalId;
        }

        public void UnregisterBody(GummyBody body)
        {
            if (body == null)
                return;
            var key = body.GetInstanceID();
            if (!_bodyToLogical.TryGetValue(key, out var id))
                return;
            Population.SetEmbodied(id, false);
            _bodyToLogical.Remove(key);
        }

        public void ResetPopulation()
        {
            TearDownNaive();
            Phase = BenchPhase.Idle;
            Population.Clear();
            _bodyToLogical.Clear();
            ShowGhosts = false;
        }

        public void WriteBack(GummyBody body, LogicalIntent intent, float pain)
        {
            if (body == null)
                return;
            if (!_bodyToLogical.TryGetValue(body.GetInstanceID(), out var id))
                return;
            Population.WriteBack(id, body.Position, body.Velocity, pain, intent);
        }

        public int SeedArmy(int n)
        {
            var added = Population.SeedBlock(n, new Vector3(7.2f, 0.9f, 0f));
            LastSeeded = added;
            return added;
        }

        public void SetMarching(bool marching) => Population.Marching = marching;

        public void Tick(float dt, Vector3 westFlag, Vector3 incoming)
        {
            _watch.Restart();
            Population.Tick(dt, westFlag, incoming);
            _watch.Stop();
            LastTickMs = (float)_watch.Elapsed.TotalMilliseconds;
            if (Phase == BenchPhase.Logical)
                TickSampler.Add(LastTickMs);

            if (Phase == BenchPhase.Naive)
                StepNaive(dt, westFlag);
        }

        public void BeginBenchB(int n)
        {
            TearDownNaive();
            if (Population.DisembodiedCount < n)
                SeedArmy(n - Population.DisembodiedCount);
            Population.Marching = true;
            ShowGhosts = true;
            TickSampler.Begin();
            NaiveSampler.Begin();
            Phase = BenchPhase.Logical;
            _benchN = n;
            _phaseUntil = Time.unscaledTime + 5f;
            GameEvents.RaiseStatus("bench B — ticking " + Population.DisembodiedCount + " logicals (no GameObjects)");
        }

        public bool PollBench()
        {
            if (Phase == BenchPhase.Idle)
                return false;
            if (Time.unscaledTime < _phaseUntil)
                return false;

            if (Phase == BenchPhase.Logical)
            {
                TickSampler.End();
                BuildNaive(_benchN);
                Phase = BenchPhase.Naive;
                _phaseUntil = Time.unscaledTime + 5f;
                GameEvents.RaiseStatus("bench B — naive " + _naive.Count + " GameObjects (same kinematic march)");
                return false;
            }

            NaiveSampler.End();
            TearDownNaive();
            Phase = BenchPhase.Idle;
            GameEvents.RaiseStatus(
                $"bench B N={_benchN}  logical p50={TickSampler.P50:0.000} p95={TickSampler.P95:0.000} ms   " +
                $"GO p50={NaiveSampler.P50:0.000} p95={NaiveSampler.P95:0.000} ms");
            BenchSink.Write("B", "logical tick", _benchN, TickSampler.P50, TickSampler.P95, TickSampler.P99, "ms per Tick()");
            BenchSink.Write("B", "naive GameObjects", _benchN, NaiveSampler.P50, NaiveSampler.P95, NaiveSampler.P99, "ms per transform march");
            return true;
        }

        public int FirstDisembodied(int max, List<int> into)
        {
            into.Clear();
            for (var i = 0; i < Population.Count && into.Count < max; i++)
            {
                if (Population[i].Embodied)
                    continue;
                into.Add(Population[i].Id);
            }
            return into.Count;
        }

        void BuildNaive(int n)
        {
            TearDownNaive();
            var origin = new Vector3(7.2f, 0.9f, 0f);
            for (var i = 0; i < n; i++)
            {
                var col = i % 20;
                var row = i / 20;
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "NaiveLogical";
                go.transform.position = origin + new Vector3(col * 0.55f, 0f, (row - 10) * 0.55f);
                go.transform.localScale = Vector3.one * 0.28f;
                var colr = go.GetComponent<Collider>();
                if (colr != null)
                    Object.Destroy(colr);
                _naive.Add(go);
            }
        }

        void StepNaive(float dt, Vector3 westFlag)
        {
            _watch.Restart();
            for (var i = 0; i < _naive.Count; i++)
            {
                var t = _naive[i].transform;
                var wish = westFlag - t.position;
                wish.y = 0f;
                if (wish.sqrMagnitude > 0.04f)
                    t.position += wish.normalized * (LogicalPopulation.MarchSpeed * dt);
            }
            _watch.Stop();
            NaiveSampler.Add((float)_watch.Elapsed.TotalMilliseconds);
        }

        void TearDownNaive()
        {
            for (var i = 0; i < _naive.Count; i++)
            {
                if (_naive[i] != null)
                    Object.Destroy(_naive[i]);
            }
            _naive.Clear();
        }
    }
}
