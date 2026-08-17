#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using GummyDynasty.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GummyDynasty.Editor
{
    /// <summary>
    /// When the editor is already open, the agent can drop a request file
    /// and get real PhysX back. No Hub launch, no Play click.
    /// Inbox: Tools/Lab/inbox.json   Result: Logs/lab-result.json
    /// </summary>
    [InitializeOnLoad]
    public static class LabProbe
    {
        const string InboxRel = "Tools/Lab/inbox.json";
        const string ResultRel = "Logs/lab-result.json";

        static LabProbe()
        {
            EditorApplication.update += Poll;
        }

        [MenuItem("GummyDynasty/Run Behavior Lab (PhysX)")]
        public static void RunMenu()
        {
            var result = RunPhysX();
            WriteResult(result);
            Debug.Log(result);
        }

        static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            var inbox = Path.GetFullPath(InboxRel);
            if (!File.Exists(inbox))
                return;
            try
            {
                File.Delete(inbox);
                var result = RunPhysX();
                WriteResult(result);
                Debug.Log(result);
            }
            catch (Exception ex)
            {
                WriteResult("FAIL  lab probe  (" + ex.Message + ")\n");
            }
        }

        static void WriteResult(string text)
        {
            var path = Path.GetFullPath(ResultRel);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "Logs");
            File.WriteAllText(path, text);
        }

        static string RunPhysX()
        {
            var log = new StringBuilder();
            var failed = 0;
            void Check(string name, bool ok, string detail)
            {
                if (!ok) failed++;
                log.Append(ok ? "PASS  " : "FAIL  ");
                log.Append(name);
                if (!string.IsNullOrEmpty(detail))
                {
                    log.Append("  (");
                    log.Append(detail);
                    log.Append(')');
                }
                log.Append('\n');
            }

            var scene = SceneManager.CreateScene(
                "GummyLab_" + DateTime.UtcNow.Ticks,
                new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            var previous = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(scene);
            var phys = scene.GetPhysicsScene();
            var spawned = new System.Collections.Generic.List<GameObject>(80);

            try
            {
                SolidWall(phys, spawned, Check);
                TwoColumnDoor(phys, spawned, Check);
                CandyHits(phys, spawned, Check);
                DropLands(phys, spawned, Check);
                DeckCarry(phys, spawned, Check);
            }
            finally
            {
                for (var i = 0; i < spawned.Count; i++)
                {
                    if (spawned[i] != null)
                        UnityEngine.Object.DestroyImmediate(spawned[i]);
                }
                SceneManager.SetActiveScene(previous);
                SceneManager.UnloadSceneAsync(scene);
            }

            log.Append(failed == 0 ? "OK  physx-lab\n" : "FAILED  " + failed + "\n");
            return log.ToString();
        }

        static void SolidWall(PhysicsScene phys, System.Collections.Generic.List<GameObject> spawned, Action<string, bool, string> check)
        {
            Clear(spawned);
            Ground(spawned);
            PitWall(spawned, Array.Empty<int>());
            var hopper = Sphere(spawned, new Vector3(2.4f, 0.4f, 0f), WallMeasure.MarcherBellyRadius, 2.2f);
            var rb = hopper.GetComponent<Rigidbody>();
            Step(phys, 5.5f, () => rb.AddForce(Vector3.left * 14f, ForceMode.Acceleration));
            check("physx solid wall blocks hopper",
                hopper.transform.position.x > -2f,
                hopper.transform.position.x.ToString("0.00"));
        }

        static void TwoColumnDoor(PhysicsScene phys, System.Collections.Generic.List<GameObject> spawned, Action<string, bool, string> check)
        {
            Clear(spawned);
            Ground(spawned);
            PitWall(spawned, new[] { 3, 4 });
            var hopper = Sphere(spawned, new Vector3(2.4f, 0.4f, 0.28f), WallMeasure.MarcherBellyRadius, 2.2f);
            var rb = hopper.GetComponent<Rigidbody>();
            Step(phys, 7f, () => rb.AddForce(Vector3.left * 14f, ForceMode.Acceleration));
            check("physx two-column door lets a marcher through",
                hopper.transform.position.x < -2.4f,
                hopper.transform.position.x.ToString("0.00"));
        }

        static void CandyHits(PhysicsScene phys, System.Collections.Generic.List<GameObject> spawned, Action<string, bool, string> check)
        {
            Clear(spawned);
            Ground(spawned);
            PitWall(spawned, Array.Empty<int>());
            var candy = Sphere(spawned, new Vector3(7.2f, 1.05f, 0f), 0.45f, 5f);
            var rb = candy.GetComponent<Rigidbody>();
            rb.linearDamping = 0.22f;
            rb.AddForce(Vector3.left * CandyShot.Speed, ForceMode.VelocityChange);
            Step(phys, 1.6f, null);
            check("physx candy hits the wall",
                candy.transform.position.x > -3.4f,
                candy.transform.position.x.ToString("0.00"));
        }

        static void DropLands(PhysicsScene phys, System.Collections.Generic.List<GameObject> spawned, Action<string, bool, string> check)
        {
            Clear(spawned);
            Ground(spawned);
            var crate = Box(spawned, new Vector3(0f, 8f, 0f), Vector3.one * WallMeasure.CrateVisual, 1.1f, false);
            Step(phys, 3f, null);
            var y = crate.transform.position.y;
            check("physx DROP crate lands",
                y < 0.6f && y > 0.05f,
                crate.transform.position.ToString());
        }

        static void DeckCarry(PhysicsScene phys, System.Collections.Generic.List<GameObject> spawned, Action<string, bool, string> check)
        {
            Clear(spawned);
            var deck = Box(spawned, new Vector3(4f, 0.4f, 0f), new Vector3(2.4f, 0.4f, 1.4f), 0f, true);
            var hopper = Sphere(spawned, new Vector3(4f, 0.85f, 0f), WallMeasure.MarcherBellyRadius, 2.2f);
            var start = hopper.transform.position.x;
            Step(phys, 2.5f, () =>
            {
                deck.transform.position += Vector3.left * (1.6f * (1f / 60f));
                var deckRb = deck.GetComponent<Rigidbody>();
                if (deckRb != null)
                    deckRb.MovePosition(deck.transform.position);
            });
            var slip = Mathf.Abs(hopper.transform.position.x - deck.transform.position.x);
            check("physx moving deck carries hopper",
                slip < 1.1f && hopper.transform.position.x < start - 1f,
                "slip=" + slip.ToString("0.00"));
        }

        static void Step(PhysicsScene phys, float seconds, Action each)
        {
            const float dt = 1f / 60f;
            var t = 0f;
            while (t < seconds)
            {
                each?.Invoke();
                phys.Simulate(dt);
                t += dt;
            }
        }

        static void Ground(System.Collections.Generic.List<GameObject> spawned)
        {
            Box(spawned, new Vector3(0f, -0.25f, 0f), new Vector3(40f, 0.5f, 20f), 0f, true);
        }

        static void PitWall(System.Collections.Generic.List<GameObject> spawned, int[] dropZ)
        {
            var parts = new System.Collections.Generic.List<Vector3>(64);
            WallMeasure.LayoutStack(parts, new Vector3(-2f, 0.35f, 0f), 2, 4, 7);
            for (var i = 0; i < parts.Count; i++)
            {
                var zIndex = Mathf.RoundToInt(parts[i].z / WallMeasure.CratePitch + 3f);
                var drop = false;
                for (var k = 0; k < dropZ.Length; k++)
                    if (zIndex == dropZ[k]) drop = true;
                if (drop)
                    continue;
                Box(spawned, parts[i], Vector3.one * WallMeasure.CrateVisual, 0f, true);
            }
        }

        static GameObject Sphere(System.Collections.Generic.List<GameObject> spawned, Vector3 pos, float radius, float mass)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "lab-sphere";
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * (radius * 2f);
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            spawned.Add(go);
            return go;
        }

        static GameObject Box(System.Collections.Generic.List<GameObject> spawned, Vector3 pos, Vector3 size, float mass, bool kinematic)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "lab-box";
            go.transform.position = pos;
            go.transform.localScale = size;
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = Mathf.Max(0.05f, mass);
            rb.isKinematic = kinematic;
            spawned.Add(go);
            return go;
        }

        static void Clear(System.Collections.Generic.List<GameObject> spawned)
        {
            for (var i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] != null)
                    UnityEngine.Object.DestroyImmediate(spawned[i]);
            }
            spawned.Clear();
        }
    }
}
#endif
