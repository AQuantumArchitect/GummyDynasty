using GummyDynasty.Cognition;
using NUnit.Framework;

namespace GummyDynasty.Tests.EditMode
{
    public sealed class HierarchyCognitionTests
    {
        [Test]
        public void Inherit_PullsChildTowardParentCommand()
        {
            var field = new BeliefField();
            BattleHierarchy.Install(field);
            BattleHierarchy.AttachActor(field, "gummy-1");
            BattleHierarchy.IssueWestOrder(field);

            var before = field.GetLocal("gummy-1", HierarchyRoles.Objective).Value;
            field.Propagate(0.5f);
            var after = field.Get("gummy-1", HierarchyRoles.Objective).Value;
            Assert.Greater(after, before);
            Assert.Greater(after, 0.7f);
        }

        [Test]
        public void Override_BlocksInheritOnObjective()
        {
            var field = new BeliefField();
            BattleHierarchy.Install(field);
            BattleHierarchy.AttachActor(field, "gummy-1");
            BattleHierarchy.IssueWestOrder(field);
            field.SetOverride("gummy-1", HierarchyRoles.Objective, 1f);

            var local = field.GetLocal("gummy-1", HierarchyRoles.Objective).Value;
            field.Propagate(0.5f);
            Assert.AreEqual(local, field.Get("gummy-1", HierarchyRoles.Objective).Value, 0.02f);
        }

        [Test]
        public void UnitaryFactionOrder_IsNotClobberedByChildren()
        {
            var field = new BeliefField();
            BattleHierarchy.Install(field);
            BattleHierarchy.AttachActor(field, "a");
            BattleHierarchy.AttachActor(field, "b");
            BattleHierarchy.IssueWestOrder(field);
            field.Observe(new Observation("a", HierarchyRoles.Objective, 0.1f, 1f));
            field.Observe(new Observation("b", HierarchyRoles.Objective, 0.1f, 1f));
            field.Propagate(0.1f);
            Assert.Greater(field.Get(HierarchyIds.FactionRed, HierarchyRoles.Objective).Value, 0.85f);
        }

        [Test]
        public void FormationCost_MeansChildren()
        {
            var field = new BeliefField();
            BattleHierarchy.Install(field);
            BattleHierarchy.AttachActor(field, "a");
            BattleHierarchy.AttachActor(field, "b");
            field.Observe(new Observation("a", HierarchyRoles.Cost, 1f, 1f));
            field.Observe(new Observation("b", HierarchyRoles.Cost, 0f, 1f));
            field.Propagate(0f);
            Assert.AreEqual(0.5f, field.Get(HierarchyIds.FormationRed, HierarchyRoles.Cost).Value, 0.08f);
        }

        [Test]
        public void Ancestry_WalksFactionArmyFormationActor()
        {
            var field = new BeliefField();
            BattleHierarchy.Install(field);
            BattleHierarchy.AttachActor(field, "gummy-1");
            var chain = new System.Collections.Generic.List<string>();
            field.CopyAncestry("gummy-1", chain);
            Assert.AreEqual(5, chain.Count);
            Assert.AreEqual("gummy-1", chain[0]);
            Assert.AreEqual(HierarchyIds.FormationRed, chain[1]);
            Assert.AreEqual(HierarchyIds.ArmyWest, chain[2]);
            Assert.AreEqual(HierarchyIds.FactionRed, chain[3]);
            Assert.AreEqual(HierarchyIds.World, chain[4]);
        }

        [Test]
        public void Intent_MarchesWhenCalm()
        {
            var intent = IntentResolver.Resolve(
                "g",
                new Belief("g", "objective", 0.9f, 1f),
                new Belief("g", "threat", 0.1f, 0.2f),
                new Belief("g", "congestion", 0.1f, 0.2f),
                new Belief("g", "pain", 0.1f, 0.2f),
                upright: true);
            Assert.AreEqual(IntentResolver.MarchWest, intent.EffectiveName);
            Assert.IsFalse(intent.Overridden);
        }

        [Test]
        public void Intent_DodgesIncomingCrate()
        {
            var intent = IntentResolver.Resolve(
                "g",
                new Belief("g", "objective", 0.9f, 1f),
                new Belief("g", "threat", 0.8f, 0.8f),
                new Belief("g", "congestion", 0.2f, 0.2f),
                new Belief("g", "pain", 0.1f, 0.2f),
                upright: true);
            Assert.AreEqual(IntentResolver.Dodge, intent.EffectiveName);
            Assert.IsTrue(intent.Overridden);
            Assert.AreEqual(IntentResolver.MarchWest, intent.InheritedName);
        }

        [Test]
        public void Intent_DownWhenFlattened()
        {
            var intent = IntentResolver.Resolve(
                "g",
                new Belief("g", "objective", 0.9f, 1f),
                new Belief("g", "threat", 0.1f, 0.2f),
                new Belief("g", "congestion", 0.1f, 0.2f),
                new Belief("g", "pain", 0.9f, 0.8f),
                upright: false);
            Assert.AreEqual(IntentResolver.Down, intent.EffectiveName);
            Assert.IsTrue(intent.Overridden);
        }
    }
}
