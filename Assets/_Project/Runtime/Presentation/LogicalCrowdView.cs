using GummyDynasty.Core;
using GummyDynasty.Simulation;
using UnityEngine;
using UnityEngine.Rendering;

namespace GummyDynasty.Presentation
{
    /// <summary>Research overlay. Off unless the host seeds a logical army.</summary>
    public sealed class LogicalCrowdView : MonoBehaviour
    {
        LogicalDirector _logic;
        Mesh _mesh;
        Material _mat;
        readonly Vector3[] _pos = new Vector3[LogicalPopulation.MaxCapacity];
        readonly Matrix4x4[] _batch = new Matrix4x4[1023];

        void Start()
        {
            ServiceRegistry.Current?.TryGet(out _logic);
            if (_logic == null)
                _logic = FindFirstObjectByType<LogicalDirector>();

            var tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _mesh = tmp.GetComponent<MeshFilter>().sharedMesh;
            Destroy(tmp);

            var sh = Shader.Find("Universal Render Pipeline/Unlit")
                     ?? Shader.Find("Universal Render Pipeline/Lit")
                     ?? Shader.Find("Unlit/Color")
                     ?? Shader.Find("Sprites/Default");
            _mat = new Material(sh);
            _mat.enableInstancing = true;
            var color = new Color(1f, 0.38f, 0.22f, 0.92f);
            if (_mat.HasProperty("_BaseColor")) _mat.SetColor("_BaseColor", color);
            if (_mat.HasProperty("_Color")) _mat.SetColor("_Color", color);
        }

        void LateUpdate()
        {
            if (_logic == null)
            {
                ServiceRegistry.Current?.TryGet(out _logic);
                if (_logic == null)
                    _logic = FindFirstObjectByType<LogicalDirector>();
            }
            if (_logic == null || !_logic.ShowGhosts || _mesh == null || _mat == null)
                return;
            var n = _logic.Population.CopyDisembodied(_pos);
            if (n == 0)
                return;

            var scale = Vector3.one * 0.32f;
            var drawn = 0;
            while (drawn < n)
            {
                var batch = Mathf.Min(1023, n - drawn);
                for (var i = 0; i < batch; i++)
                    _batch[i] = Matrix4x4.TRS(_pos[drawn + i], Quaternion.identity, scale);
                Graphics.DrawMeshInstanced(_mesh, 0, _mat, _batch, batch, null, ShadowCastingMode.Off, false);
                drawn += batch;
            }
        }

        void OnDestroy()
        {
            if (_mat != null)
                Destroy(_mat);
        }
    }
}
