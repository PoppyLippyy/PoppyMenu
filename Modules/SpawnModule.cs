using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace PoppyMenu
{
    internal class SpawnModule : PoppyModule
    {
        internal override string Name => "Spawn";

        private static int _count = 1;
        private static bool _ally;

        private static Vector3 ComputePos()
        {
            if (Physics.Raycast(PlayerContext.AimRay(), out RaycastHit hit, 1000f))
                return hit.point;
            if (PlayerContext.HasBody)
                return PlayerContext.Body.footPosition + PlayerContext.Body.transform.forward * 5f;
            return Vector3.zero;
        }

        internal override void DrawMenu()
        {
            if (Catalogs.SpawnCards.Count == 0)
            {
                Widgets.Label("No spawn cards loaded, start a stage.");
                return;
            }

            var monsters = new List<ListPicker.Row>();
            var interactables = new List<ListPicker.Row>();
            foreach (var entry in Catalogs.SpawnCards)
            {
                string name = entry.Name;
                if (entry.IsInteractable)
                    interactables.Add(new ListPicker.Row(name, Color.cyan, () =>
                    {
                        Vector3 pos = ComputePos();
                        NetUtil.Do(PoppyOp.Spawn, i1: _count, i2: _ally ? (int)TeamIndex.Player : (int)TeamIndex.Monster, s1: name, f1: pos.x, f2: pos.y, f3: pos.z);
                    }));
                else
                    monsters.Add(new ListPicker.Row(name, Color.red, () =>
                    {
                        Vector3 pos = ComputePos();
                        NetUtil.Do(PoppyOp.Spawn, i1: _count, i2: _ally ? (int)TeamIndex.Player : (int)TeamIndex.Monster, s1: name, f1: pos.x, f2: pos.y, f3: pos.z);
                    }));
            }

            Widgets.SectionBegin("Spawn at Crosshair");
            _count = Widgets.IntStepper("Count", _count, 1, 1, 50);
            _ally = Widgets.Toggle("Spawn as ally (your team)", _ally);
            Widgets.PickerButton("Spawn Monster...", "Spawn Monster", monsters);
            Widgets.PickerButton("Spawn Interactable...", "Spawn Interactable", interactables);
            Widgets.Hint("Spawns where you aim, or just ahead of you.");
            Widgets.SectionEnd();

            Widgets.SectionBegin("Combat");
            Widgets.DangerButton("Kill All Enemies", () => NetUtil.Do(PoppyOp.KillAllEnemies));
            Widgets.SectionEnd();
        }
    }
}
