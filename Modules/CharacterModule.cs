using System.Collections.Generic;
using UnityEngine;

namespace PoppyMenu
{
    internal class CharacterModule : PoppyModule
    {
        internal override string Name => "Character";

        internal override void DrawMenu()
        {
            Widgets.SectionBegin("Change Character");
            Widgets.Hint("Respawns you here as whatever you pick. Survivors and monsters both.");

            List<ListPicker.Row> rows = new List<ListPicker.Row>(Catalogs.Bodies.Count);
            foreach (Catalogs.BodyEntry entry in Catalogs.Bodies)
            {
                if (entry.Prefab == null) continue;
                Catalogs.BodyEntry e = entry;
                rows.Add(new ListPicker.Row(e.Name, Color.white, () =>
                {
                    Vector3 p = PlayerContext.HasBody ? PlayerContext.Body.footPosition : Vector3.zero;
                    NetUtil.Do(PoppyOp.ChangeBody, f1: p.x, f2: p.y, f3: p.z, s1: e.Prefab.name);
                }));
            }
            Widgets.PickerButton("Change Character / Play As...", "Bodies", rows);
            Widgets.SectionEnd();
        }
    }
}
