using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using RoR2;

namespace PoppyMenu
{
    internal static class ConsoleCommands
    {
        private static Harmony _h;
        private static bool _collected;

        internal static void Init()
        {
            _h = new Harmony("poppy.concommands");
            MethodInfo init = AccessTools.Method(typeof(RoR2.Console), "InitConVars");
            if (init != null) _h.Patch(init, postfix: new HarmonyMethod(typeof(ConsoleCommands), nameof(AfterInit)));
            else Log.Warning("ConsoleCommands: Console.InitConVars not found; commands may not register.");

            if (RoR2.Console.instance != null) Collect(RoR2.Console.instance);
        }

        internal static void Shutdown() { _h?.UnpatchSelf(); _h = null; }

        private static void AfterInit(RoR2.Console __instance) => Collect(__instance);

        private static void Collect(RoR2.Console console)
        {
            if (_collected || console == null) return;
            try
            {
                IDictionary catalog = FindCatalog(console);
                if (catalog == null) { Log.Warning("ConsoleCommands: command catalog not found."); return; }

                int added = 0;
                foreach (MethodInfo m in typeof(DebugCommands).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attr = m.GetCustomAttribute<ConCommandAttribute>();
                    if (attr == null) continue;
                    string name = attr.commandName.ToLowerInvariant();
                    if (catalog.Contains(name)) continue;
                    catalog[name] = new RoR2.Console.ConCommand
                    {
                        flags = attr.flags,
                        helpText = attr.helpText,
                        action = (RoR2.Console.ConCommandDelegate)Delegate.CreateDelegate(typeof(RoR2.Console.ConCommandDelegate), m)
                    };
                    added++;
                }
                _collected = true;
                Log.Message($"Poppy: registered {added} console commands.");
            }
            catch (Exception e) { Log.Error("ConsoleCommands.Collect: " + e); }
        }

        private static IDictionary FindCatalog(RoR2.Console console)
        {
            foreach (FieldInfo f in typeof(RoR2.Console).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (!typeof(IDictionary).IsAssignableFrom(f.FieldType)) continue;
                if (f.Name.ToLowerInvariant().Contains("concommand"))
                    return f.GetValue(console) as IDictionary;
            }
            return null;
        }
    }
}
