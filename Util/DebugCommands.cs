using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace PoppyMenu
{
    internal static class DebugCommands
    {
        private const StringComparison OIC = StringComparison.OrdinalIgnoreCase;

        [ConCommand(commandName = "give_item", flags = ConVarFlags.ExecuteOnServer, helpText = "Give an item directly to a target. give_item {item} [count:1]")]
        private static void CCGiveItem(ConCommandArgs args)
        {
            CharacterMaster m = args.senderMaster;
            if (m == null) { Print("give_item: no target."); return; }
            ItemIndex item = ResolveItem(Arg(args, 0));
            if (item == ItemIndex.None) { Print("give_item: unknown item."); return; }
            NetUtil.Execute(PoppyOp.GiveItem, m, (int)item, ArgInt(args, 1, 1), 0, 0, 0, false, null);
        }

        [ConCommand(commandName = "remove_item", flags = ConVarFlags.ExecuteOnServer, helpText = "Remove an item from a target. remove_item {item} [count:1]")]
        private static void CCRemoveItem(ConCommandArgs args)
        {
            CharacterMaster m = args.senderMaster;
            if (m == null) return;
            ItemIndex item = ResolveItem(Arg(args, 0));
            if (item == ItemIndex.None) { Print("remove_item: unknown item."); return; }
            NetUtil.Execute(PoppyOp.RemoveItem, m, (int)item, ArgInt(args, 1, 1), 0, 0, 0, false, null);
        }

        [ConCommand(commandName = "remove_all_items", flags = ConVarFlags.ExecuteOnServer, helpText = "Remove all items from a target.")]
        private static void CCRemoveAllItems(ConCommandArgs args)
        {
            if (args.senderMaster != null) NetUtil.Execute(PoppyOp.ClearInventory, args.senderMaster, 0, 0, 0, 0, 0, false, null);
        }

        [ConCommand(commandName = "give_equip", flags = ConVarFlags.ExecuteOnServer, helpText = "Give equipment directly. give_equip {(equip|'random')}")]
        private static void CCGiveEquipment(ConCommandArgs args)
        {
            CharacterMaster m = args.senderMaster;
            if (m == null) return;
            string q = Arg(args, 0);
            EquipmentIndex e = string.Equals(q, "random", OIC) ? RandomEquip() : ResolveEquip(q);
            if (e == EquipmentIndex.None) { Print("give_equip: unknown equipment."); return; }
            NetUtil.Execute(PoppyOp.GiveEquipment, m, (int)e, 0, 0, 0, 0, false, null);
        }

        [ConCommand(commandName = "give_money", flags = ConVarFlags.ExecuteOnServer, helpText = "Give money. give_money {amount}")]
        private static void CCGiveMoney(ConCommandArgs args)
        {
            if (args.senderMaster != null) NetUtil.Execute(PoppyOp.GiveMoney, args.senderMaster, ArgInt(args, 0, 0), 0, 0, 0, 0, false, null);
        }

        [ConCommand(commandName = "give_lunar", flags = ConVarFlags.ExecuteOnServer, helpText = "Give lunar coins. give_lunar [amount:1]")]
        private static void CCGiveLunar(ConCommandArgs args)
        {
            if (args.senderMaster != null) NetUtil.Execute(PoppyOp.GiveLunar, args.senderMaster, ArgInt(args, 0, 1), 0, 0, 0, 0, false, null);
        }

        [ConCommand(commandName = "random_items", flags = ConVarFlags.ExecuteOnServer, helpText = "Give random items. random_items {count}")]
        private static void CCRandomItems(ConCommandArgs args)
        {
            CharacterMaster m = args.senderMaster;
            if (m == null || Catalogs.Items.Count == 0) return;
            int count = Mathf.Clamp(ArgInt(args, 0, 5), 1, 1000);
            for (int i = 0; i < count; i++)
            {
                ItemIndex idx = Catalogs.Items[UnityEngine.Random.Range(0, Catalogs.Items.Count)].Index;
                NetUtil.Execute(PoppyOp.GiveItem, m, (int)idx, 1, 0, 0, 0, false, null);
            }
        }

        [ConCommand(commandName = "give_buff", flags = ConVarFlags.ExecuteOnServer, helpText = "Give a buff. give_buff {buff} [count:1] [duration:0]")]
        private static void CCGiveBuff(ConCommandArgs args)
        {
            CharacterMaster m = args.senderMaster;
            if (m == null) return;
            BuffIndex b = ResolveBuff(Arg(args, 0));
            if (b == BuffIndex.None) { Print("give_buff: unknown buff."); return; }
            float duration = ArgFloat(args, 2, 0f);
            if (duration > 0f) NetUtil.Execute(PoppyOp.GiveTimedBuff, m, (int)b, 0, duration, 0, 0, false, null);
            else NetUtil.Execute(PoppyOp.GiveBuff, m, (int)b, ArgInt(args, 1, 1), 0, 0, 0, false, null);
        }

        [ConCommand(commandName = "remove_buff", flags = ConVarFlags.ExecuteOnServer, helpText = "Remove a buff. remove_buff {buff}")]
        private static void CCRemoveBuff(ConCommandArgs args)
        {
            CharacterMaster m = args.senderMaster;
            if (m == null) return;
            BuffIndex b = ResolveBuff(Arg(args, 0));
            if (b == BuffIndex.None) { Print("remove_buff: unknown buff."); return; }
            NetUtil.Execute(PoppyOp.RemoveBuff, m, (int)b, 0, 0, 0, 0, false, null);
        }

        [ConCommand(commandName = "remove_all_buffs", flags = ConVarFlags.ExecuteOnServer, helpText = "Remove all buffs from a target.")]
        private static void CCRemoveAllBuffs(ConCommandArgs args)
        {
            if (args.senderMaster != null) NetUtil.Execute(PoppyOp.RemoveAllBuffs, args.senderMaster, 0, 0, 0, 0, 0, false, null);
        }

        [ConCommand(commandName = "god", flags = ConVarFlags.ExecuteOnServer, helpText = "Prevent player damage. god [enable (0|1)]")]
        private static void CCGod(ConCommandArgs args) { PlayerModule.GodMode = Flag(args, 0, PlayerModule.GodMode); Print("god " + PlayerModule.GodMode); }

        [ConCommand(commandName = "buddha", flags = ConVarFlags.ExecuteOnServer, helpText = "Make damage non-lethal. buddha [enable (0|1)]")]
        private static void CCBuddha(ConCommandArgs args) { Safety.Buddha = Flag(args, 0, Safety.Buddha); Print("buddha " + Safety.Buddha); }

        [ConCommand(commandName = "noclip", flags = ConVarFlags.None, helpText = "Toggle flight and collision bypass. noclip [enable (0|1)]")]
        private static void CCNoclip(ConCommandArgs args) { MovementModule.NoClip = Flag(args, 0, MovementModule.NoClip); Print("noclip " + MovementModule.NoClip); }

        [ConCommand(commandName = "no_enemies", flags = ConVarFlags.ExecuteOnServer, helpText = "Prevent enemy spawns. no_enemies [enable (0|1)]")]
        private static void CCNoEnemies(ConCommandArgs args) { Safety.NoEnemies = Flag(args, 0, Safety.NoEnemies); Print("no_enemies " + Safety.NoEnemies); }

        [ConCommand(commandName = "lock_exp", flags = ConVarFlags.ExecuteOnServer, helpText = "Prevent EXP gain. lock_exp [enable (0|1)]")]
        private static void CCLockExp(ConCommandArgs args) { Safety.LockExp = Flag(args, 0, Safety.LockExp); Print("lock_exp " + Safety.LockExp); }

        [ConCommand(commandName = "kill_all", flags = ConVarFlags.ExecuteOnServer, helpText = "Kill all members of a team. kill_all [team:Monster]")]
        private static void CCKillAll(ConCommandArgs args) { NetUtil.Execute(PoppyOp.KillAllEnemies, null, 0, 0, 0, 0, 0, false, null); }

        [ConCommand(commandName = "true_kill", flags = ConVarFlags.ExecuteOnServer, helpText = "Truly kill a player, ignoring revival effects.")]
        private static void CCTrueKill(ConCommandArgs args) { if (args.senderMaster != null) NetUtil.Execute(PoppyOp.TrueKillTarget, args.senderMaster, 0, 0, 0, 0, 0, false, null); }

        [ConCommand(commandName = "respawn", flags = ConVarFlags.ExecuteOnServer, helpText = "Respawn a player at the map spawn point.")]
        private static void CCRespawn(ConCommandArgs args) { if (args.senderMaster != null) NetUtil.Execute(PoppyOp.Respawn, args.senderMaster, 0, 0, 0, 0, 0, false, null); }

        [ConCommand(commandName = "heal", flags = ConVarFlags.ExecuteOnServer, helpText = "Heal a target. heal {amount}")]
        private static void CCHeal(ConCommandArgs args)
        {
            CharacterMaster m = args.senderMaster;
            if (m == null) return;
            if (args.Count == 0) NetUtil.Execute(PoppyOp.HealFull, m, 0, 0, 0, 0, 0, false, null);
            else NetUtil.Execute(PoppyOp.HealAmount, m, 0, 0, ArgFloat(args, 0, 0f), 0, 0, false, null);
        }

        [ConCommand(commandName = "hurt", flags = ConVarFlags.ExecuteOnServer, helpText = "Deal generic damage to a target. hurt {amount}")]
        private static void CCHurt(ConCommandArgs args)
        {
            if (args.senderMaster != null) NetUtil.Execute(PoppyOp.HurtBody, args.senderMaster, 0, 0, ArgFloat(args, 0, 0f), 0, 0, false, null);
        }

        [ConCommand(commandName = "teleport_on_cursor", flags = ConVarFlags.ExecuteOnServer, helpText = "Teleport to the location under your cursor.")]
        private static void CCTeleportOnCursor(ConCommandArgs args)
        {
            CharacterBody body = args.senderBody;
            if (body == null || body.inputBank == null) return;
            Ray ray = new Ray(body.inputBank.aimOrigin, body.inputBank.aimDirection);
            if (Physics.Raycast(ray, out RaycastHit hit, 2000f, LayerIndex.world.mask))
                TeleportHelper.TeleportBody(body, hit.point);
        }

        [ConCommand(commandName = "change_team", flags = ConVarFlags.ExecuteOnServer, helpText = "Change a player's team. change_team {team}")]
        private static void CCChangeTeam(ConCommandArgs args)
        {
            CharacterMaster m = args.senderMaster;
            if (m == null) return;
            if (!TryResolveTeam(Arg(args, 0), out TeamIndex team)) { Print("change_team: unknown team."); return; }
            NetUtil.Execute(PoppyOp.SetTeam, m, (int)team, 0, 0, 0, 0, false, null);
        }

        [ConCommand(commandName = "spawn_as", flags = ConVarFlags.ExecuteOnServer, helpText = "Spawn as a new character body. spawn_as {body}")]
        private static void CCSpawnAs(ConCommandArgs args)
        {
            CharacterMaster m = args.senderMaster;
            if (m == null) return;
            GameObject prefab = ResolveBodyPrefab(Arg(args, 0));
            if (prefab == null) { Print("spawn_as: unknown body."); return; }
            Vector3 p = m.GetBody() != null ? m.GetBody().footPosition : Vector3.zero;
            NetUtil.Execute(PoppyOp.ChangeBody, m, 0, 0, p.x, p.y, p.z, false, prefab.name);
        }

        [ConCommand(commandName = "spawn_ai", flags = ConVarFlags.ExecuteOnServer, helpText = "Spawn an AI. spawn_ai {ai} [count:1] [team:Monster]")]
        private static void CCSpawnAi(ConCommandArgs args) => SummonAi(args, false);

        [ConCommand(commandName = "spawn_body", flags = ConVarFlags.ExecuteOnServer, helpText = "Spawn a CharacterBody without AI. spawn_body {body}")]
        private static void CCSpawnBody(ConCommandArgs args) => SummonAi(args, true);

        [ConCommand(commandName = "spawn_interactable", flags = ConVarFlags.ExecuteOnServer, helpText = "Spawn an interactable. spawn_interactable {interactable}")]
        private static void CCSpawnInteractable(ConCommandArgs args) => SpawnCardAt(args);

        [ConCommand(commandName = "spawn_interactible", flags = ConVarFlags.ExecuteOnServer, helpText = "Spawn an interactable. spawn_interactible {interactable}")]
        private static void CCSpawnInteractible(ConCommandArgs args) => SpawnCardAt(args);

        [ConCommand(commandName = "spawn_portal", flags = ConVarFlags.ExecuteOnServer, helpText = "Spawn a portal. spawn_portal {blue|gold|celestial}")]
        private static void CCSpawnPortal(ConCommandArgs args) => Portal(Arg(args, 0));

        [ConCommand(commandName = "add_portal", flags = ConVarFlags.ExecuteOnServer, helpText = "Add a portal to the teleporter. add_portal {blue|gold|celestial}")]
        private static void CCAddPortal(ConCommandArgs args) => Portal(Arg(args, 0));

        [ConCommand(commandName = "next_stage", flags = ConVarFlags.ExecuteOnServer, helpText = "Advance to the next stage. next_stage [specific_stage]")]
        private static void CCNextStage(ConCommandArgs args)
        {
            string scene = Arg(args, 0);
            if (!string.IsNullOrEmpty(scene))
            {
                SceneDef def = SceneCatalog.GetSceneDefFromSceneName(scene);
                if (def != null && Run.instance != null) { Run.instance.AdvanceStage(def); return; }
            }
            NetUtil.Execute(PoppyOp.SkipStage, null, 0, 0, 0, 0, 0, false, null);
        }

        [ConCommand(commandName = "fixed_time", flags = ConVarFlags.ExecuteOnServer, helpText = "Set the time that has progressed in the run. fixed_time {time}")]
        private static void CCFixedTime(ConCommandArgs args) { Run.instance?.SetRunStopwatch(Mathf.Max(0f, ArgFloat(args, 0, 0f))); }

        [ConCommand(commandName = "stop_timer", flags = ConVarFlags.ExecuteOnServer, helpText = "Pause/unpause the run timer. stop_timer [enable (0|1)]")]
        private static void CCStopTimer(ConCommandArgs args)
        {
            WorldModule.FreezeTimer = Flag(args, 0, WorldModule.FreezeTimer);
            Run.instance?.SetRunStopwatchPaused(WorldModule.FreezeTimer);
            Print("stop_timer " + WorldModule.FreezeTimer);
        }

        [ConCommand(commandName = "charge_zone", flags = ConVarFlags.ExecuteOnServer, helpText = "Set charge of the active holdout zone (0-100). charge_zone {charge}")]
        private static void CCChargeZone(ConCommandArgs args)
        {
            TeleporterInteraction tp = TeleporterInteraction.instance;
            if (tp == null || tp.holdoutZoneController == null) return;
            tp.holdoutZoneController.charge = Mathf.Clamp01(ArgFloat(args, 0, 100f) / 100f);
        }

        [ConCommand(commandName = "set_artifact", flags = ConVarFlags.ExecuteOnServer, helpText = "Enable/disable an artifact. set_artifact {artifact} [enable (0|1)]")]
        private static void CCSetArtifact(ConCommandArgs args)
        {
            ArtifactDef ad = ResolveArtifact(Arg(args, 0));
            if (ad == null || RunArtifactManager.instance == null) { Print("set_artifact: unknown artifact."); return; }
            bool enable = args.Count > 1 ? Flag(args, 1, false) : !RunArtifactManager.instance.IsArtifactEnabled(ad);
            RunArtifactManager.instance.SetArtifactEnabledServer(ad, enable);
        }

        [ConCommand(commandName = "time_scale", flags = ConVarFlags.ExecuteOnServer, helpText = "Set the game time scale. time_scale [time_scale]")]
        private static void CCTimeScale(ConCommandArgs args) { WorldModule.TimeScale = Mathf.Max(0f, ArgFloat(args, 0, 1f)); Print("time_scale " + WorldModule.TimeScale); }

        [ConCommand(commandName = "run_set_stages_cleared", flags = ConVarFlags.ExecuteOnServer, helpText = "Set the number of stages cleared. run_set_stages_cleared {count}")]
        private static void CCSetStagesCleared(ConCommandArgs args) { NetUtil.Execute(PoppyOp.SetStagesCleared, null, ArgInt(args, 0, 0), 0, 0, 0, 0, false, null); }

        [ConCommand(commandName = "team_set_level", flags = ConVarFlags.ExecuteOnServer, helpText = "Set a team's level. team_set_level {team} {level}")]
        private static void CCTeamSetLevel(ConCommandArgs args)
        {
            if (!TryResolveTeam(Arg(args, 0), out TeamIndex team)) team = TeamIndex.Player;
            NetUtil.Execute(PoppyOp.SetTeamLevel, null, (int)team, ArgInt(args, 1, 1), 0, 0, 0, false, null);
        }

        [ConCommand(commandName = "list_item", flags = ConVarFlags.None, helpText = "List all items.")]
        private static void CCListItem(ConCommandArgs args) { foreach (var e in Catalogs.Items) { var d = ItemCatalog.GetItemDef(e.Index); Print($"[{(int)e.Index}] {(d != null ? d.name : "?")} / {e.Name}"); } }

        [ConCommand(commandName = "list_equip", flags = ConVarFlags.None, helpText = "List all equipment.")]
        private static void CCListEquip(ConCommandArgs args) { foreach (var e in Catalogs.Equipment) { var d = EquipmentCatalog.GetEquipmentDef(e.Index); Print($"[{(int)e.Index}] {(d != null ? d.name : "?")} / {e.Name}"); } }

        [ConCommand(commandName = "list_buff", flags = ConVarFlags.None, helpText = "List all buffs.")]
        private static void CCListBuff(ConCommandArgs args) { foreach (var e in Catalogs.Buffs) Print($"[{(int)e.Index}] {e.Name}"); }

        [ConCommand(commandName = "list_body", flags = ConVarFlags.None, helpText = "List all bodies.")]
        private static void CCListBody(ConCommandArgs args) { foreach (var e in Catalogs.Bodies) if (e.Prefab != null) Print($"{e.Prefab.name} / {e.Name}"); }

        [ConCommand(commandName = "list_artifact", flags = ConVarFlags.None, helpText = "List all artifacts.")]
        private static void CCListArtifact(ConCommandArgs args)
        {
            var defs = ArtifactCatalog.artifactDefs;
            if (defs == null) return;
            foreach (ArtifactDef d in defs) if (d != null) Print($"[{(int)d.artifactIndex}] {d.cachedName}");
        }

        [ConCommand(commandName = "list_survivor", flags = ConVarFlags.None, helpText = "List all survivors.")]
        private static void CCListSurvivor(ConCommandArgs args) { foreach (var e in Catalogs.Survivors) if (e.Def != null) Print($"{e.Def.cachedName} / {e.Name}"); }

        [ConCommand(commandName = "list_interactables", flags = ConVarFlags.None, helpText = "List loaded interactables.")]
        private static void CCListInteractables(ConCommandArgs args) { foreach (var e in Catalogs.SpawnCards) if (e.IsInteractable) Print(e.Name); }

        [ConCommand(commandName = "midgame", flags = ConVarFlags.ExecuteOnServer, helpText = "Random mid-game loadout.")]
        private static void CCMidgame(ConCommandArgs args) { GrantTier(args.senderMaster, ItemTier.Tier1, 20); GrantTier(args.senderMaster, ItemTier.Tier2, 8); GrantTier(args.senderMaster, ItemTier.Tier3, 2); }

        [ConCommand(commandName = "lategame", flags = ConVarFlags.ExecuteOnServer, helpText = "Random end-game loadout.")]
        private static void CCLategame(ConCommandArgs args) { GrantTier(args.senderMaster, ItemTier.Tier1, 40); GrantTier(args.senderMaster, ItemTier.Tier2, 20); GrantTier(args.senderMaster, ItemTier.Tier3, 8); GrantTier(args.senderMaster, ItemTier.Boss, 1); }

        [ConCommand(commandName = "dtzoom", flags = ConVarFlags.ExecuteOnServer, helpText = "Give movement items for fast travel.")]
        private static void CCDtzoom(ConCommandArgs args)
        {
            CharacterMaster m = args.senderMaster;
            if (m == null) return;
            ItemIndex hoof = ResolveItem("Hoof"), feather = ResolveItem("Feather");
            if (hoof != ItemIndex.None) NetUtil.Execute(PoppyOp.GiveItem, m, (int)hoof, 20, 0, 0, 0, false, null);
            if (feather != ItemIndex.None) NetUtil.Execute(PoppyOp.GiveItem, m, (int)feather, 200, 0, 0, 0, false, null);
        }

        private static void SummonAi(ConCommandArgs args, bool braindead)
        {
            CharacterBody senderBody = args.senderBody;
            GameObject master = ResolveMaster(Arg(args, 0));
            if (master == null) { Print("spawn: unknown ai/body."); return; }
            Vector3 pos = senderBody != null ? senderBody.footPosition : Vector3.zero;
            TeamIndex team = TeamIndex.Monster;
            int teamArgIndex = braindead ? -1 : 2;
            if (teamArgIndex >= 0 && TryResolveTeam(Arg(args, teamArgIndex), out TeamIndex t)) team = t;
            int count = Mathf.Clamp(braindead ? 1 : ArgInt(args, 1, 1), 1, 50);

            for (int i = 0; i < count; i++)
            {
                var summon = new MasterSummon
                {
                    masterPrefab = master,
                    position = pos,
                    rotation = Quaternion.identity,
                    ignoreTeamMemberLimit = true,
                    teamIndexOverride = team,
                    summonerBodyObject = senderBody != null ? senderBody.gameObject : null
                };
                summon.Perform();
            }
        }

        private static void SpawnCardAt(ConCommandArgs args)
        {
            CharacterBody body = args.senderBody;
            string q = Arg(args, 0);
            if (string.IsNullOrEmpty(q)) return;
            foreach (var e in Catalogs.SpawnCards)
            {
                if (e.Name.IndexOf(q, OIC) < 0) continue;
                Vector3 p = body != null ? body.footPosition : Vector3.zero;
                NetUtil.Execute(PoppyOp.Spawn, null, 1, (int)TeamIndex.Monster, p.x, p.y, p.z, false, e.Name);
                return;
            }
            Print("spawn: unknown interactable.");
        }

        private static void Portal(string kind)
        {
            kind = (kind ?? "").ToLowerInvariant();
            if (kind.StartsWith("blue") || kind.StartsWith("shop")) NetUtil.Execute(PoppyOp.SpawnShopPortal, null, 0, 0, 0, 0, 0, false, null);
            else if (kind.StartsWith("gold")) NetUtil.Execute(PoppyOp.SpawnGoldshoresPortal, null, 0, 0, 0, 0, 0, false, null);
            else if (kind.StartsWith("cele") || kind.StartsWith("moon")) NetUtil.Execute(PoppyOp.SpawnMSPortal, null, 0, 0, 0, 0, 0, false, null);
            else Print("portal must be blue, gold, or celestial.");
        }

        private static void GrantTier(CharacterMaster m, ItemTier tier, int count)
        {
            if (m == null) return;
            var pool = new List<ItemIndex>();
            foreach (var e in Catalogs.Items) if (e.Tier == tier) pool.Add(e.Index);
            if (pool.Count == 0) return;
            for (int i = 0; i < count; i++)
                NetUtil.Execute(PoppyOp.GiveItem, m, (int)pool[UnityEngine.Random.Range(0, pool.Count)], 1, 0, 0, 0, false, null);
        }

        private static void Print(string s) => Debug.Log("[Poppy] " + s);

        private static string Arg(ConCommandArgs a, int i) => i >= 0 && i < a.Count ? a[i] : null;
        private static int ArgInt(ConCommandArgs a, int i, int def) { string s = Arg(a, i); return int.TryParse(s, out int v) ? v : def; }
        private static float ArgFloat(ConCommandArgs a, int i, float def) { string s = Arg(a, i); return float.TryParse(s, out float v) ? v : def; }
        private static bool Flag(ConCommandArgs a, int i, bool current)
        {
            string s = Arg(a, i);
            if (string.IsNullOrEmpty(s)) return !current;
            return s == "1" || s.Equals("true", OIC) || s.Equals("on", OIC);
        }

        private static ItemIndex ResolveItem(string s)
        {
            if (string.IsNullOrEmpty(s)) return ItemIndex.None;
            if (int.TryParse(s, out int n) && ItemCatalog.GetItemDef((ItemIndex)n) != null) return (ItemIndex)n;
            ItemIndex exact = ItemCatalog.FindItemIndex(s);
            if (exact != ItemIndex.None) return exact;
            foreach (var e in Catalogs.Items) if (e.Name.IndexOf(s, OIC) >= 0) return e.Index;
            foreach (var e in Catalogs.Items) { var d = ItemCatalog.GetItemDef(e.Index); if (d != null && d.name.IndexOf(s, OIC) >= 0) return e.Index; }
            return ItemIndex.None;
        }

        private static EquipmentIndex ResolveEquip(string s)
        {
            if (string.IsNullOrEmpty(s)) return EquipmentIndex.None;
            if (int.TryParse(s, out int n) && EquipmentCatalog.GetEquipmentDef((EquipmentIndex)n) != null) return (EquipmentIndex)n;
            EquipmentIndex exact = EquipmentCatalog.FindEquipmentIndex(s);
            if (exact != EquipmentIndex.None) return exact;
            foreach (var e in Catalogs.Equipment) if (e.Name.IndexOf(s, OIC) >= 0) return e.Index;
            return EquipmentIndex.None;
        }

        private static EquipmentIndex RandomEquip()
        {
            return Catalogs.Equipment.Count > 0 ? Catalogs.Equipment[UnityEngine.Random.Range(0, Catalogs.Equipment.Count)].Index : EquipmentIndex.None;
        }

        private static BuffIndex ResolveBuff(string s)
        {
            if (string.IsNullOrEmpty(s)) return BuffIndex.None;
            if (int.TryParse(s, out int n)) return (BuffIndex)n;
            foreach (var e in Catalogs.Buffs) if (e.Name.IndexOf(s, OIC) >= 0) return e.Index;
            return BuffIndex.None;
        }

        private static GameObject ResolveBodyPrefab(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            GameObject p = BodyCatalog.FindBodyPrefab(s);
            if (p != null) return p;
            foreach (var e in Catalogs.Bodies)
                if (e.Prefab != null && (e.Name.IndexOf(s, OIC) >= 0 || e.Prefab.name.IndexOf(s, OIC) >= 0)) return e.Prefab;
            return null;
        }

        private static GameObject ResolveMaster(string s)
        {
            if (string.IsNullOrEmpty(s) || MasterCatalog.masterPrefabs == null) return null;
            foreach (GameObject g in MasterCatalog.masterPrefabs)
                if (g != null && g.name.IndexOf(s, OIC) >= 0) return g;
            return null;
        }

        private static ArtifactDef ResolveArtifact(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            var defs = ArtifactCatalog.artifactDefs;
            if (defs == null) return null;
            if (int.TryParse(s, out int n)) { ArtifactDef d = ArtifactCatalog.GetArtifactDef((ArtifactIndex)n); if (d != null) return d; }
            foreach (ArtifactDef d in defs)
            {
                if (d == null) continue;
                if (d.cachedName.IndexOf(s, OIC) >= 0) return d;
                string loc = Language.GetString(d.nameToken);
                if (!string.IsNullOrEmpty(loc) && loc.IndexOf(s, OIC) >= 0) return d;
            }
            return null;
        }

        private static bool TryResolveTeam(string s, out TeamIndex team)
        {
            team = TeamIndex.Monster;
            if (string.IsNullOrEmpty(s)) return false;
            if (int.TryParse(s, out int n)) { team = (TeamIndex)n; return true; }
            foreach (TeamIndex t in Enum.GetValues(typeof(TeamIndex)))
                if (t.ToString().Equals(s, OIC)) { team = t; return true; }
            return false;
        }
    }
}
