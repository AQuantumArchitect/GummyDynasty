using UnityEngine;

namespace GummyDynasty.Simulation
{
    /// <summary>
    /// World delta of a kinematic platform. Hoppers and still-attached
    /// cargo add this each physics step so a train is a floor, not a rug-pull.
    /// </summary>
    public sealed class MovingDeck : MonoBehaviour
    {
        public Vector3 Delta { get; private set; }
        public Vector3 Velocity { get; private set; }

        Vector3 _last;
        bool _hasLast;

        void FixedUpdate()
        {
            var p = transform.position;
            if (_hasLast)
            {
                Delta = p - _last;
                var dt = Time.fixedDeltaTime;
                Velocity = dt > 1e-5f ? Delta / dt : Vector3.zero;
            }
            else
            {
                Delta = Vector3.zero;
                Velocity = Vector3.zero;
            }

            _last = p;
            _hasLast = true;
        }
    }
}
