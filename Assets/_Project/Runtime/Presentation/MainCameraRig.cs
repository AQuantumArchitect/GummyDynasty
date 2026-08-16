using UnityEngine;
using UnityEngine.InputSystem;

namespace GummyDynasty.Presentation
{
    [RequireComponent(typeof(Camera))]
    public sealed class MainCameraRig : MonoBehaviour
    {
        [SerializeField] Vector3 lookTarget = new Vector3(0f, 0.8f, 0f);
        [SerializeField] float distance = 14f;
        [SerializeField] float yaw = 20f;
        [SerializeField] float pitch = 28f;
        [SerializeField] float zoomSpeed = 6f;
        [SerializeField] Vector2 pitchLimits = new Vector2(12f, 70f);

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

            var rot = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = lookTarget + rot * (Vector3.back * distance);
            transform.LookAt(lookTarget);
        }
    }
}
