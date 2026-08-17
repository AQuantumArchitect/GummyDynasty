using System.Collections.Generic;
using GummyDynasty.Cognition;
using GummyDynasty.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace GummyDynasty.Tests.EditMode
{
    public sealed class WallMeasureTests
    {
        static readonly Vector3 Origin = new Vector3(-2f, 0.35f, 0f);
        static readonly Vector3 East = new Vector3(2.5f, 0.8f, 0f);

        static List<Vector3> Pit(params int[] dropZ)
        {
            var parts = new List<Vector3>(64);
            WallMeasure.LayoutStack(parts, Origin, 2, 4, 7);
            if (dropZ == null || dropZ.Length == 0)
                return parts;
            var kept = new List<Vector3>(parts.Count);
            for (var i = 0; i < parts.Count; i++)
            {
                var zIndex = Mathf.RoundToInt(parts[i].z / WallMeasure.CratePitch + 3f);
                var drop = false;
                for (var k = 0; k < dropZ.Length; k++)
                {
                    if (zIndex == dropZ[k])
                    {
                        drop = true;
                        break;
                    }
                }
                if (!drop)
                    kept.Add(parts[i]);
            }
            return kept;
        }

        [Test]
        public void IntactPitWall_IsAheadNotAHole()
        {
            var report = WallMeasure.Measure(Pit(), East);
            Assert.IsTrue(report.Exists);
            Assert.IsTrue(report.Ahead);
            Assert.IsFalse(report.Hole);
            Assert.IsFalse(WallMeasure.Passable(report.GapAir));
        }

        [Test]
        public void OneColumn_IsASlit_NotADoor()
        {
            var report = WallMeasure.Measure(Pit(3), East);
            Assert.Less(report.GapAir, WallMeasure.PassableAir);
            Assert.IsFalse(report.Hole);
            Assert.AreEqual(FormationTactics.Spread,
                FormationTactics.Resolve(1f, false, report.WallValue, report.HoleValue));
        }

        [Test]
        public void TwoColumns_IsADoor()
        {
            var report = WallMeasure.Measure(Pit(3, 4), East);
            Assert.GreaterOrEqual(report.GapAir, WallMeasure.PassableAir);
            Assert.IsTrue(report.Hole);
            Assert.AreEqual(FormationTactics.ThroughBreach,
                FormationTactics.Resolve(1f, false, report.WallValue, report.HoleValue));
        }

        [Test]
        public void OldCenterGapHint_WouldLieAboutOneColumn()
        {
            var report = WallMeasure.Measure(Pit(3), East);
            Assert.Greater(report.GapWidth, SmashWallQuery.MinGap,
                "the old 0.95 m center-gap would have called this a hole");
            Assert.IsFalse(report.Hole, "air, not crate-center spacing, decides a hole");
        }

        [Test]
        public void TightBreach_QueuesSingleFile()
        {
            var report = WallMeasure.Measure(Pit(3, 4), East);
            var com = new Vector3(1f, 0.8f, 0f);
            for (var i = 0; i < 5; i++)
            {
                var slot = MarchFormation.SlotWorld(
                    i, 5, com, new Vector3(-10f, 0.2f, 0f),
                    FormationTactics.ThroughBreach, report.BreachPoint, report.GapAir);
                Assert.AreEqual(report.BreachPoint.z, slot.z, 0.08f);
            }
        }

        [Test]
        public void MarcherBellyFitsTwoColumns_NotOne()
        {
            var one = WallMeasure.AirFromCenterGap(WallMeasure.CratePitch * 2f);
            var two = WallMeasure.AirFromCenterGap(WallMeasure.CratePitch * 3f);
            Assert.IsFalse(WallMeasure.Passable(one));
            Assert.IsTrue(WallMeasure.Passable(two));
        }
    }
}
