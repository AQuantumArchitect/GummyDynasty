using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>Second machine on the same tendrils. Flatter shot than the catapult.</summary>
    public sealed class PitCannon : Machine
    {
        Transform _yaw;
        Transform _barrel;
        Transform _muzzle;
        Vector3 _aimAt;
        bool _hasAimAt;

        public void Build()
        {
            Label = "Cannon";
            var iron = new Color(0.22f, 0.22f, 0.24f);
            var brass = new Color(0.72f, 0.52f, 0.18f);

            var carriage = Prim(PrimitiveType.Cube, "Carriage", new Vector3(0f, 0.28f, 0f), new Vector3(1.15f, 0.28f, 0.7f), iron);
            carriage.transform.SetParent(transform, false);

            Wheel(new Vector3(-0.35f, 0.28f, 0.42f), iron);
            Wheel(new Vector3(-0.35f, 0.28f, -0.42f), iron);
            Wheel(new Vector3(0.35f, 0.28f, 0.42f), iron);
            Wheel(new Vector3(0.35f, 0.28f, -0.42f), iron);

            _yaw = new GameObject("Yaw").transform;
            _yaw.SetParent(transform, false);
            _yaw.localPosition = new Vector3(0f, 0.48f, 0f);

            var trunnion = Prim(PrimitiveType.Cube, "Trunnion", new Vector3(0f, 0.08f, 0f), new Vector3(0.28f, 0.22f, 0.55f), brass);
            trunnion.transform.SetParent(_yaw, false);

            _barrel = new GameObject("Barrel").transform;
            _barrel.SetParent(_yaw, false);
            _barrel.localPosition = new Vector3(0f, 0.12f, 0f);

            var tube = Prim(PrimitiveType.Cylinder, "Tube", new Vector3(0f, 0f, 0.85f), new Vector3(0.34f, 0.95f, 0.34f), iron);
            tube.transform.SetParent(_barrel, false);
            tube.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            _muzzle = Prim(PrimitiveType.Cylinder, "Muzzle", new Vector3(0f, 0f, 1.85f), new Vector3(0.4f, 0.12f, 0.4f), brass).transform;
            _muzzle.SetParent(_barrel, false);
            _muzzle.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var sign = new GameObject("Sign");
            sign.transform.SetParent(transform, false);
            sign.transform.localPosition = new Vector3(0f, 1.45f, 0.15f);
            var tm = sign.AddComponent<TextMesh>();
            tm.text = "CANNON";
            tm.fontSize = 48;
            tm.characterSize = 0.07f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.white;
            tm.fontStyle = FontStyle.Bold;

            transform.rotation = Quaternion.LookRotation(Vector3.left);
        }

        public void ResetPose()
        {
            Controls.Set(MachineControls.Aim, 0f);
            Controls.Set(MachineControls.Draw, 0f);
            Controls.Loaded = false;
            Controls.PayloadId = null;
        }

        public void LoadStone()
        {
            Controls.Pulse(MachineControls.Load);
            Controls.PayloadId = "candy";
        }

        public void AimToward(Vector3 worldDir)
        {
            var dir = worldDir;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f)
                return;
            dir.Normalize();
            var facing = transform.forward;
            facing.y = 0f;
            if (facing.sqrMagnitude < 0.01f)
                facing = Vector3.left;
            var angle = Vector3.SignedAngle(facing.normalized, dir, Vector3.up);
            Controls.Set(MachineControls.Aim, Mathf.Clamp(angle / 50f, -1f, 1f));
        }

        public void AimAtPoint(Vector3 worldPoint)
        {
            _aimAt = worldPoint;
            _hasAimAt = true;
            AimToward(worldPoint - transform.position);
        }

        public bool TryFire()
        {
            ApplyPose();
            if (!Controls.Pulse(MachineControls.Release))
                return false;
            FireNow();
            ApplyPose();
            return true;
        }

        void ApplyPose()
        {
            if (_yaw == null || _barrel == null)
                return;
            _yaw.localRotation = Quaternion.Euler(0f, Controls.AimValue * 50f, 0f);
            _barrel.localRotation = Quaternion.Euler(Mathf.Lerp(6f, -18f, Controls.DrawValue), 0f, 0f);
        }

        void LateUpdate() => ApplyPose();

        void FireNow()
        {
            var origin = _muzzle != null ? _muzzle.position : transform.position + Vector3.up * 0.6f + transform.forward;
            Vector3 dir;
            if (_hasAimAt)
                dir = CandyShot.DirectDirection(origin, _aimAt);
            else
            {
                dir = transform.forward;
                dir.y = 0.12f;
                if (dir.sqrMagnitude > 0.01f)
                    dir.Normalize();
            }

            _hasAimAt = false;
            CandyShot.Spawn(origin, dir, null, PersonalityCatalog.Candy());
        }

        void Wheel(Vector3 local, Color color)
        {
            var w = Prim(PrimitiveType.Cylinder, "Wheel", local, new Vector3(0.55f, 0.08f, 0.55f), color);
            w.transform.SetParent(transform, false);
            w.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }

        static GameObject Prim(PrimitiveType type, string name, Vector3 localPos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = GummyLook.Material(color);
            var col = go.GetComponent<Collider>();
            if (col != null)
                Object.Destroy(col);
            return go;
        }
    }
}
