using System.Collections.Generic;
using System.IO;
using BepInEx;
using Newtonsoft.Json;

namespace PoppyMenu
{
    internal static class PresetStore
    {
        internal static List<Preset> Presets = new List<Preset>();

        private static string FilePath => Path.Combine(Paths.ConfigPath, "PoppyMenu.presets.json");

        internal static void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    Presets = JsonConvert.DeserializeObject<List<Preset>>(File.ReadAllText(FilePath)) ?? new List<Preset>();
            }
            catch (System.Exception e)
            {
                Log.Error("Preset load failed: " + e);
                Presets = new List<Preset>();
            }
        }

        internal static void Save()
        {
            try
            {
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(Presets, Formatting.Indented));
            }
            catch (System.Exception e)
            {
                Log.Error("Preset save failed: " + e);
            }
        }

        internal static Preset AddFromCurrent(string name)
        {
            Preset p = Cheats.Capture(string.IsNullOrWhiteSpace(name) ? "Preset " + (Presets.Count + 1) : name.Trim());
            Presets.Add(p);
            Save();
            return p;
        }

        internal static void Delete(Preset p)
        {
            if (p != null && Presets.Remove(p)) Save();
        }

        internal static void ApplyAutoPresets()
        {
            foreach (Preset p in Presets)
                if (p != null && p.AutoApplyOnSpawn)
                    Cheats.Apply(p);
        }

        private static bool _startupDone;

        internal static void ApplyStartupPresets()
        {
            if (_startupDone) return;
            _startupDone = true;
            foreach (Preset p in Presets)
                if (p != null && p.LoadOnStartup)
                    Cheats.Apply(p);
        }
    }
}
