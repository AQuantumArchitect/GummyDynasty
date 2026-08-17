using System.Collections.Generic;
using UnityEngine;

namespace GummyDynasty.Simulation
{
    public static class SmashWallQuery
    {
        public const float MinGap = WallMeasure.CenterGapHint;
        public const float StandingY = WallMeasure.StandingY;

        static readonly List<Vector3> Scratch = new List<Vector3>(64);

        public static WallReport Query(Transform wall, Vector3 com)
        {
            if (wall == null)
                return WallMeasure.Missing(com);

            var parts = wall.GetComponentsInChildren<BreakablePart>();
            if (parts == null || parts.Length == 0)
                return WallMeasure.Empty(wall.position);

            Scratch.Clear();
            for (var i = 0; i < parts.Length; i++)
                Scratch.Add(parts[i].transform.position);
            return WallMeasure.Measure(Scratch, com);
        }

        public static bool TryLargestGap(
            IList<float> zs,
            float lo,
            float hi,
            float minWidth,
            out float center,
            out float width)
        {
            return WallMeasure.TryLargestGap(zs, lo, hi, minWidth, out center, out width);
        }
    }
}
