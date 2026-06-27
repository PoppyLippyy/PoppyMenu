using System;
using System.Collections.Generic;
using UnityEngine;

namespace PoppyMenu
{
    internal static class ListPicker
    {
        internal struct Row
        {
            internal string Label;
            internal Color Color;
            internal Action OnClick;
            internal Row(string label, Color color, Action onClick) { Label = label; Color = color; OnClick = onClick; }
        }

        private const int WindowId = 0x5B7A01;
        private static bool _open;
        private static string _title = "";
        private static string _search = "";
        private static Vector2 _scroll;
        private static List<Row> _rows = new List<Row>();
        private static Rect _rect = new Rect(600, 90, 340, 560);

        internal static bool IsOpen => _open;

        internal static void Open(string title, List<Row> rows)
        {
            _title = title;
            _rows = rows ?? new List<Row>();
            _search = "";
            _scroll = Vector2.zero;
            _open = true;
        }

        internal static void Close() => _open = false;

        internal static void Draw()
        {
            if (!_open) return;
            _rect = GUI.Window(WindowId, _rect, DrawWindow, "", Theme.Window);
        }

        private static void DrawWindow(int id)
        {
            if (Event.current.type == EventType.MouseDown
                && new Rect(_rect.width - 26f, _rect.height - 26f, 26f, 26f).Contains(Event.current.mousePosition))
            {
                _resizing = true;
                Event.current.Use();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(_title.ToUpperInvariant(), Theme.SubHeader);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", Theme.IconBtn, GUILayout.Width(24), GUILayout.Height(20))) Close();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.SetNextControlName("poppy_picker_search");
            _search = GUILayout.TextField(_search ?? "", Theme.Search);
            if (GUILayout.Button("clear", Theme.Button, GUILayout.Width(48))) _search = "";
            GUILayout.EndHorizontal();

            string filter = (_search ?? "").Trim();
            bool hasFilter = filter.Length > 0;

            _scroll = GUILayout.BeginScrollView(_scroll);
            int matched = 0;
            for (int i = 0; i < _rows.Count; i++)
            {
                Row row = _rows[i];
                if (hasFilter && row.Label.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                matched++;

                bool clicked = GUILayout.Button(row.Label, Theme.RowButton, GUILayout.Height(24));
                Rect r = GUILayoutUtility.GetLastRect();
                Theme.Fill(new Rect(r.x + 7f, r.y + (r.height - 12f) / 2f, 12f, 12f), row.Color);
                if (clicked) row.OnClick?.Invoke();
            }
            if (matched == 0) GUILayout.Label("No matches.", Theme.Hint);
            GUILayout.EndScrollView();

            GUILayout.Label($"{matched} of {_rows.Count} · Esc to close", Theme.Hint);

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                Close();
                Event.current.Use();
            }

            HandleResize();
            GUI.DragWindow(new Rect(0, 0, _rect.width, 24));
        }

        private static bool _resizing;
        private static void HandleResize()
        {
            var grip = new Rect(_rect.width - 18f, _rect.height - 18f, 16f, 16f);
            Theme.Fill(grip, Theme.Accent);
            Theme.Fill(new Rect(grip.x + 5f, grip.y + 10f, 9f, 2f), new Color(1, 1, 1, 0.55f));
            Theme.Fill(new Rect(grip.x + 10f, grip.y + 5f, 2f, 9f), new Color(1, 1, 1, 0.55f));

            if (!_resizing) return;

            float scale = Mathf.Max(0.1f, ModConfig.UiScale.Value);
            float mouseX = Input.mousePosition.x / scale;
            float mouseY = (Screen.height - Input.mousePosition.y) / scale;
            _rect.width = Mathf.Clamp(mouseX - _rect.x + 3f, 260f, 800f);
            _rect.height = Mathf.Clamp(mouseY - _rect.y + 3f, 240f, 900f);

            if (!Input.GetMouseButton(0)) _resizing = false;
        }
    }
}
