using System.Collections.Generic;
using System.IO;
using BepInEx;
using Newtonsoft.Json;

namespace PoppyMenu
{
    internal static class HomeLayoutStore
    {
        internal static List<CustomStep> Shortcuts = new List<CustomStep>();

        private static string FilePath => Path.Combine(Paths.ConfigPath, "PoppyMenu.home.json");

        internal static void Load()
        {
            try { if (File.Exists(FilePath)) Shortcuts = JsonConvert.DeserializeObject<List<CustomStep>>(File.ReadAllText(FilePath)) ?? new List<CustomStep>(); }
            catch (System.Exception e) { Log.Error("Home layout load failed: " + e); Shortcuts = new List<CustomStep>(); }
        }

        internal static void Save()
        {
            try { File.WriteAllText(FilePath, JsonConvert.SerializeObject(Shortcuts, Formatting.Indented)); }
            catch (System.Exception e) { Log.Error("Home layout save failed: " + e); }
        }

        internal static void Add(CustomStep s) { if (s != null) { Shortcuts.Add(s); Save(); } }
        internal static void Remove(CustomStep s) { if (Shortcuts.Remove(s)) Save(); }
        internal static void MoveUp(int i) { if (i > 0 && i < Shortcuts.Count) { var t = Shortcuts[i]; Shortcuts[i] = Shortcuts[i - 1]; Shortcuts[i - 1] = t; Save(); } }
    }
}
