using GummyDynasty.Core;
using GummyDynasty.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GummyDynasty.Input
{
    /// <summary>Host-debug keys. Phones will never use this path.</summary>
    public sealed class ToyHostInput : MonoBehaviour
    {
        ToySandboxDirector _toy;

        void Start()
        {
            ServiceRegistry.Current?.TryGet(out _toy);
            if (_toy == null)
                _toy = FindFirstObjectByType<ToySandboxDirector>();
        }

        void Update()
        {
            if (_toy == null)
                return;

            var kb = Keyboard.current;
            var mouse = Mouse.current;
            if (kb != null)
            {
                if (kb.digit1Key.wasPressedThisFrame) _toy.SpawnDefault();
                if (kb.digit2Key.wasPressedThisFrame) _toy.SpawnKnight();
                if (kb.digit3Key.wasPressedThisFrame) _toy.SpawnScout();
                if (kb.spaceKey.wasPressedThisFrame) _toy.LaunchSelected();
                if (kb.kKey.wasPressedThisFrame) _toy.KnockSelected();
                if (kb.fKey.wasPressedThisFrame) _toy.FireProjectile();
                if (kb.bKey.wasPressedThisFrame) _toy.SmashWall();
                if (kb.rKey.wasPressedThisFrame) _toy.ResetArena();
                if (kb.f5Key.wasPressedThisFrame) _toy.BeginBench(8);
                if (kb.f6Key.wasPressedThisFrame) _toy.BeginBench(16);
                if (kb.f7Key.wasPressedThisFrame) _toy.BeginBench(32);
                if (kb.f8Key.wasPressedThisFrame) _toy.BeginBench(64);
            }

            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                _toy.SelectFromScreen(mouse.position.ReadValue());
        }
    }
}
