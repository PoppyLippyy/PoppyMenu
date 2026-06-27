using System.Collections.Generic;
using System.IO;
using BepInEx;
using Newtonsoft.Json;
using UnityEngine;

namespace PoppyMenu
{
    internal static class BindStore
    {
        internal static List<Bind> Binds = new List<Bind>();

        private static string FilePath => Path.Combine(Paths.ConfigPath, "PoppyMenu.binds.json");

        internal static void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    Binds = JsonConvert.DeserializeObject<List<Bind>>(File.ReadAllText(FilePath)) ?? new List<Bind>();
            }
            catch (System.Exception e)
            {
                Log.Error("Bind load failed: " + e);
                Binds = new List<Bind>();
            }
        }

        internal static void Save()
        {
            try { File.WriteAllText(FilePath, JsonConvert.SerializeObject(Binds, Formatting.Indented)); }
            catch (System.Exception e) { Log.Error("Bind save failed: " + e); }
        }

        internal static void Add(KeyCode key, string actionId) { Binds.Add(new Bind { Key = key, ActionId = actionId }); Save(); }
        internal static void Remove(Bind b) { if (Binds.Remove(b)) Save(); }
        internal static void Clear() { Binds.Clear(); Save(); }

        internal static KeyCode KeyFor(string actionId)
        {
            foreach (Bind b in Binds)
                if (b != null && b.Key != KeyCode.None && b.ActionId == actionId) return b.Key;
            return KeyCode.None;
        }

        internal static void Poll()
        {
            if (Rebind.IsActive || MenuRoot.Visible || ListPicker.IsOpen) return;
            for (int i = 0; i < Binds.Count; i++)
            {
                Bind b = Binds[i];
                if (b != null && b.Key != KeyCode.None && Input.GetKeyDown(b.Key))
                    ActionRegistry.RunId(b.ActionId);
            }
        }
    }
}
