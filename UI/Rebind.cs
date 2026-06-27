using System;
using BepInEx.Configuration;
using UnityEngine;

namespace PoppyMenu
{
    internal static class Rebind
    {
        internal static ConfigEntry<KeyCode> Listening;
        private static Action<KeyCode> _callback;

        internal static bool IsActive => Listening != null || _callback != null;

        internal static void Capture(ConfigEntry<KeyCode> entry) { Listening = entry; _callback = null; }
        internal static void Capture(Action<KeyCode> callback) { _callback = callback; Listening = null; }
        internal static void Cancel() { Listening = null; _callback = null; }

        private static void Apply(KeyCode kc)
        {
            if (Listening != null) Listening.Value = kc;
            else _callback?.Invoke(kc);
            Listening = null; _callback = null;
        }

        internal static void Poll()
        {
            if (!IsActive) return;

            if (Input.GetKeyDown(KeyCode.Escape)) { Cancel(); return; }

            for (int b = 1; b <= 6; b++)
                if (Input.GetMouseButtonDown(b)) { Apply((KeyCode)((int)KeyCode.Mouse0 + b)); return; }

            if (!Input.anyKeyDown) return;
            foreach (KeyCode kc in Enum.GetValues(typeof(KeyCode)))
            {
                if (kc == KeyCode.Escape) continue;
                int v = (int)kc;
                if (v >= (int)KeyCode.Mouse0 && v <= (int)KeyCode.Mouse6) continue;
                if (Input.GetKeyDown(kc)) { Apply(kc); return; }
            }
        }
    }
}
