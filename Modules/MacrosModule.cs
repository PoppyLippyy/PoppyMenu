using UnityEngine;

namespace PoppyMenu
{
    internal class MacrosModule : PoppyModule
    {
        internal override string Name => "Macros";

        private static string _newName = "";
        private static int _expanded = -1;
        private static Macro _pendingDelete;

        internal override void DrawMenu()
        {
            Widgets.SectionBegin("Macros");
            Widgets.Hint("A macro runs several custom actions at once. Add any items, buffs, currency, spawns, or features, then bind it to a key or pin it on Home.");
            GUILayout.BeginHorizontal();
            _newName = GUILayout.TextField(_newName ?? "", Theme.Search);
            if (GUILayout.Button("+ New Macro", Theme.Primary, GUILayout.Width(110)))
            {
                string name = string.IsNullOrWhiteSpace(_newName) ? "Macro " + (MacroStore.Macros.Count + 1) : _newName.Trim();
                MacroStore.Macros.Add(new Macro { Name = name });
                MacroStore.Save();
                _newName = "";
                _expanded = MacroStore.Macros.Count - 1;
            }
            GUILayout.EndHorizontal();
            Widgets.SectionEnd();

            if (MacroStore.Macros.Count == 0)
            {
                Widgets.Label("No macros yet. Name one above and click + New Macro.");
                return;
            }

            for (int i = 0; i < MacroStore.Macros.Count; i++)
            {
                Macro m = MacroStore.Macros[i];
                Widgets.SectionBegin(null);

                GUILayout.BeginHorizontal();
                string nn = GUILayout.TextField(m.Name ?? "", Theme.Search);
                if (nn != m.Name) { m.Name = nn; MacroStore.Save(); }
                if (GUILayout.Button(_expanded == i ? "v Edit" : "> Edit", Theme.Button, GUILayout.Width(64)))
                    _expanded = _expanded == i ? -1 : i;
                GUILayout.EndHorizontal();

                KeyCode key = BindStore.KeyFor("macro:" + m.Name);
                Widgets.Hint(m.Steps.Count + (m.Steps.Count == 1 ? " step" : " steps") +
                             (key != KeyCode.None ? "   bound to [" + key + "]" : ""));

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Run now", Theme.Primary)) MacroStore.Run(m.Name);
                Macro bindTarget = m;
                if (GUILayout.Button("Bind key", Theme.Button)) Rebind.Capture(kc => BindStore.Add(kc, "macro:" + bindTarget.Name));
                GUILayout.EndHorizontal();
                if (Rebind.IsActive) Widgets.Hint("Press a key or button to bind this macro. Esc cancels.");

                if (_expanded == i) DrawEditor(m);
                Widgets.SectionEnd();
            }

            if (_pendingDelete != null) { MacroStore.Macros.Remove(_pendingDelete); MacroStore.Save(); _pendingDelete = null; _expanded = -1; }
        }

        private static void DrawEditor(Macro m)
        {
            Widgets.Header("Steps (run top to bottom)");
            StepBuilder.Draw(s => { m.Steps.Add(s); MacroStore.Save(); });

            for (int k = 0; k < m.Steps.Count; k++)
            {
                int removeAt = k;
                GUILayout.BeginHorizontal();
                GUILayout.Label((k + 1) + ". " + m.Steps[k].Label, Theme.Label, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("X", Theme.Danger_, GUILayout.Width(28))) { m.Steps.RemoveAt(removeAt); MacroStore.Save(); break; }
                GUILayout.EndHorizontal();
            }
            if (m.Steps.Count == 0) Widgets.Hint("No steps yet. Use the buttons above to add any item, buff, spawn, feature, and more.");

            Widgets.Separator();
            Widgets.ConfirmButton("macro.delete." + m.Name, "Delete this macro", () => _pendingDelete = m);
        }
    }
}
