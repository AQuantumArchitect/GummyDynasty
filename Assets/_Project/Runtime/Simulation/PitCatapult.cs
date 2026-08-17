using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>Pit catapult. Host HUD and later phones call the same tendrils. Not bound to G.</summary>
    public sealed class PitCatapult : Machine
    {
        Transform _yaw;
        Transform _armPivot;
        Transform _spoon;
        GummyBody _payloadBody;
        Tossable _payloadToss;
        Vector3 _aimAt;
        bool _hasAimAt;

        public void Build()
        {
            Label = "Catapult";
            var wood = new Color(0.48f, 0.3f, 0.16f);
            var dark = new Color(0.32f, 0.18f, 0.1f);

            var slab = Prim(PrimitiveType.Cube, "Base", new Vector3(0f, 0.2f, 0f), new Vector3(1.5f, 0.4f, 1.2f), wood);
            slab.transform.SetParent(transform, false);

            _yaw = new GameObject("Yaw").transform;
            _yaw.SetParent(transform, false);
            _yaw.localPosition = new Vector3(0f, 0.42f, 0f);

            var upright = Prim(PrimitiveType.Cube, "Upright", new Vector3(0f, 0.4f, 0.15f), new Vector3(0.22f, 0.85f, 0.22f), dark);
            upright.transform.SetParent(_yaw, false);

            _armPivot = new GameObject("ArmPivot").transform;
            _armPivot.SetParent(_yaw, false);
            _armPivot.localPosition = new Vector3(0f, 0.62f, 0.05f);

            var arm = Prim(PrimitiveType.Cube, "Arm", new Vector3(0f, 0f, -0.95f), new Vector3(0.16f, 0.16f, 1.9f), wood);
            arm.transform.SetParent(_armPivot, false);

            _spoon = Prim(PrimitiveType.Sphere, "Spoon", new Vector3(0f, 0.08f, -1.85f), Vector3.one * 0.46f, new Color(0.9f, 0.22f, 0.28f)).transform;
            _spoon.SetParent(_armPivot, false);

            var sign = new GameObject("Sign");
            sign.transform.SetParent(transform, false);
            sign.transform.localPosition = new Vector3(0f, 1.7f, 0.2f);
            var tm = sign.AddComponent<TextMesh>();
            tm.text = "CATAPULT";
            tm.fontSize = 48;
            tm.characterSize = 0.08f;
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
            _payloadBody = null;
            _payloadToss = null;
        }

        public void LoadBody(GummyBody body)
        {
            _payloadBody = body;
            _payloadToss = null;
            Controls.Pulse(MachineControls.Load);
            Controls.PayloadId = body != null ? body.AgentId : null;
        }

        public void LoadTossable(Tossable toss)
        {
            _payloadBody = null;
            _payloadToss = toss;
            Controls.Pulse(MachineControls.Load);
            Controls.PayloadId = toss != null ? toss.Label : null;
        }

        public void LoadStone()
        {
            _payloadBody = null;
            _payloadToss = null;
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
            Controls.Set(MachineControls.Aim, Mathf.Clamp(angle / 55f, -1f, 1f));
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
            if (_yaw == null || _armPivot == null)
                return;
            _yaw.localRotation = Quaternion.Euler(0f, Controls.AimValue * 55f, 0f);
            _armPivot.localRotation = Quaternion.Euler(Mathf.Lerp(16f, -50f, Controls.DrawValue), 0f, 0f);
        }

        void LateUpdate() => ApplyPose();

        void FireNow()
        {
            var origin = _spoon != null ? _spoon.position : transform.position + Vector3.up + transform.forward;
            Vector3 dir;
            if (_hasAimAt)
                dir = CandyShot.BallisticDirection(origin, _aimAt);
            else
                dir = LaunchDirection();
            _hasAimAt = false;
            _payloadBody = null;
            _payloadToss = null;
            CandyShot.Spawn(origin, dir, null, PersonalityCatalog.Candy());
        }

        Vector3 LaunchDirection()
        {
            if (_spoon != null && _armPivot != null)
            {
                var dir = _spoon.position - _armPivot.position;
                dir.y += 0.55f;
                if (dir.sqrMagnitude > 0.01f)
                    return dir.normalized;
            }

            var fallback = transform.forward;
            fallback.y = 0.35f;
            return fallback.normalized;
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
