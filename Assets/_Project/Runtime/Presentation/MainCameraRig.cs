using UnityEngine;

namespace GummyDynasty.Presentation
{
    [RequireComponent(typeof(Camera))]
    public sealed class MainCameraRig : MonoBehaviour
    {
        [SerializeField] Vector3 defaultPosition = new Vector3(0f, 8f, -10f);
        [SerializeField] Vector3 lookTarget = Vector3.zero;

        void Reset()
        {
            transform.position = defaultPosition;
            transform.LookAt(lookTarget);
        }

        void Awake()
        {
            if (transform.position == Vector3.zero)
            {
                transform.position = defaultPosition;
                transform.LookAt(lookTarget);
            }
        }
    }
}
