using System.Collections.Generic;
using GummyDynasty.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace GummyDynasty.Tests.EditMode
{
    public sealed class CandyShotFeelTests
    {
        SimulationMode _prev;
        readonly List<GameObject> _spawned = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            _prev = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
        }

        [TearDown]
        public void TearDown()
        {
            for (var i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null)
                    Object.DestroyImmediate(_spawned[i]);
            }
            _spawned.Clear();
            Physics.simulationMode = _prev;
        }

        [Test]
        public void CandyEnergy_SplitsFoamAndBowling()
        {
            Assert.AreEqual(CandyShot.MidpointKe, CandyShot.KineticEnergy, 20f);
            Assert.Greater(CandyShot.Mass, 4f, "catapult stone, not a foam dart");
            Assert.Greater(CandyShot.Speed, 9.5f, "has to actually reach the pile");
            Assert.Less(CandyShot.Speed, 14f, "not the old 18 m/s bowling ball");
            Assert.Less(CandyShot.KineticEnergy, ImpliedSmashWallEnergy(), "still under the X-button wall");
            Assert.Greater(CandyShot.KineticEnergy, CandyShot.FoamKe * 5f);
            Assert.Less(CandyShot.KineticEnergy, CandyShot.BowlingKe * 0.7f);
        }

        [Test]
        public void CannonArc_ReachesThePitWall_ButStaysFlatterThanLob()
        {
            var origin = new Vector3(7.2f, 1.05f, -1.4f);
            var gun = new Vector3(7.2f, 0.85f, 2.2f);
            var wall = new Vector3(-2f, 1.1f, 0f);
            var lob = Fly(origin, CandyShot.BallisticDirection(origin, wall));
            var cannon = Fly(gun, CandyShot.DirectDirection(gun, wall));
            Assert.Less(lob.x, 0.5f, "LOB should plant on the wall, not the east grass. land=" + lob.x);
            Assert.Less(cannon.x, 0.5f, "cannon must still reach the road. land=" + cannon.x);
            Assert.Greater(cannon.x, lob.x + 0.1f, "cannon stays flatter / shorter than the catapult");
        }

        static Vector3 Fly(Vector3 origin, Vector3 dir)
        {
            var pos = origin;
            var vel = dir * CandyShot.Speed;
            const float dt = 0.02f;
            for (var i = 0; i < 180; i++)
            {
                vel.y -= 9.81f * dt;
                vel *= 1f / (1f + 0.22f * dt);
                pos += vel * dt;
                if (pos.y <= 0.4f && i > 4)
                    return pos;
            }

            return pos;
        }

        [Test]
        public void Hop_IsBouncingBallNotAWave()
        {
            Assert.Greater(GummyBody.HopUp, 1.6f);
            Assert.Greater(GummyBody.HopForward, 1f);
            Assert.Less(GummyBody.HopHz, 1.8f, "peppy bounce, not a scramble");
            Assert.Greater(GummyBody.HopHz, 1.1f);
        }

        [Test]
        public void Hop_MovesUprightAxisWest()
        {
            KinematicBox(new Vector3(0f, -0.25f, 0f), new Vector3(24f, 0.5f, 24f), GroundMat());
            var personality = PhysicalPersonality.CreateRuntime("Hopper", new Color(0.9f, 0.2f, 0.3f), 2.8f, 1.85f, 1f, 6f);
            personality.WalkForce = 18f;
            var gummy = GummyFactory.Spawn(personality, new Vector3(2.2f, 1.5f, 0f), Quaternion.identity);
            _spawned.Add(gummy.gameObject);

            for (var i = 0; i < 70; i++)
            {
                gummy.TickMotors(0.02f);
                Physics.Simulate(0.02f);
            }

            var start = gummy.Position;
            var startAxis = AxisLength(gummy);
            for (var i = 0; i < 180; i++)
            {
                gummy.Locomote(Vector3.left, 1f);
                gummy.TickMotors(0.02f);
                Physics.Simulate(0.02f);
            }

            var moved = start.x - gummy.Position.x;
            Assert.Greater(moved, 0.9f, "should hop west. moved=" + moved);
            Assert.Greater(gummy.Head.position.y, gummy.Root.position.y + 0.22f, "head must stay above hips — not a folded caterpillar");
            Assert.AreEqual(startAxis, AxisLength(gummy), 0.55f, "axis length should hold; a worm accordions");
        }

        static float AxisLength(GummyBody gummy)
        {
            if (gummy.Head == null || gummy.Root == null)
                return 0f;
            return (gummy.Head.position - gummy.Root.position).magnitude;
        }

        [Test]
        public void CandyShot_IntoSpherePile_StopsSliding()
        {
            KinematicBox(new Vector3(0f, -0.25f, 0f), new Vector3(24f, 0.5f, 24f), GroundMat());
            var jelly = JellyMat();
            var bodies = new List<Rigidbody>(8);
            for (var i = 0; i < 8; i++)
            {
                var pos = new Vector3((i % 4) * 0.55f - 0.8f, 0.7f, (i / 4) * 0.55f);
                bodies.Add(JellyBall(pos, jelly));
            }

            for (var i = 0; i < 50; i++)
                Physics.Simulate(0.02f);

            var shot = CandyShot.Spawn(new Vector3(-2.8f, 0.85f, 0.2f), new Vector3(1f, 0.06f, 0f), null);
            _spawned.Add(shot);

            for (var i = 0; i < 35; i++)
                Physics.Simulate(0.02f);
            var peak = MeanHorizontalSpeed(bodies);

            for (var i = 0; i < 90; i++)
                Physics.Simulate(0.02f);
            var settled = MeanHorizontalSpeed(bodies);

            Assert.Less(peak, 12f, "catapult may punch, not scatter the pit. peak=" + peak);
            Assert.Less(settled, 1.2f, "pile must stop sliding after the stone plants. settled=" + settled);
        }

        [Test]
        public void CandyShot_IntoGummyPile_StopsSliding()
        {
            KinematicBox(new Vector3(0f, -0.25f, 0f), new Vector3(24f, 0.5f, 24f), GroundMat());
            var personality = PhysicalPersonality.CreateRuntime("Pile", new Color(0.9f, 0.2f, 0.3f), 2.8f, 1.6f, 1f, 6f);
            var bodies = new List<Rigidbody>(24);
            for (var i = 0; i < 6; i++)
            {
                var pos = new Vector3((i % 3) * 0.95f - 0.95f, 1.2f, (i / 3) * 0.95f);
                var gummy = GummyFactory.Spawn(personality, pos, Quaternion.identity);
                _spawned.Add(gummy.gameObject);
                CollectBodies(gummy, bodies);
            }

            for (var i = 0; i < 80; i++)
                Physics.Simulate(0.02f);

            var shot = CandyShot.Spawn(new Vector3(-3.2f, 1.25f, 0.4f), new Vector3(1f, 0.05f, 0f), null);
            _spawned.Add(shot);

            for (var i = 0; i < 40; i++)
                Physics.Simulate(0.02f);
            var peak = MeanHorizontalSpeed(bodies);

            for (var i = 0; i < 100; i++)
                Physics.Simulate(0.02f);
            var settled = MeanHorizontalSpeed(bodies);

            Assert.Less(peak, 14f, "gummy pile knock must stay below bowling-strike. peak=" + peak);
            Assert.Less(settled, 1.4f, "gummy pile must stop sliding. settled=" + settled);
        }

        static float ImpliedSmashWallEnergy()
        {
            const float crateMass = 1.1f;
            var dv = CandyShot.SmashImpulsePerCrate / crateMass;
            return CandyShot.SmashCrateCount * 0.5f * crateMass * dv * dv;
        }

        static float MeanHorizontalSpeed(List<Rigidbody> bodies)
        {
            var acc = 0f;
            var n = 0;
            for (var i = 0; i < bodies.Count; i++)
            {
                if (bodies[i] == null) continue;
                var v = bodies[i].linearVelocity;
                acc += new Vector2(v.x, v.z).magnitude;
                n++;
            }
            return n == 0 ? 0f : acc / n;
        }

        static void CollectBodies(GummyBody gummy, List<Rigidbody> into)
        {
            var parts = gummy.Parts;
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i] != null)
                    into.Add(parts[i]);
            }
        }

        Rigidbody JellyBall(Vector3 pos, PhysicsMaterial mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "JellyStandIn";
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.5f;
            go.GetComponent<Collider>().sharedMaterial = mat;
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.9f;
            rb.linearDamping = 0.6f;
            rb.angularDamping = 1.4f;
            _spawned.Add(go);
            return rb;
        }

        static PhysicsMaterial JellyMat()
        {
            return new PhysicsMaterial("TestJelly")
            {
                bounciness = 0.22f,
                dynamicFriction = 0.72f,
                staticFriction = 0.85f,
                bounceCombine = PhysicsMaterialCombine.Average,
                frictionCombine = PhysicsMaterialCombine.Average
            };
        }

        GameObject KinematicBox(Vector3 pos, Vector3 scale, PhysicsMaterial mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "TestGround";
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Collider>().sharedMaterial = mat;
            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            _spawned.Add(go);
            return go;
        }

        static PhysicsMaterial GroundMat()
        {
            return new PhysicsMaterial("TestGround")
            {
                bounciness = 0.04f,
                dynamicFriction = 0.9f,
                staticFriction = 0.98f,
                bounceCombine = PhysicsMaterialCombine.Minimum,
                frictionCombine = PhysicsMaterialCombine.Maximum
            };
        }
    }
}
