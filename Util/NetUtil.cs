using System.Collections.Generic;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace PoppyMenu
{
    internal enum PoppyOp : byte
    {
        GiveItem, RemoveItem, GiveEquipment, GiveAllItems, ClearInventory, StackInventory, RollItems,
        GiveMoney, GiveXp, GiveLunar,
        GiveBuff, RemoveBuff, RemoveAllBuffs,
        Respawn, ChangeBody, HealFull,
        KillAllEnemies, Spawn,
        ChargeTeleporter, SkipStage, AddMountainShrine,
        SpawnShopPortal, SpawnGoldshoresPortal, SpawnMSPortal,
        SetStagesCleared, SetRunTime, SetTeamLevel, SetArtifact,
        TrueKillTarget, TeleportBody, SetTeam, HurtBody,
        HealAmount, GiveTimedBuff, InflictDot,
        ChangeScene, UndoInventory
    }

    internal static class NetUtil
    {
        internal static bool IsServer => NetworkServer.active;

        internal static void Init()
        {
            NetworkingAPI.RegisterMessageType<PoppyCommandMessage>();
        }

        internal static void Do(PoppyOp op, int i1 = 0, int i2 = 0, float f1 = 0, float f2 = 0, float f3 = 0, bool b1 = false, string s1 = null)
            => DoFor(PlayerContext.Master, op, i1, i2, f1, f2, f3, b1, s1);

        internal static void DoFor(CharacterMaster target, PoppyOp op, int i1 = 0, int i2 = 0, float f1 = 0, float f2 = 0, float f3 = 0, bool b1 = false, string s1 = null)
        {
            if (IsServer)
            {
                Execute(op, target, i1, i2, f1, f2, f3, b1, s1);
                Notify.Push(Describe(op, s1));
                return;
            }

            if (ModConfig.RequireServerForCheats.Value)
            {
                Notify.Push("Skipped: host only");
                Log.Warning($"{op} skipped: not the server (RequireServerForCheats is on).");
                return;
            }

            NetworkInstanceId id = target != null ? target.netId : NetworkInstanceId.Invalid;
            new PoppyCommandMessage(op, id, i1, i2, f1, f2, f3, b1, s1).Send(NetworkDestination.Server);
            Notify.Push(Describe(op, s1) + " (requested)");
        }

        private static string Describe(PoppyOp op, string s1)
        {
            switch (op)
            {
                case PoppyOp.GiveItem: return "Item given";
                case PoppyOp.GiveEquipment: return "Equipment given";
                case PoppyOp.GiveAllItems: return "Gave all items";
                case PoppyOp.ClearInventory: return "Inventory cleared";
                case PoppyOp.StackInventory: return "Inventory stacked";
                case PoppyOp.RollItems: return "Items rerolled";
                case PoppyOp.GiveMoney: return "Money granted";
                case PoppyOp.GiveXp: return "XP granted";
                case PoppyOp.GiveLunar: return "Lunar coins granted";
                case PoppyOp.GiveBuff: return "Buff applied";
                case PoppyOp.RemoveAllBuffs: return "Buffs removed";
                case PoppyOp.Respawn: return "Respawned";
                case PoppyOp.HealFull: return "Healed to full";
                case PoppyOp.ChangeBody: return "Became " + (s1 ?? "body");
                case PoppyOp.KillAllEnemies: return "Killed all enemies";
                case PoppyOp.Spawn: return "Spawned " + (s1 ?? "object");
                case PoppyOp.ChargeTeleporter: return "Teleporter charged";
                case PoppyOp.SkipStage: return "Skipping stage";
                case PoppyOp.AddMountainShrine: return "Mountain shrine added";
                case PoppyOp.SpawnShopPortal: return "Shop portal spawned";
                case PoppyOp.SpawnGoldshoresPortal: return "Gold portal spawned";
                case PoppyOp.SpawnMSPortal: return "Celestial portal spawned";
                case PoppyOp.SetStagesCleared: return "Stages cleared set";
                case PoppyOp.SetRunTime: return "Run time set";
                case PoppyOp.SetTeamLevel: return "Team level set";
                case PoppyOp.SetArtifact: return "Artifact toggled";
                case PoppyOp.TrueKillTarget: return "Player killed";
                case PoppyOp.TeleportBody: return "Teleported";
                case PoppyOp.SetTeam: return "Team changed";
                case PoppyOp.HurtBody: return "Damage dealt";
                case PoppyOp.HealAmount: return "Healed";
                case PoppyOp.GiveTimedBuff: return "Buff applied";
                case PoppyOp.InflictDot: return "DoT applied";
                case PoppyOp.ChangeScene: return "Changing scene";
                case PoppyOp.UndoInventory: return "Inventory restored";
                default: return op.ToString();
            }
        }

        internal static NetworkUser UserForMaster(CharacterMaster master)
        {
            if (master == null) return null;
            foreach (NetworkUser u in NetworkUser.readOnlyInstancesList)
                if (u != null && u.master == master) return u;
            return null;
        }

        internal static void KickUser(NetworkUser user)
        {
            if (!NetworkServer.active || user == null) { Notify.Push("Kick is host only"); return; }
            NetworkConnection conn = ConnectionFor(user);
            if (conn == null) { Notify.Push("Couldn't find that connection"); return; }
            var reason = new RoR2.Networking.NetworkManagerSystem.SimpleLocalizedKickReason("KICK_REASON_KICK");
            RoR2.Networking.NetworkManagerSystem.singleton.ServerKickClient(conn, reason);
            Notify.Push("Kicked " + user.userName);
        }

        internal static void BanUser(NetworkUser user)
        {
            if (!NetworkServer.active || user == null) { Notify.Push("Ban is host only"); return; }
            NetworkConnection conn = ConnectionFor(user);
            if (conn == null) { Notify.Push("Couldn't find that connection"); return; }
            RoR2.Networking.NetworkManagerSystem.singleton.ServerBanClient(conn);
            Notify.Push("Banned " + user.userName);
        }

        private static NetworkConnection ConnectionFor(NetworkUser user)
        {
            foreach (NetworkConnection c in NetworkServer.connections)
                if (c != null && user.connectionToClient == c) return c;
            return null;
        }

        private static void GiveBestOfEachItem(Inventory inv, int count)
        {
            var groups = new Dictionary<string, List<Catalogs.ItemEntry>>();
            foreach (Catalogs.ItemEntry e in Catalogs.Items)
            {
                if (!groups.TryGetValue(e.Name, out var list)) { list = new List<Catalogs.ItemEntry>(); groups[e.Name] = list; }
                list.Add(e);
            }

            foreach (var pair in groups)
            {
                List<Catalogs.ItemEntry> variants = pair.Value;
                if (!GroupHasBeneficialItem(variants)) continue;

                bool found = false;
                Catalogs.ItemEntry best = default;
                foreach (Catalogs.ItemEntry v in variants)
                {
                    ItemDef def = ItemCatalog.GetItemDef(v.Index);
                    if (def == null || def.hidden || def.tier == ItemTier.Lunar || def.ContainsTag(ItemTag.WorldUnique)) continue;
                    if (!found || IsBetterVariant(v, best)) { best = v; found = true; }
                }
                if (found) inv.GiveItem(best.Index, Mathf.Max(1, count));
            }
        }

        private static bool GroupHasBeneficialItem(List<Catalogs.ItemEntry> variants)
        {
            foreach (Catalogs.ItemEntry v in variants)
            {
                ItemDef def = ItemCatalog.GetItemDef(v.Index);
                if (def == null || def.hidden || def.ContainsTag(ItemTag.WorldUnique)) continue;
                switch (def.tier)
                {
                    case ItemTier.Tier1:
                    case ItemTier.Tier2:
                    case ItemTier.Tier3:
                    case ItemTier.Boss:
                    case ItemTier.VoidTier1:
                    case ItemTier.VoidTier2:
                    case ItemTier.VoidTier3:
                    case ItemTier.VoidBoss:
                        return true;
                }
            }
            return false;
        }

        private static bool IsBetterVariant(Catalogs.ItemEntry a, Catalogs.ItemEntry b)
        {
            int ra = QualityRank(a.Quality), rb = QualityRank(b.Quality);
            if (ra != rb) return ra > rb;
            return (int)a.Index > (int)b.Index;
        }

        private static int QualityRank(string q)
        {
            if (string.IsNullOrEmpty(q)) return 0;
            string s = q.ToLowerInvariant();
            if (s.Contains("mythic") || s.Contains("godly")) return 8;
            if (s.Contains("legend")) return 7;
            if (s.Contains("epic")) return 6;
            if (s.Contains("unique")) return 6;
            if (s.Contains("rare")) return 5;
            if (s.Contains("uncommon")) return 4;
            if (s.Contains("common")) return 3;
            return 1;
        }

        internal static void Execute(PoppyOp op, CharacterMaster master, int i1, int i2, float f1, float f2, float f3, bool b1, string s1)
        {
            try
            {
                CharacterBody body = master != null ? master.GetBody() : null;
                Inventory inv = master != null ? master.inventory : null;

                switch (op)
                {
                    case PoppyOp.GiveItem:
                        inv?.GiveItem((ItemIndex)i1, Mathf.Max(1, i2));
                        break;
                    case PoppyOp.RemoveItem:
                        inv?.RemoveItem((ItemIndex)i1, Mathf.Max(1, i2));
                        break;
                    case PoppyOp.GiveEquipment:
                        inv?.SetEquipmentIndex((EquipmentIndex)i1);
                        break;
                    case PoppyOp.GiveAllItems:
                        if (inv != null)
                        {
                            Snapshot(inv);
                            GiveBestOfEachItem(inv, i2);
                            if (body != null && body.healthComponent != null) body.healthComponent.HealFraction(1f, default);
                        }
                        break;
                    case PoppyOp.ClearInventory:
                        if (inv != null) { Snapshot(inv); inv.CleanInventory(); }
                        break;
                    case PoppyOp.StackInventory:
                        Snapshot(inv);
                        StackInventory(inv);
                        break;
                    case PoppyOp.RollItems:
                        Snapshot(inv);
                        RollItems(inv);
                        break;
                    case PoppyOp.UndoInventory:
                        RestoreSnapshot(inv);
                        break;
                    case PoppyOp.GiveMoney:
                        master?.GiveMoney((uint)Mathf.Max(0, i1));
                        break;
                    case PoppyOp.GiveXp:
                        master?.GiveExperience((ulong)Mathf.Max(0, i1));
                        break;
                    case PoppyOp.GiveLunar:
                        UserForMaster(master)?.AwardLunarCoins((uint)Mathf.Max(0, i1));
                        break;
                    case PoppyOp.GiveBuff:
                        if (body != null) body.SetBuffCount((BuffIndex)i1, body.GetBuffCount((BuffIndex)i1) + Mathf.Max(1, i2));
                        break;
                    case PoppyOp.RemoveBuff:
                        body?.RemoveBuff((BuffIndex)i1);
                        break;
                    case PoppyOp.RemoveAllBuffs:
                        RemoveAllBuffs(body);
                        break;
                    case PoppyOp.Respawn:
                        if (master != null && master.GetBody() == null) master.RespawnExtraLife();
                        break;
                    case PoppyOp.HealFull:
                        if (body != null && body.healthComponent != null)
                            body.healthComponent.Heal(body.healthComponent.fullCombinedHealth, default, true);
                        break;
                    case PoppyOp.ChangeBody:
                        ChangeBody(master, s1, new Vector3(f1, f2, f3));
                        break;
                    case PoppyOp.KillAllEnemies:
                        KillAllEnemies();
                        break;
                    case PoppyOp.Spawn:
                        Spawn(s1, new Vector3(f1, f2, f3), Mathf.Max(1, i1), i2);
                        break;
                    case PoppyOp.ChargeTeleporter:
                        ChargeTeleporter();
                        break;
                    case PoppyOp.SkipStage:
                        SkipStage();
                        break;
                    case PoppyOp.AddMountainShrine:
                        if (TeleporterInteraction.instance != null) TeleporterInteraction.instance.AddShrineStack();
                        break;
                    case PoppyOp.SpawnShopPortal:
                        TeleporterInteraction.instance?.AttemptToSpawnShopPortal();
                        break;
                    case PoppyOp.SpawnGoldshoresPortal:
                        TeleporterInteraction.instance?.AttemptToSpawnGoldshoresPortal();
                        break;
                    case PoppyOp.SpawnMSPortal:
                        TeleporterInteraction.instance?.AttemptToSpawnMSPortal();
                        break;
                    case PoppyOp.SetStagesCleared:
                        if (Run.instance != null) Run.instance.stageClearCount = Mathf.Max(0, i1);
                        break;
                    case PoppyOp.SetRunTime:
                        Run.instance?.SetRunStopwatch(Mathf.Max(0f, f1));
                        break;
                    case PoppyOp.SetTeamLevel:
                        if (TeamManager.instance != null)
                            TeamManager.instance.SetTeamLevel((TeamIndex)i1, (uint)Mathf.Max(1, i2));
                        break;
                    case PoppyOp.SetArtifact:
                        {
                            ArtifactDef ad = ArtifactCatalog.GetArtifactDef((ArtifactIndex)i1);
                            if (ad != null && RunArtifactManager.instance != null)
                                RunArtifactManager.instance.SetArtifactEnabledServer(ad, b1);
                        }
                        break;
                    case PoppyOp.TrueKillTarget:
                        master?.TrueKill();
                        break;
                    case PoppyOp.TeleportBody:
                        if (body != null) TeleportHelper.TeleportBody(body, new Vector3(f1, f2, f3));
                        break;
                    case PoppyOp.SetTeam:
                        if (body != null && body.teamComponent != null) body.teamComponent.teamIndex = (TeamIndex)i1;
                        if (master != null) master.teamIndex = (TeamIndex)i1;
                        break;
                    case PoppyOp.HurtBody:
                        if (body != null && body.healthComponent != null)
                        {
                            var di = new DamageInfo
                            {
                                damage = Mathf.Max(0f, f1),
                                position = body.corePosition,
                                damageColorIndex = DamageColorIndex.Default,
                                damageType = DamageType.Generic
                            };
                            body.healthComponent.TakeDamage(di);
                        }
                        break;
                    case PoppyOp.HealAmount:
                        if (body != null && body.healthComponent != null)
                            body.healthComponent.Heal(Mathf.Max(0f, f1), default, true);
                        break;
                    case PoppyOp.GiveTimedBuff:
                        body?.AddTimedBuff((BuffIndex)i1, Mathf.Max(0.1f, f1));
                        break;
                    case PoppyOp.InflictDot:
                        if (body != null)
                        {
                            var dot = new InflictDotInfo
                            {
                                victimObject = body.gameObject,
                                attackerObject = body.gameObject,
                                dotIndex = (DotController.DotIndex)i1,
                                duration = Mathf.Max(0.1f, f1),
                                damageMultiplier = 1f
                            };
                            DotController.InflictDot(ref dot);
                        }
                        break;
                    case PoppyOp.ChangeScene:
                        {
                            SceneDef def = SceneCatalog.GetSceneDef((SceneIndex)i1);
                            if (def != null && Run.instance != null) Run.instance.AdvanceStage(def);
                        }
                        break;
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"NetUtil.Execute({op}) failed: {e}");
            }
        }

        private static readonly List<ItemIndex> _undoItems = new List<ItemIndex>();
        private static readonly List<int> _undoCounts = new List<int>();
        private static EquipmentIndex _undoEquip = EquipmentIndex.None;
        private static bool _hasUndo;

        private static void Snapshot(Inventory inv)
        {
            _undoItems.Clear();
            _undoCounts.Clear();
            if (inv == null) { _hasUndo = false; return; }
            foreach (ItemIndex idx in inv.itemAcquisitionOrder) { _undoItems.Add(idx); _undoCounts.Add(inv.GetItemCount(idx)); }
            _undoEquip = inv.GetEquipmentIndex();
            _hasUndo = true;
        }

        private static void RestoreSnapshot(Inventory inv)
        {
            if (inv == null || !_hasUndo) { Notify.Push("Nothing to undo"); return; }
            inv.CleanInventory();
            for (int k = 0; k < _undoItems.Count; k++) inv.GiveItem(_undoItems[k], _undoCounts[k]);
            inv.SetEquipmentIndex(_undoEquip);
            _hasUndo = false;
        }

        private static void RemoveAllBuffs(CharacterBody body)
        {
            if (body == null) return;
            BuffDef[] defs = BuffCatalog.buffDefs;
            if (defs == null) return;
            foreach (BuffDef def in defs)
            {
                if (def == null) continue;
                int count = body.GetBuffCount(def.buffIndex);
                for (int i = 0; i < count; i++) body.RemoveBuff(def.buffIndex);
            }
        }

        private static void StackInventory(Inventory inv)
        {
            if (inv == null) return;
            var byTier = new Dictionary<ItemTier, List<ItemIndex>>();
            foreach (ItemIndex idx in new List<ItemIndex>(inv.itemAcquisitionOrder))
            {
                ItemDef def = ItemCatalog.GetItemDef(idx);
                if (def == null) continue;
                if (!byTier.TryGetValue(def.tier, out var list)) { list = new List<ItemIndex>(); byTier[def.tier] = list; }
                list.Add(idx);
            }
            foreach (var pair in byTier)
            {
                List<ItemIndex> items = pair.Value;
                if (items.Count == 0) continue;
                int total = 0;
                ItemIndex best = items[0];
                int bestCount = -1;
                foreach (ItemIndex idx in items)
                {
                    int c = inv.GetItemCount(idx);
                    total += c;
                    if (c > bestCount) { bestCount = c; best = idx; }
                }
                foreach (ItemIndex idx in items) inv.RemoveItem(idx, inv.GetItemCount(idx));
                inv.GiveItem(best, total);
            }
        }

        private static void RollItems(Inventory inv)
        {
            if (inv == null) return;
            var tierLists = new Dictionary<ItemTier, List<ItemIndex>>();
            foreach (var e in Catalogs.Items)
            {
                if (!tierLists.TryGetValue(e.Tier, out var list)) { list = new List<ItemIndex>(); tierLists[e.Tier] = list; }
                list.Add(e.Index);
            }
            foreach (ItemIndex idx in new List<ItemIndex>(inv.itemAcquisitionOrder))
            {
                ItemDef def = ItemCatalog.GetItemDef(idx);
                if (def == null) continue;
                int count = inv.GetItemCount(idx);
                if (!tierLists.TryGetValue(def.tier, out var pool) || pool.Count == 0) continue;
                ItemIndex roll = pool[Random.Range(0, pool.Count)];
                inv.RemoveItem(idx, count);
                inv.GiveItem(roll, count);
            }
        }

        private static CharacterMaster _goGuardMaster;
        private static float _goGuardUntil;

        internal static void TickGuards()
        {
            if (_goGuardMaster != null && Time.time >= _goGuardUntil)
            {
                _goGuardMaster.preventGameOver = false;
                _goGuardMaster = null;
            }
        }

        private static void ChangeBody(CharacterMaster master, string bodyName, Vector3 fallbackPos)
        {
            if (master == null || string.IsNullOrEmpty(bodyName)) return;
            GameObject prefab = BodyCatalog.FindBodyPrefab(bodyName);
            if (prefab == null) { Log.Warning($"ChangeBody: no body '{bodyName}'."); return; }

            CharacterBody current = master.GetBody();
            Vector3 pos = current != null ? current.footPosition : fallbackPos;
            Quaternion rot = current != null ? current.transform.rotation : Quaternion.identity;
            GameObject original = master.bodyPrefab;

            master.preventGameOver = true;
            _goGuardMaster = master;
            _goGuardUntil = Time.time + 5f;

            master.bodyPrefab = prefab;
            CharacterBody nb = master.Respawn(pos, rot, false);
            master.lostBodyToDeath = false;

            if (nb == null && original != null)
            {
                master.bodyPrefab = original;
                master.Respawn(pos, rot, false);
                Notify.Push("Couldn't spawn that body, reverted");
            }
        }

        private static void KillAllEnemies()
        {
            foreach (CharacterMaster m in CharacterMaster.readOnlyInstancesList)
            {
                if (m == null) continue;
                CharacterBody b = m.GetBody();
                if (b == null || b.teamComponent == null) continue;
                if (b.teamComponent.teamIndex == TeamIndex.Monster || b.teamComponent.teamIndex == TeamIndex.Void)
                    m.TrueKill();
            }
        }

        private static void Spawn(string cardName, Vector3 position, int count, int teamRaw)
        {
            if (string.IsNullOrEmpty(cardName) || DirectorCore.instance == null || Run.instance == null) return;
            SpawnCard card = null;
            bool interactable = false;
            foreach (var e in Catalogs.SpawnCards)
            {
                if (e.Name == cardName) { card = e.Card; interactable = e.IsInteractable; break; }
            }
            if (card == null) { Log.Warning($"Spawn: no card '{cardName}'."); return; }

            TeamIndex team = (TeamIndex)teamRaw;
            for (int i = 0; i < Mathf.Clamp(count, 1, 50); i++)
            {
                var placement = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    position = position,
                    minDistance = 0f,
                    maxDistance = 40f
                };
                var request = new DirectorSpawnRequest(card, placement, Run.instance.spawnRng)
                {
                    ignoreTeamMemberLimit = true
                };
                if (!interactable)
                    request.teamIndexOverride = team;
                DirectorCore.instance.TrySpawnObject(request);
            }
        }

        private static void ChargeTeleporter()
        {
            TeleporterInteraction tp = TeleporterInteraction.instance;
            if (tp != null && tp.holdoutZoneController != null)
                tp.holdoutZoneController.FullyChargeHoldoutZone();
        }

        private static void SkipStage()
        {
            if (Run.instance == null) return;
            Run.instance.PickNextStageSceneFromCurrentSceneDestinations();
            if (Run.instance.nextStageScene != null)
                Run.instance.AdvanceStage(Run.instance.nextStageScene);
        }
    }

    internal class PoppyCommandMessage : INetMessage
    {
        private byte _op;
        private NetworkInstanceId _target;
        private int _i1, _i2;
        private float _f1, _f2, _f3;
        private bool _b1;
        private string _s1;

        public PoppyCommandMessage() { }

        public PoppyCommandMessage(PoppyOp op, NetworkInstanceId target, int i1, int i2, float f1, float f2, float f3, bool b1, string s1)
        {
            _op = (byte)op; _target = target; _i1 = i1; _i2 = i2; _f1 = f1; _f2 = f2; _f3 = f3; _b1 = b1; _s1 = s1 ?? string.Empty;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(_op);
            writer.Write(_target);
            writer.Write(_i1);
            writer.Write(_i2);
            writer.Write(_f1);
            writer.Write(_f2);
            writer.Write(_f3);
            writer.Write(_b1);
            writer.Write(_s1 ?? string.Empty);
        }

        public void Deserialize(NetworkReader reader)
        {
            _op = reader.ReadByte();
            _target = reader.ReadNetworkId();
            _i1 = reader.ReadInt32();
            _i2 = reader.ReadInt32();
            _f1 = reader.ReadSingle();
            _f2 = reader.ReadSingle();
            _f3 = reader.ReadSingle();
            _b1 = reader.ReadBoolean();
            _s1 = reader.ReadString();
        }

        public void OnReceived()
        {
            if (!NetworkServer.active) return;
            if (!ModConfig.AllowClientCheats.Value) return;
            CharacterMaster master = null;
            if (!_target.IsEmpty())
            {
                GameObject obj = Util.FindNetworkObject(_target);
                if (obj != null) master = obj.GetComponent<CharacterMaster>();
            }
            NetUtil.Execute((PoppyOp)_op, master, _i1, _i2, _f1, _f2, _f3, _b1, _s1);
        }
    }
}
