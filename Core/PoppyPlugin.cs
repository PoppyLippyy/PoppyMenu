using BepInEx;
using UnityEngine;

namespace PoppyMenu
{
    [BepInPlugin(Guid, "Poppy Menu", Version)]
    [BepInDependency("com.bepis.r2api.networking")]
    public class PoppyPlugin : BaseUnityPlugin
    {
        internal const string Guid = "com.poppy.poppymenu";
        internal const string Version = "2.2.3";

        internal static PoppyPlugin Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            Log.Init(Logger);
            ModConfig.Init(Config);
            NetUtil.Init();
            InputCapture.Init();
            Aim.Init();
            Safety.Init();
            ConsoleCommands.Init();
            PresetStore.Load();
            BindStore.Load();
            MacroStore.Load();
            HomeLayoutStore.Load();

            var go = new GameObject("PoppyMenu");
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            go.AddComponent<PoppyController>();

            Log.Message($"Poppy Menu v{Version} loaded. Press {ModConfig.ToggleMenuKey.Value} to open.");
        }
    }
}
