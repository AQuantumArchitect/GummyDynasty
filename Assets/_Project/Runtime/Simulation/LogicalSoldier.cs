using System;
using UnityEngine;

namespace GummyDynasty.Simulation
{
    public enum LogicalIntent : byte
    {
        Idle = 0,
        MarchWest = 1,
        Dodge = 2,
        Down = 3
    }

    /// <summary>Cheap soldier. Always exists. No GameObject unless embodied.</summary>
    [Serializable]
    public struct LogicalSoldier
    {
        public int Id;
        public int Faction;
        public int Formation;
        public Vector3 Position;
        public Vector3 Velocity;
        public float HeadingY;
        public LogicalIntent Intent;
        public float Pain;
        public float Threat;
        public float Objective;
        public bool Embodied;

        public static LogicalSoldier Create(int id, Vector3 position, int faction = 0, int formation = 0)
        {
            return new LogicalSoldier
            {
                Id = id,
                Faction = faction,
                Formation = formation,
                Position = position,
                Velocity = Vector3.zero,
                HeadingY = -90f,
                Intent = LogicalIntent.Idle,
                Pain = 0f,
                Threat = 0f,
                Objective = 0f,
                Embodied = false
            };
        }
    }
}
