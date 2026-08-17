using System.Collections.Generic;
using UnityEngine;

namespace GummyDynasty.Harness
{
    /// <summary>
    /// Tiny contact world for behavior tests. Not PhysX, not hop feel.
    /// Owns: occupancy, wall blocking, gap passage, ballistic hits,
    /// stack support, kinematic decks. Feel stays a human Play job.
    /// </summary>
    sealed class ToyWorld
    {
        public const float Gravity = 9.81f;
        public float GroundY = 0f;
        public readonly List<LabBody> Bodies = new List<LabBody>(64);

        public LabBody AddStaticBox(string name, Vector3 center, Vector3 half)
        {
            var b = new LabBody
            {
                Id = Bodies.Count,
                Name = name,
                Pos = center,
                Half = half,
                IsBox = true,
                Static = true,
                Mass = 0f
            };
            Bodies.Add(b);
            return b;
        }

        public LabBody AddKinematicBox(string name, Vector3 center, Vector3 half)
        {
            var b = AddStaticBox(name, center, half);
            b.Static = false;
            b.Kinematic = true;
            b.Mass = 0f;
            return b;
        }

        public LabBody AddBox(string name, Vector3 center, Vector3 half, float mass)
        {
            var b = new LabBody
            {
                Id = Bodies.Count,
                Name = name,
                Pos = center,
                Half = half,
                IsBox = true,
                Mass = mass
            };
            Bodies.Add(b);
            return b;
        }

        public LabBody AddSphere(string name, Vector3 center, float radius, float mass)
        {
            var b = new LabBody
            {
                Id = Bodies.Count,
                Name = name,
                Pos = center,
                Radius = radius,
                Mass = mass
            };
            Bodies.Add(b);
            return b;
        }

        public void Step(float dt)
        {
            if (dt <= 0f)
                return;
            var sub = dt;
            var n = 1;
            if (dt > 1f / 120f)
            {
                n = Mathf.Max(1, (int)System.Math.Ceiling(dt * 120f));
                sub = dt / n;
            }
            for (var i = 0; i < n; i++)
                Substep(sub);
        }

        public void StepSeconds(float seconds, float dt = 1f / 60f)
        {
            var t = 0f;
            while (t < seconds - 1e-6f)
            {
                Step(dt);
                t += dt;
            }
        }

        void Substep(float dt)
        {
            for (var i = 0; i < Bodies.Count; i++)
            {
                var b = Bodies[i];
                if (!b.Alive || b.Static)
                    continue;
                if (b.Kinematic)
                {
                    b.LastDelta = b.Vel * dt;
                    b.Pos += b.LastDelta;
                    continue;
                }
                b.LastDelta = Vector3.zero;
                b.Vel.y -= Gravity * dt;
                b.Vel *= 1f / (1f + b.LinearDamp * dt);
                b.Pos += b.Vel * dt;
            }

            CarryRiders();

            for (var iter = 0; iter < 6; iter++)
            {
                for (var i = 0; i < Bodies.Count; i++)
                {
                    var a = Bodies[i];
                    if (!a.Alive)
                        continue;
                    Ground(a, applyFriction: false);
                    for (var j = i + 1; j < Bodies.Count; j++)
                    {
                        var b = Bodies[j];
                        if (!b.Alive)
                            continue;
                        Contact(a, b);
                    }
                }
            }

            for (var i = 0; i < Bodies.Count; i++)
                Ground(Bodies[i], applyFriction: true);
            CarryRiders();
        }

        void CarryRiders()
        {
            for (var i = 0; i < Bodies.Count; i++)
            {
                var deck = Bodies[i];
                if (!deck.Alive || !deck.Kinematic || !deck.IsBox)
                    continue;
                var top = deck.Pos.y + deck.Half.y;
                for (var j = 0; j < Bodies.Count; j++)
                {
                    var s = Bodies[j];
                    if (!s.Alive || s.IsBox || s.Kinematic || s.Static)
                        continue;
                    var dx = Mathf.Abs(s.Pos.x - deck.Pos.x);
                    var dz = Mathf.Abs(s.Pos.z - deck.Pos.z);
                    if (dx > deck.Half.x + s.Radius * 0.25f)
                        continue;
                    if (dz > deck.Half.z + s.Radius * 0.25f)
                        continue;
                    var bottom = s.Pos.y - s.Radius;
                    if (bottom < top - 0.08f || bottom > top + 0.18f)
                        continue;
                    s.Pos.x += deck.LastDelta.x;
                    s.Pos.z += deck.LastDelta.z;
                    s.Vel.x = deck.Vel.x;
                    s.Vel.z = deck.Vel.z;
                }
            }
        }

        void Ground(LabBody b, bool applyFriction)
        {
            if (b.Static || b.Kinematic)
                return;
            var bottom = b.IsBox ? b.Pos.y - b.Half.y : b.Pos.y - b.Radius;
            if (bottom >= GroundY)
                return;
            var pen = GroundY - bottom;
            b.Pos.y += pen;
            if (b.Vel.y < 0f)
                b.Vel.y *= -b.Bounce;
            if (applyFriction)
            {
                b.Vel.x *= 0.94f;
                b.Vel.z *= 0.94f;
            }
            b.Contacts++;
        }

        void Contact(LabBody a, LabBody b)
        {
            if (a.IsBox && b.IsBox)
                BoxBox(a, b);
            else if (!a.IsBox && !b.IsBox)
                SphereSphere(a, b);
            else if (a.IsBox)
                SphereBox(b, a);
            else
                SphereBox(a, b);
        }

        void SphereSphere(LabBody a, LabBody b)
        {
            var d = a.Pos - b.Pos;
            var min = a.Radius + b.Radius;
            var mag = d.magnitude;
            if (mag >= min || mag < 1e-6f)
                return;
            var n = d / mag;
            Separate(a, b, n, min - mag, a.Bounce < b.Bounce ? a.Bounce : b.Bounce);
        }

        void SphereBox(LabBody s, LabBody box)
        {
            var min = box.Pos - box.Half;
            var max = box.Pos + box.Half;
            var closest = Vector3.Clamp(s.Pos, min, max);
            var d = s.Pos - closest;
            var mag = d.magnitude;
            Vector3 n;
            float pen;
            if (mag < 1e-5f)
            {
                var toMin = s.Pos - min;
                var toMax = max - s.Pos;
                n = Vector3.right;
                pen = toMin.x;
                if (toMax.x < pen) { pen = toMax.x; n = Vector3.left; }
                if (toMin.y < pen) { pen = toMin.y; n = Vector3.up; }
                if (toMax.y < pen) { pen = toMax.y; n = new Vector3(0f, -1f, 0f); }
                if (toMin.z < pen) { pen = toMin.z; n = new Vector3(0f, 0f, 1f); }
                if (toMax.z < pen) { pen = toMax.z; n = new Vector3(0f, 0f, -1f); }
                pen += s.Radius;
            }
            else
            {
                if (mag >= s.Radius)
                    return;
                n = d / mag;
                pen = s.Radius - mag;
            }
            var bounce = s.Bounce < box.Bounce ? s.Bounce : box.Bounce;
            Separate(s, box, n, pen, bounce);
            if (box.Kinematic && n.y > 0.45f)
            {
                s.Pos.x += box.LastDelta.x;
                s.Pos.z += box.LastDelta.z;
                s.Vel.x = s.Vel.x * 0.15f + box.Vel.x * 0.85f;
                s.Vel.z = s.Vel.z * 0.15f + box.Vel.z * 0.85f;
                if (s.Vel.y < 0f)
                    s.Vel.y = 0f;
            }
        }

        void BoxBox(LabBody a, LabBody b)
        {
            var d = a.Pos - b.Pos;
            var ox = a.Half.x + b.Half.x - Mathf.Abs(d.x);
            var oy = a.Half.y + b.Half.y - Mathf.Abs(d.y);
            var oz = a.Half.z + b.Half.z - Mathf.Abs(d.z);
            if (ox <= 0f || oy <= 0f || oz <= 0f)
                return;
            Vector3 n;
            float pen;
            if (ox < oy && ox < oz)
            {
                pen = ox;
                n = d.x >= 0f ? Vector3.right : Vector3.left;
            }
            else if (oy < oz)
            {
                pen = oy;
                n = d.y >= 0f ? Vector3.up : new Vector3(0f, -1f, 0f);
            }
            else
            {
                pen = oz;
                n = d.z >= 0f ? new Vector3(0f, 0f, 1f) : new Vector3(0f, 0f, -1f);
            }
            Separate(a, b, n, pen, a.Bounce < b.Bounce ? a.Bounce : b.Bounce);
        }

        void Separate(LabBody a, LabBody b, Vector3 n, float pen, float bounce)
        {
            var invA = InverseMass(a);
            var invB = InverseMass(b);
            var inv = invA + invB;
            if (inv < 1e-8f)
                return;
            if (!a.Static && !a.Kinematic)
                a.Pos += n * (pen * (invA / inv));
            if (!b.Static && !b.Kinematic)
                b.Pos -= n * (pen * (invB / inv));

            var rel = a.Vel - b.Vel;
            var vn = Vector3.Dot(rel, n);
            if (vn < 0f)
            {
                var j = -(1f + bounce) * vn / inv;
                if (!a.Static && !a.Kinematic)
                    a.Vel += n * (j * invA);
                if (!b.Static && !b.Kinematic)
                    b.Vel -= n * (j * invB);
            }

            var t = rel - n * Vector3.Dot(rel, n);
            var tMag = t.magnitude;
            if (tMag > 1e-5f)
            {
                t /= tMag;
                var jt = -Vector3.Dot(a.Vel - b.Vel, t) / inv;
                var mu = 0.55f;
                var maxF = mu * Mathf.Max(0f, -(1f + bounce) * vn / inv);
                if (jt > maxF) jt = maxF;
                if (jt < -maxF) jt = -maxF;
                if (!a.Static && !a.Kinematic)
                    a.Vel += t * (jt * invA);
                if (!b.Static && !b.Kinematic)
                    b.Vel -= t * (jt * invB);
            }

            a.Contacts++;
            b.Contacts++;
        }

        static float InverseMass(LabBody b)
        {
            if (!b.Alive || b.Static || b.Kinematic || b.Mass <= 1e-5f)
                return 0f;
            return 1f / b.Mass;
        }
    }

    sealed class LabBody
    {
        public int Id;
        public string Name = "";
        public Vector3 Pos;
        public Vector3 Vel;
        public Vector3 Half;
        public float Radius = 0.3f;
        public float Mass = 1f;
        public float Bounce = 0.05f;
        public float LinearDamp = 0.18f;
        public bool IsBox;
        public bool Static;
        public bool Kinematic;
        public bool Alive = true;
        public int Contacts;
        public Vector3 LastDelta;
    }
}
