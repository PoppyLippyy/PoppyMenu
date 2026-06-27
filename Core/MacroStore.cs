using System.Collections.Generic;
using System.IO;
using BepInEx;
using Newtonsoft.Json;

namespace PoppyMenu
{
    internal static class MacroStore
    {
        internal static List<Macro> Macros = new List<Macro>();

        private static string FilePath => Path.Combine(Paths.ConfigPath, "PoppyMenu.macros.json");

        internal static void Load()
        {
            try { if (File.Exists(FilePath)) Macros = JsonConvert.DeserializeObject<List<Macro>>(File.ReadAllText(FilePath)) ?? new List<Macro>(); }
            catch (System.Exception e) { Log.Error("Macro load failed: " + e); Macros = new List<Macro>(); }
        }

        internal static void Save()
        {
            try { File.WriteAllText(FilePath, JsonConvert.SerializeObject(Macros, Formatting.Indented)); }
            catch (System.Exception e) { Log.Error("Macro save failed: " + e); }
        }

        internal static Macro Get(string name)
        {
            foreach (Macro m in Macros) if (m != null && m.Name == name) return m;
            return null;
        }

        internal static void Run(string name)
        {
            Macro m = Get(name);
            if (m == null) return;
            foreach (CustomStep s in m.Steps) StepRunner.Run(s);
        }
    }
}
