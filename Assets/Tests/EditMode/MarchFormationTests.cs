using GummyDynasty.Cognition;
using GummyDynasty.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace GummyDynasty.Tests.EditMode
{
    public sealed class MarchFormationTests
    {
        [Test]
        public void FiveHoppers_GetTwoRanksNotOnePile()
        {
            var a = MarchFormation.Offset(0, 5);
            var b = MarchFormation.Offset(1, 5);
            var c = MarchFormation.Offset(2, 5);
            var d = MarchFormation.Offset(3, 5);
            Assert.Greater((a - b).sqrMagnitude, 1f, "front rank is spaced");
            Assert.Greater((a - c).sqrMagnitude, 1f);
            Assert.Greater(d.x, a.x + 0.8f, "second rank stands behind the first");
        }

        [Test]
        public void Slots_PlantJustEastOfTheFlag_InsideVictory()
        {
            var flag = new Vector3(-10f, 0.2f, 0f);
            var com = new Vector3(-9.1f, 0.8f, 0.1f);
            Assert.IsFalse(MarchFormation.Traveling(com, flag));
            var slot = MarchFormation.SlotWorld(0, 5, com, flag);
            Assert.Greater(slot.x, flag.x + 0.4f);
            Assert.Less(slot.x, flag.x + GameModeRules.HoldWest().ArriveBand);
            Assert.IsTrue(GameModeRules.HoldWest().CheckVictory(slot.x, flag.x, true));
        }

        [Test]
        public void HoldSlots_PlantInPlace_AndStillWin()
        {
            var flag = new Vector3(-10f, 0.2f, 0f);
            var com = new Vector3(-8.6f, 0.8f, 0.2f);
            var slot = MarchFormation.SlotWorld(0, 5, com, flag, FormationTactics.Hold, com);
            Assert.AreEqual(com.x, slot.x, 0.05f);
            Assert.IsTrue(GameModeRules.HoldWest().CheckVictory(com.x, flag.x, true));
        }

        [Test]
        public void TwoRankHold_ComStillInsideVictoryBand()
        {
            var flagX = -10f;
            var comX = flagX + MarchFormation.HoldEast + MarchFormation.Rank * 0.4f;
            Assert.IsTrue(GameModeRules.HoldWest().CheckVictory(comX, flagX, true),
                "hold geometry must be able to HELD, com=" + comX);
        }

        [Test]
        public void Traveling_NeverHeadsEast_EvenIfSlotIsBehind()
        {
            var flag = new Vector3(-10f, 0.2f, 0f);
            var pos = new Vector3(-1f, 0.8f, 2f);
            var slotEast = new Vector3(2f, 0.8f, 0f);
            Assert.IsTrue(MarchFormation.Traveling(pos, flag));
            var heading = MarchFormation.MarchHeading(pos, slotEast, flag, true);
            Assert.Less(heading.x, -0.4f, "west order must not walk back east to the blob");
        }

        [Test]
        public void NoSlot_HeadsTowardTheFlag()
        {
            var flag = new Vector3(-10f, 0.2f, 0f);
            var pos = new Vector3(0f, 0.8f, 1.5f);
            var heading = MarchFormation.MarchHeading(pos, pos, flag, false);
            Assert.Less(heading.x, -0.8f);
        }

        [Test]
        public void SpreadSlots_AreWiderThanMarchSlots()
        {
            var flag = new Vector3(-10f, 0.2f, 0f);
            var com = new Vector3(2f, 0.8f, 0f);
            var tight = MarchFormation.SlotWorld(0, 3, com, flag);
            var wide = MarchFormation.SlotWorld(0, 3, com, flag, FormationTactics.Spread, com);
            Assert.Greater(Mathf.Abs(wide.z), Mathf.Abs(tight.z) + 0.4f);
        }

        [Test]
        public void ThroughBreach_FunnelsEastOfTheHole()
        {
            var flag = new Vector3(-10f, 0.2f, 0f);
            var com = new Vector3(1f, 0.8f, 0f);
            var breach = new Vector3(-2f, 0.8f, 0.4f);
            var slot = MarchFormation.SlotWorld(1, 5, com, flag, FormationTactics.ThroughBreach, breach);
            Assert.AreEqual(breach.x, slot.x, 1.2f);
            Assert.AreEqual(breach.z, slot.z, 1.6f);
        }
    }
}
