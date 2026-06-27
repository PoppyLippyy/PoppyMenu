using UnityEngine;

namespace PoppyMenu
{
    internal class ConsoleModule : PoppyModule
    {
        internal override string Name => "Console";

        private static string _input = "";
        private static string _filter = "";
        private static Vector2 _logScroll;

        internal override void DrawMenu()
        {
            Widgets.SectionBegin("Console");
            Widgets.Hint("Type a command and hit Run. Unknown ones fall through to the game console. Type help for the list.");

            GUILayout.BeginHorizontal();
            GUI.SetNextControlName("poppyConsoleInput");
            _input = GUILayout.TextField(_input ?? "", Theme.Search);
            if (Widgets.Button("Run")) Submit();
            GUILayout.EndHorizontal();

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return
                && GUI.GetNameOfFocusedControl() == "poppyConsoleInput")
            {
                Submit();
                Event.current.Use();
            }

            _logScroll = GUILayout.BeginScrollView(_logScroll, GUILayout.Height(220));
            foreach (string line in PoppyConsole.History)
                GUILayout.Label(line, Theme.Hint);
            GUILayout.EndScrollView();
            if (Widgets.Button("Clear log")) PoppyConsole.History.Clear();
            Widgets.SectionEnd();

            Widgets.SectionBegin("Command reference");
            _filter = GUILayout.TextField(_filter ?? "", Theme.Search);
            foreach (string entry in PoppyConsole.CommandList())
            {
                if (!string.IsNullOrEmpty(_filter) && entry.IndexOf(_filter, System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                GUILayout.Label(entry, Theme.Hint);
            }
            Widgets.SectionEnd();
        }

        private static void Submit()
        {
            if (string.IsNullOrWhiteSpace(_input)) return;
            PoppyConsole.Submit(_input);
            _input = "";
            _logScroll.y = float.MaxValue;
        }
    }
}
