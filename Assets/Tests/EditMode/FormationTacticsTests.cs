using GummyDynasty.Cognition;
using GummyDynasty.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace GummyDynasty.Tests.EditMode
{
    public sealed class FormationTacticsTests
    {
        [Test]
        public void WallAhead_NoHole_Spreads()
        {
            Assert.AreEqual(
                FormationTactics.Spread,
                FormationTactics.Resolve(1f, false, 1f, 0f));
        }

        [Test]
        public void HolePresent_GoesThroughBreach()
        {
            Assert.AreEqual(
                FormationTactics.ThroughBreach,
                FormationTactics.Resolve(1f, false, 1f, 1f));
        }

        [Test]
        public void Arrived_Holds_EvenWithWall()
        {
            Assert.AreEqual(
                FormationTactics.Hold,
                FormationTactics.Resolve(1f, true, 1f, 0f));
        }

        [Test]
        public void OpenRoad_MarchesWest()
        {
            Assert.AreEqual(
                FormationTactics.MarchWest,
                FormationTactics.Resolve(1f, false, 0f, 0f));
        }

        [Test]
        public void NoWestOrder_Idles()
        {
            Assert.AreEqual(
                IntentResolver.Idle,
                FormationTactics.Resolve(0.2f, false, 1f, 0f));
        }

        [Test]
        public void UnitaryFactionOrder_SurvivesRoadObserve()
        {
            var field = new BeliefField();
            BattleHierarchy.Install(field);
            BattleHierarchy.AttachActor(field, "a");
            BattleHierarchy.IssueWestOrder(field);
            BattleHierarchy.ObserveRoad(field, 1f, 0f);
            field.Propagate(0.2f);
            Assert.Greater(field.Get(HierarchyIds.FactionRed, HierarchyRoles.Objective).Value, 0.85f);
            Assert.Greater(field.Get(HierarchyIds.FormationRed, HierarchyRoles.Wall).Value, 0.8f);
        }

        [Test]
        public void IncomingThreat_StillDodgesWhileFileSpreads()
        {
            var intent = IntentResolver.Resolve(
                "g",
                new Belief("g", "objective", 0.9f, 1f),
                new Belief("g", "threat", 0.8f, 0.8f),
                new Belief("g", "congestion", 0.8f, 0.8f),
                new Belief("g", "pain", 0.1f, 0.2f),
                upright: true,
                FormationTactics.Spread);
            Assert.AreEqual(IntentResolver.Dodge, intent.EffectiveName);
            Assert.AreEqual(FormationTactics.Spread, intent.InheritedName);
        }

        [Test]
        public void Flattened_StillDownWhileFileBreaches()
        {
            var intent = IntentResolver.Resolve(
                "g",
                new Belief("g", "objective", 0.9f, 1f),
                new Belief("g", "threat", 0.1f, 0.2f),
                new Belief("g", "congestion", 0.1f, 0.2f),
                new Belief("g", "pain", 0.9f, 0.8f),
                upright: false,
                FormationTactics.ThroughBreach);
            Assert.AreEqual(IntentResolver.Down, intent.EffectiveName);
            Assert.AreEqual(FormationTactics.ThroughBreach, intent.InheritedName);
        }

        [Test]
        public void UntouchedFileMate_InheritsSpread_NotDodge()
        {
            var intent = IntentResolver.Resolve(
                "g",
                new Belief("g", "objective", 0.9f, 1f),
                new Belief("g", "threat", 0.1f, 0.2f),
                new Belief("g", "congestion", 0.9f, 0.9f),
                new Belief("g", "pain", 0.1f, 0.2f),
                upright: true,
                FormationTactics.Spread);
            Assert.AreEqual(FormationTactics.Spread, intent.EffectiveName);
            Assert.IsFalse(intent.Overridden);
        }

        [Test]
        public void GapFinder_ReportsMiddleHole()
        {
            var zs = new[] { -1.5f, -1.0f, 1.0f, 1.5f };
            Assert.IsTrue(SmashWallQuery.TryLargestGap(zs, -2f, 2f, 0.95f, out var center, out var width));
            Assert.Greater(width, 1.5f);
            Assert.AreEqual(0f, center, 0.2f);
        }
    }
}
