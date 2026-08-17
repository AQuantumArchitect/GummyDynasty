using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using GummyDynasty.Cognition;
using GummyDynasty.Core;
using GummyDynasty.Simulation;
using UnityEngine;

namespace GummyDynasty.Harness
{
    static class Program
    {
        static int _failed;
        static readonly StringBuilder Log = new StringBuilder(4096);

        static int Main()
        {
            Line("GummyDynasty harness  (no Unity license)");
            Line("host  " + Environment.MachineName + "  " + Environment.OSVersion);
            CandyBudget();
            CognitionSmoke();
            HistoryIntent();
            FormationRoad();
            MachineSmoke();
            PhoneSmoke();
            PlayMarchRoad();
            PlayArtilleryArc();
            PlayPhoneHttp();
            BehaviorLab.Run(Check);
            LogicalArmy();
            LogicalBench(1000, 300);
            LogicalBench(3000, 200);

            var root = FindRepo();
            var logs = Path.Combine(root, "Logs");
            Directory.CreateDirectory(logs);
            var report = Path.Combine(logs, "harness-report.md");
            File.WriteAllText(report, Log.ToString());
            Line("wrote  " + report);
            if (_failed > 0)
            {
                Line("FAILED  " + _failed);
                return 1;
            }
            Line("OK");
            return 0;
        }

        static void CandyBudget()
        {
            const float mass = 5f;
            const float speed = 10.77f;
            const float foam = 0.5f * 0.92f * 8f * 8f;
            const float bowl = 0.5f * 3.4f * 18f * 18f;
            var ke = 0.5f * mass * speed * speed;
            var mid = (foam + bowl) * 0.5f;
            Check("candy KE ~ midpoint", Math.Abs(ke - mid) < 20f, ke + " vs " + mid);
            Check("candy under smash wall", ke < 626f, ke.ToString("0.0"));
        }

        static void CognitionSmoke()
        {
            var field = new BeliefField();
            field.AddNode("a", kind: NodeKind.Actor);
            field.EnsureRole("a", "pain");
            field.Observe(new Observation("a", "pain", 1f, 0f));
            Check("eta 0 is no-op", Math.Abs(field.Get("a", "pain").Value - 0.5f) < 0.0001f, "");

            BattleHierarchy.Install(field);
            BattleHierarchy.AttachActor(field, "gummy-1");
            BattleHierarchy.IssueWestOrder(field);
            var before = field.GetLocal("gummy-1", HierarchyRoles.Objective).Value;
            field.Propagate(0.5f);
            var after = field.Get("gummy-1", HierarchyRoles.Objective).Value;
            Check("inherit pulls toward west order", after > before && after > 0.7f, after.ToString("0.00"));

            var intent = IntentResolver.Resolve(
                "gummy-1",
                field.Get("gummy-1", HierarchyRoles.Objective),
                field.Get("gummy-1", HierarchyRoles.Threat),
                field.Get("gummy-1", HierarchyRoles.Congestion),
                field.Get("gummy-1", HierarchyRoles.Pain),
                true);
            Check("shadow-first march", intent.EffectiveName == IntentResolver.MarchWest, intent.EffectiveName);

            field.SetOverride("gummy-1", HierarchyRoles.Objective, 1f);
            var local = field.GetLocal("gummy-1", HierarchyRoles.Objective).Value;
            field.Propagate(0.5f);
            Check("override blocks inherit", Math.Abs(local - field.Get("gummy-1", HierarchyRoles.Objective).Value) < 0.02f, "");
        }

        static void HistoryIntent()
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
            Check("E2 flattened stays down", flat.EffectiveName == IntentResolver.Down, flat.EffectiveName);
            Check("E2 neighbor still marches", calm.EffectiveName == IntentResolver.MarchWest, calm.EffectiveName);
            Check("E2 mad at candy", flatMem.LastHitter == "candy", flatMem.LastHitter);
        }

        static void FormationRoad()
        {
            Check("E8 wall+no hole = spread",
                FormationTactics.Resolve(1f, false, 1f, 0f) == FormationTactics.Spread,
                FormationTactics.Resolve(1f, false, 1f, 0f));
            Check("E8 hole = through-breach",
                FormationTactics.Resolve(1f, false, 1f, 1f) == FormationTactics.ThroughBreach,
                FormationTactics.Resolve(1f, false, 1f, 1f));
            Check("E8 arrived = hold",
                FormationTactics.Resolve(1f, true, 1f, 0f) == FormationTactics.Hold,
                FormationTactics.Resolve(1f, true, 1f, 0f));

            var field = new BeliefField();
            BattleHierarchy.Install(field);
            BattleHierarchy.AttachActor(field, "flat");
            BattleHierarchy.AttachActor(field, "calm");
            BattleHierarchy.IssueWestOrder(field);
            BattleHierarchy.ObserveRoad(field, 1f, 0f);
            field.Propagate(0.2f);
            Check("E8 faction still west",
                field.Get(HierarchyIds.FactionRed, HierarchyRoles.Objective).Value > 0.85f,
                field.Get(HierarchyIds.FactionRed, HierarchyRoles.Objective).Value.ToString("0.00"));

            var incoming = IntentResolver.Resolve(
                "flat",
                field.Get("flat", HierarchyRoles.Objective),
                new Belief("flat", HierarchyRoles.Threat, 0.8f, 0.8f),
                field.GetLocal("flat", HierarchyRoles.Congestion),
                field.GetLocal("flat", HierarchyRoles.Pain),
                true,
                FormationTactics.Spread);
            var down = IntentResolver.Resolve(
                "flat",
                field.Get("flat", HierarchyRoles.Objective),
                field.GetLocal("flat", HierarchyRoles.Threat),
                field.GetLocal("flat", HierarchyRoles.Congestion),
                new Belief("flat", HierarchyRoles.Pain, 0.9f, 0.8f),
                false,
                FormationTactics.Spread);
            var calm = IntentResolver.Resolve(
                "calm",
                field.Get("calm", HierarchyRoles.Objective),
                field.GetLocal("calm", HierarchyRoles.Threat),
                new Belief("calm", HierarchyRoles.Congestion, 0.9f, 0.9f),
                field.GetLocal("calm", HierarchyRoles.Pain),
                true,
                FormationTactics.Spread);
            Check("E8 incoming still dodges", incoming.EffectiveName == IntentResolver.Dodge, incoming.EffectiveName);
            Check("E8 flattened still down", down.EffectiveName == IntentResolver.Down, down.EffectiveName);
            Check("E8 neighbor inherits spread", calm.EffectiveName == FormationTactics.Spread, calm.EffectiveName);
        }

        static void MachineSmoke()
        {
            var m = new MachineControls();
            Check("release without draw is no-op", !m.Pulse(MachineControls.Release), "");
            Check("load pulses", m.Pulse(MachineControls.Load), "");
            m.Set(MachineControls.Draw, 1f);
            m.Set(MachineControls.Aim, 0.25f);
            Check("aim held", Math.Abs(m.AimValue - 0.25f) < 0.001f, m.AimValue.ToString("0.00"));
            Check("release after draw fires", m.Pulse(MachineControls.Release), "");
            Check("draw resets on release", m.DrawValue < 0.01f, m.DrawValue.ToString("0.00"));
            Check("unload on release", !m.Loaded, "");
        }

        static void PhoneSmoke()
        {
            Check("E9 garbage rejected", !PhoneCommand.TryParse("nope", out _, out _), "");
            Check("E9 join commander",
                PhoneCommand.TryParse("{\"op\":\"join\",\"role\":\"commander\"}", out var join, out _)
                && join.Role == PhoneCommand.RoleCommander, "");

            var session = new PhoneSession();
            Check("E9 first commander", session.TryJoin("commander", out var c, out _), c);
            Check("E9 first artillery", session.TryJoin("artillery", out var a, out _), a);
            Check("E9 second commander blocked", !session.TryJoin("commander", out _, out var taken), taken);

            PhoneCommand.TryParse("cmd role=commander action=west player=" + c, out var west, out _);
            Check("E9 commander west ok", session.TryValidate(west, out _), "");
            PhoneCommand.TryParse("cmd role=commander action=fire player=" + c, out var fire, out var fireErr);
            Check("E9 commander cannot fire", fire == null, fireErr);
            PhoneCommand.TryParse(
                "{\"op\":\"cmd\",\"role\":\"artillery\",\"action\":\"fire\",\"player\":\"" + a + "\"}",
                out var arty, out _);
            Check("E9 artillery fire ok", session.TryValidate(arty, out _), "");

            var mode = GameModeRules.HoldWest();
            Check("E9 hold west wins", mode.CheckVictory(-10f, -10f, true), "");
            Check("E9 still marching is not a win", !mode.CheckVictory(3f, -10f, true), "");
            Check("E9 ordered hold beats wall",
                FormationTactics.Resolve(1f, false, 1f, 0f, 1f) == FormationTactics.Hold, "");

            var qr = QrEncode.Encode("http://192.168.1.10:8787/");
            Check("E9 qr square", qr != null && qr.GetLength(0) == qr.GetLength(1), qr != null ? qr.GetLength(0).ToString() : "null");
            Check("E9 qr finder", qr != null && qr[0, 0] && qr[6, 0] && qr[0, 6], "");
        }

        static void PlayMarchRoad()
        {
            var flag = new Vector3(-10f, 0.2f, 0f);
            const float wall = -2f;
            var comX = 3.2f;
            var hole = false;
            var arrivedHold = false;
            var sawSpread = false;
            var sawBreach = false;
            var sawHold = false;
            var won = false;
            var mode = GameModeRules.HoldWest();
            string last = IntentResolver.Idle;

            for (var i = 0; i < 500 && !won; i++)
            {
                comX -= 0.045f;
                if (!hole && comX < wall + 3.4f)
                    hole = true;
                var wallAhead = !hole && comX > wall + 0.4f ? 1f : 0f;
                var holeV = hole && comX > wall + 0.4f ? 1f : 0f;
                var arrived = comX <= flag.x + IntentResolver.ArriveBand
                    && comX >= flag.x - IntentResolver.ArriveBand * 2f;
                if (arrived)
                    arrivedHold = true;
                last = FormationTactics.Resolve(1f, arrived || arrivedHold, wallAhead, holeV, arrivedHold ? 1f : 0f);
                if (last == FormationTactics.Spread) sawSpread = true;
                if (last == FormationTactics.ThroughBreach) sawBreach = true;
                if (last == FormationTactics.Hold) sawHold = true;
                if (mode.CheckVictory(comX, flag.x, true))
                    won = true;
            }

            Check("play march spread at wall", sawSpread, last);
            Check("play march through-breach", sawBreach, last);
            Check("play march hold at WEST", sawHold, last);
            Check("play march HELD", won, "com=" + comX.ToString("0.00") + " last=" + last);
            Check("play hold geometry wins",
                mode.CheckVictory(flag.x + MarchFormation.HoldEast, flag.x, true),
                MarchFormation.HoldEast.ToString("0.00"));
            Check("play old 2.2 plant would miss",
                !mode.CheckVictory(flag.x + 2.2f, flag.x, true), "");
        }

        static void PlayArtilleryArc()
        {
            var origin = new Vector3(7.2f, 1.05f, -1.4f);
            var target = new Vector3(-2f, 1.1f, 0f);
            var land = SimulateCandy(origin, BallisticDir(origin, target, 1f, 1.25f), 10.77f);
            var err = Math.Abs(land.x - target.x);
            Check("play LOB lands near wall", err < 3.2f && land.x < 2f,
                "land=" + land.x.ToString("0.00") + " err=" + err.ToString("0.00"));

            var gun = new Vector3(7.2f, 0.85f, 2.2f);
            var flat = SimulateCandy(gun, BallisticDir(gun, target, 0.86f, 1.1f), 10.77f);
            Check("play CANNON reaches the road", flat.x < 0f,
                "land=" + flat.x.ToString("0.00"));
            Check("play CANNON still shorter than LOB", flat.x > land.x + 0.1f,
                "cannon=" + flat.x.ToString("0.00") + " lob=" + land.x.ToString("0.00"));
        }

        static Vector3 BallisticDir(Vector3 origin, Vector3 target, float loft, float tMax)
        {
            var to = target - origin;
            var t = Mathf.Clamp(to.magnitude / 10.77f, 0.08f, tMax);
            to.y += 0.5f * 9.81f * t * t * loft;
            return to.sqrMagnitude > 0.0001f ? to.normalized : Vector3.forward;
        }

        static Vector3 SimulateCandy(Vector3 origin, Vector3 dir, float speed)
        {
            var pos = origin;
            var vel = dir * speed;
            const float dt = 0.02f;
            for (var i = 0; i < 180; i++)
            {
                vel.y -= 9.81f * dt;
                vel.x *= 1f / (1f + 0.22f * dt);
                vel.y *= 1f / (1f + 0.22f * dt);
                vel.z *= 1f / (1f + 0.22f * dt);
                pos = pos + vel * dt;
                if (pos.y <= 0.4f && i > 4)
                    return pos;
            }

            return pos;
        }

        static void PlayPhoneHttp()
        {
            var session = new PhoneSession();
            var host = new PhoneHost(session);
            if (!host.TryStart(18787))
            {
                Check("play phone bind", false, host.LastError);
                return;
            }

            try
            {
                var joinPage = Http("GET", host.LocalUrl, null);
                Check("play phone join page", joinPage.Contains("COMMANDER") && joinPage.Contains("ARTILLERY"), "");

                var commander = Http("POST", host.LocalUrl + "join", "{\"op\":\"join\",\"role\":\"commander\"}");
                Check("play phone join commander", commander.Contains("\"ok\":true") && commander.Contains("p1"), commander);
                var artillery = Http("POST", host.LocalUrl + "join", "{\"op\":\"join\",\"role\":\"artillery\"}");
                Check("play phone join artillery", artillery.Contains("\"ok\":true"), artillery);
                var taken = Http("POST", host.LocalUrl + "join", "{\"op\":\"join\",\"role\":\"commander\"}");
                Check("play phone commander taken", taken.Contains("taken") || taken.Contains("\"ok\":false"), taken);

                var west = Http("POST", host.LocalUrl + "cmd",
                    "{\"op\":\"cmd\",\"role\":\"commander\",\"player\":\"p1\",\"action\":\"west\"}");
                Check("play phone WEST accepted", west.Contains("\"ok\":true"), west);
                Check("play phone WEST queued", host.TryDequeue(out var westCmd) && westCmd.Action == PhoneCommand.ActWest,
                    westCmd != null ? westCmd.Action : "none");

                var fire = Http("POST", host.LocalUrl + "cmd",
                    "{\"op\":\"cmd\",\"role\":\"artillery\",\"player\":\"p2\",\"action\":\"fire\",\"machine\":\"cannon\",\"x\":-2,\"z\":0}");
                Check("play phone FIRE accepted", fire.Contains("\"ok\":true"), fire);
                Check("play phone FIRE queued", host.TryDequeue(out var fireCmd) && fireCmd.Machine == PhoneCommand.MachineCannon,
                    fireCmd != null ? fireCmd.Machine : "none");

                host.PublishState("{\"mode\":\"Hold WEST\",\"tactic\":\"march-west\",\"victory\":false}");
                var state = Http("GET", host.LocalUrl + "state", null);
                Check("play phone state", state.Contains("Hold WEST"), state);

                var svg = Http("GET", host.LocalUrl + "qr.svg", null);
                Check("play phone qr.svg", svg.Contains("<svg") && svg.Contains("rect"), "");

                var commanderPage = Http("GET", host.LocalUrl + "commander", null);
                Check("play phone commander surface", commanderPage.Contains("WEST") && commanderPage.Contains("HOLD"), "");
                var artyPage = Http("GET", host.LocalUrl + "artillery", null);
                Check("play phone artillery surface", artyPage.Contains("CATAPULT") && artyPage.Contains("CANNON"), "");
            }
            catch (Exception ex)
            {
                Check("play phone http", false, ex.Message);
            }
            finally
            {
                host.Dispose();
            }
        }

        static string Http(string method, string url, string body)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = method;
            req.Timeout = 4000;
            req.ReadWriteTimeout = 4000;
            if (body != null)
            {
                var bytes = Encoding.UTF8.GetBytes(body);
                req.ContentType = "application/json";
                req.ContentLength = bytes.Length;
                using (var s = req.GetRequestStream())
                    s.Write(bytes, 0, bytes.Length);
            }

            try
            {
                using (var resp = (HttpWebResponse)req.GetResponse())
                using (var reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    return reader.ReadToEnd();
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                    throw;
                using (var reader = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8))
                    return reader.ReadToEnd();
            }
        }

        static void LogicalArmy()
        {
            var pop = new LogicalPopulation();
            pop.SeedBlock(80, new Vector3(8f, 0.9f, 0f));
            pop.Marching = true;
            var start = pop.CenterOfMass();
            var flag = new Vector3(-10f, 0.2f, 0f);
            var incoming = new Vector3(80f, 4f, 0f);
            for (var i = 0; i < 240; i++)
                pop.Tick(0.05f, flag, incoming);
            var end = pop.CenterOfMass();
            Check("army flows west", end.x < start.x - 4f, "start=" + start.x.ToString("0.0") + " end=" + end.x.ToString("0.0"));
            Check("army stops at flag, not the rail", end.x >= flag.x - 0.2f && end.x <= flag.x + LogicalPopulation.ArriveBand + 3f, end.x.ToString("0.00"));
            Check("count conserved", pop.Count == 80, pop.Count.ToString());

            var blob = pop.ToBlob();
            var clone = new LogicalPopulation();
            clone.LoadBlob(blob);
            Check("blob roundtrip count", clone.Count == 80, clone.Count.ToString());
            Check("blob roundtrip COM", Math.Abs(clone.CenterOfMass().x - end.x) < 0.001f, "");
        }

        static void LogicalBench(int n, int frames)
        {
            var pop = new LogicalPopulation();
            pop.SeedBlock(n, new Vector3(8f, 0.9f, 0f));
            pop.Marching = true;
            var flag = new Vector3(-10f, 0.2f, 0f);
            var incoming = new Vector3(80f, 4f, 0f);
            pop.Tick(0.02f, flag, incoming);
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < frames; i++)
                pop.Tick(0.02f, flag, incoming);
            sw.Stop();
            var per = sw.Elapsed.TotalMilliseconds / frames;
            Line($"bench B  N={n}  frames={frames}  total={sw.Elapsed.TotalMilliseconds:0.00} ms  per-tick={per:0.000} ms");
            Check("logical tick cheap N=" + n, per < 8.0, per.ToString("0.000") + " ms");
        }

        static void Check(string name, bool ok, string detail)
        {
            if (!ok)
                _failed++;
            Line((ok ? "PASS  " : "FAIL  ") + name + (string.IsNullOrEmpty(detail) ? "" : "  (" + detail + ")"));
        }

        static void Line(string s)
        {
            Console.WriteLine(s);
            Log.AppendLine(s);
        }

        static string FindRepo()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            for (var i = 0; i < 8; i++)
            {
                if (File.Exists(Path.Combine(dir, "README.md")) && Directory.Exists(Path.Combine(dir, "Assets")))
                    return dir;
                dir = Directory.GetParent(dir)?.FullName ?? dir;
            }
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        }
    }
}
