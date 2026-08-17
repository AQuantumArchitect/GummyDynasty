using GummyDynasty.Cognition;
using GummyDynasty.Simulation;
using NUnit.Framework;

namespace GummyDynasty.Tests.EditMode
{
    public sealed class GameModeRulesTests
    {
        [Test]
        public void HoldWest_WinsInsideBand()
        {
            var mode = GameModeRules.HoldWest();
            Assert.IsTrue(mode.CheckVictory(-10f, -10f, true));
            Assert.IsFalse(mode.CheckVictory(3f, -10f, true));
            Assert.IsFalse(mode.CheckVictory(-10f, -10f, false));
        }

        [Test]
        public void HoldEast_MustSitInsideVictoryBand()
        {
            var mode = GameModeRules.HoldWest();
            Assert.IsFalse(mode.CheckVictory(-10f + 2.2f, -10f, true),
                "the old 2.2m plant line sat outside ArriveBand and never HELD");
            Assert.IsTrue(mode.CheckVictory(-10f + MarchFormation.HoldEast, -10f, true));
        }

        [Test]
        public void OrderedHold_BeatsWall()
        {
            Assert.AreEqual(
                FormationTactics.Hold,
                FormationTactics.Resolve(1f, false, 1f, 0f, 1f));
        }

        [Test]
        public void IssueHold_SetsUnitaryHold()
        {
            var field = new BeliefField();
            BattleHierarchy.Install(field);
            BattleHierarchy.IssueHoldOrder(field);
            Assert.Greater(field.Get(HierarchyIds.FormationRed, HierarchyRoles.Hold).Value, 0.85f);
            Assert.Greater(field.Get(HierarchyIds.FactionRed, HierarchyRoles.Objective).Value, 0.85f);
        }
    }
}
