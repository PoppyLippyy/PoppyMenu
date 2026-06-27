using System;
using System.Collections.Generic;
using UnityEngine;

namespace PoppyMenu
{
    internal class HomeModule : PoppyModule
    {
        internal override string Name => "Home";

        private static string _query = "";
        private static bool _customizing;
        private const StringComparison OIC = StringComparison.OrdinalIgnoreCase;

        internal override void DrawMenu()
        {
            DrawSearch();
            DrawShortcuts();

            Widgets.SectionBegin("Quick Toggles");
            PlayerModule.GodMode = Widgets.Toggle("God Mode", PlayerModule.GodMode);
            MovementModule.Flight = Widgets.Toggle("Flight", MovementModule.Flight);
            MovementModule.AlwaysSprint = Widgets.Toggle("Always Sprint", MovementModule.AlwaysSprint);
            PlayerModule.InfiniteSkills = Widgets.Toggle("Infinite Skills", PlayerModule.InfiniteSkills);
            Aim.Enabled = Widgets.Toggle("Aimbot", Aim.Enabled);
            ItemsModule.NoEquipmentCooldown = Widgets.Toggle("No Equipment Cooldown", ItemsModule.NoEquipmentCooldown);

            bool esp = RenderModule.EspMobs || RenderModule.EspInteractables || RenderModule.EspTeleporter;
            bool newEsp = Widgets.Toggle("ESP (all)", esp);
            if (newEsp != esp)
                RenderModule.EspMobs = RenderModule.EspInteractables = RenderModule.EspTeleporter = newEsp;
            Widgets.SectionEnd();

            Widgets.SectionBegin("Give");
            ModConfig.GiveMoneyAmount.Value = Widgets.IntStepper("Money", ModConfig.GiveMoneyAmount.Value, 1000, 0, 1000000000);
            ModConfig.GiveXpAmount.Value = Widgets.IntStepper("XP", ModConfig.GiveXpAmount.Value, 100, 0, 1000000000);
            ModConfig.GiveCoinsAmount.Value = Widgets.IntStepper("Coins", ModConfig.GiveCoinsAmount.Value, 5, 0, 100000);
            Widgets.Button("Give Money", () => NetUtil.Do(PoppyOp.GiveMoney, i1: ModConfig.GiveMoneyAmount.Value));
            Widgets.Button("Give XP", () => NetUtil.Do(PoppyOp.GiveXp, i1: ModConfig.GiveXpAmount.Value));
            Widgets.Button("Give Lunar Coins", () => NetUtil.Do(PoppyOp.GiveLunar, i1: ModConfig.GiveCoinsAmount.Value));
            Widgets.SectionEnd();

            Widgets.SectionBegin("Quick Actions");
            Widgets.PrimaryButton("Give All Items", () => NetUtil.Do(PoppyOp.GiveAllItems, i2: 1));
            Widgets.Button("Heal to Full", () => NetUtil.Do(PoppyOp.HealFull));
            Widgets.Button("Respawn", () => NetUtil.Do(PoppyOp.Respawn));
            Widgets.Button("Instant Charge Teleporter", () => NetUtil.Do(PoppyOp.ChargeTeleporter));
            Widgets.Button("Kill All Enemies", () => NetUtil.Do(PoppyOp.KillAllEnemies));
            Widgets.ConfirmButton("home.clearinv", "Clear Inventory", () => NetUtil.Do(PoppyOp.ClearInventory));
            Widgets.SectionEnd();

            List<string> active = MenuRoot.ActiveEffects();
            Widgets.SectionBegin("Status");
            Widgets.Hint(active.Count > 0 ? "Active: " + string.Join(", ", active) : "No effects active.");
            if (active.Count > 0)
                Widgets.DangerButton("Disable All", Cheats.DisableAll);
            Widgets.SectionEnd();
        }

        private static void DrawSearch()
        {
            Widgets.SectionBegin("Search");
            _query = GUILayout.TextField(_query ?? "", Theme.Search);
            string q = (_query ?? "").Trim();
            if (q.Length == 0) { Widgets.Hint("Find any feature or item, run it, or pin it to Home."); Widgets.SectionEnd(); return; }

            int features = 0;
            foreach (PoppyAction a in ActionRegistry.All)
            {
                if (features >= 10) break;
                if (a.Name.IndexOf(q, OIC) < 0 && a.Category.IndexOf(q, OIC) < 0) continue;
                string id = a.Id;
                KeyCode key = BindStore.KeyFor(id);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(a.Category + ": " + a.Name + (key != KeyCode.None ? "   [" + key + "]" : ""), Theme.Button, GUILayout.ExpandWidth(true)))
                    ActionRegistry.RunId(id);
                if (GUILayout.Button("Pin", Theme.Button, GUILayout.Width(52))) HomeLayoutStore.Add(Steps.Feature(id));
                GUILayout.EndHorizontal();
                features++;
            }

            int items = 0;
            var seen = new HashSet<string>();
            foreach (Catalogs.ItemEntry e in Catalogs.Items)
            {
                if (items >= 8) break;
                if (e.Name.IndexOf(q, OIC) < 0 || !seen.Add(e.Name)) continue;
                Catalogs.ItemEntry ee = e;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Give: " + ee.Name, Theme.Button, GUILayout.ExpandWidth(true)))
                    NetUtil.Do(PoppyOp.GiveItem, (int)ee.Index, ItemsModule.GiveCount);
                if (GUILayout.Button("Pin", Theme.Button, GUILayout.Width(52))) HomeLayoutStore.Add(Steps.Item((int)ee.Index, ItemsModule.GiveCount));
                GUILayout.EndHorizontal();
                items++;
            }

            if (features == 0 && items == 0) Widgets.Hint("No matches.");
            Widgets.SectionEnd();
        }

        private static void DrawShortcuts()
        {
            Widgets.SectionBegin("My Shortcuts");
            List<CustomStep> list = HomeLayoutStore.Shortcuts;
            if (list.Count == 0)
                Widgets.Hint("Pin things from search above, or turn on Customize to add any item, buff, spawn, macro, and more.");

            for (int i = 0; i < list.Count; i++)
            {
                CustomStep s = list[i];
                int idx = i;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(s.Label, Theme.Primary, GUILayout.ExpandWidth(true))) StepRunner.Run(s);
                if (GUILayout.Button("^", Theme.Button, GUILayout.Width(26))) { HomeLayoutStore.MoveUp(idx); break; }
                if (GUILayout.Button("X", Theme.Danger_, GUILayout.Width(26))) { HomeLayoutStore.Remove(s); break; }
                GUILayout.EndHorizontal();
            }

            _customizing = Widgets.Toggle("Customize (add anything)", _customizing);
            if (_customizing)
                StepBuilder.Draw(s => HomeLayoutStore.Add(s));
            Widgets.SectionEnd();
        }
    }
}
