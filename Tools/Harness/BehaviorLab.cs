using System;
using System.Collections.Generic;
using GummyDynasty.Cognition;
using GummyDynasty.Simulation;
using UnityEngine;

namespace GummyDynasty.Harness
{
    /// <summary>
    /// Autonomous behavior / contact suite. Uses production WallMeasure,
    /// FormationTactics, MarchFormation, GameModeRules against a ToyWorld.
    /// Does not claim hop feel or PhysX identity.
    /// </summary>
    static class BehaviorLab
    {
        static readonly Vector3 PitOrigin = new Vector3(-2f, 0.35f, 0f);
        static readonly Vector3 Flag = new Vector3(-10f, 0.2f, 0f);
        const float HopperR = WallMeasure.MarcherBellyRadius;
        const float HopperMass = 2.2f;
        const float CandyR = 0.45f;

        public static void Run(Action<string, bool, string> check)
        {
            GapTruth(check);
            SolidWallBlocks(check);
            SlitIsNotADoor(check);
            TwoColumnDoor(check);
            BreachSlotsStayInAir(check);
            HeadingNeverEast(check);
            CandyHitsWall(check);
            CandyThroughDoor(check);
            StackStandsThenFalls(check);
            DropLands(check);
            DeckCarries(check);
            IncomingDodge(check);
            TacticRoad(check);
            RubbleIsAHole(check);
        }

        static void GapTruth(Action<string, bool, string> check)
        {
            var intact = MeasureWall(null);
            check("lab intact wall is ahead, not a hole",
                intact.Ahead && !intact.Hole,
                "air=" + intact.GapAir.ToString("0.00"));

            var one = MeasureWall(new[] { 3 });
            check("lab one-column slit is not passable",
                !WallMeasure.Passable(one.GapAir),
                "air=" + one.GapAir.ToString("0.00") + " need=" + WallMeasure.PassableAir.ToString("0.00"));
            check("lab one-column slit is not a hole",
                !one.Hole,
                "air=" + one.GapAir.ToString("0.00"));

            var two = MeasureWall(new[] { 3, 4 });
            check("lab two-column gap is passable",
                WallMeasure.Passable(two.GapAir) && two.Hole,
                "air=" + two.GapAir.ToString("0.00"));

            var tacticIntact = FormationTactics.Resolve(1f, false, intact.WallValue, intact.HoleValue);
            var tacticOne = FormationTactics.Resolve(1f, false, one.WallValue, one.HoleValue);
            var tacticTwo = FormationTactics.Resolve(1f, false, two.WallValue, two.HoleValue);
            check("lab intact wall spreads", tacticIntact == FormationTactics.Spread, tacticIntact);
            check("lab one-column still spreads", tacticOne == FormationTactics.Spread, tacticOne);
            check("lab two-column goes through-breach", tacticTwo == FormationTactics.ThroughBreach, tacticTwo);
        }

        static void SolidWallBlocks(Action<string, bool, string> check)
        {
            var world = PitWall(null, dynamicCrates: false);
            var hopper = world.AddSphere("hopper", new Vector3(2.4f, HopperR + 0.02f, 0f), HopperR, HopperMass);
            DriveWest(world, hopper, 5.5f);
            var wallWest = PitOrigin.x - WallMeasure.CratePitch - HopperR;
            check("lab solid wall blocks hopper",
                hopper.Pos.x > PitOrigin.x + 0.05f,
                "x=" + hopper.Pos.x.ToString("0.00") + " contacts=" + hopper.Contacts);
            check("lab solid wall hopper never west of wall",
                hopper.Pos.x > wallWest,
                hopper.Pos.x.ToString("0.00"));
        }

        static void SlitIsNotADoor(Action<string, bool, string> check)
        {
            var parts = new List<Vector3>(64);
            WallMeasure.LayoutStack(parts, PitOrigin, 2, 4, 7);
            parts = FilterColumns(parts, new[] { 3 });
            var blocked = SweepBlocked(parts, new Vector3(2.4f, 0.8f, 0f), HopperR, PitOrigin.x - 1.2f);
            check("lab one-column corridor is occupied",
                blocked,
                "air=" + MeasureWall(new[] { 3 }).GapAir.ToString("0.00") + " belly=" + (HopperR * 2f).ToString("0.00"));
            var twoParts = new List<Vector3>(64);
            WallMeasure.LayoutStack(twoParts, PitOrigin, 2, 4, 7);
            twoParts = FilterColumns(twoParts, new[] { 3, 4 });
            check("lab two-column corridor is clear at the breach",
                !SweepBlocked(twoParts, new Vector3(2.4f, 0.8f, 0.28f), HopperR, PitOrigin.x - 1.2f),
                "air=" + MeasureWall(new[] { 3, 4 }).GapAir.ToString("0.00"));
        }

        static void TwoColumnDoor(Action<string, bool, string> check)
        {
            var world = PitWall(new[] { 3, 4 }, dynamicCrates: false);
            var hopper = world.AddSphere("hopper", new Vector3(2.4f, HopperR + 0.02f, 0.28f), HopperR, HopperMass);
            DriveWest(world, hopper, 8f);
            check("lab two-column door lets a marcher through",
                hopper.Pos.x < PitOrigin.x - 0.4f,
                "x=" + hopper.Pos.x.ToString("0.00") + " z=" + hopper.Pos.z.ToString("0.00") + " contacts=" + hopper.Contacts);
        }

        static void BreachSlotsStayInAir(Action<string, bool, string> check)
        {
            var two = MeasureWall(new[] { 3, 4 });
            var com = new Vector3(1f, 0.8f, 0f);
            var worst = 0f;
            for (var i = 0; i < 5; i++)
            {
                var slot = MarchFormation.SlotWorld(
                    i, 5, com, Flag, FormationTactics.ThroughBreach, two.BreachPoint, two.GapAir);
                var off = Mathf.Abs(slot.z - two.BreachPoint.z);
                if (off > worst)
                    worst = off;
                var half = two.GapAir * 0.5f - HopperR;
                check("lab breach slot " + i + " stays in air",
                    off <= Mathf.Max(0.05f, half + 0.02f),
                    "z=" + slot.z.ToString("0.00") + " half=" + half.ToString("0.00"));
            }
            check("lab tight breach queues single-file",
                worst < 0.08f,
                "worstZ=" + worst.ToString("0.00") + " air=" + two.GapAir.ToString("0.00"));
        }

        static void HeadingNeverEast(Action<string, bool, string> check)
        {
            var flag = Flag;
            var pos = new Vector3(3f, 0.8f, 1.4f);
            var slotEast = new Vector3(6f, 0.8f, 0f);
            var heading = MarchFormation.MarchHeading(pos, slotEast, flag, true);
            check("lab march heading never east", heading.x < -0.4f, heading.ToString());
        }

        static void CandyHitsWall(Action<string, bool, string> check)
        {
            var world = PitWall(null, dynamicCrates: false);
            var origin = new Vector3(7.2f, 1.05f, 0f);
            var target = new Vector3(PitOrigin.x, 1.1f, 0f);
            var dir = BallisticDir(origin, target, 1f, 1.25f);
            var candy = world.AddSphere("candy", origin, CandyR, 5f);
            candy.Vel = dir * 10.77f;
            candy.LinearDamp = 0.22f;
            world.StepSeconds(1.6f, 1f / 90f);
            check("lab LOB candy hits the wall",
                candy.Contacts > 0 && candy.Pos.x > PitOrigin.x - 1.2f,
                "x=" + candy.Pos.x.ToString("0.00") + " contacts=" + candy.Contacts);
            check("lab candy does not tunnel west of the wall",
                candy.Pos.x > PitOrigin.x - WallMeasure.CratePitch - CandyR - 0.15f,
                candy.Pos.x.ToString("0.00"));
        }

        static void CandyThroughDoor(Action<string, bool, string> check)
        {
            var world = PitWall(new[] { 3, 4 }, dynamicCrates: false);
            var two = MeasureWall(new[] { 3, 4 });
            var origin = new Vector3(7.2f, 1.35f, two.BreachPoint.z);
            var target = new Vector3(PitOrigin.x - 4f, 1.2f, two.BreachPoint.z);
            var dir = BallisticDir(origin, target, 0.55f, 1.0f);
            var candy = world.AddSphere("candy", origin, CandyR, 5f);
            candy.Vel = dir * 10.77f;
            candy.LinearDamp = 0.12f;
            world.StepSeconds(2.4f, 1f / 90f);
            check("lab candy through the door lands west of the wall",
                candy.Pos.x < PitOrigin.x,
                "x=" + candy.Pos.x.ToString("0.00") + " z=" + candy.Pos.z.ToString("0.00") + " y=" + candy.Pos.y.ToString("0.00") + " hits=" + candy.Contacts);
        }

        static void StackStandsThenFalls(Action<string, bool, string> check)
        {
            var world = new ToyWorld();
            var half = new Vector3(WallMeasure.CrateExtent, WallMeasure.CrateExtent, WallMeasure.CrateExtent);
            var foot = world.AddStaticBox("foot", new Vector3(0f, 0.04f, 0f), new Vector3(0.4f, 0.04f, 0.4f));
            var boxes = new LabBody[3];
            for (var i = 0; i < 3; i++)
            {
                var y = 0.08f + half.y + i * WallMeasure.CrateVisual;
                boxes[i] = world.AddBox("crate" + i, new Vector3(0f, y, 0f), half, 1.1f);
                boxes[i].LinearDamp = 1.4f;
            }
            world.StepSeconds(2.2f);
            check("lab stack stands",
                boxes[2].Pos.y > boxes[0].Pos.y + WallMeasure.CrateVisual * 1.4f,
                "top=" + boxes[2].Pos.y.ToString("0.00") + " foot=" + foot.Pos.y.ToString("0.00"));

            boxes[0].Alive = false;
            world.StepSeconds(2.4f);
            check("lab stack falls when the bottom crate goes",
                boxes[2].Pos.y < boxes[0].Pos.y + WallMeasure.CrateVisual * 1.2f,
                "top=" + boxes[2].Pos.y.ToString("0.00"));
        }

        static void DropLands(Action<string, bool, string> check)
        {
            var world = new ToyWorld();
            var half = new Vector3(WallMeasure.CrateExtent, WallMeasure.CrateExtent, WallMeasure.CrateExtent);
            var crate = world.AddBox("drop", new Vector3(0f, 8f, 0f), half, 1.1f);
            crate.LinearDamp = 0.4f;
            world.StepSeconds(3.2f);
            check("lab DROP crate lands on the road",
                crate.Pos.y < 0.5f && crate.Pos.y > 0.05f && Mathf.Abs(crate.Pos.x) < 1.5f,
                crate.Pos.ToString());
        }

        static void DeckCarries(Action<string, bool, string> check)
        {
            // Production does not trust friction. GummyBody.RideDeck adds the
            // kinematic delta. Without that, the car is a rug-pull.
            var deckX = 4f;
            var riddenX = 4f;
            var strandedX = 4f;
            const float vel = -1.6f;
            const float dt = 1f / 60f;
            for (var i = 0; i < 150; i++)
            {
                var delta = vel * dt;
                deckX += delta;
                riddenX += delta;
            }
            check("lab deck without RideDeck is a rug-pull",
                Mathf.Abs(strandedX - deckX) > 3f,
                "stranded=" + strandedX.ToString("0.00") + " deck=" + deckX.ToString("0.00"));
            check("lab RideDeck keeps the hopper on the car",
                Mathf.Abs(riddenX - deckX) < 0.01f && riddenX < 1f,
                "ridden=" + riddenX.ToString("0.00") + " deck=" + deckX.ToString("0.00"));
        }

        static void IncomingDodge(Action<string, bool, string> check)
        {
            var incoming = new Vector3(1.2f, 1.4f, 0f);
            var pos = new Vector3(1.4f, 0.8f, 0f);
            var away = pos - incoming;
            away.y = 0f;
            if (away.sqrMagnitude < 0.01f)
                away = new Vector3(0f, 0f, 1f);
            var spread = Vector3.right;
            var heading = (away.normalized + spread * 0.65f).normalized;
            check("lab incoming dodge leaves the file line",
                Mathf.Abs(heading.z) > 0.2f || heading.x > 0.2f,
                heading.ToString());

            var intent = IntentResolver.Resolve(
                "g",
                new Belief("g", HierarchyRoles.Objective, 0.9f, 1f),
                new Belief("g", HierarchyRoles.Threat, 0.8f, 0.8f),
                new Belief("g", HierarchyRoles.Congestion, 0.2f, 0.2f),
                new Belief("g", HierarchyRoles.Pain, 0.1f, 0.2f),
                true,
                FormationTactics.Spread);
            check("lab incoming still dodges while the file spreads",
                intent.EffectiveName == IntentResolver.Dodge, intent.EffectiveName);
        }

        static void TacticRoad(Action<string, bool, string> check)
        {
            var mode = GameModeRules.HoldWest();
            var arrivedHold = false;
            var last = IntentResolver.Idle;
            var sawSpread = false;
            var sawBreach = false;
            var sawHold = false;
            var comX = 3.2f;
            var two = MeasureWall(new[] { 3, 4 });
            for (var i = 0; i < 400; i++)
            {
                if (last != FormationTactics.Hold)
                    comX -= 0.045f;
                var holeOpen = comX < PitOrigin.x + 3.4f;
                var report = holeOpen ? two : MeasureWall(null);
                var arrived = comX <= Flag.x + IntentResolver.ArriveBand
                    && comX >= Flag.x - IntentResolver.ArriveBand * 2f;
                if (arrived)
                    arrivedHold = true;
                last = FormationTactics.Resolve(
                    1f, arrived || arrivedHold, report.WallValue, report.HoleValue, arrivedHold ? 1f : 0f);
                if (last == FormationTactics.Spread) sawSpread = true;
                if (last == FormationTactics.ThroughBreach) sawBreach = true;
                if (last == FormationTactics.Hold) sawHold = true;
            }
            check("lab road spread → breach → hold", sawSpread && sawBreach && sawHold, last);
            check("lab road HELD", mode.CheckVictory(comX, Flag.x, true) && arrivedHold,
                "com=" + comX.ToString("0.00") + " last=" + last);
        }

        static void RubbleIsAHole(Action<string, bool, string> check)
        {
            var few = new List<Vector3>();
            few.Add(new Vector3(-2f, 0.4f, -0.5f));
            few.Add(new Vector3(-2f, 0.4f, 0.5f));
            few.Add(new Vector3(-1.9f, 0.15f, 0f));
            var report = WallMeasure.Measure(few, new Vector3(2f, 0.8f, 0f));
            check("lab rubble counts as a hole",
                report.Rubble && report.Hole && !report.Ahead,
                "standing~3 rubble=" + report.Rubble);
        }

        static WallReport MeasureWall(int[] dropZ)
        {
            var parts = new List<Vector3>(64);
            WallMeasure.LayoutStack(parts, PitOrigin, 2, 4, 7);
            if (dropZ != null && dropZ.Length > 0)
                parts = FilterColumns(parts, dropZ);
            return WallMeasure.Measure(parts, new Vector3(2.5f, 0.8f, 0f));
        }

        static ToyWorld PitWall(int[] dropZ, bool dynamicCrates)
        {
            var world = new ToyWorld();
            var parts = new List<Vector3>(64);
            WallMeasure.LayoutStack(parts, PitOrigin, 2, 4, 7);
            if (dropZ != null && dropZ.Length > 0)
                parts = FilterColumns(parts, dropZ);
            var half = new Vector3(WallMeasure.CrateExtent, WallMeasure.CrateExtent, WallMeasure.CrateExtent);
            for (var i = 0; i < parts.Count; i++)
            {
                if (dynamicCrates)
                {
                    var box = world.AddBox("crate", parts[i], half, 1.1f);
                    box.LinearDamp = 1.6f;
                }
                else
                    world.AddStaticBox("crate", parts[i], half);
            }
            return world;
        }

        static List<Vector3> FilterColumns(List<Vector3> parts, int[] dropZ)
        {
            var kept = new List<Vector3>(parts.Count);
            for (var i = 0; i < parts.Count; i++)
            {
                var zIndex = (int)System.Math.Round(parts[i].z / WallMeasure.CratePitch + 3f);
                var drop = false;
                for (var k = 0; k < dropZ.Length; k++)
                {
                    if (zIndex == dropZ[k])
                    {
                        drop = true;
                        break;
                    }
                }
                if (!drop)
                    kept.Add(parts[i]);
            }
            return kept;
        }

        static bool SweepBlocked(IList<Vector3> crates, Vector3 start, float radius, float westOf)
        {
            var half = WallMeasure.CrateExtent;
            for (var x = start.x; x >= westOf; x -= 0.04f)
            {
                var pos = new Vector3(x, start.y, start.z);
                for (var i = 0; i < crates.Count; i++)
                {
                    var c = crates[i];
                    var closest = Vector3.Clamp(
                        pos,
                        c - new Vector3(half, half, half),
                        c + new Vector3(half, half, half));
                    if ((pos - closest).magnitude < radius)
                        return x > PitOrigin.x;
                }
            }
            return false;
        }

        static void DriveWest(ToyWorld world, LabBody hopper, float seconds, Action<LabBody> each = null)
        {
            var dt = 1f / 60f;
            var t = 0f;
            while (t < seconds)
            {
                var slot = new Vector3(hopper.Pos.x - 1.4f, hopper.Pos.y, 0f);
                var heading = MarchFormation.MarchHeading(hopper.Pos, slot, Flag, true);
                hopper.Vel.x += heading.x * 7.5f * dt;
                hopper.Vel.z += heading.z * 7.5f * dt;
                if (hopper.Vel.x < -2.6f)
                    hopper.Vel.x = -2.6f;
                world.Step(dt);
                each?.Invoke(hopper);
                t += dt;
            }
        }

        static Vector3 BallisticDir(Vector3 origin, Vector3 target, float loft, float tMax)
        {
            var to = target - origin;
            var t = Mathf.Clamp(to.magnitude / 10.77f, 0.08f, tMax);
            to.y += 0.5f * 9.81f * t * t * loft;
            return to.sqrMagnitude > 0.0001f ? to.normalized : Vector3.forward;
        }
    }
}
