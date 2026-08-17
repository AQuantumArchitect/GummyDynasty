using GummyDynasty.Cognition;
using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>
    /// Rank-and-file slots for the west march. Not a crowd abstraction —
    /// just "stand here relative to the blob / the flag."
    /// </summary>
    public static class MarchFormation
    {
        public const float File = 1.35f;
        public const float SpreadFile = 2.55f;
        public const float Rank = 1.25f;
        // Must sit inside GameModeRules.ArriveBand (1.6). 2.2 parked them
        // east of WEST and they never HELD.
        public const float HoldEast = 0.9f;
        public const float Lookahead = 1.9f;
        public const float Arrive = 0.72f;

        public static Vector3 Offset(int index, int count, float fileWidth = File)
        {
            if (count < 1)
                count = 1;
            var rank = index / 3;
            var file = index % 3;
            var inRank = count - rank * 3;
            if (inRank > 3)
                inRank = 3;
            var z = (file - (inRank - 1) * 0.5f) * fileWidth;
            return new Vector3(rank * Rank, 0f, z);
        }

        public static bool Traveling(Vector3 com, Vector3 flag)
        {
            return com.x > flag.x + HoldEast + 0.45f;
        }

        public static Vector3 Anchor(Vector3 com, Vector3 flag)
        {
            if (Traveling(com, flag))
                return new Vector3(com.x - Lookahead, com.y, com.z);
            return new Vector3(flag.x + HoldEast, com.y, flag.z);
        }

        public static Vector3 SlotWorld(int index, int count, Vector3 com, Vector3 flag)
        {
            return Anchor(com, flag) + Offset(index, count);
        }

        public static Vector3 SlotWorld(
            int index,
            int count,
            Vector3 com,
            Vector3 flag,
            string tactic,
            Vector3 breach)
        {
            return SlotWorld(index, count, com, flag, tactic, breach, 0f);
        }

        public static Vector3 SlotWorld(
            int index,
            int count,
            Vector3 com,
            Vector3 flag,
            string tactic,
            Vector3 breach,
            float gapAir)
        {
            if (tactic == FormationTactics.Hold)
                return new Vector3(com.x, com.y, com.z) + Offset(index, count);

            if (tactic == FormationTactics.Spread)
                return Anchor(com, flag) + Offset(index, count, SpreadFile);

            if (tactic == FormationTactics.ThroughBreach)
                return ThroughBreachSlot(index, count, com, flag, breach, gapAir);

            return SlotWorld(index, count, com, flag);
        }

        static Vector3 ThroughBreachSlot(
            int index,
            int count,
            Vector3 com,
            Vector3 flag,
            Vector3 breach,
            float gapAir)
        {
            if (com.x <= breach.x + 0.35f)
                return SlotWorld(index, count, com, flag);

            // A slit that a belly cannot fit is not a door. Queue single-file
            // through the air we actually have; 3-abreast only when the hole
            // is wide enough for three marchers.
            var threeWide = WallMeasure.MarcherBellyRadius * 2f * 3f + 0.18f;
            if (gapAir > 0.01f && gapAir < threeWide)
                return new Vector3(breach.x + 0.2f + index * 0.7f, com.y, breach.z);

            var file = Offset(index, count, File * 0.72f);
            var rank = index / 3;
            var z = file.z;
            if (gapAir > 0.01f)
            {
                var half = gapAir * 0.5f - WallMeasure.MarcherBellyRadius;
                if (half < 0.05f)
                    z = 0f;
                else if (z > half)
                    z = half;
                else if (z < -half)
                    z = -half;
            }
            return new Vector3(breach.x + 0.2f + rank * 0.55f, com.y, breach.z + z);
        }

        /// <summary>
        /// World heading. Travel is always west; a slot only lines up the file.
        /// Scattered hoppers must not walk east (or any facing they landed in) to join the blob.
        /// </summary>
        public static Vector3 MarchHeading(Vector3 pos, Vector3 slot, Vector3 flag, bool hasSlot)
        {
            if (!hasSlot)
            {
                var toFlag = flag - pos;
                toFlag.y = 0f;
                return toFlag.sqrMagnitude > 0.0001f ? toFlag.normalized : Vector3.left;
            }

            var toSlot = slot - pos;
            toSlot.y = 0f;
            if (!Traveling(pos, flag))
                return toSlot.sqrMagnitude > 0.0001f ? toSlot.normalized : Vector3.zero;

            var x = slot.x < pos.x ? slot.x - pos.x : -1f;
            var heading = new Vector3(x, 0f, slot.z - pos.z);
            if (heading.x > -0.2f)
                heading.x = -1f;
            return heading.normalized;
        }
    }
}
