using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace PoppyMenu
{
    internal static class StepBuilder
    {
        private static int _amount = 1;
        private static float _duration;
        private static string _command = "";

        internal static void Draw(Action<CustomStep> onAdd)
        {
            _amount = Widgets.IntStepper("Count / amount", _amount, 1, 1, 1000000000);
            _duration = Widgets.Slider("Buff duration (0 = permanent)", _duration, 0f, 120f);

            GUILayout.BeginHorizontal();
            Widgets.Button("Item", () => ItemPicker.Open("Pick an item", idx => Done(onAdd, Steps.Item((int)idx, _amount))));
            Widgets.Button("Buff", () => ListPicker.Open("Pick a buff", BuffRows(onAdd)));
            Widgets.Button("Equipment", () => ListPicker.Open("Pick equipment", EquipRows(onAdd)));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Widgets.Button("Become", () => ListPicker.Open("Pick a body", BodyRows(onAdd)));
            Widgets.Button("Spawn", () => ListPicker.Open("Pick something to spawn", SpawnRows(onAdd)));
            Widgets.Button("Feature", () => ListPicker.Open("Pick a feature", FeatureRows(onAdd)));
            Widgets.Button("Macro", () => ListPicker.Open("Pick a macro", MacroRows(onAdd)));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Widgets.Button("Money +" + _amount, () => Done(onAdd, Steps.Money(_amount)));
            Widgets.Button("XP +" + _amount, () => Done(onAdd, Steps.Xp(_amount)));
            Widgets.Button("Lunar +" + _amount, () => Done(onAdd, Steps.Lunar(_amount)));
            Widgets.Button("Heal full", () => Done(onAdd, Steps.Heal(0)));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _command = GUILayout.TextField(_command ?? "", Theme.Search);
            if (GUILayout.Button("Add command", Theme.Button, GUILayout.Width(110)) && !string.IsNullOrWhiteSpace(_command))
            {
                Done(onAdd, Steps.Command(_command.Trim()));
                _command = "";
            }
            GUILayout.EndHorizontal();
        }

        private static void Done(Action<CustomStep> onAdd, CustomStep s) { onAdd(s); Notify.Push("Added " + s.Label); }

        private static List<ListPicker.Row> BuffRows(Action<CustomStep> onAdd)
        {
            var rows = new List<ListPicker.Row>(Catalogs.Buffs.Count);
            foreach (var entry in Catalogs.Buffs)
            {
                var e = entry;
                rows.Add(new ListPicker.Row(e.Name, e.IsDebuff ? Color.red : Color.cyan, () => Done(onAdd, Steps.Buff((int)e.Index, _duration))));
            }
            return rows;
        }

        private static List<ListPicker.Row> EquipRows(Action<CustomStep> onAdd)
        {
            var rows = new List<ListPicker.Row>(Catalogs.Equipment.Count);
            foreach (var entry in Catalogs.Equipment)
            {
                var e = entry;
                rows.Add(new ListPicker.Row(e.Name, e.Color, () => Done(onAdd, Steps.Equipment((int)e.Index))));
            }
            return rows;
        }

        private static List<ListPicker.Row> BodyRows(Action<CustomStep> onAdd)
        {
            var rows = new List<ListPicker.Row>(Catalogs.Bodies.Count);
            foreach (var entry in Catalogs.Bodies)
            {
                if (entry.Prefab == null) continue;
                var e = entry;
                rows.Add(new ListPicker.Row(e.Name, Color.white, () => Done(onAdd, Steps.Become(e.Prefab.name, e.Name))));
            }
            return rows;
        }

        private static List<ListPicker.Row> SpawnRows(Action<CustomStep> onAdd)
        {
            var rows = new List<ListPicker.Row>(Catalogs.SpawnCards.Count);
            foreach (var entry in Catalogs.SpawnCards)
            {
                var e = entry;
                Color c = e.IsInteractable ? Color.cyan : Color.red;
                rows.Add(new ListPicker.Row(e.Name, c, () => Done(onAdd, Steps.Spawn(e.Name, _amount, (int)TeamIndex.Monster, e.Name))));
            }
            return rows;
        }

        private static List<ListPicker.Row> FeatureRows(Action<CustomStep> onAdd)
        {
            var rows = new List<ListPicker.Row>(ActionRegistry.All.Count);
            foreach (PoppyAction a in ActionRegistry.All)
            {
                PoppyAction act = a;
                rows.Add(new ListPicker.Row(act.Category + ": " + act.Name, Color.white, () => Done(onAdd, Steps.Feature(act.Id))));
            }
            return rows;
        }

        private static List<ListPicker.Row> MacroRows(Action<CustomStep> onAdd)
        {
            var rows = new List<ListPicker.Row>(MacroStore.Macros.Count);
            foreach (Macro m in MacroStore.Macros)
            {
                Macro macro = m;
                rows.Add(new ListPicker.Row(macro.Name, Color.yellow, () => Done(onAdd, Steps.Macro(macro.Name))));
            }
            return rows;
        }
    }
}
