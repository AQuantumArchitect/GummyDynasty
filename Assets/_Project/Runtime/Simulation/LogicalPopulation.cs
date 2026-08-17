using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>
    /// 1k–3k soldiers as a plain array. No GameObjects, no per-soldier belief nodes.
    /// Formation prior is an argument to Tick. Embodiment is a flag + write-back.
    /// </summary>
    public sealed class LogicalPopulation
    {
        public const int MaxCapacity = 4096;
        public const float MarchSpeed = 1.6f;
        public const float DodgeSpeed = 2.2f;
        public const float ArriveBand = 1.6f;
        const int Magic = 0x534C4447; // "GDLS"
        const ushort Version = 1;

        LogicalSoldier[] _items = new LogicalSoldier[64];
        readonly Dictionary<int, int> _index = new Dictionary<int, int>(256);
        int _count;
        int _nextId = 1;

        public bool Marching;
        public int Count => _count;
        public int NextId => _nextId;
        public int EmbodiedCount { get; private set; }
        public int DisembodiedCount => _count - EmbodiedCount;
        public LogicalSoldier this[int i] => _items[i];

        public int Spawn(Vector3 position, int faction = 0, int formation = 0)
        {
            Ensure(_count + 1);
            var id = _nextId++;
            var soldier = LogicalSoldier.Create(id, position, faction, formation);
            if (Marching)
            {
                soldier.Intent = LogicalIntent.MarchWest;
                soldier.Objective = 1f;
            }
            _items[_count] = soldier;
            _index[id] = _count;
            _count++;
            return id;
        }

        public int SeedBlock(int n, Vector3 origin, float spacing = 0.55f, int columns = 20)
        {
            var added = 0;
            for (var i = 0; i < n; i++)
            {
                if (_count >= MaxCapacity)
                    break;
                var col = i % columns;
                var row = i / columns;
                Spawn(origin + new Vector3(col * spacing, 0f, (row - columns * 0.5f) * spacing));
                added++;
            }
            return added;
        }

        public bool TryGet(int id, out LogicalSoldier soldier)
        {
            if (_index.TryGetValue(id, out var i))
            {
                soldier = _items[i];
                return true;
            }
            soldier = default;
            return false;
        }

        public void SetEmbodied(int id, bool embodied)
        {
            if (!_index.TryGetValue(id, out var i))
                return;
            if (_items[i].Embodied == embodied)
                return;
            _items[i].Embodied = embodied;
            EmbodiedCount += embodied ? 1 : -1;
        }

        public void WriteBack(int id, Vector3 position, Vector3 velocity, float pain, LogicalIntent intent)
        {
            if (!_index.TryGetValue(id, out var i))
                return;
            _items[i].Position = position;
            _items[i].Velocity = velocity;
            _items[i].Pain = pain;
            _items[i].Intent = intent;
        }

        public void Tick(float dt, Vector3 westFlag, Vector3 incoming)
        {
            if (dt <= 0f || _count == 0)
                return;

            var incomingLive = incoming.sqrMagnitude < 40000f && incoming.x < 80f;
            for (var i = 0; i < _count; i++)
            {
                if (_items[i].Embodied)
                    continue;
                Step(ref _items[i], dt, westFlag, incoming, incomingLive);
            }
        }

        void Step(ref LogicalSoldier s, float dt, Vector3 westFlag, Vector3 incoming, bool incomingLive)
        {
            var threat = 0f;
            if (incomingLive)
            {
                var d = Vector3.Distance(s.Position, incoming);
                if (d < 6f)
                    threat = (6f - d) / 6f;
            }
            s.Threat = threat;

            if (s.Pain >= 0.7f)
                s.Intent = LogicalIntent.Down;
            else if (threat >= 0.55f)
                s.Intent = LogicalIntent.Dodge;
            else if (Marching || s.Objective >= 0.55f)
                s.Intent = LogicalIntent.MarchWest;
            else
                s.Intent = LogicalIntent.Idle;

            Vector3 wish;
            float speed;
            switch (s.Intent)
            {
                case LogicalIntent.MarchWest:
                    if (s.Position.x <= westFlag.x + ArriveBand)
                    {
                        s.Velocity = Vector3.zero;
                        s.Intent = LogicalIntent.Idle;
                        return;
                    }
                    wish = westFlag - s.Position;
                    wish.y = 0f;
                    if (wish.sqrMagnitude < 0.04f)
                    {
                        s.Velocity = Vector3.zero;
                        return;
                    }
                    wish.Normalize();
                    speed = MarchSpeed;
                    break;
                case LogicalIntent.Dodge:
                    wish = s.Position - incoming;
                    wish.y = 0f;
                    if (wish.sqrMagnitude < 0.01f)
                        wish = Vector3.forward;
                    wish.Normalize();
                    speed = DodgeSpeed;
                    break;
                case LogicalIntent.Down:
                    s.Velocity *= Mathf.Max(0f, 1f - dt * 4f);
                    s.Position += s.Velocity * dt;
                    Clamp(ref s);
                    return;
                default:
                    s.Velocity *= Mathf.Max(0f, 1f - dt * 3f);
                    s.Position += s.Velocity * dt;
                    Clamp(ref s);
                    return;
            }

            s.Velocity = wish * speed;
            s.HeadingY = Mathf.Atan2(wish.x, wish.z) * Mathf.Rad2Deg;
            s.Position += s.Velocity * dt;
            s.Position.y = 0.9f;
            Clamp(ref s);
        }

        static void Clamp(ref LogicalSoldier s)
        {
            s.Position.x = Mathf.Clamp(s.Position.x, -13.2f, 13.2f);
            s.Position.z = Mathf.Clamp(s.Position.z, -13.2f, 13.2f);
            s.Position.y = Mathf.Max(0.35f, s.Position.y);
        }

        public Vector3 CenterOfMass()
        {
            if (_count == 0)
                return Vector3.zero;
            var sum = Vector3.zero;
            for (var i = 0; i < _count; i++)
                sum += _items[i].Position;
            return sum / _count;
        }

        public Vector3 MeanVelocity()
        {
            if (_count == 0)
                return Vector3.zero;
            var sum = Vector3.zero;
            for (var i = 0; i < _count; i++)
                sum += _items[i].Velocity;
            return sum / _count;
        }

        public int CopyDisembodied(Vector3[] dest)
        {
            if (dest == null)
                return 0;
            var n = 0;
            for (var i = 0; i < _count && n < dest.Length; i++)
            {
                if (_items[i].Embodied)
                    continue;
                dest[n++] = _items[i].Position;
            }
            return n;
        }

        public void Clear()
        {
            _count = 0;
            EmbodiedCount = 0;
            _index.Clear();
        }

        public string ToJson()
        {
            var wrap = new JsonWrap
            {
                nextId = _nextId,
                marching = Marching,
                soldiers = new LogicalSoldier[_count]
            };
            Array.Copy(_items, wrap.soldiers, _count);
            return JsonUtility.ToJson(wrap);
        }

        public void LoadJson(string json)
        {
            var wrap = JsonUtility.FromJson<JsonWrap>(json);
            if (wrap?.soldiers == null)
                throw new InvalidOperationException("empty logical snapshot");
            LoadSoldiers(wrap.soldiers, wrap.nextId, wrap.marching);
        }

        public byte[] ToBlob()
        {
            using (var ms = new MemoryStream(32 + _count * 64))
            using (var w = new BinaryWriter(ms, Encoding.UTF8, true))
            {
                w.Write(Magic);
                w.Write(Version);
                w.Write(_nextId);
                w.Write(Marching);
                w.Write(_count);
                for (var i = 0; i < _count; i++)
                {
                    var s = _items[i];
                    w.Write(s.Id);
                    w.Write(s.Faction);
                    w.Write(s.Formation);
                    w.Write(s.Position.x);
                    w.Write(s.Position.y);
                    w.Write(s.Position.z);
                    w.Write(s.Velocity.x);
                    w.Write(s.Velocity.y);
                    w.Write(s.Velocity.z);
                    w.Write(s.HeadingY);
                    w.Write((byte)s.Intent);
                    w.Write(s.Pain);
                    w.Write(s.Threat);
                    w.Write(s.Objective);
                    w.Write(s.Embodied);
                }
                return ms.ToArray();
            }
        }

        public void LoadBlob(byte[] data)
        {
            if (data == null || data.Length < 16)
                throw new InvalidOperationException("logical blob too small");
            using (var ms = new MemoryStream(data))
            using (var r = new BinaryReader(ms, Encoding.UTF8, true))
            {
                if (r.ReadInt32() != Magic)
                    throw new InvalidOperationException("not a GDLS blob");
                var ver = r.ReadUInt16();
                if (ver != Version)
                    throw new InvalidOperationException("unsupported logical blob v" + ver);
                var nextId = r.ReadInt32();
                var marching = r.ReadBoolean();
                var n = r.ReadInt32();
                var soldiers = new LogicalSoldier[n];
                for (var i = 0; i < n; i++)
                {
                    soldiers[i] = new LogicalSoldier
                    {
                        Id = r.ReadInt32(),
                        Faction = r.ReadInt32(),
                        Formation = r.ReadInt32(),
                        Position = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()),
                        Velocity = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()),
                        HeadingY = r.ReadSingle(),
                        Intent = (LogicalIntent)r.ReadByte(),
                        Pain = r.ReadSingle(),
                        Threat = r.ReadSingle(),
                        Objective = r.ReadSingle(),
                        Embodied = r.ReadBoolean()
                    };
                }
                LoadSoldiers(soldiers, nextId, marching);
            }
        }

        void LoadSoldiers(LogicalSoldier[] soldiers, int nextId, bool marching)
        {
            Clear();
            Ensure(soldiers.Length);
            _nextId = Mathf.Max(1, nextId);
            Marching = marching;
            for (var i = 0; i < soldiers.Length; i++)
            {
                var s = soldiers[i];
                if (s.Id <= 0)
                    s.Id = _nextId++;
                if (s.Id >= _nextId)
                    _nextId = s.Id + 1;
                s.Embodied = false;
                _items[_count] = s;
                _index[s.Id] = _count;
                _count++;
            }
        }

        void Ensure(int n)
        {
            if (n > MaxCapacity)
                n = MaxCapacity;
            if (_items.Length >= n)
                return;
            var cap = _items.Length;
            while (cap < n)
                cap *= 2;
            if (cap > MaxCapacity)
                cap = MaxCapacity;
            Array.Resize(ref _items, cap);
        }

        [Serializable]
        class JsonWrap
        {
            public int nextId;
            public bool marching;
            public LogicalSoldier[] soldiers;
        }
    }
}
