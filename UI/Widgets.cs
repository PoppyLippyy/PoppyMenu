using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace PoppyMenu
{
    internal static class Widgets
    {
        private static GUIStyle _chip;
        private static GUIStyle Chip
        {
            get
            {
                if (_chip == null)
                    _chip = new GUIStyle(GUI.skin.label)
                    { fontSize = 10, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                _chip.normal.textColor = Color.white;
                return _chip;
            }
        }

        internal static void Header(string text)
        {
            GUILayout.Space(2);
            GUILayout.Label(text.ToUpperInvariant(), Theme.SubHeader);
        }

        internal static void Label(string text) => GUILayout.Label(text, Theme.Label);
        internal static void Hint(string text) => GUILayout.Label(text, Theme.Hint);

        internal static void Separator()
        {
            Rect r = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true));
            Theme.Fill(new Rect(r.x, r.y, r.width, 1), new Color(1, 1, 1, 0.06f));
        }

        internal static bool Button(string text) => GUILayout.Button(text, Theme.Button);
        internal static void Button(string text, Action onClick) { if (GUILayout.Button(text, Theme.Button)) onClick?.Invoke(); }
        internal static void PrimaryButton(string text, Action onClick) { if (GUILayout.Button(text, Theme.Primary)) onClick?.Invoke(); }
        internal static void DangerButton(string text, Action onClick) { if (GUILayout.Button(text, Theme.Danger_)) onClick?.Invoke(); }

        private static string _armed;
        internal static void ConfirmButton(string id, string text, Action onConfirm)
        {
            bool armed = _armed == id;
            if (GUILayout.Button(armed ? "Confirm, " + text + "?" : text, armed ? Theme.Danger_ : Theme.Button))
            {
                if (armed) { _armed = null; onConfirm?.Invoke(); }
                else _armed = id;
            }
        }

        internal static bool Toggle(string label, bool value) => Toggle(label, value, KeyCode.None);

        internal static bool Toggle(string label, bool value, KeyCode key)
        {
            bool clicked = GUILayout.Button(label, value ? Theme.SwitchOn : Theme.SwitchOff, GUILayout.Height(28));
            Rect r = GUILayoutUtility.GetLastRect();

            float x = r.xMax - 46f;
            if (key != KeyCode.None)
            {
                var kr = new Rect(r.xMax - 92f, r.y + 6f, 42f, 16f);
                GUI.Label(kr, key.ToString(), Theme.Hint);
            }
            var chip = new Rect(x, r.y + (r.height - 16f) / 2f, 38f, 16f);
            Theme.Fill(chip, value ? Theme.On : Theme.SlotOff);
            GUI.Label(chip, value ? "ON" : "OFF", Chip);

            return clicked ? !value : value;
        }

        internal static int IntStepper(string label, int value, int step, int min = int.MinValue, int max = int.MaxValue)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, Theme.Label, GUILayout.MinWidth(80));
            if (GUILayout.Button("-", Theme.Button, GUILayout.Width(30))) value = Mathf.Clamp(value - step, min, max);
            GUILayout.Label(value.ToString(), Theme.Label, GUILayout.Width(64));
            if (GUILayout.Button("+", Theme.Button, GUILayout.Width(30))) value = Mathf.Clamp(value + step, min, max);
            GUILayout.EndHorizontal();
            return value;
        }

        internal static float Slider(string label, float value, float min, float max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: {value:0.##}", Theme.Label, GUILayout.MinWidth(120));
            value = GUILayout.HorizontalSlider(value, min, max, GUILayout.MinWidth(110), GUILayout.Height(18));
            GUILayout.EndHorizontal();
            return value;
        }

        internal static int OpenSections;

        internal static void SectionBegin(string title)
        {
            GUILayout.BeginVertical(Theme.Card);
            OpenSections++;
            if (!string.IsNullOrEmpty(title))
                GUILayout.Label(title.ToUpperInvariant(), Theme.SubHeader);
        }

        internal static void SectionEnd()
        {
            if (OpenSections <= 0) return;
            GUILayout.EndVertical();
            OpenSections--;
        }

        internal static void KeybindRow(string label, ConfigEntry<KeyCode> entry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, Theme.Label, GUILayout.MinWidth(96));
            bool listening = Rebind.Listening == entry;
            if (GUILayout.Button(listening ? "press a key/button..." : entry.Value.ToString(),
                                 listening ? Theme.Primary : Theme.Button, GUILayout.Height(24)))
            {
                if (listening) Rebind.Cancel(); else Rebind.Capture(entry);
            }
            if (GUILayout.Button("X", Theme.Button, GUILayout.Width(24)))
            {
                entry.Value = KeyCode.None;
                if (listening) Rebind.Cancel();
            }
            GUILayout.EndHorizontal();
        }

        internal static void PickerButton(string text, string title, List<ListPicker.Row> rows)
        {
            if (GUILayout.Button(text, Theme.Button, GUILayout.Height(26)))
                ListPicker.Open(title, rows);
        }
    }
}
