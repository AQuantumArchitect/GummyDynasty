using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>Three kinematic cars. Same pit primitives ride this — not a new brain.</summary>
    public sealed class TrainRig : MonoBehaviour
    {
        public Transform Head { get; private set; }
        public Transform Mid { get; private set; }
        public Transform Tail { get; private set; }
        public Vector3 Origin;
        public float Speed = 1.05f;
        public float Travel = 9.5f;

        float _t;
        Rigidbody _headRb;
        Rigidbody _midRb;
        Rigidbody _tailRb;

        public Vector3 FlagWorld => Head != null
            ? Head.position + new Vector3(-1.7f, 0.25f, 0f)
            : Origin + Vector3.left * 4f;

        public Vector3 TailDeck => Tail != null
            ? Tail.position + Vector3.up * 0.55f
            : Origin + new Vector3(4f, 0.55f, 0f);

        public void Build(Transform parent, Vector3 origin)
        {
            Origin = origin;
            transform.SetParent(parent, false);
            transform.position = origin;

            Head = Car("HeadCar", new Vector3(-4.2f, 0.38f, 0f), new Color(0.86f, 0.18f, 0.28f), out _headRb);
            Mid = Car("MidCar", new Vector3(0f, 0.38f, 0f), new Color(0.95f, 0.45f, 0.16f), out _midRb);
            Tail = Car("TailCar", new Vector3(4.2f, 0.38f, 0f), new Color(0.28f, 0.42f, 0.78f), out _tailRb);
            Head.gameObject.AddComponent<MovingDeck>();
            Mid.gameObject.AddComponent<MovingDeck>();
            Tail.gameObject.AddComponent<MovingDeck>();

            var sign = new GameObject("Sign");
            sign.transform.SetParent(Head, false);
            sign.transform.localPosition = new Vector3(-2.1f, 1.4f, 0f);
            var tm = sign.AddComponent<TextMesh>();
            tm.text = "TRAIN";
            tm.fontSize = 48;
            tm.characterSize = 0.08f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.white;
            tm.fontStyle = FontStyle.Bold;
        }

        public Vector3 RankedSpawn(int i)
        {
            var deck = TailDeck;
            return deck + new Vector3((i % 2) * 0.55f, 0.9f, (i - 2) * 0.45f);
        }

        void FixedUpdate()
        {
            if (Head == null)
                return;
            var prev = Mid != null ? Mid.position : transform.position;
            _t += Time.fixedDeltaTime * Speed;
            var x = Origin.x - Mathf.PingPong(_t, Travel);
            Place(_headRb, Head, new Vector3(x - 4.2f, Origin.y + 0.38f, Origin.z));
            Place(_midRb, Mid, new Vector3(x, Origin.y + 0.38f, Origin.z));
            Place(_tailRb, Tail, new Vector3(x + 4.2f, Origin.y + 0.38f, Origin.z));
            var delta = (Mid != null ? Mid.position : transform.position) - prev;
            CarryCargo(delta);
        }

        void CarryCargo(Vector3 delta)
        {
            delta.y = 0f;
            if (delta.sqrMagnitude < 1e-8f || Mid == null)
                return;
            var wall = Mid.Find("SmashWall");
            if (wall == null)
                return;
            var rbs = wall.GetComponentsInChildren<Rigidbody>();
            for (var i = 0; i < rbs.Length; i++)
            {
                var rb = rbs[i];
                if (rb == null)
                    continue;
                var part = rb.GetComponent<BreakablePart>();
                if (part != null && part.Detached)
                    continue;
                rb.MovePosition(rb.position + delta);
            }
        }

        static void Place(Rigidbody rb, Transform t, Vector3 pos)
        {
            if (rb != null)
                rb.MovePosition(pos);
            else if (t != null)
                t.position = pos;
        }

        Transform Car(string name, Vector3 local, Color color, out Rigidbody rb)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(transform, false);
            go.transform.localPosition = local;
            go.transform.localScale = new Vector3(3.8f, 0.42f, 1.85f);
            go.GetComponent<Renderer>().sharedMaterial = GummyLook.Material(color);
            rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            return go.transform;
        }
    }
}
