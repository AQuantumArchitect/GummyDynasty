using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>Connected crate stack. Smash wall, crate pile, and the tower are the same primitive.</summary>
    public static class BreakableStack
    {
        public const float Size = 0.55f;
        public const float JointBreakForce = 22f;
        public const float JointBreakTorque = 16f;

        public static Transform Build(Transform parent, string name, Vector3 origin, int wide, int high, int deep = 1)
        {
            var group = new GameObject(name);
            group.transform.SetParent(parent, false);

            var footing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            footing.name = "Footing";
            footing.transform.SetParent(group.transform, false);
            footing.transform.position = origin + new Vector3(0f, 0.04f, 0f);
            footing.transform.localScale = new Vector3(wide * Size + 0.25f, 0.08f, deep * Size + 0.25f);
            footing.GetComponent<Renderer>().sharedMaterial = GummyLook.Material(new Color(0.34f, 0.26f, 0.18f));
            footing.GetComponent<Collider>().sharedMaterial = CrateMat();
            var footRb = footing.AddComponent<Rigidbody>();
            footRb.isKinematic = true;

            var cells = new Rigidbody[wide, high, deep];
            const float s = Size;
            for (var y = 0; y < high; y++)
            for (var z = 0; z < deep; z++)
            for (var x = 0; x < wide; x++)
            {
                var pos = origin + new Vector3((x - (wide - 1) * 0.5f) * s, y * s + s * 0.5f + 0.08f, (z - (deep - 1) * 0.5f) * s);
                var crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crate.name = "Crate";
                crate.transform.SetParent(group.transform, true);
                crate.transform.position = pos;
                crate.transform.localScale = Vector3.one * (s * 0.92f);
                crate.GetComponent<Renderer>().sharedMaterial = GummyLook.Material(new Color(0.72f, 0.55f, 0.28f));
                crate.GetComponent<Collider>().sharedMaterial = CrateMat();

                var rb = crate.AddComponent<Rigidbody>();
                rb.mass = 1.1f;
                rb.linearDamping = 0.85f;
                rb.angularDamping = 2.8f;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                crate.AddComponent<BreakablePart>();
                cells[x, y, z] = rb;

                var below = y == 0 ? footRb : cells[x, y - 1, z];
                Pin(rb, below);
                if (x > 0)
                    Pin(rb, cells[x - 1, y, z]);
                if (z > 0)
                    Pin(rb, cells[x, y, z - 1]);
            }

            return group.transform;
        }

        static void Pin(Rigidbody a, Rigidbody b)
        {
            if (a == null || b == null)
                return;
            var joint = a.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = b;
            joint.breakForce = JointBreakForce;
            joint.breakTorque = JointBreakTorque;
            joint.enableCollision = false;
        }

        static PhysicsMaterial _crateMat;

        static PhysicsMaterial CrateMat()
        {
            if (_crateMat != null)
                return _crateMat;
            _crateMat = new PhysicsMaterial("StackCrate")
            {
                bounciness = 0.04f,
                dynamicFriction = 0.88f,
                staticFriction = 0.96f,
                bounceCombine = PhysicsMaterialCombine.Minimum,
                frictionCombine = PhysicsMaterialCombine.Maximum
            };
            return _crateMat;
        }
    }
}
