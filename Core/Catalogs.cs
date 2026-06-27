using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine;

namespace PoppyMenu
{
    internal static class Catalogs
    {
        internal readonly struct ItemEntry
        {
            internal readonly ItemIndex Index;
            internal readonly string Name;
            internal readonly string Quality;
            internal readonly ItemTier Tier;
            internal readonly Color Color;
            internal ItemEntry(ItemIndex index, string name, string quality, ItemTier tier, Color color)
            { Index = index; Name = name; Quality = quality; Tier = tier; Color = color; }
        }

        internal readonly struct EquipEntry
        {
            internal readonly EquipmentIndex Index;
            internal readonly string Name;
            internal readonly Color Color;
            internal readonly bool IsLunar;
            internal readonly bool IsBoss;
            internal EquipEntry(EquipmentIndex index, string name, Color color, bool isLunar, bool isBoss)
            { Index = index; Name = name; Color = color; IsLunar = isLunar; IsBoss = isBoss; }
        }

        internal readonly struct BuffEntry
        {
            internal readonly BuffIndex Index;
            internal readonly string Name;
            internal readonly bool IsDebuff;
            internal BuffEntry(BuffIndex index, string name, bool isDebuff)
            { Index = index; Name = name; IsDebuff = isDebuff; }
        }

        internal readonly struct BodyEntry
        {
            internal readonly GameObject Prefab;
            internal readonly string Name;
            internal BodyEntry(GameObject prefab, string name) { Prefab = prefab; Name = name; }
        }

        internal readonly struct SurvivorEntry
        {
            internal readonly SurvivorDef Def;
            internal readonly string Name;
            internal SurvivorEntry(SurvivorDef def, string name) { Def = def; Name = name; }
        }

        internal readonly struct SpawnEntry
        {
            internal readonly SpawnCard Card;
            internal readonly string Name;
            internal readonly bool IsInteractable;
            internal SpawnEntry(SpawnCard card, string name, bool isInteractable)
            { Card = card; Name = name; IsInteractable = isInteractable; }
        }

        internal static readonly List<ItemEntry> Items = new List<ItemEntry>();
        internal static readonly List<EquipEntry> Equipment = new List<EquipEntry>();
        internal static readonly List<BuffEntry> Buffs = new List<BuffEntry>();
        internal static readonly List<BodyEntry> Bodies = new List<BodyEntry>();
        internal static readonly List<SurvivorEntry> Survivors = new List<SurvivorEntry>();
        internal static readonly List<SpawnEntry> SpawnCards = new List<SpawnEntry>();

        internal static bool Ready { get; private set; }

        internal static void Refresh()
        {
            BuildItems();
            BuildEquipment();
            BuildBuffs();
            BuildBodies();
            BuildSurvivors();
            BuildSpawnCards();
            Ready = true;
            Log.Info($"Catalogs: {Items.Count} items, {Equipment.Count} equipment, {Buffs.Count} buffs, " +
                     $"{Bodies.Count} bodies, {Survivors.Count} survivors, {SpawnCards.Count} spawn cards.");
        }

        private static string Loc(string token) => string.IsNullOrEmpty(token) ? token : Language.GetString(token);

        private static string Sanitize(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var sb = new System.Text.StringBuilder(s.Length);
            bool inTag = false;
            foreach (char c in s)
            {
                if (c == '<') { inTag = true; continue; }
                if (c == '>') { inTag = false; continue; }
                if (!inTag) sb.Append(c);
            }
            return System.Text.RegularExpressions.Regex.Replace(sb.ToString(), "\\s+", " ").Trim();
        }

        private static string ExtractQuality(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return null;
            int i = raw.IndexOf("name=", System.StringComparison.OrdinalIgnoreCase);
            if (i < 0) return null;
            i += 5;
            if (i >= raw.Length) return null;
            char quote = (raw[i] == '"' || raw[i] == '\'') ? raw[i] : '\0';
            int start = quote != '\0' ? i + 1 : i;
            int end = start;
            while (end < raw.Length)
            {
                char c = raw[end];
                if (quote != '\0') { if (c == quote) break; }
                else if (c == ' ' || c == '>' || c == ']' || c == '\t') break;
                end++;
            }
            if (end <= start) return null;
            string val = raw.Substring(start, end - start);
            if (val.StartsWith("Quality", System.StringComparison.OrdinalIgnoreCase)) val = val.Substring(7);
            val = val.Trim();
            return string.IsNullOrEmpty(val) ? null : val;
        }

        private static void BuildItems()
        {
            Items.Clear();
            foreach (ItemIndex index in ItemCatalog.allItems)
            {
                ItemDef def = ItemCatalog.GetItemDef(index);
                if (def == null)
                    continue;
                string raw = Loc(def.nameToken);
                if (string.IsNullOrWhiteSpace(raw))
                    raw = def.name;
                string name = Sanitize(raw);
                if (string.IsNullOrEmpty(name))
                    name = def.name;
                Color color = ColorCatalog.GetColor(def.darkColorIndex);
                Items.Add(new ItemEntry(index, name, ExtractQuality(raw), def.tier, color));
            }
            Items.Sort((a, b) =>
            {
                int ta = TierOrder(a.Tier), tb = TierOrder(b.Tier);
                return ta != tb ? ta.CompareTo(tb) : string.Compare(a.Name, b.Name, System.StringComparison.OrdinalIgnoreCase);
            });
        }

        private static int TierOrder(ItemTier tier)
        {
            switch (tier)
            {
                case ItemTier.Tier1: return 0;
                case ItemTier.Tier2: return 1;
                case ItemTier.Tier3: return 2;
                case ItemTier.Boss: return 3;
                case ItemTier.Lunar: return 4;
                case ItemTier.VoidTier1: return 5;
                case ItemTier.VoidTier2: return 6;
                case ItemTier.VoidTier3: return 7;
                case ItemTier.VoidBoss: return 8;
                default: return 99;
            }
        }

        private static void BuildEquipment()
        {
            Equipment.Clear();
            foreach (EquipmentIndex index in EquipmentCatalog.allEquipment)
            {
                EquipmentDef def = EquipmentCatalog.GetEquipmentDef(index);
                if (def == null)
                    continue;
                string name = Sanitize(Loc(def.nameToken));
                if (string.IsNullOrWhiteSpace(name))
                    name = def.name;
                Color color = ColorCatalog.GetColor(def.colorIndex);
                Equipment.Add(new EquipEntry(index, name, color, def.isLunar, def.isBoss));
            }
            Equipment.Sort((a, b) => string.Compare(a.Name, b.Name, System.StringComparison.OrdinalIgnoreCase));
        }

        private static void BuildBuffs()
        {
            Buffs.Clear();
            BuffDef[] defs = BuffCatalog.buffDefs;
            if (defs == null)
                return;
            foreach (BuffDef def in defs)
            {
                if (def == null)
                    continue;
                Buffs.Add(new BuffEntry(def.buffIndex, def.name, def.isDebuff));
            }
            Buffs.Sort((a, b) => string.Compare(a.Name, b.Name, System.StringComparison.OrdinalIgnoreCase));
        }

        private static void BuildBodies()
        {
            Bodies.Clear();
            foreach (GameObject prefab in BodyCatalog.allBodyPrefabs)
            {
                if (prefab == null)
                    continue;
                string name = prefab.name;
                CharacterBody body = prefab.GetComponent<CharacterBody>();
                if (body != null)
                {
                    string loc = Sanitize(Loc(body.baseNameToken));
                    if (!string.IsNullOrWhiteSpace(loc))
                        name = loc;
                }
                Bodies.Add(new BodyEntry(prefab, name));
            }
            Bodies.Sort((a, b) => string.Compare(a.Name, b.Name, System.StringComparison.OrdinalIgnoreCase));
        }

        private static void BuildSurvivors()
        {
            Survivors.Clear();
            foreach (SurvivorDef def in SurvivorCatalog.orderedSurvivorDefs)
            {
                if (def == null)
                    continue;
                string name = Loc(def.displayNameToken);
                if (string.IsNullOrWhiteSpace(name))
                    name = def.cachedName;
                Survivors.Add(new SurvivorEntry(def, name));
            }
        }

        internal static void RefreshSpawnCards()
        {
            try { BuildSpawnCards(); }
            catch (System.Exception e) { Log.Error($"RefreshSpawnCards: {e}"); }
        }

        private static void BuildSpawnCards()
        {
            SpawnCards.Clear();
            foreach (SpawnCard card in Resources.FindObjectsOfTypeAll<SpawnCard>())
            {
                if (card == null)
                    continue;
                bool interactable = card is InteractableSpawnCard;
                SpawnCards.Add(new SpawnEntry(card, card.name, interactable));
            }
            SpawnCards.Sort((a, b) => string.Compare(a.Name, b.Name, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
