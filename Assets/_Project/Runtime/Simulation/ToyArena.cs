using UnityEngine;

namespace GummyDynasty.Simulation
{
    public sealed class ToyArena : MonoBehaviour
    {
        public Transform GummyRoot { get; private set; }
        public Transform PropRoot { get; private set; }

        public void Build()
        {
            if (GummyRoot != null)
                return;

            GummyRoot = new GameObject("Gummies").transform;
            GummyRoot.SetParent(transform, false);
            PropRoot = new GameObject("Props").transform;
            PropRoot.SetParent(transform, false);

            Slab("Ground", new Vector3(0f, -0.25f, 0f), new Vector3(28f, 0.5f, 28f), new Color(0.18f, 0.22f, 0.2f), true);
            Slab("RailN", new Vector3(0f, 0.4f, 14f), new Vector3(28f, 1.2f, 0.4f), new Color(0.25f, 0.28f, 0.26f), true);
            Slab("RailS", new Vector3(0f, 0.4f, -14f), new Vector3(28f, 1.2f, 0.4f), new Color(0.25f, 0.28f, 0.26f), true);
            Slab("RailE", new Vector3(14f, 0.4f, 0f), new Vector3(0.4f, 1.2f, 28f), new Color(0.25f, 0.28f, 0.26f), true);
            Slab("RailW", new Vector3(-14f, 0.4f, 0f), new Vector3(0.4f, 1.2f, 28f), new Color(0.25f, 0.28f, 0.26f), true);
            Slab("Ramp", new Vector3(-6f, 0.6f, -4f), new Vector3(4f, 0.25f, 3.2f), new Color(0.32f, 0.26f, 0.18f), true)
                .transform.rotation = Quaternion.Euler(-18f, 35f, 0f);

            BuildCrateWall(new Vector3(4.5f, 0.35f, 3.5f), 5, 4);
        }

        public void ResetProps()
        {
            if (PropRoot == null)
                return;
            for (var i = PropRoot.childCount - 1; i >= 0; i--)
                Destroy(PropRoot.GetChild(i).gameObject);
            BuildCrateWall(new Vector3(4.5f, 0.35f, 3.5f), 5, 4);
        }

        public void SmashWall()
        {
            foreach (var part in GetComponentsInChildren<BreakablePart>())
                part.Detach();
        }

        void BuildCrateWall(Vector3 origin, int wide, int high)
        {
            const float s = 0.55f;
            for (var y = 0; y < high; y++)
            for (var x = 0; x < wide; x++)
            {
                var pos = origin + new Vector3((x - wide * 0.5f) * s, y * s + s * 0.5f, 0f);
                var crate = Slab("Crate", pos, Vector3.one * (s * 0.92f), new Color(0.72f, 0.55f, 0.28f), false);
                crate.transform.SetParent(PropRoot, true);
                var rb = crate.AddComponent<Rigidbody>();
                rb.mass = 1.1f;
                rb.linearDamping = 0.2f;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                crate.AddComponent<BreakablePart>();
            }
        }

        GameObject Slab(string name, Vector3 pos, Vector3 scale, Color color, bool staticCollider)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(transform, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = GummyLook.Material(color);
            if (staticCollider)
            {
                var rb = go.AddComponent<Rigidbody>();
                rb.isKinematic = true;
            }
            return go;
        }
    }
}
