using System.Collections.Generic;
using UnityEngine;

namespace PoppyMenu
{
    internal static class Notify
    {
        private struct Toast { internal string Msg; internal float Until; }

        private static readonly List<Toast> _toasts = new List<Toast>();
        private static GUIStyle _style;

        internal static void Push(string msg, float seconds = 2.5f)
        {
            if (string.IsNullOrEmpty(msg)) return;
            _toasts.Add(new Toast { Msg = msg, Until = Time.realtimeSinceStartup + seconds });
            if (_toasts.Count > 5) _toasts.RemoveAt(0);
        }

        internal static void Draw()
        {
            if (_toasts.Count == 0) return;
            float now = Time.realtimeSinceStartup;
            _toasts.RemoveAll(t => t.Until <= now);
            if (_toasts.Count == 0) return;

            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.box)
                { fontSize = 12, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, padding = new RectOffset(14, 14, 6, 6), richText = true };
                _style.normal.textColor = Color.white;
                _style.normal.background = Theme.Solid(Theme.Accent);
            }

            const float w = 300f;
            float x = (Screen.width - w) / 2f;
            float y = 42f;
            for (int i = 0; i < _toasts.Count; i++)
            {
                GUI.Box(new Rect(x, y, w, 26f), _toasts[i].Msg, _style);
                y += 30f;
            }
        }
    }
}
