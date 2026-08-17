using UnityEngine;
using UnityEngine.InputSystem;

namespace GummyDynasty.Presentation
{
    [RequireComponent(typeof(Camera))]
    public sealed class MainCameraRig : MonoBehaviour
    {
        [SerializeField] Vector3 lookTarget = new Vector3(0f, 1.1f, 0f);
        [SerializeField] float distance = 10f;
        [SerializeField] float yaw = 18f;
        [SerializeField] float pitch = 26f;
        [SerializeField] float zoomSpeed = 6f;
        [SerializeField] Vector2 pitchLimits = new Vector2(12f, 70f);

        void OnEnable()
        {
            var cam = GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.38f, 0.66f, 0.9f);
            if (GetComponent<LogicalCrowdView>() == null)
                gameObject.AddComponent<LogicalCrowdView>();
            Place();
        }

        void LateUpdate()
        {
            var mouse = Mouse.current;
            if (mouse != null)
            {
                var scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                    distance = Mathf.Clamp(distance - scroll * 0.01f * zoomSpeed, 6f, 32f);

                if (mouse.rightButton.isPressed)
                {
                    var delta = mouse.delta.ReadValue();
                    yaw += delta.x * 0.18f;
                    pitch = Mathf.Clamp(pitch - delta.y * 0.18f, pitchLimits.x, pitchLimits.y);
                }
            }

            Place();
        }

        void Place()
        {
            var rot = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = lookTarget + rot * (Vector3.back * distance);
            transform.LookAt(lookTarget);
        }
    }
}
