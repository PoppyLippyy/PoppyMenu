using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RoR2;
using UnityEngine;

namespace PoppyMenu
{
    internal class PresetsModule : PoppyModule
    {
        internal override string Name => "Presets";

        private static string _newName = "";
        private static string _importCode = "";
        private static int _expanded = -1;
        private static Preset _pendingDelete;

        internal override void DrawMenu()
        {
            Widgets.SectionBegin("Presets");
            Widgets.Hint("Save items and toggles, then flip on Auto-grant to apply them on every spawn.");
            GUILayout.BeginHorizontal();
            _newName = GUILayout.TextField(_newName ?? "", Theme.Search);
            if (GUILayout.Button("+ New Preset", Theme.Primary, GUILayout.Width(110)))
            {
                Preset p = NewEmpty(_newName);
                _newName = "";
                _expanded = PresetStore.Presets.IndexOf(p);
            }
            GUILayout.EndHorizontal();
            Widgets.Button("Save current setup as a preset", () =>
            {
                Preset p = PresetStore.AddFromCurrent(_newName);
                _expanded = PresetStore.Presets.IndexOf(p);
                Notify.Push("Saved preset: " + p.Name);
            });
            GUILayout.BeginHorizontal();
            _importCode = GUILayout.TextField(_importCode ?? "", Theme.Search);
            if (GUILayout.Button("Import code", Theme.Button, GUILayout.Width(100))) ImportCode();
            GUILayout.EndHorizontal();
            Widgets.Hint("Paste a code to import a shared preset, or use Export on one below to copy its code.");
            Widgets.SectionEnd();

            if (PresetStore.Presets.Count == 0)
            {
                Widgets.Label("No presets yet, click \"+ New Preset\" above to start.");
                return;
            }

            for (int i = 0; i < PresetStore.Presets.Count; i++)
            {
                Preset p = PresetStore.Presets[i];
                Widgets.SectionBegin(null);

                GUILayout.BeginHorizontal();
                string nn = GUILayout.TextField(p.Name ?? "", Theme.Search);
                if (nn != p.Name) { p.Name = nn; PresetStore.Save(); }
                if (GUILayout.Button(_expanded == i ? "v Edit" : "> Edit", Theme.Button, GUILayout.Width(64)))
                    _expanded = _expanded == i ? -1 : i;
                GUILayout.EndHorizontal();

                GUILayout.Label((p.LoadOnStartup ? "<color=#F0C24F>* STARTUP</color>  " : "") + (p.AutoApplyOnSpawn ? "<color=#4FC76B>* AUTO</color>  " : "") + Summary(p),
                    new GUIStyle(Theme.Hint) { richText = true });

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Apply now", Theme.Primary)) Cheats.Apply(p);
                if (GUILayout.Button(p.AutoApplyOnSpawn ? "Auto-grant: ON" : "Auto-grant: OFF",
                                     p.AutoApplyOnSpawn ? Theme.SwitchOn : Theme.SwitchOff))
                { p.AutoApplyOnSpawn = !p.AutoApplyOnSpawn; PresetStore.Save(); }
                Preset exportTarget = p;
                if (GUILayout.Button("Export", Theme.Button, GUILayout.Width(72))) ExportCode(exportTarget);
                GUILayout.EndHorizontal();

                if (_expanded == i) DrawEditor(p);
                Widgets.SectionEnd();
            }

            if (_pendingDelete != null) { PresetStore.Delete(_pendingDelete); _pendingDelete = null; _expanded = -1; }
        }

        private static void ExportCode(Preset p)
        {
            try
            {
                string json = JsonConvert.SerializeObject(p);
                GUIUtility.systemCopyBuffer = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
                Notify.Push("Copied " + p.Name + " code to clipboard");
            }
            catch (Exception e) { Log.Error("Preset export failed: " + e); Notify.Push("Export failed"); }
        }

        private static void ImportCode()
        {
            string code = (_importCode ?? "").Trim();
            if (code.Length == 0) return;
            try
            {
                byte[] bytes = Convert.FromBase64String(code);
                Preset p = JsonConvert.DeserializeObject<Preset>(System.Text.Encoding.UTF8.GetString(bytes));
                if (p == null) { Notify.Push("Bad preset code"); return; }
                if (string.IsNullOrWhiteSpace(p.Name)) p.Name = "Imported";
                PresetStore.Presets.Add(p);
                PresetStore.Save();
                _importCode = "";
                Notify.Push("Imported preset: " + p.Name);
            }
            catch (Exception e) { Log.Error("Preset import failed: " + e); Notify.Push("Bad preset code"); }
        }

        private static Preset NewEmpty(string name)
        {
            Preset p = new Preset { Name = string.IsNullOrWhiteSpace(name) ? "Preset " + (PresetStore.Presets.Count + 1) : name.Trim() };
            PresetStore.Presets.Add(p);
            PresetStore.Save();
            Notify.Push("Created preset: " + p.Name);
            return p;
        }

        private static void DrawEditor(Preset p)
        {
            Widgets.Header("Items granted on spawn");
            Preset preset = p;
            Widgets.Button("+ Add item", () => ItemPicker.Open("Add item to " + preset.Name, idx => AddItem(preset, idx)));
            if (p.Items.Count == 0 && p.Equipment.Count == 0 && !p.GiveAllItems)
                Widgets.Hint("No items yet, click \"+ Add item\".");

            for (int k = p.Items.Count - 1; k >= 0; k--)
            {
                GrantItem g = p.Items[k];
                GUILayout.BeginHorizontal();
                GUILayout.Label(string.IsNullOrEmpty(g.Display) ? g.Name : g.Display, Theme.Label, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("-", Theme.Button, GUILayout.Width(24))) { g.Count = Mathf.Max(1, g.Count - 1); PresetStore.Save(); }
                GUILayout.Label("x" + g.Count, Theme.Label, GUILayout.Width(38));
                if (GUILayout.Button("+", Theme.Button, GUILayout.Width(24))) { g.Count++; PresetStore.Save(); }
                if (GUILayout.Button("X", Theme.Danger_, GUILayout.Width(24))) { p.Items.RemoveAt(k); PresetStore.Save(); }
                GUILayout.EndHorizontal();
            }

            Widgets.PickerButton("+ Add equipment", "Add equipment to preset", BuildEquipRows(p));
            for (int k = p.Equipment.Count - 1; k >= 0; k--)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(p.Equipment[k], Theme.Label, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("X", Theme.Danger_, GUILayout.Width(24))) { p.Equipment.RemoveAt(k); PresetStore.Save(); }
                GUILayout.EndHorizontal();
            }

            Tog("Give ALL items", ref p.GiveAllItems);
            Step("Gold", ref p.Money, 1000, 0, 1000000000);
            Step("XP", ref p.Xp, 100, 0, 1000000000);
            Step("Lunar coins", ref p.Coins, 5, 0, 100000);

            Widgets.Header("Features enabled on spawn");
            Tog("God Mode", ref p.God);
            Tog("Infinite Skills", ref p.Skills);
            Tog("Silent Aim", ref p.SilentAim);
            Tog("Flight", ref p.Flight);
            Tog("Always Sprint", ref p.Sprint);
            Tog("Jump Pack", ref p.JumpPack);
            Tog("No Equipment Cooldown", ref p.NoEquipCd);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy current stat mults", Theme.Button)) { CopyStats(p); PresetStore.Save(); }
            if (GUILayout.Button("Clear", Theme.Button, GUILayout.Width(56))) { ClearStats(p); PresetStore.Save(); }
            GUILayout.EndHorizontal();
            Widgets.Hint(StatSummary(p));

            Widgets.Header("Automation");
            Tog("Auto-grant on spawn", ref p.AutoApplyOnSpawn);
            Widgets.Hint(p.AutoApplyOnSpawn
                ? "This preset runs automatically every time your character spawns."
                : "Off, use \"Apply now\" to run it manually.");

            Tog("Load on startup (your default)", ref p.LoadOnStartup);
            Widgets.Hint(p.LoadOnStartup
                ? "Applied once when you first load into a game each session, so this setup is always there when you reopen the game."
                : "Off. Turn on to make this your default setup that loads every time.");

            Widgets.Separator();
            Widgets.ConfirmButton("preset.delete." + p.Name, "Delete this preset", () => _pendingDelete = p);
        }

        private static void Tog(string label, ref bool field)
        {
            bool v = Widgets.Toggle(label, field);
            if (v != field) { field = v; PresetStore.Save(); }
        }

        private static void Step(string label, ref int field, int step, int min, int max)
        {
            int v = Widgets.IntStepper(label, field, step, min, max);
            if (v != field) { field = v; PresetStore.Save(); }
        }

        private static void AddItem(Preset p, ItemIndex idx)
        {
            ItemDef def = ItemCatalog.GetItemDef(idx);
            if (def == null) return;

            string display = def.name;
            foreach (var e in Catalogs.Items)
                if (e.Index == idx) { display = e.Name; break; }

            GrantItem existing = p.Items.Find(g => g.Name == def.name);
            if (existing != null) existing.Count++;
            else p.Items.Add(new GrantItem { Name = def.name, Display = display, Count = 1 });
            PresetStore.Save();
            Notify.Push("Added " + display);
        }

        private static List<ListPicker.Row> BuildEquipRows(Preset p)
        {
            var rows = new List<ListPicker.Row>(Catalogs.Equipment.Count);
            foreach (var entry in Catalogs.Equipment)
            {
                var e = entry;
                rows.Add(new ListPicker.Row(e.Name, e.Color, () =>
                {
                    EquipmentDef def = EquipmentCatalog.GetEquipmentDef(e.Index);
                    if (def != null && !p.Equipment.Contains(def.name)) { p.Equipment.Add(def.name); PresetStore.Save(); Notify.Push("Added " + e.Name); }
                }));
            }
            return rows;
        }

        private static void CopyStats(Preset p)
        {
            p.DmgOn = StatsModule.DamageOn; p.AtkOn = StatsModule.AttackSpeedOn; p.MoveOn = StatsModule.MoveSpeedOn;
            p.ArmorOn = StatsModule.ArmorOn; p.CritOn = StatsModule.CritOn; p.HpOn = StatsModule.MaxHealthOn;
            p.DmgMul = StatsModule.DamageMult; p.AtkMul = StatsModule.AttackSpeedMult; p.MoveMul = StatsModule.MoveSpeedMult;
            p.ArmorMul = StatsModule.ArmorMult; p.CritMul = StatsModule.CritMult; p.HpMul = StatsModule.MaxHealthMult;
        }

        private static void ClearStats(Preset p)
        {
            p.DmgOn = p.AtkOn = p.MoveOn = p.ArmorOn = p.CritOn = p.HpOn = false;
        }

        private static string StatSummary(Preset p)
        {
            var parts = new List<string>();
            if (p.DmgOn) parts.Add($"Dmg x{p.DmgMul:0.#}");
            if (p.AtkOn) parts.Add($"Atk x{p.AtkMul:0.#}");
            if (p.MoveOn) parts.Add($"Move x{p.MoveMul:0.#}");
            if (p.ArmorOn) parts.Add($"Armor x{p.ArmorMul:0.#}");
            if (p.CritOn) parts.Add($"Crit x{p.CritMul:0.#}");
            if (p.HpOn) parts.Add($"HP x{p.HpMul:0.#}");
            return parts.Count == 0 ? "Stat multipliers: none" : "Stat multipliers: " + string.Join(", ", parts);
        }

        private static string Summary(Preset p)
        {
            var parts = new List<string>();
            int items = p.Items.Count + p.Equipment.Count;
            if (items > 0) parts.Add(items + (items == 1 ? " item" : " items"));
            if (p.GiveAllItems) parts.Add("all items");
            if (p.Money > 0 || p.Xp > 0 || p.Coins > 0) parts.Add("currency");
            if (p.God) parts.Add("God");
            if (p.Flight) parts.Add("Flight");
            if (p.SilentAim) parts.Add("Silent Aim");
            if (p.Skills) parts.Add("Skills");
            return parts.Count == 0 ? "empty" : string.Join(" · ", parts);
        }
    }
}
