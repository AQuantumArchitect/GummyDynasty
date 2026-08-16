using GummyDynasty.Simulation;
using NUnit.Framework;

namespace GummyDynasty.Tests.EditMode
{
    public sealed class SessionStateTests
    {
        [Test]
        public void Start_SetsRunningAndResetsClock()
        {
            var state = new SessionState();
            state.Advance(1.5f);
            state.Start();
            Assert.AreEqual(SessionPhase.Running, state.Phase);
            Assert.AreEqual(0f, state.ElapsedSeconds);
            Assert.AreEqual(0, state.Tick);
        }

        [Test]
        public void Advance_OnlyWhileRunning()
        {
            var state = new SessionState();
            state.Advance(1f);
            Assert.AreEqual(0, state.Tick);

            state.Start();
            state.Advance(0.25f);
            Assert.AreEqual(1, state.Tick);
            Assert.AreEqual(0.25f, state.ElapsedSeconds, 0.0001f);

            state.Pause();
            state.Advance(1f);
            Assert.AreEqual(1, state.Tick);
        }
    }
}
