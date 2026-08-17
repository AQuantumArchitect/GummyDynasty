using UnityEngine;
using UnityEngine.Rendering;

namespace GummyDynasty.Simulation
{
    [ExecuteAlways]
    [DefaultExecutionOrder(-50)]
    public sealed class ToyArena : MonoBehaviour
    {
        public Transform GummyRoot { get; private set; }
        public Transform PropRoot { get; private set; }
        public PitCatapult Catapult { get; private set; }
        public PitCannon Cannon { get; private set; }
        public TrainRig Train { get; private set; }
        public ShowcaseKind Kind { get; private set; } = ShowcaseKind.Pit;
        public Vector3 FlagPosition { get; private set; } = new Vector3(-10f, 0.2f, 0f);
        public Vector3 SmashWallOrigin { get; private set; } = new Vector3(-2f, 0.35f, 0f);
        public Transform SmashWallRoot
        {
            get
            {
                if (Train != null && Train.Mid != null)
                {
                    var onCar = Train.Mid.Find("SmashWall");
                    if (onCar != null)
                        return onCar;
                }
                return PropRoot != null ? PropRoot.Find("SmashWall") : null;
            }
        }

        void OnEnable()
        {
            Build();
            ApplyLook();
        }

        public void Build()
        {
            RebindRoots();
            if (transform.Find("Ground") != null)
            {
                if (transform.Find("WestFlag") == null)
                    BuildFlag(FlagPosition);
                EnsureToys();
                ApplyMatsToExisting();
                return;
            }

            BuildGround();
            BuildKind();
        }

        public void Rebuild(ShowcaseKind kind)
        {
            Kind = kind;
            for (var i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
            GummyRoot = null;
            PropRoot = null;
            Catapult = null;
            Cannon = null;
            Train = null;
            RebindRoots();
            BuildGround();
            BuildKind();
            ApplyLook();
        }

        public void ResetProps()
        {
            if (PropRoot == null)
                return;
            for (var i = PropRoot.childCount - 1; i >= 0; i--)
                DestroyImmediate(PropRoot.GetChild(i).gameObject);
            Catapult = null;
            Cannon = null;
            Train = null;
            var flag = transform.Find("WestFlag");
            if (flag != null)
                DestroyImmediate(flag.gameObject);
            BuildKind();
        }

        void BuildGround()
        {
            Slab("Ground", new Vector3(0f, -0.25f, 0f), new Vector3(28f, 0.5f, 28f), new Color(0.38f, 0.82f, 0.55f), true);
            PaintChecker();
            Slab("RailN", new Vector3(0f, 0.55f, 14f), new Vector3(28.8f, 1.6f, 0.55f), new Color(0.92f, 0.28f, 0.55f), true);
            Slab("RailS", new Vector3(0f, 0.55f, -14f), new Vector3(28.8f, 1.6f, 0.55f), new Color(0.92f, 0.28f, 0.55f), true);
            Slab("RailE", new Vector3(14f, 0.55f, 0f), new Vector3(0.55f, 1.6f, 28.8f), new Color(0.95f, 0.45f, 0.2f), true);
            Slab("RailW", new Vector3(-14f, 0.55f, 0f), new Vector3(0.55f, 1.6f, 28.8f), new Color(0.95f, 0.45f, 0.2f), true);
        }

        void BuildKind()
        {
            RebindRoots();
            if (Kind == ShowcaseKind.Castle)
                BuildCastle();
            else if (Kind == ShowcaseKind.Train)
                BuildTrain();
            else
                BuildPit();
        }

        void BuildPit()
        {
            SmashWallOrigin = new Vector3(-2f, 0.35f, 0f);
            FlagPosition = new Vector3(-10f, 0.2f, 0f);
            if (transform.Find("Ramp") == null)
            {
                Slab("Ramp", new Vector3(-6f, 0.7f, -4f), new Vector3(5f, 0.3f, 3.6f), new Color(1f, 0.85f, 0.2f), true)
                    .transform.rotation = Quaternion.Euler(-18f, 35f, 0f);
            }
            EnsureBanner("GUMMY PIT", new Vector3(0f, 3.2f, 10.5f));
            BuildFlag(FlagPosition);
            BuildCrateWall("SmashWall", SmashWallOrigin, 2, 4, 7);
            BuildCrateWall("CratePile", new Vector3(-3.5f, 0.35f, -5.5f), 3, 3);
            BuildCrateWall("BreakTower", new Vector3(1.2f, 0.35f, 6.4f), 2, 6);
            BuildLooseToys();
            BuildCatapult(new Vector3(7.2f, 0f, -1.4f));
            BuildCannon(new Vector3(7.2f, 0f, 2.2f));
        }

        void BuildCastle()
        {
            SmashWallOrigin = new Vector3(-3.1f, 0.35f, 0f);
            FlagPosition = new Vector3(-9.2f, 0.2f, 0f);
            EnsureBanner("GUMMY KEEP", new Vector3(0f, 3.4f, 10.5f));
            BuildFlag(FlagPosition);
            BuildCrateWall("SmashWall", SmashWallOrigin, 2, 5, 9);
            BuildCrateWall("KeepNorth", new Vector3(-7.2f, 0.35f, 3.7f), 8, 4, 2);
            BuildCrateWall("KeepSouth", new Vector3(-7.2f, 0.35f, -3.7f), 8, 4, 2);
            BuildCrateWall("KeepWest", new Vector3(-11.2f, 0.35f, 0f), 2, 4, 7);
            BuildLooseToys();
            BuildCatapult(new Vector3(7.4f, 0f, -2.2f));
            BuildCannon(new Vector3(7.4f, 0f, 2.4f));
        }

        void BuildTrain()
        {
            SmashWallOrigin = new Vector3(0f, 0.95f, 0f);
            FlagPosition = new Vector3(-6f, 0.6f, 0f);
            EnsureBanner("GUMMY TRAIN", new Vector3(0f, 3.4f, 10.5f));
            var go = new GameObject("Train");
            Train = go.AddComponent<TrainRig>();
            Train.Build(PropRoot, new Vector3(0f, 0f, 0f));
            BuildCrateWallOn(Train.Mid, "SmashWall", new Vector3(0f, 0.55f, 0f), 2, 3, 3);
            BuildFlag(Train.FlagWorld);
            var flag = transform.Find("WestFlag");
            if (flag != null && Train.Head != null)
                flag.SetParent(Train.Head, true);
            BuildCatapult(new Vector3(9.2f, 0f, -3.4f));
            BuildCannon(new Vector3(9.2f, 0f, 3.4f));
        }

        void LateUpdate()
        {
            if (Train != null)
                FlagPosition = Train.FlagWorld;
            var wall = SmashWallRoot;
            if (wall != null)
                SmashWallOrigin = wall.position;
        }

        public Vector3 DefaultSpawn(int i)
        {
            if (Kind == ShowcaseKind.Train && Train != null)
                return Train.TailDeck + new Vector3(0.2f + i * 0.45f, 1.1f, (i - 1) * 0.4f);
            if (Kind == ShowcaseKind.Castle)
            {
                var pts = new[]
                {
                    new Vector3(3.4f, 2.4f, 0f),
                    new Vector3(4.4f, 2.8f, 0.7f),
                    new Vector3(3.8f, 2.2f, 1.3f)
                };
                return pts[i % pts.Length];
            }

            var pit = new[]
            {
                new Vector3(1.8f, 2.4f, 0f),
                new Vector3(3.1f, 2.8f, 0.7f),
                new Vector3(2.4f, 2.2f, 1.3f)
            };
            return pit[i % pit.Length];
        }

        public Vector3 RankedSpawn(int i)
        {
            if (Kind == ShowcaseKind.Train && Train != null)
                return Train.RankedSpawn(i);
            var east = Kind == ShowcaseKind.Castle ? 4.4f : 3.2f;
            return new Vector3(east + (i % 2) * 0.7f, 2.3f, (i - 2) * 1.05f);
        }

        void EnsureToys()
        {
            RebindRoots();
            if (PropRoot == null)
                return;
            if (Kind == ShowcaseKind.Pit)
                EnsureSmashOnRoad();
            if (Kind == ShowcaseKind.Pit && PropRoot.Find("CratePile") == null)
                BuildCrateWall("CratePile", new Vector3(-3.5f, 0.35f, -5.5f), 3, 3);
            if (Kind == ShowcaseKind.Pit && PropRoot.Find("BreakTower") == null)
                BuildCrateWall("BreakTower", new Vector3(1.2f, 0.35f, 6.4f), 2, 6);
            if (PropRoot.Find("LooseToys") == null && Kind != ShowcaseKind.Train)
                BuildLooseToys();
            if (PropRoot.Find("PitCatapult") == null)
                BuildCatapult(new Vector3(7.2f, 0f, -1.4f));
            else if (Catapult == null)
                Catapult = PropRoot.Find("PitCatapult").GetComponent<PitCatapult>();
            if (PropRoot.Find("PitCannon") == null)
                BuildCannon(new Vector3(7.2f, 0f, 2.2f));
            else if (Cannon == null)
                Cannon = PropRoot.Find("PitCannon").GetComponent<PitCannon>();
        }

        bool AdoptOrphanCrates(string groupName)
        {
            var adopted = 0;
            Transform group = null;
            for (var i = PropRoot.childCount - 1; i >= 0; i--)
            {
                var child = PropRoot.GetChild(i);
                if (child.GetComponent<BreakablePart>() == null)
                    continue;
                if (group == null)
                {
                    group = new GameObject(groupName).transform;
                    group.SetParent(PropRoot, false);
                }
                child.SetParent(group, true);
                adopted++;
            }
            return adopted > 0;
        }

        public int SmashWall()
        {
            var wall = SmashWallRoot;
            if (wall == null)
                return 0;
            var parts = wall.GetComponentsInChildren<BreakablePart>();
            if (parts.Length == 0)
                return 0;

            var origin = Vector3.zero;
            for (var i = 0; i < parts.Length; i++)
                origin += parts[i].transform.position;
            origin /= parts.Length;

            for (var i = 0; i < parts.Length; i++)
                parts[i].Blast(origin, 7.5f + (i % 3) * 1.2f);
            return parts.Length;
        }

        void EnsureSmashOnRoad()
        {
            var wall = PropRoot != null ? PropRoot.Find("SmashWall") : null;
            if (wall != null)
            {
                var mid = Vector3.zero;
                var parts = wall.GetComponentsInChildren<BreakablePart>();
                if (parts.Length > 0)
                {
                    for (var i = 0; i < parts.Length; i++)
                        mid += parts[i].transform.position;
                    mid /= parts.Length;
                    if (mid.x < 0.6f)
                        return;
                }
                Object.DestroyImmediate(wall.gameObject);
            }
            else if (AdoptOrphanCrates("SmashWall"))
            {
                wall = PropRoot.Find("SmashWall");
                if (wall != null)
                    return;
            }
            BuildCrateWall("SmashWall", SmashWallOrigin, 2, 4, 7);
        }

        void BuildCrateWall(string groupName, Vector3 origin, int wide, int high, int deep = 1)
        {
            BuildCrateWallOn(PropRoot, groupName, origin, wide, high, deep);
        }

        void BuildCrateWallOn(Transform parent, string groupName, Vector3 origin, int wide, int high, int deep = 1)
        {
            BreakableStack.Build(parent, groupName, origin, wide, high, deep);
        }

        void BuildCatapult(Vector3 pos)
        {
            var go = new GameObject("PitCatapult");
            go.transform.SetParent(PropRoot, false);
            go.transform.position = pos;
            var cat = go.AddComponent<PitCatapult>();
            cat.Build();
            Catapult = cat;
        }

        void BuildCannon(Vector3 pos)
        {
            var go = new GameObject("PitCannon");
            go.transform.SetParent(PropRoot, false);
            go.transform.position = pos;
            var gun = go.AddComponent<PitCannon>();
            gun.Build();
            Cannon = gun;
        }

        void BuildLooseToys()
        {
            var group = new GameObject("LooseToys");
            group.transform.SetParent(PropRoot, false);

            var drop = PersonalityCatalog.Gumdrop();
            var breaker = PersonalityCatalog.Jawbreaker();
            for (var i = 0; i < 7; i++)
            {
                var ox = (i % 4) * 0.55f;
                var oz = (i / 4) * 0.55f;
                SpawnTossable(
                    group.transform,
                    drop.DisplayName,
                    new Vector3(2.1f + ox, 0.45f, -6.2f + oz),
                    drop.Scale,
                    drop.Mass,
                    drop.LaunchMul,
                    drop.Color);
            }

            SpawnTossable(
                group.transform,
                breaker.DisplayName,
                new Vector3(3.4f, 0.7f, -5.4f),
                breaker.Scale,
                breaker.Mass,
                breaker.LaunchMul,
                breaker.Color);
        }

        static void SpawnTossable(Transform parent, string label, Vector3 pos, float scale, float mass, float launchMul, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = label;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * scale;
            go.GetComponent<Renderer>().sharedMaterial = GummyLook.Material(color);
            go.GetComponent<Collider>().sharedMaterial = CrateMat();
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.linearDamping = 0.75f;
            rb.angularDamping = 4.2f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var toss = go.AddComponent<Tossable>();
            toss.Label = label;
            toss.LaunchMul = launchMul;
        }

        GameObject Slab(string name, Vector3 pos, Vector3 scale, Color color, bool staticCollider)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(transform, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = GummyLook.Material(color);
            go.GetComponent<Collider>().sharedMaterial = staticCollider ? GroundMat() : CrateMat();
            if (staticCollider)
            {
                var rb = go.AddComponent<Rigidbody>();
                rb.isKinematic = true;
            }
            return go;
        }

        void ApplyMatsToExisting()
        {
            var cols = GetComponentsInChildren<Collider>();
            for (var i = 0; i < cols.Length; i++)
            {
                var col = cols[i];
                if (col.GetComponent<BreakablePart>() != null)
                    col.sharedMaterial = CrateMat();
                else if (col.attachedRigidbody != null && col.attachedRigidbody.isKinematic)
                    col.sharedMaterial = GroundMat();
            }
        }

        static PhysicsMaterial _groundMat;
        static PhysicsMaterial _crateMat;

        static PhysicsMaterial GroundMat()
        {
            if (_groundMat != null)
                return _groundMat;
            _groundMat = new PhysicsMaterial("GummyGround")
            {
                bounciness = 0.04f,
                dynamicFriction = 0.9f,
                staticFriction = 0.98f,
                bounceCombine = PhysicsMaterialCombine.Minimum,
                frictionCombine = PhysicsMaterialCombine.Maximum
            };
            return _groundMat;
        }

        static PhysicsMaterial CrateMat()
        {
            if (_crateMat != null)
                return _crateMat;
            _crateMat = new PhysicsMaterial("GummyCrate")
            {
                bounciness = 0.04f,
                dynamicFriction = 0.88f,
                staticFriction = 0.96f,
                bounceCombine = PhysicsMaterialCombine.Minimum,
                frictionCombine = PhysicsMaterialCombine.Maximum
            };
            return _crateMat;
        }

        void RebindRoots()
        {
            if (GummyRoot == null)
            {
                var existing = transform.Find("Gummies");
                GummyRoot = existing != null ? existing : new GameObject("Gummies").transform;
                GummyRoot.SetParent(transform, false);
            }

            if (PropRoot == null)
            {
                var existing = transform.Find("Props");
                PropRoot = existing != null ? existing : new GameObject("Props").transform;
                PropRoot.SetParent(transform, false);
            }
        }

        void PaintChecker()
        {
            const int n = 7;
            const float cell = 4f;
            for (var z = 0; z < n; z++)
            for (var x = 0; x < n; x++)
            {
                if (((x + z) & 1) == 0)
                    continue;
                var pos = new Vector3((x - n * 0.5f + 0.5f) * cell, 0.01f, (z - n * 0.5f + 0.5f) * cell);
                Slab("Tile", pos, new Vector3(cell * 0.96f, 0.04f, cell * 0.96f), new Color(0.22f, 0.62f, 0.4f), true);
            }
        }

        void BuildFlag(Vector3 pos)
        {
            FlagPosition = pos;
            var root = new GameObject("WestFlag");
            root.transform.SetParent(transform, false);
            root.transform.position = pos;

            var pole = Slab("FlagPole", pos + new Vector3(0f, 1.6f, 0f), new Vector3(0.16f, 3.2f, 0.16f), new Color(0.75f, 0.62f, 0.2f), true);
            pole.transform.SetParent(root.transform, true);
            var cloth = Slab("FlagCloth", pos + new Vector3(0.7f, 2.55f, 0f), new Vector3(1.4f, 0.85f, 0.08f), new Color(0.95f, 0.15f, 0.22f), true);
            cloth.transform.SetParent(root.transform, true);
            var star = Slab("FlagStar", pos + new Vector3(0.75f, 2.55f, 0.08f), new Vector3(0.28f, 0.28f, 0.08f), new Color(1f, 0.92f, 0.2f), true);
            star.transform.SetParent(root.transform, true);
            Sign("WEST", pos + new Vector3(0f, 3.5f, 0f), root.transform);
        }

        void EnsureBanner(string text, Vector3 pos)
        {
            var existing = transform.Find("Banner");
            if (existing != null)
            {
                var tm = existing.GetComponent<TextMesh>();
                if (tm != null)
                    tm.text = text;
                existing.position = pos;
                return;
            }
            var go = new GameObject("Banner");
            go.transform.SetParent(transform, false);
            go.transform.position = pos;
            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.fontSize = 64;
            mesh.characterSize = 0.12f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = Color.white;
            mesh.fontStyle = FontStyle.Bold;
        }

        void Sign(string text, Vector3 pos, Transform parent = null)
        {
            var go = new GameObject("Sign");
            go.transform.SetParent(parent != null ? parent : transform, false);
            go.transform.position = pos;
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 64;
            tm.characterSize = 0.12f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.white;
            tm.fontStyle = FontStyle.Bold;
        }

        void ApplyLook()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.62f, 0.56f, 0.7f);
            RenderSettings.fog = false;
            RenderSettings.skybox = null;

            var cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.38f, 0.66f, 0.9f);
            }

            var light = FindFirstObjectByType<Light>();
            if (light == null)
                return;
            light.intensity = 1.65f;
            light.color = new Color(1f, 0.94f, 0.86f);
            var extra = System.Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalAdditionalLightData, Unity.RenderPipelines.Universal.Runtime");
            if (extra != null && light.GetComponent(extra) == null)
                light.gameObject.AddComponent(extra);
        }
    }
}
