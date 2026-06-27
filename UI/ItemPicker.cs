using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace PoppyMenu
{
    internal static class ItemPicker
    {
        internal static void Open(string title, Action<ItemIndex> give) => ListPicker.Open(title, Groups(title, give));

        private static List<ListPicker.Row> Groups(string title, Action<ItemIndex> give)
        {
            var byName = new Dictionary<string, List<Catalogs.ItemEntry>>();
            var order = new List<string>();
            foreach (Catalogs.ItemEntry e in Catalogs.Items)
            {
                if (!byName.TryGetValue(e.Name, out var list)) { list = new List<Catalogs.ItemEntry>(); byName[e.Name] = list; order.Add(e.Name); }
                list.Add(e);
            }

            var rows = new List<ListPicker.Row>(order.Count);
            foreach (string name in order)
            {
                List<Catalogs.ItemEntry> variants = byName[name];
                if (variants.Count == 1)
                {
                    Catalogs.ItemEntry e = variants[0];
                    rows.Add(new ListPicker.Row($"{name}   [{e.Tier}]", e.Color, () => give(e.Index)));
                }
                else
                {
                    string n = name;
                    List<Catalogs.ItemEntry> vs = variants;
                    rows.Add(new ListPicker.Row($"{name}   ({variants.Count} qualities)", variants[0].Color,
                        () => ListPicker.Open(n, Variants(title, n, vs, give))));
                }
            }
            return rows;
        }

        private static List<ListPicker.Row> Variants(string title, string name, List<Catalogs.ItemEntry> variants, Action<ItemIndex> give)
        {
            var rows = new List<ListPicker.Row>(variants.Count + 1);
            rows.Add(new ListPicker.Row("< Back", Color.gray, () => ListPicker.Open(title, Groups(title, give))));
            foreach (Catalogs.ItemEntry entry in variants)
            {
                Catalogs.ItemEntry e = entry;
                string label = !string.IsNullOrEmpty(e.Quality) ? e.Quality : e.Tier.ToString();
                rows.Add(new ListPicker.Row($"{name}  -  {label}", e.Color, () => give(e.Index)));
            }
            return rows;
        }
    }
}
