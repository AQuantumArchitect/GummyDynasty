using GummyDynasty.Simulation;
using NUnit.Framework;

namespace GummyDynasty.Tests.EditMode
{
    public sealed class PhoneCommandTests
    {
        [Test]
        public void Garbage_IsRejected()
        {
            Assert.IsFalse(PhoneCommand.TryParse("", out _, out var err));
            Assert.AreEqual("empty", err);
            Assert.IsFalse(PhoneCommand.TryParse("{\"op\":\"dance\"}", out _, out _));
            Assert.IsFalse(PhoneCommand.TryParse("cmd role=wizard action=west", out _, out _));
        }

        [Test]
        public void JsonJoin_ParsesCommander()
        {
            Assert.IsTrue(PhoneCommand.TryParse("{\"op\":\"join\",\"role\":\"commander\"}", out var cmd, out _));
            Assert.AreEqual(PhoneCommand.OpJoin, cmd.Op);
            Assert.AreEqual(PhoneCommand.RoleCommander, cmd.Role);
        }

        [Test]
        public void CommanderCannotFire()
        {
            Assert.IsFalse(PhoneCommand.TryParse(
                "{\"op\":\"cmd\",\"role\":\"commander\",\"action\":\"fire\"}", out _, out var err));
            Assert.IsTrue(err.Contains("west") || err.Contains("hold"));
        }

        [Test]
        public void ArtilleryAim_NeedsPoint()
        {
            Assert.IsFalse(PhoneCommand.TryParse(
                "cmd role=artillery action=aim machine=catapult", out _, out var err));
            Assert.AreEqual("aim needs x and z", err);
        }

        [Test]
        public void ArtilleryAim_ParsesPoint()
        {
            Assert.IsTrue(PhoneCommand.TryParse(
                "{\"op\":\"cmd\",\"role\":\"artillery\",\"action\":\"aim\",\"x\":-2.5,\"z\":1}", out var cmd, out _));
            Assert.AreEqual(PhoneCommand.ActAim, cmd.Action);
            Assert.AreEqual(-2.5f, cmd.X, 0.001f);
            Assert.AreEqual(1f, cmd.Z, 0.001f);
            Assert.AreEqual(PhoneCommand.MachineCatapult, cmd.Machine);
        }
    }
}
