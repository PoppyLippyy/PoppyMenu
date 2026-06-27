using UnityEngine;

namespace PoppyMenu
{
    internal class SettingsModule : PoppyModule
    {
        internal override string Name => "Settings";

        internal override void DrawMenu()
        {
            Widgets.SectionBegin("Keybinds");
            Widgets.KeybindRow("Open Menu", ModConfig.ToggleMenuKey);
            if (Rebind.IsActive) Widgets.Hint("Press any key or mouse button (right/middle/side) to bind. Esc cancels. X clears.");
            Widgets.Hint("Bind any feature to any key over in the Keybinds tab.");
            Widgets.SectionEnd();

            Widgets.SectionBegin("Display");
            ModConfig.UiScale.Value = Widgets.Slider("UI Scale", ModConfig.UiScale.Value, 0.6f, 2f);
            ModConfig.ShowHud.Value = Widgets.Toggle("Active-Effects HUD", ModConfig.ShowHud.Value);
            Widgets.SectionEnd();

            DrawThemeSection();

            Widgets.SectionBegin("Multiplayer");
            ModConfig.AllowClientCheats.Value = Widgets.Toggle("Allow Others To Use This (host)", ModConfig.AllowClientCheats.Value);
            Widgets.Hint("HOST: off by default, so others can't use this menu in YOUR game without permission. Turn on only if you trust the lobby.");
            ModConfig.RequireServerForCheats.Value = Widgets.Toggle("Host-Only (skip on client)", ModConfig.RequireServerForCheats.Value);
            Widgets.Hint("On: as a client, don't even request server-side actions, just skip them.");
            Widgets.SectionEnd();

            Widgets.SectionBegin("Maintenance");
            Widgets.Button("Refresh Catalogs", () => { Catalogs.Refresh(); Notify.Push("Catalogs refreshed"); });
            Widgets.Button("Reset Window Position", () => { MenuRoot.ResetPosition(); Notify.Push("Window reset"); });
            Widgets.SectionEnd();

            Widgets.SectionBegin("About");
            Widgets.Label("Poppy Menu  v" + PoppyPlugin.Version);
            Widgets.Hint("Risk of Rain 2 cheat and debug menu. Single-player and private lobbies.");
            Widgets.SectionEnd();
        }

        private static void DrawThemeSection()
        {
            Widgets.SectionBegin("Theme");
            float r = ModConfig.AccentR.Value, g = ModConfig.AccentG.Value, b = ModConfig.AccentB.Value;
            float nr = Widgets.Slider("Accent R", r, 0f, 1f);
            float ng = Widgets.Slider("Accent G", g, 0f, 1f);
            float nb = Widgets.Slider("Accent B", b, 0f, 1f);
            if (nr != r || ng != g || nb != b)
            {
                ModConfig.AccentR.Value = nr; ModConfig.AccentG.Value = ng; ModConfig.AccentB.Value = nb;
                Theme.ApplyAccent(new Color(nr, ng, nb));
            }
            Widgets.Button("Reset to poppy red", () =>
            {
                ModConfig.AccentR.Value = 0.898f; ModConfig.AccentG.Value = 0.219f; ModConfig.AccentB.Value = 0.290f;
                Theme.ApplyAccent(new Color(0.898f, 0.219f, 0.290f));
            });
            Widgets.SectionEnd();
        }
    }
}
