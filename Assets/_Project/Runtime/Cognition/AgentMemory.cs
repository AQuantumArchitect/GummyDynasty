using System.Collections.Generic;

namespace GummyDynasty.Cognition
{
    public enum MemoryKind : byte
    {
        Hit = 0,
        Flattened = 1,
        SawAllyDown = 2,
        Incoming = 3
    }

    public readonly struct MemoryEvent
    {
        public readonly MemoryKind Kind;
        public readonly float Severity;
        public readonly string OtherId;
        public readonly float Age;

        public MemoryEvent(MemoryKind kind, float severity, string otherId, float age)
        {
            Kind = kind;
            Severity = severity < 0f ? 0f : severity > 1f ? 1f : severity;
            OtherId = otherId ?? "";
            Age = age < 0f ? 0f : age;
        }

        public MemoryEvent Aged(float dt) => new MemoryEvent(Kind, Severity, OtherId, Age + dt);

        public float Recency(float fadePerSecond)
        {
            return Severity / (1f + Age * fadePerSecond);
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(OtherId))
                return Kind.ToString();
            return Kind + " " + OtherId;
        }
    }

    /// <summary>Short ring of recent events. Unity-free so the harness can prove E2.</summary>
    public sealed class AgentMemory
    {
        public const int Capacity = 8;
        public const float FadePerSecond = 0.12f;

        readonly MemoryEvent[] _buf = new MemoryEvent[Capacity];
        int _head;
        int _count;

        public int Count => _count;
        public string LastHitter { get; private set; }
        public string LastDownedAlly { get; private set; }

        public void Remember(MemoryKind kind, float severity, string otherId = null)
        {
            var who = otherId ?? "";
            _buf[_head] = new MemoryEvent(kind, severity, who, 0f);
            _head = (_head + 1) % Capacity;
            if (_count < Capacity)
                _count++;

            if (kind == MemoryKind.Hit && who.Length > 0)
                LastHitter = who;
            if (kind == MemoryKind.SawAllyDown && who.Length > 0)
                LastDownedAlly = who;
        }

        public void Tick(float deltaSeconds)
        {
            if (deltaSeconds <= 0f || _count == 0)
                return;
            for (var i = 0; i < _count; i++)
            {
                var idx = IndexFromNewest(i);
                _buf[idx] = _buf[idx].Aged(deltaSeconds);
            }
        }

        public float Recency(MemoryKind kind)
        {
            var best = 0f;
            for (var i = 0; i < _count; i++)
            {
                var ev = _buf[IndexFromNewest(i)];
                if (ev.Kind != kind)
                    continue;
                var r = ev.Recency(FadePerSecond);
                if (r > best)
                    best = r;
            }
            return best;
        }

        public void CopyRecent(List<MemoryEvent> into, int n)
        {
            into.Clear();
            if (n < 0)
                n = 0;
            if (n > _count)
                n = _count;
            for (var i = 0; i < n; i++)
                into.Add(_buf[IndexFromNewest(i)]);
        }

        int IndexFromNewest(int offset)
        {
            return (_head - 1 - offset + Capacity * 4) % Capacity;
        }
    }
}
