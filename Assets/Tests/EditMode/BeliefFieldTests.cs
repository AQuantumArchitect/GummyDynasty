using GummyDynasty.Cognition;
using NUnit.Framework;

namespace GummyDynasty.Tests.EditMode
{
    public sealed class BeliefFieldTests
    {
        [Test]
        public void EtaZero_IsNoOp()
        {
            var field = new BeliefField();
            field.AddNode("a", kind: NodeKind.Actor);
            field.EnsureRole("a", "pain");
            field.Observe(new Observation("a", "pain", 1f, 0f));
            var b = field.Get("a", "pain");
            Assert.AreEqual(0.5f, b.Value, 0.0001f);
            Assert.AreEqual(0f, b.Confidence, 0.0001f);
        }

        [Test]
        public void Observe_MovesValueAndConfidence()
        {
            var field = new BeliefField();
            field.AddNode("a", kind: NodeKind.Actor);
            field.EnsureRole("a", "pain");
            field.Observe(new Observation("a", "pain", 1f, 1f));
            var b = field.Get("a", "pain");
            Assert.Greater(b.Value, 0.7f);
            Assert.Greater(b.Confidence, 0.1f);
        }

        [Test]
        public void SelfTagged_DoesNotUpdateWorldNode()
        {
            var field = new BeliefField();
            field.AddNode("world", kind: NodeKind.World);
            field.EnsureRole("world", "threat");
            field.Observe(new Observation("world", "threat", 1f, 1f, "gummy-1", selfTagged: true));
            Assert.AreEqual(0.5f, field.Get("world", "threat").Value, 0.0001f);
        }

        [Test]
        public void SelfTagged_UpdatesActorNode()
        {
            var field = new BeliefField();
            field.AddNode("gummy-1", kind: NodeKind.Actor);
            field.EnsureRole("gummy-1", "threat");
            field.Observe(new Observation("gummy-1", "threat", 1f, 1f, "gummy-1", selfTagged: true));
            Assert.Greater(field.Get("gummy-1", "threat").Value, 0.7f);
        }

        [Test]
        public void Dissipative_DecaysTowardUnknown()
        {
            var field = new BeliefField();
            field.AddNode("a", kind: NodeKind.Actor);
            field.EnsureRole("a", "pain", RoleMode.Dissipative, 2f);
            field.Observe(new Observation("a", "pain", 1f, 1f));
            var before = field.Get("a", "pain").Value;
            field.Tick(1f);
            var after = field.Get("a", "pain").Value;
            Assert.Less(after, before);
            Assert.Greater(after, 0.5f);
        }

        [Test]
        public void Unitary_DoesNotDecay()
        {
            var field = new BeliefField();
            field.AddNode("a", kind: NodeKind.Actor);
            field.EnsureRole("a", "objective", RoleMode.Unitary);
            field.Observe(new Observation("a", "objective", 0.9f, 1f));
            var before = field.Get("a", "objective").Value;
            field.Tick(1f);
            Assert.AreEqual(before, field.Get("a", "objective").Value, 0.0001f);
        }

        [Test]
        public void ParentReduce_MeansChildren()
        {
            var field = new BeliefField();
            field.AddNode("form");
            field.EnsureRole("form", "pain", RoleMode.Dissipative, 0f, ReduceOp.Mean);
            field.AddNode("a", "form", NodeKind.Actor);
            field.AddNode("b", "form", NodeKind.Actor);
            field.EnsureRole("a", "pain");
            field.EnsureRole("b", "pain");
            field.Observe(new Observation("a", "pain", 1f, 1f));
            field.Observe(new Observation("b", "pain", 0f, 1f));
            field.Propagate(0f);
            var mean = field.Get("form", "pain").Value;
            Assert.AreEqual(0.5f, mean, 0.08f);
        }
    }
}
