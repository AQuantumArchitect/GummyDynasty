using GummyDynasty.Simulation;
using NUnit.Framework;

namespace GummyDynasty.Tests.EditMode
{
    public sealed class MachineControlsTests
    {
        [Test]
        public void Release_WithoutDraw_IsNoOp()
        {
            var m = new MachineControls();
            Assert.IsFalse(m.Pulse(MachineControls.Release));
        }

        [Test]
        public void LoadDrawRelease_FiresAndResets()
        {
            var m = new MachineControls();
            Assert.IsTrue(m.Pulse(MachineControls.Load));
            Assert.IsTrue(m.Loaded);
            m.Set(MachineControls.Draw, 1f);
            m.Set(MachineControls.Aim, 0.4f);
            Assert.AreEqual(0.4f, m.AimValue, 0.001f);
            Assert.IsTrue(m.Pulse(MachineControls.Release));
            Assert.IsFalse(m.Loaded);
            Assert.Less(m.DrawValue, 0.01f);
        }

        [Test]
        public void Aim_ClampsToTendrilRange()
        {
            var m = new MachineControls();
            m.Set(MachineControls.Aim, 4f);
            Assert.AreEqual(1f, m.AimValue, 0.001f);
            m.Set(MachineControls.Aim, -4f);
            Assert.AreEqual(-1f, m.AimValue, 0.001f);
        }
    }
}
