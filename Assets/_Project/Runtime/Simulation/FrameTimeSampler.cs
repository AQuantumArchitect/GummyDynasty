using System.Collections.Generic;
using UnityEngine;

namespace GummyDynasty.Simulation
{
    public sealed class FrameTimeSampler
    {
        readonly List<float> _ms = new List<float>(512);
        public bool Running { get; private set; }
        public int Count => _ms.Count;
        public float P50 { get; private set; }
        public float P95 { get; private set; }
        public float P99 { get; private set; }

        public void Begin()
        {
            _ms.Clear();
            Running = true;
            P50 = P95 = P99 = 0f;
        }

        public void Sample()
        {
            Add(Time.unscaledDeltaTime * 1000f);
        }

        public void Add(float ms)
        {
            if (!Running)
                return;
            _ms.Add(ms);
        }

        public void End()
        {
            Running = false;
            if (_ms.Count == 0)
                return;
            _ms.Sort();
            P50 = Percentile(0.50f);
            P95 = Percentile(0.95f);
            P99 = Percentile(0.99f);
        }

        float Percentile(float p)
        {
            var i = Mathf.Clamp(Mathf.RoundToInt((_ms.Count - 1) * p), 0, _ms.Count - 1);
            return _ms[i];
        }
    }
}
