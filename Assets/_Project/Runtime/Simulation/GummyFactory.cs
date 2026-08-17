using UnityEngine;

namespace GummyDynasty.Simulation
{
    public static class GummyFactory
    {
        static int _serial;
        public static GummyBody Spawn(PhysicalPersonality personality, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (personality == null)
                throw new System.ArgumentNullException(nameof(personality));

            var id = "gummy-" + (++_serial);
            var root = new GameObject(personality.DisplayName + " " + _serial);
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(position, rotation);

            var h = personality.Height;
            var w = 0.28f * h * personality.Width;
            var mat = SharedMaterial(personality);

            var hips = Part(root.transform, "Hips", PrimitiveType.Sphere, new Vector3(0f, h * 0.22f, 0f), Vector3.one * (w * 1.15f), mat, personality.Color);
            var belly = Part(root.transform, "Belly", PrimitiveType.Sphere, new Vector3(0f, h * 0.48f, 0f), new Vector3(w * 1.35f, w * 1.2f, w * 1.25f), mat, personality.Color);
            var head = Part(root.transform, "Head", PrimitiveType.Sphere, new Vector3(0f, h * 0.82f, 0f), Vector3.one * (w * 0.95f * personality.HeadScale), mat, Color.Lerp(personality.Color, Color.white, 0.12f));
            var armL = Part(root.transform, "ArmL", PrimitiveType.Sphere, new Vector3(-w * 1.15f, h * 0.5f, 0f), Vector3.one * (w * 0.55f * personality.ArmScale), mat, Darken(personality.Color, 0.15f));
            var armR = Part(root.transform, "ArmR", PrimitiveType.Sphere, new Vector3(w * 1.15f, h * 0.5f, 0f), Vector3.one * (w * 0.55f * personality.ArmScale), mat, Darken(personality.Color, 0.15f));

            var hipsRb = Body(hips, personality, personality.Mass * 0.34f);
            var bellyRb = Body(belly, personality, personality.Mass * 0.30f);
            var headRb = Body(head, personality, personality.Mass * 0.16f);
            var armLRb = Body(armL, personality, personality.Mass * 0.10f);
            var armRRb = Body(armR, personality, personality.Mass * 0.10f);

            SoftJoint(bellyRb, hipsRb, personality, 1.1f);
            SoftJoint(headRb, bellyRb, personality, 0.85f);
            SoftJoint(armLRb, bellyRb, personality, 0.7f);
            SoftJoint(armRRb, bellyRb, personality, 0.7f);

            IgnorePair(hips, belly);
            IgnorePair(belly, head);
            IgnorePair(belly, armL);
            IgnorePair(belly, armR);

            Face(head.transform, new Vector3(0f, 0.08f, 0.38f), w * 0.18f, new Color(0.08f, 0.08f, 0.1f));
            Face(head.transform, new Vector3(0.16f, 0.12f, 0.32f), w * 0.12f, Color.white);
            Face(head.transform, new Vector3(-0.16f, 0.12f, 0.32f), w * 0.12f, Color.white);

            var parts = new[] { hipsRb, bellyRb, headRb, armLRb, armRRb };
            var body = root.AddComponent<GummyBody>();
            body.Bind(id, personality, hipsRb, bellyRb, headRb, parts);

            var agent = root.AddComponent<GummyAgent>();
            agent.Bind(body);
            return body;
        }

        static GameObject Part(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 scale, PhysicsMaterial mat, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            var col = go.GetComponent<Collider>();
            col.sharedMaterial = mat;
            var rend = go.GetComponent<Renderer>();
            rend.sharedMaterial = GummyLook.Material(color);
            return go;
        }

        static Rigidbody Body(GameObject go, PhysicalPersonality p, float mass)
        {
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = Mathf.Max(0.05f, mass);
            rb.linearDamping = p.Drag;
            rb.angularDamping = p.AngularDrag;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            return rb;
        }

        static void SoftJoint(Rigidbody a, Rigidbody b, PhysicalPersonality p, float springMul)
        {
            var j = a.gameObject.AddComponent<ConfigurableJoint>();
            j.connectedBody = b;
            j.autoConfigureConnectedAnchor = true;
            j.xMotion = ConfigurableJointMotion.Limited;
            j.yMotion = ConfigurableJointMotion.Limited;
            j.zMotion = ConfigurableJointMotion.Limited;
            j.angularXMotion = ConfigurableJointMotion.Limited;
            j.angularYMotion = ConfigurableJointMotion.Limited;
            j.angularZMotion = ConfigurableJointMotion.Limited;
            var limit = new SoftJointLimit { limit = 0.08f };
            j.linearLimit = limit;
            var spring = new SoftJointLimitSpring { spring = p.JointSpring * springMul, damper = p.JointDamper };
            j.linearLimitSpring = spring;
            var ang = new SoftJointLimit { limit = 28f };
            j.lowAngularXLimit = new SoftJointLimit { limit = -28f };
            j.highAngularXLimit = ang;
            j.angularYLimit = ang;
            j.angularZLimit = ang;
            var drive = new JointDrive
            {
                positionSpring = p.JointSpring * 0.35f * springMul,
                positionDamper = p.JointDamper,
                maximumForce = 250f
            };
            j.angularXDrive = drive;
            j.angularYZDrive = drive;
            j.enableCollision = false;
        }

        static void IgnorePair(GameObject a, GameObject b)
        {
            Physics.IgnoreCollision(a.GetComponent<Collider>(), b.GetComponent<Collider>(), true);
        }

        static void Face(Transform head, Vector3 local, float size, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Face";
            go.transform.SetParent(head, false);
            go.transform.localPosition = local;
            go.transform.localScale = Vector3.one * size;
            Object.Destroy(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = GummyLook.Material(color);
        }

        static PhysicsMaterial SharedMaterial(PhysicalPersonality p)
        {
            var mat = new PhysicsMaterial(p.DisplayName + "Jelly")
            {
                bounciness = p.Bounciness,
                dynamicFriction = Mathf.Max(0.82f, p.DynamicFriction),
                staticFriction = Mathf.Max(0.92f, p.StaticFriction),
                bounceCombine = PhysicsMaterialCombine.Minimum,
                frictionCombine = PhysicsMaterialCombine.Maximum
            };
            return mat;
        }

        static Color Darken(Color c, float amt)
        {
            return new Color(c.r * (1f - amt), c.g * (1f - amt), c.b * (1f - amt), c.a);
        }
    }

    static class GummyLook
    {
        static Material _src;
        static readonly System.Collections.Generic.Dictionary<Color, Material> Cache = new System.Collections.Generic.Dictionary<Color, Material>();

        public static Material Material(Color color)
        {
            if (Cache.TryGetValue(color, out var existing))
                return existing;
            if (_src == null)
            {
                var sh = Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Unlit/Color")
                         ?? Shader.Find("Sprites/Default")
                         ?? Shader.Find("Hidden/InternalErrorShader");
                _src = new Material(sh);
                if (_src.HasProperty("_Smoothness")) _src.SetFloat("_Smoothness", 0.72f);
                if (_src.HasProperty("_Metallic")) _src.SetFloat("_Metallic", 0.05f);
            }
            var m = new Material(_src) { color = color };
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            Cache[color] = m;
            return m;
        }
    }
}
