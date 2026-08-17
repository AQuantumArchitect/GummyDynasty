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
                if (kb.spaceKey.wasPressedThisFrame || kb.vKey.wasPressedThisFrame) _toy.LaunchSelected();
                if (kb.kKey.wasPressedThisFrame) _toy.KnockSelected();
                if (kb.fKey.wasPressedThisFrame || kb.gKey.wasPressedThisFrame) _toy.FireProjectile();
                if (kb.cKey.wasPressedThisFrame) _toy.LobCatapult();
                if (kb.tKey.wasPressedThisFrame) _toy.DropCrate();
                if (kb.bKey.wasPressedThisFrame || kb.xKey.wasPressedThisFrame) _toy.SmashWall();
                if (kb.digit4Key.wasPressedThisFrame) _toy.BeginWestMarch();
                if (kb.digit7Key.wasPressedThisFrame) _toy.LoadShowcase(ShowcaseKind.Pit);
                if (kb.digit8Key.wasPressedThisFrame) _toy.LoadShowcase(ShowcaseKind.Castle);
                if (kb.digit9Key.wasPressedThisFrame) _toy.LoadShowcase(ShowcaseKind.Train);
                if (kb.backquoteKey.wasPressedThisFrame) _toy.ToggleResearchHud();
                if (_toy.ShowResearchHud && kb.digit5Key.wasPressedThisFrame) _toy.SeedLogicalArmy(1000);
                if (_toy.ShowResearchHud && kb.digit6Key.wasPressedThisFrame) _toy.EmbodyLogicals(8);
                if (kb.rKey.wasPressedThisFrame) _toy.ResetArena();
                if (kb.f5Key.wasPressedThisFrame) _toy.BeginBench(8);
                if (kb.f6Key.wasPressedThisFrame) _toy.BeginBench(16);
                if (kb.f7Key.wasPressedThisFrame) _toy.BeginBench(32);
                if (kb.f8Key.wasPressedThisFrame) _toy.BeginBench(64);
                if (_toy.ShowResearchHud && kb.f9Key.wasPressedThisFrame) _toy.BeginLogicalBench(1000);
            }

            if (mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame)
                    _toy.SelectFromScreen(mouse.position.ReadValue());
                if (mouse.middleButton.wasPressedThisFrame)
                    _toy.FireProjectile();
            }
        }
    }
}
