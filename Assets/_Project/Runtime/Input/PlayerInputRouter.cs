using UnityEngine;
using UnityEngine.InputSystem;

namespace GummyDynasty.Input
{
    /// <summary>Thin Input System facade. Bind actions here; Simulation consumes intents, not raw devices.</summary>
    public sealed class PlayerInputRouter : MonoBehaviour
    {
        [SerializeField] InputActionAsset actions;

        InputAction _move;
        InputAction _confirm;
        InputAction _cancel;

        public Vector2 Move { get; private set; }
        public bool ConfirmPressed { get; private set; }
        public bool CancelPressed { get; private set; }

        void OnEnable()
        {
            if (actions == null)
                return;

            var map = actions.FindActionMap("Player", throwIfNotFound: false);
            if (map == null)
                return;

            _move = map.FindAction("Move");
            _confirm = map.FindAction("Confirm");
            _cancel = map.FindAction("Cancel");
            map.Enable();
        }

        void OnDisable()
        {
            actions?.FindActionMap("Player")?.Disable();
        }

        void Update()
        {
            Move = _move != null ? _move.ReadValue<Vector2>() : Vector2.zero;
            ConfirmPressed = _confirm != null && _confirm.WasPressedThisFrame();
            CancelPressed = _cancel != null && _cancel.WasPressedThisFrame();
        }
    }
}
