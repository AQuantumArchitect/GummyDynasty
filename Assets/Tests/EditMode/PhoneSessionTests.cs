using GummyDynasty.Simulation;
using NUnit.Framework;

namespace GummyDynasty.Tests.EditMode
{
    public sealed class PhoneSessionTests
    {
        [Test]
        public void TwoRoles_ThenThirdRejected()
        {
            var s = new PhoneSession();
            Assert.IsTrue(s.TryJoin("commander", out var c, out _));
            Assert.IsTrue(s.TryJoin("artillery", out var a, out _));
            Assert.AreNotEqual(c, a);
            Assert.IsFalse(s.TryJoin("commander", out _, out var err));
            Assert.AreEqual("commander taken", err);
            Assert.AreEqual(2, s.PlayerCount);
        }

        [Test]
        public void WrongPlayer_Rejected()
        {
            var s = new PhoneSession();
            s.TryJoin("commander", out var id, out _);
            PhoneCommand.TryParse("cmd role=commander action=west player=nope", out var cmd, out _);
            Assert.IsFalse(s.TryValidate(cmd, out var err));
            Assert.AreEqual("not the commander", err);

            PhoneCommand.TryParse("cmd role=commander action=hold player=" + id, out cmd, out _);
            Assert.IsTrue(s.TryValidate(cmd, out _));
        }
    }
}
