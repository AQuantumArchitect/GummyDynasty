using System.Collections.Generic;
using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>Cheap road report for the existing SmashWall. No extra stacks.</summary>
    public readonly struct WallReport
    {
        public readonly bool Exists;
        public readonly bool Ahead;
        public readonly bool Hole;
        public readonly bool Rubble;
        public readonly Vector3 Center;
        public readonly Vector3 BreachPoint;
        public readonly float GapAir;
        public readonly float GapWidth;

        public WallReport(bool exists, bool ahead, bool hole, bool rubble, Vector3 center, Vector3 breach)
            : this(exists, ahead, hole, rubble, center, breach, 0f, 0f)
        {
        }

        public WallReport(
            bool exists,
            bool ahead,
            bool hole,
            bool rubble,
            Vector3 center,
            Vector3 breach,
            float gapAir,
            float gapWidth)
        {
            Exists = exists;
            Ahead = ahead;
            Hole = hole;
            Rubble = rubble;
            Center = center;
            BreachPoint = breach;
            GapAir = gapAir;
            GapWidth = gapWidth;
        }

        public float WallValue => Ahead ? 1f : 0f;
        public float HoleValue => Hole ? 1f : 0f;
    }

    /// <summary>
    /// Unity-free road geometry. SmashWallQuery.Query is the Transform
    /// wrapper; the harness and the PhysX probe both call this.
    /// </summary>
    public static class WallMeasure
    {
        public const float CratePitch = 0.55f;
        public const float CrateVisual = CratePitch * 0.92f;
        public const float CrateExtent = CrateVisual * 0.5f;
        public const float StandingY = 0.28f;
        public const float CenterGapHint = 0.95f;

        // Marcher belly: 0.28 * height * width * 1.35, as a radius.
        public const float MarcherHeight = 1.65f;
        public const float MarcherBellyRadius = 0.28f * MarcherHeight * 1.35f * 0.5f;
        public const float PassableSlack = 0.12f;
        public static float PassableAir => MarcherBellyRadius * 2f + PassableSlack;

        public static float AirFromCenterGap(float centerGap)
        {
            var air = centerGap - CrateVisual;
            return air > 0f ? air : 0f;
        }

        public static bool Passable(float air)
        {
            return air >= PassableAir;
        }

        public static void LayoutStack(IList<Vector3> into, Vector3 origin, int wide, int high, int deep)
        {
            if (into == null)
                return;
            const float s = CratePitch;
            for (var y = 0; y < high; y++)
            for (var z = 0; z < deep; z++)
            for (var x = 0; x < wide; x++)
            {
                into.Add(origin + new Vector3(
                    (x - (wide - 1) * 0.5f) * s,
                    y * s + s * 0.5f + 0.08f,
                    (z - (deep - 1) * 0.5f) * s));
            }
        }

        public static WallReport Missing(Vector3 fallback)
        {
            return new WallReport(false, false, false, true, fallback, fallback, 0f, 0f);
        }

        public static WallReport Empty(Vector3 fallback)
        {
            return new WallReport(false, false, true, true, fallback, fallback, 8f, 8f);
        }

        public static WallReport Measure(IList<Vector3> parts, Vector3 com)
        {
            if (parts == null || parts.Count == 0)
                return Empty(com);

            var center = Vector3.zero;
            var standing = 0;
            var zMin = float.PositiveInfinity;
            var zMax = float.NegativeInfinity;
            var zs = new float[parts.Count];
            for (var i = 0; i < parts.Count; i++)
            {
                var p = parts[i];
                center += p;
                if (p.z < zMin) zMin = p.z;
                if (p.z > zMax) zMax = p.z;
                if (p.y >= StandingY)
                    zs[standing++] = p.z;
            }
            center /= parts.Count;

            var rubble = standing <= 3 || standing < parts.Count * 0.3f;
            var eastOfWall = com.x > center.x + 0.4f;
            var lo = zMin - 0.3f;
            var hi = zMax + 0.3f;
            var breach = new Vector3(center.x, 0.8f, center.z);
            var gapWidth = 0f;
            var gapAir = 0f;
            var gap = rubble;
            if (rubble)
            {
                gapAir = 8f;
                gapWidth = 8f;
            }
            else
            {
                var slice = standing == zs.Length ? (IList<float>)zs : Slice(zs, standing);
                TryLargestGap(slice, lo, hi, 0.01f, out var gapZ, out gapWidth);
                gapAir = AirFromCenterGap(gapWidth);
                if (Passable(gapAir))
                {
                    gap = true;
                    breach = new Vector3(center.x, 0.8f, gapZ);
                }
            }

            var ahead = eastOfWall && !rubble;
            var hole = eastOfWall && gap;
            return new WallReport(true, ahead, hole, rubble, center, breach, gapAir, gapWidth);
        }

        public static bool TryLargestGap(
            IList<float> zs,
            float lo,
            float hi,
            float minWidth,
            out float center,
            out float width)
        {
            center = (lo + hi) * 0.5f;
            width = 0f;
            if (hi - lo < minWidth)
                return false;

            if (zs == null || zs.Count == 0)
            {
                width = hi - lo;
                return width >= minWidth;
            }

            var n = zs.Count;
            var sorted = new float[n];
            for (var i = 0; i < n; i++)
                sorted[i] = zs[i];
            System.Array.Sort(sorted);

            var bestLo = lo;
            var bestHi = sorted[0];
            width = bestHi - bestLo;
            for (var i = 0; i < n - 1; i++)
            {
                var w = sorted[i + 1] - sorted[i];
                if (w > width)
                {
                    width = w;
                    bestLo = sorted[i];
                    bestHi = sorted[i + 1];
                }
            }
            var tail = hi - sorted[n - 1];
            if (tail > width)
            {
                width = tail;
                bestLo = sorted[n - 1];
                bestHi = hi;
            }

            center = (bestLo + bestHi) * 0.5f;
            return width >= minWidth;
        }

        static float[] Slice(float[] src, int n)
        {
            var a = new float[n];
            for (var i = 0; i < n; i++)
                a[i] = src[i];
            return a;
        }
    }
}
