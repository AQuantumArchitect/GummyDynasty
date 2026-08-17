namespace GummyDynasty.Simulation
{
    public sealed class MachineTendril
    {
        public readonly string Name;
        public float Min;
        public float Max;
        public float Value;

        public MachineTendril(string name, float min, float max, float value)
        {
            Name = name;
            Min = min;
            Max = max;
            Value = value;
        }

        public void Set(float v)
        {
            if (v < Min) v = Min;
            else if (v > Max) v = Max;
            Value = v;
        }
    }

    /// <summary>
    /// Semantic machine controls. Host HUD and later phones call the same tendrils.
    /// Unity-free so the harness can prove load/draw/release without PhysX.
    /// </summary>
    public sealed class MachineControls
    {
        public const string Aim = "aim";
        public const string Draw = "draw";
        public const string Release = "release";
        public const string Load = "load";
        public const float ReleaseFloor = 0.15f;

        public readonly MachineTendril AimAxis = new MachineTendril(Aim, -1f, 1f, 0f);
        public readonly MachineTendril DrawAxis = new MachineTendril(Draw, 0f, 1f, 0f);
        public bool Loaded;
        public string PayloadId;

        public float AimValue => AimAxis.Value;
        public float DrawValue => DrawAxis.Value;

        public MachineTendril Named(string name)
        {
            if (name == Aim) return AimAxis;
            if (name == Draw) return DrawAxis;
            return null;
        }

        public void Set(string name, float value)
        {
            var t = Named(name);
            t?.Set(value);
        }

        public bool Pulse(string name)
        {
            if (name == Load)
            {
                Loaded = true;
                return true;
            }

            if (name == Release)
            {
                if (DrawAxis.Value < ReleaseFloor)
                    return false;
                DrawAxis.Set(0f);
                Loaded = false;
                PayloadId = null;
                return true;
            }

            return false;
        }
    }
}
