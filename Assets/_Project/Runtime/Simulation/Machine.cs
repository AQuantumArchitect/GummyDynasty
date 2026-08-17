using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>Physical machine. Tendrils are named; do not bind one to a single key.</summary>
    public class Machine : MonoBehaviour
    {
        public string Label = "Machine";
        public MachineControls Controls { get; } = new MachineControls();

        public void SetTendril(string name, float value) => Controls.Set(name, value);

        public bool PulseTendril(string name) => Controls.Pulse(name);
    }
}
