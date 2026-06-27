using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace PoppyMenu
{
    internal static class PoppyConsole
    {
        internal static readonly List<string> History = new List<string>();

        private struct Command
        {
            public string Usage;
            public string Help;
            public Action<string[]> Run;
            public Command(string usage, string help, Action<string[]> run) { Usage = usage; Help = help; Run = run; }
        }

        private static readonly Dictionary<string, Command> Commands = new Dictionary<string, Command>(StringComparer.OrdinalIgnoreCase);

        static PoppyConsole() => RegisterAll();

        internal static void Print(string line)
        {
            History.Add(line);
            if (History.Count > 300) History.RemoveAt(0);
        }

        internal static IEnumerable<string> CommandList()
        {
            var keys = new List<string>(Commands.Keys);
            keys.Sort(StringComparer.OrdinalIgnoreCase);
            foreach (string k in keys)
            {
                Command c = Commands[k];
                yield return string.IsNullOrEmpty(c.Usage) ? k + "  -  " + c.Help : k + " " + c.Usage + "  -  " + c.Help;
            }
        }

        internal static void Submit(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return;
            Print("> " + line);
            string[] tokens = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string name = tokens[0];
            string[] args = new string[tokens.Length - 1];
            Array.Copy(tokens, 1, args, 0, args.Length);

            if (Commands.TryGetValue(name, out Command cmd))
            {
                try { cmd.Run(args); }
                catch (Exception e) { Print("error: " + e.Message); }
            }
            else ForwardToGame(line);
        }

        private static void ForwardToGame(string line)
        {
            try
            {
                if (RoR2.Console.instance == null) { Print("Unknown command. Type 'help'."); return; }
                NetworkUser sender = NetworkUser.readOnlyLocalPlayersList.Count > 0 ? NetworkUser.readOnlyLocalPlayersList[0] : null;
                RoR2.Console.instance.SubmitCmd(sender, line);
                Print("(sent to the game console)");
            }
            catch (Exception e) { Print("Unknown command (" + e.Message + ")"); }
        }

        private static void RegisterAll()
        {
            void Add(string n, string usage, string help, Action<string[]> run) => Commands[n] = new Command(usage, help, run);

            Add("help", "", "List every command", a => { foreach (string s in CommandList()) Print(s); });

            Add("give", "<item> [count]", "Give yourself an item", a =>
            {
                var (q, c) = NameAndCount(a, 1);
                int idx = FindItem(q);
                if (idx < 0) { Print("no item matching '" + q + "'"); return; }
                NetUtil.Do(PoppyOp.GiveItem, i1: idx, i2: c);
            });
            Add("give_all", "[count]", "Give one of every item", a => NetUtil.Do(PoppyOp.GiveAllItems, i2: Int(a, 0, 1)));
            Add("clear", "", "Clear your inventory", a => NetUtil.Do(PoppyOp.ClearInventory));
            Add("stack", "", "Stack inventory (Shrine of Order)", a => NetUtil.Do(PoppyOp.StackInventory));
            Add("reroll", "", "Reroll your items", a => NetUtil.Do(PoppyOp.RollItems));
            Add("equip", "<name>", "Give equipment", a =>
            {
                int idx = FindEquip(Join(a, 0));
                if (idx < 0) { Print("no equipment matching that"); return; }
                NetUtil.Do(PoppyOp.GiveEquipment, i1: idx);
            });
            Add("buff", "<name> [count]", "Give a buff", a =>
            {
                var (q, c) = NameAndCount(a, 1);
                int idx = FindBuff(q);
                if (idx < 0) { Print("no buff matching '" + q + "'"); return; }
                NetUtil.Do(PoppyOp.GiveBuff, idx, c);
            });

            Add("spawn", "<name> [count]", "Spawn a monster or interactable at you", a =>
            {
                var (q, c) = NameAndCount(a, 1);
                string card = FindSpawn(q);
                if (card == null) { Print("no spawnable matching '" + q + "'"); return; }
                Vector3 p = PlayerContext.HasBody ? PlayerContext.Body.footPosition : Vector3.zero;
                NetUtil.Do(PoppyOp.Spawn, i1: c, i2: (int)TeamIndex.Monster, f1: p.x, f2: p.y, f3: p.z, s1: card);
            });
            Add("become", "<body>", "Turn into a body", a =>
            {
                string body = FindBody(Join(a, 0));
                if (body == null) { Print("no body matching that"); return; }
                Vector3 p = PlayerContext.HasBody ? PlayerContext.Body.footPosition : Vector3.zero;
                NetUtil.Do(PoppyOp.ChangeBody, f1: p.x, f2: p.y, f3: p.z, s1: body);
            });
            Add("kill_all", "", "Kill all enemies", a => NetUtil.Do(PoppyOp.KillAllEnemies));

            Add("money", "<amount>", "Give money", a => NetUtil.Do(PoppyOp.GiveMoney, i1: Int(a, 0, 0)));
            Add("xp", "<amount>", "Give experience", a => NetUtil.Do(PoppyOp.GiveXp, i1: Int(a, 0, 0)));
            Add("lunar", "<amount>", "Give lunar coins", a => NetUtil.Do(PoppyOp.GiveLunar, i1: Int(a, 0, 1)));
            Add("heal", "[amount]", "Heal yourself (no amount = full)", a =>
            {
                if (a.Length == 0) NetUtil.Do(PoppyOp.HealFull);
                else NetUtil.Do(PoppyOp.HealAmount, f1: Float(a, 0, 0));
            });
            Add("hurt", "<amount>", "Damage yourself", a => NetUtil.Do(PoppyOp.HurtBody, f1: Float(a, 0, 0)));
            Add("respawn", "", "Respawn at the map spawn", a => NetUtil.Do(PoppyOp.Respawn));

            Add("god", "[0/1]", "Toggle god mode", a => { PlayerModule.GodMode = Flag(a, 0, PlayerModule.GodMode); Print("god " + PlayerModule.GodMode); });
            Add("buddha", "[0/1]", "Toggle buddha (survive lethal hits)", a => { Safety.Buddha = Flag(a, 0, Safety.Buddha); Print("buddha " + Safety.Buddha); });
            Add("noclip", "[0/1]", "Toggle noclip", a => { MovementModule.NoClip = Flag(a, 0, MovementModule.NoClip); Print("noclip " + MovementModule.NoClip); });
            Add("flight", "[0/1]", "Toggle flight", a => { MovementModule.Flight = Flag(a, 0, MovementModule.Flight); Print("flight " + MovementModule.Flight); });
            Add("sprint", "[0/1]", "Toggle always-sprint", a => { MovementModule.AlwaysSprint = Flag(a, 0, MovementModule.AlwaysSprint); Print("sprint " + MovementModule.AlwaysSprint); });
            Add("no_enemies", "[0/1]", "Stop enemies from spawning", a => { Safety.NoEnemies = Flag(a, 0, Safety.NoEnemies); Print("no_enemies " + Safety.NoEnemies); });
            Add("lock_exp", "[0/1]", "Stop experience gain", a => { Safety.LockExp = Flag(a, 0, Safety.LockExp); Print("lock_exp " + Safety.LockExp); });

            Add("skip", "", "Skip to the next stage", a => NetUtil.Do(PoppyOp.SkipStage));
            Add("charge", "", "Fully charge the teleporter", a => NetUtil.Do(PoppyOp.ChargeTeleporter));
            Add("portal", "<blue|gold|celestial>", "Spawn a portal", a =>
            {
                string k = a.Length > 0 ? a[0].ToLowerInvariant() : "";
                if (k.StartsWith("blue") || k.StartsWith("shop")) NetUtil.Do(PoppyOp.SpawnShopPortal);
                else if (k.StartsWith("gold")) NetUtil.Do(PoppyOp.SpawnGoldshoresPortal);
                else if (k.StartsWith("cele") || k.StartsWith("moon")) NetUtil.Do(PoppyOp.SpawnMSPortal);
                else Print("portal type must be blue, gold, or celestial");
            });
            Add("stages", "<n>", "Set stages cleared", a => NetUtil.Do(PoppyOp.SetStagesCleared, i1: Int(a, 0, 0)));
            Add("runtime", "<minutes>", "Set the run time", a => NetUtil.Do(PoppyOp.SetRunTime, f1: Float(a, 0, 0) * 60f));
            Add("teamlevel", "<n>", "Set the player team level", a => NetUtil.Do(PoppyOp.SetTeamLevel, i1: (int)TeamIndex.Player, i2: Int(a, 0, 1)));
            Add("timescale", "<x>", "Set the time scale", a => { WorldModule.TimeScale = Float(a, 0, 1f); Print("timescale " + WorldModule.TimeScale); });
            Add("freeze", "[0/1]", "Freeze the whole match", a => { WorldModule.FreezeMatch = Flag(a, 0, WorldModule.FreezeMatch); Print("freeze " + WorldModule.FreezeMatch); });
            Add("freeze_timer", "[0/1]", "Freeze the run timer", a => { WorldModule.FreezeTimer = Flag(a, 0, WorldModule.FreezeTimer); Print("freeze_timer " + WorldModule.FreezeTimer); });

            Add("random_items", "<count>", "Give random items", a => GiveRandom(Int(a, 0, 5), null));
            Add("midgame", "", "Random mid-game loadout plus money", a => Midgame());
            Add("lategame", "", "Random end-game loadout plus money", a => Lategame());
            Add("dtzoom", "", "20 hooves and 200 feathers for fast movement", a => { GiveByName("Hoof", 20); GiveByName("Feather", 200); });
            Add("dump_stats", "", "Print your stats to the log", a => DumpStats());
        }

        private static void GiveRandom(int count, ItemTier? tier)
        {
            var pool = new List<int>();
            foreach (var e in Catalogs.Items)
                if (tier == null || e.Tier == tier.Value) pool.Add((int)e.Index);
            if (pool.Count == 0) { Print("no items available for that tier"); return; }
            count = Mathf.Clamp(count, 1, 1000);
            for (int i = 0; i < count; i++)
                NetUtil.Do(PoppyOp.GiveItem, i1: pool[UnityEngine.Random.Range(0, pool.Count)], i2: 1);
            Print("gave " + count + " random items");
        }

        private static void GiveByName(string name, int count)
        {
            int idx = FindItem(name);
            if (idx < 0) { Print("no item matching '" + name + "'"); return; }
            NetUtil.Do(PoppyOp.GiveItem, i1: idx, i2: count);
        }

        private static void Midgame()
        {
            GiveRandom(20, ItemTier.Tier1);
            GiveRandom(8, ItemTier.Tier2);
            GiveRandom(2, ItemTier.Tier3);
            NetUtil.Do(PoppyOp.GiveMoney, i1: 1000);
            Print("mid-game loadout granted");
        }

        private static void Lategame()
        {
            GiveRandom(40, ItemTier.Tier1);
            GiveRandom(20, ItemTier.Tier2);
            GiveRandom(8, ItemTier.Tier3);
            GiveRandom(1, ItemTier.Boss);
            NetUtil.Do(PoppyOp.GiveMoney, i1: 10000);
            Print("end-game loadout granted");
        }

        private static void DumpStats()
        {
            CharacterBody b = PlayerContext.Body;
            if (b == null) { Print("no body"); return; }
            Print($"{b.GetDisplayName()}  level {b.level:0}");
            Print($"  HP {Mathf.CeilToInt(b.healthComponent != null ? b.healthComponent.combinedHealth : 0)} / {Mathf.CeilToInt(b.maxHealth)}");
            Print($"  damage {b.damage:0.0}   attack speed {b.attackSpeed:0.00}   crit {b.crit:0}%");
            Print($"  move speed {b.moveSpeed:0.0}   armor {b.armor:0}   regen {b.regen:0.0}/s");
        }

        private static int Int(string[] a, int i, int def) => (i < a.Length && int.TryParse(a[i], out int v)) ? v : def;
        private static float Float(string[] a, int i, float def) => (i < a.Length && float.TryParse(a[i], out float v)) ? v : def;

        private static bool Flag(string[] a, int i, bool current)
        {
            if (i >= a.Length) return !current;
            return a[i] == "1" || a[i].Equals("true", StringComparison.OrdinalIgnoreCase) || a[i].Equals("on", StringComparison.OrdinalIgnoreCase);
        }

        private static string Join(string[] a, int from)
            => from >= a.Length ? "" : string.Join(" ", a, from, a.Length - from);

        private static (string name, int count) NameAndCount(string[] a, int defCount)
        {
            if (a.Length == 0) return ("", defCount);
            int count = defCount, nameEnd = a.Length;
            if (a.Length >= 2 && int.TryParse(a[a.Length - 1], out int c)) { count = c; nameEnd = a.Length - 1; }
            return (string.Join(" ", a, 0, nameEnd), count);
        }

        private static int FindItem(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return -1;
            foreach (var e in Catalogs.Items)
                if (e.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) return (int)e.Index;
            return -1;
        }
        private static int FindEquip(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return -1;
            foreach (var e in Catalogs.Equipment)
                if (e.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) return (int)e.Index;
            return -1;
        }
        private static int FindBuff(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return -1;
            foreach (var e in Catalogs.Buffs)
                if (e.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) return (int)e.Index;
            return -1;
        }
        private static string FindBody(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return null;
            foreach (var e in Catalogs.Bodies)
                if (e.Prefab != null && (e.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 || e.Prefab.name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0))
                    return e.Prefab.name;
            return null;
        }
        private static string FindSpawn(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return null;
            foreach (var e in Catalogs.SpawnCards)
                if (e.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) return e.Name;
            return null;
        }
    }
}
