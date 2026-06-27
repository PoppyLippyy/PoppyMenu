using BepInEx.Configuration;
using UnityEngine;

namespace PoppyMenu
{
    internal static class ModConfig
    {
        internal static ConfigEntry<KeyCode> ToggleMenuKey;
        internal static ConfigEntry<float> UiScale;
        internal static ConfigEntry<bool> RequireServerForCheats;
        internal static ConfigEntry<bool> AllowClientCheats;
        internal static ConfigEntry<bool> ShowHud;
        internal static ConfigEntry<float> WindowX;
        internal static ConfigEntry<float> WindowY;
        internal static ConfigEntry<float> WindowW;
        internal static ConfigEntry<float> WindowH;

        internal static ConfigEntry<KeyCode> SilentAimKey;

        internal static ConfigEntry<float> AccentR;
        internal static ConfigEntry<float> AccentG;
        internal static ConfigEntry<float> AccentB;

        internal static ConfigEntry<int> GiveMoneyAmount;
        internal static ConfigEntry<int> GiveXpAmount;
        internal static ConfigEntry<int> GiveCoinsAmount;
        internal static ConfigEntry<float> FlightSpeed;

        internal static void Init(ConfigFile cfg)
        {
            ToggleMenuKey = cfg.Bind("General", "ToggleMenuKey", KeyCode.Insert, "Opens/closes the Poppy menu.");
            UiScale = cfg.Bind("General", "UiScale", 1.0f, new ConfigDescription("Menu scale.", new AcceptableValueRange<float>(0.6f, 2.0f)));
            RequireServerForCheats = cfg.Bind("General", "RequireServerForCheats", false,
                "When true, server-side actions are skipped on non-host clients instead of being requested over the network.");
            AllowClientCheats = cfg.Bind("General", "AllowClientCheats", false,
                "HOST ONLY: when OFF (default), commands from other clients are ignored, so nobody can use this menu in your game without your permission. Turn ON only if you trust everyone in the lobby.");
            ShowHud = cfg.Bind("General", "ShowActiveEffectsHud", true, "Show a small active-effects HUD when the menu is closed.");
            WindowX = cfg.Bind("General", "WindowX", 40f, "Remembered menu window X position.");
            WindowY = cfg.Bind("General", "WindowY", 60f, "Remembered menu window Y position.");
            WindowW = cfg.Bind("General", "WindowW", 540f, "Remembered menu window width.");
            WindowH = cfg.Bind("General", "WindowH", 600f, "Remembered menu window height.");

            SilentAimKey = cfg.Bind("Hotkeys", "SilentAimHoldKey", KeyCode.None,
                "Hold-to-aim key. If set, Silent Aim only acts while this key is held (rebind in the Aimbot tab).");

            AccentR = cfg.Bind("Theme", "AccentR", 0.898f, "Menu accent color, red channel (0-1).");
            AccentG = cfg.Bind("Theme", "AccentG", 0.219f, "Menu accent color, green channel (0-1).");
            AccentB = cfg.Bind("Theme", "AccentB", 0.290f, "Menu accent color, blue channel (0-1).");
            Theme.ApplyAccent(new Color(AccentR.Value, AccentG.Value, AccentB.Value));

            GiveMoneyAmount = cfg.Bind("Tunables", "GiveMoneyAmount", 1000, "Gold granted per 'Give Money' click.");
            GiveXpAmount = cfg.Bind("Tunables", "GiveXpAmount", 100, "Experience granted per 'Give XP' click.");
            GiveCoinsAmount = cfg.Bind("Tunables", "GiveCoinsAmount", 10, "Lunar coins granted per 'Give Coins' click.");
            FlightSpeed = cfg.Bind("Tunables", "FlightSpeed", 40f, "Flight movement speed.");
        }
    }
}
