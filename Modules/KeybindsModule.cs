using System.Collections.Generic;
using UnityEngine;

namespace PoppyMenu
{
    internal class KeybindsModule : PoppyModule
    {
        internal override string Name => "Keybinds";

        internal override void DrawMenu()
        {
            Widgets.SectionBegin("Add a keybind");
            Widgets.Hint("Pick any feature, then press the key or mouse button you want. Bindings fire while the menu is closed.");
            Widgets.PickerButton("Bind a feature...", "Pick a feature", ActionRows());
            if (Rebind.IsActive) Widgets.Hint("Now press a key or button. Esc cancels.");
            Widgets.SectionEnd();

            Widgets.SectionBegin("Your keybinds");
            if (BindStore.Binds.Count == 0)
                Widgets.Hint("Nothing bound yet.");

            for (int i = BindStore.Binds.Count - 1; i >= 0; i--)
            {
                Bind b = BindStore.Binds[i];
                PoppyAction act = ActionRegistry.Get(b.ActionId);
                string label = act != null ? act.Category + ": " + act.Name : b.ActionId;

                GUILayout.BeginHorizontal();
                GUILayout.Label(label, Theme.Label, GUILayout.ExpandWidth(true));
                Bind captured = b;
                if (GUILayout.Button(b.Key == KeyCode.None ? "set key" : b.Key.ToString(), Theme.Button, GUILayout.Width(120)))
                    Rebind.Capture(kc => { captured.Key = kc; BindStore.Save(); });
                if (GUILayout.Button("X", Theme.Danger_, GUILayout.Width(28)))
                    BindStore.Remove(b);
                GUILayout.EndHorizontal();
            }

            if (BindStore.Binds.Count > 0)
                Widgets.ConfirmButton("keybinds.clear", "Clear all keybinds", BindStore.Clear);
            Widgets.SectionEnd();
        }

        private static List<ListPicker.Row> ActionRows()
        {
            var rows = new List<ListPicker.Row>(ActionRegistry.All.Count + MacroStore.Macros.Count);
            foreach (PoppyAction a in ActionRegistry.All)
            {
                PoppyAction act = a;
                rows.Add(new ListPicker.Row(act.Category + ": " + act.Name, Color.white,
                    () => Rebind.Capture(kc => BindStore.Add(kc, act.Id))));
            }
            foreach (Macro m in MacroStore.Macros)
            {
                Macro macro = m;
                rows.Add(new ListPicker.Row("Macro: " + macro.Name, Color.yellow,
                    () => Rebind.Capture(kc => BindStore.Add(kc, "macro:" + macro.Name))));
            }
            return rows;
        }
    }
}
