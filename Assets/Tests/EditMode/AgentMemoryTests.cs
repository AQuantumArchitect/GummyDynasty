using System.Collections.Generic;
using GummyDynasty.Cognition;
using NUnit.Framework;

namespace GummyDynasty.Tests.EditMode
{
    public sealed class AgentMemoryTests
    {
        [Test]
        public void Ring_KeepsLastEightNewestFirst()
        {
            var mem = new AgentMemory();
            for (var i = 0; i < 10; i++)
                mem.Remember(MemoryKind.Hit, 0.5f, "h" + i);

            var recent = new List<MemoryEvent>(8);
            mem.CopyRecent(recent, 8);
            Assert.AreEqual(8, recent.Count);
            Assert.AreEqual("h9", recent[0].OtherId);
            Assert.AreEqual("h2", recent[7].OtherId);
        }

        [Test]
        public void Recency_HoldsAbovePainDownForAFewSeconds()
        {
            var mem = new AgentMemory();
            mem.Remember(MemoryKind.Flattened, 1f, "candy");
            mem.Tick(2f);
            Assert.Greater(mem.Recency(MemoryKind.Flattened), IntentResolver.PainDown);
            mem.Tick(4f);
            Assert.Less(mem.Recency(MemoryKind.Flattened), IntentResolver.PainDown);
        }

        [Test]
        public void Relationships_RecordHitterAndDownedAlly()
        {
            var mem = new AgentMemory();
            mem.Remember(MemoryKind.Hit, 0.8f, "candy");
            mem.Remember(MemoryKind.SawAllyDown, 0.7f, "gummy-2");
            Assert.AreEqual("candy", mem.LastHitter);
            Assert.AreEqual("gummy-2", mem.LastDownedAlly);
        }

        [Test]
        public void E2_FlattenedHistoryChangesIntent()
        {
            var field = new BeliefField();
            BattleHierarchy.Install(field);
            BattleHierarchy.AttachActor(field, "flat");
            BattleHierarchy.AttachActor(field, "calm");
            BattleHierarchy.IssueWestOrder(field);
            field.Propagate(0.5f);

            var flatMem = new AgentMemory();
            var calmMem = new AgentMemory();
            flatMem.Remember(MemoryKind.Flattened, 1f, "candy");
            flatMem.Remember(MemoryKind.Hit, 0.9f, "candy");

            for (var i = 0; i < 8; i++)
            {
                MemorySense.Write(field, "flat", flatMem, true);
                MemorySense.Write(field, "calm", calmMem, true);
                field.Tick(0.05f);
                field.Propagate(0.22f);
                flatMem.Tick(0.05f);
                calmMem.Tick(0.05f);
            }

            var flat = IntentResolver.Resolve(
                "flat",
                field.Get("flat", HierarchyRoles.Objective),
                field.GetLocal("flat", HierarchyRoles.Threat),
                field.GetLocal("flat", HierarchyRoles.Congestion),
                field.GetLocal("flat", HierarchyRoles.Pain),
                true);
            var calm = IntentResolver.Resolve(
                "calm",
                field.Get("calm", HierarchyRoles.Objective),
                field.GetLocal("calm", HierarchyRoles.Threat),
                field.GetLocal("calm", HierarchyRoles.Congestion),
                field.GetLocal("calm", HierarchyRoles.Pain),
                true);

            Assert.AreEqual(IntentResolver.Down, flat.EffectiveName);
            Assert.AreEqual(IntentResolver.MarchWest, calm.EffectiveName);
            Assert.AreEqual("candy", flatMem.LastHitter);
        }
    }
}
