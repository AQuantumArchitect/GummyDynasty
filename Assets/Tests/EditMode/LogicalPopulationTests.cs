using System.Diagnostics;
using GummyDynasty.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace GummyDynasty.Tests.EditMode
{
    public sealed class LogicalPopulationTests
    {
        [Test]
        public void Seed1000_HasNoGameObjects()
        {
            var pop = new LogicalPopulation();
            pop.SeedBlock(1000, new Vector3(8f, 0.9f, 0f));
            Assert.AreEqual(1000, pop.Count);
            Assert.AreEqual(1000, pop.DisembodiedCount);
            Assert.AreEqual(0, pop.EmbodiedCount);
        }

        [Test]
        public void Tick_MarchesWest()
        {
            var pop = new LogicalPopulation();
            pop.SeedBlock(80, new Vector3(8f, 0.9f, 0f));
            pop.Marching = true;
            var start = pop.CenterOfMass();
            var flag = new Vector3(-10f, 0.2f, 0f);
            var incoming = new Vector3(80f, 4f, 0f);
            for (var i = 0; i < 180; i++)
                pop.Tick(0.05f, flag, incoming);
            var end = pop.CenterOfMass();
            Assert.Less(end.x, start.x - 4f, "army should flow west. start=" + start.x + " end=" + end.x);
            Assert.GreaterOrEqual(end.x, flag.x - 0.2f, "must not slide through the west rail. end=" + end.x);
            Assert.AreEqual(80, pop.Count);
        }

        [Test]
        public void Incoming_DodgesInsteadOfMarching()
        {
            var pop = new LogicalPopulation();
            var id = pop.Spawn(new Vector3(0f, 0.9f, 0f));
            pop.Marching = true;
            pop.Tick(0.05f, new Vector3(-10f, 0f, 0f), new Vector3(0.2f, 0.9f, 0f));
            Assert.IsTrue(pop.TryGet(id, out var s));
            Assert.AreEqual(LogicalIntent.Dodge, s.Intent);
        }

        [Test]
        public void Json_RoundTripPreservesIdentityAndIntent()
        {
            var pop = new LogicalPopulation();
            pop.Marching = true;
            var id = pop.Spawn(new Vector3(3.5f, 0.9f, -1.2f));
            pop.WriteBack(id, new Vector3(2f, 0.9f, -1.2f), Vector3.left, 0.2f, LogicalIntent.MarchWest);
            var json = pop.ToJson();
            var clone = new LogicalPopulation();
            clone.LoadJson(json);
            Assert.AreEqual(1, clone.Count);
            Assert.IsTrue(clone.TryGet(id, out var s));
            Assert.AreEqual(2f, s.Position.x, 0.001f);
            Assert.AreEqual(LogicalIntent.MarchWest, s.Intent);
            Assert.IsFalse(s.Embodied);
            Assert.GreaterOrEqual(clone.NextId, id + 1);
        }

        [Test]
        public void Blob_RoundTripPreservesArmy()
        {
            var pop = new LogicalPopulation();
            pop.SeedBlock(64, new Vector3(6f, 0.9f, 0f));
            pop.Marching = true;
            pop.Tick(0.2f, new Vector3(-10f, 0f, 0f), new Vector3(80f, 0f, 0f));
            var blob = pop.ToBlob();
            Assert.Greater(blob.Length, 64);
            var clone = new LogicalPopulation();
            clone.LoadBlob(blob);
            Assert.AreEqual(64, clone.Count);
            Assert.AreEqual(pop.CenterOfMass().x, clone.CenterOfMass().x, 0.001f);
            Assert.IsTrue(clone.Marching);
        }

        [Test]
        public void EmbodyFlag_DoesNotTickThatSoldier()
        {
            var pop = new LogicalPopulation();
            var id = pop.Spawn(new Vector3(4f, 0.9f, 0f));
            pop.SetEmbodied(id, true);
            pop.Marching = true;
            pop.Tick(1f, new Vector3(-10f, 0f, 0f), new Vector3(80f, 0f, 0f));
            Assert.IsTrue(pop.TryGet(id, out var s));
            Assert.AreEqual(4f, s.Position.x, 0.001f);
            Assert.AreEqual(1, pop.EmbodiedCount);
        }

        [Test]
        public void WriteBack_KeepsId()
        {
            var pop = new LogicalPopulation();
            var id = pop.Spawn(Vector3.zero);
            pop.SetEmbodied(id, true);
            pop.WriteBack(id, new Vector3(-2f, 1f, 0.4f), Vector3.right, 0.4f, LogicalIntent.Down);
            Assert.IsTrue(pop.TryGet(id, out var s));
            Assert.AreEqual(id, s.Id);
            Assert.AreEqual(-2f, s.Position.x, 0.001f);
            Assert.AreEqual(LogicalIntent.Down, s.Intent);
            Assert.AreEqual(0.4f, s.Pain, 0.001f);
        }

        [Test]
        public void Clear_ResetsCount()
        {
            var pop = new LogicalPopulation();
            pop.SeedBlock(12, Vector3.zero);
            pop.Clear();
            Assert.AreEqual(0, pop.Count);
            Assert.AreEqual(0, pop.EmbodiedCount);
        }

        [Test]
        public void LogicalTick_BeatsNaiveTransforms()
        {
            const int n = 1000;
            const int frames = 200;
            var flag = new Vector3(-10f, 0f, 0f);
            var incoming = new Vector3(80f, 0f, 0f);

            var pop = new LogicalPopulation();
            pop.SeedBlock(n, new Vector3(8f, 0.9f, 0f));
            pop.Marching = true;
            var logicalWatch = Stopwatch.StartNew();
            for (var i = 0; i < frames; i++)
                pop.Tick(0.02f, flag, incoming);
            logicalWatch.Stop();

            var gos = new GameObject[n];
            for (var i = 0; i < n; i++)
            {
                gos[i] = new GameObject("Naive");
                gos[i].transform.position = new Vector3(8f + (i % 20) * 0.55f, 0.9f, (i / 20) * 0.55f);
            }
            var goWatch = Stopwatch.StartNew();
            for (var f = 0; f < frames; f++)
            {
                for (var i = 0; i < n; i++)
                {
                    var t = gos[i].transform;
                    var wish = flag - t.position;
                    wish.y = 0f;
                    if (wish.sqrMagnitude > 0.04f)
                        t.position += wish.normalized * (LogicalPopulation.MarchSpeed * 0.02f);
                }
            }
            goWatch.Stop();

            for (var i = 0; i < n; i++)
                Object.DestroyImmediate(gos[i]);

            Assert.Less(logicalWatch.Elapsed.TotalMilliseconds, goWatch.Elapsed.TotalMilliseconds,
                "logical SoA must beat one-Transform-each. logical=" + logicalWatch.Elapsed.TotalMilliseconds +
                " go=" + goWatch.Elapsed.TotalMilliseconds);
        }
    }
}
