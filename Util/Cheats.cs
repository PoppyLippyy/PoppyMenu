using RoR2;
using UnityEngine;

namespace PoppyMenu
{
    internal static class Cheats
    {
        internal static void DisableAll()
        {
            PlayerModule.GodMode = PlayerModule.InfiniteSkills = false;
            Aim.Enabled = false; Aim.MagicBullet = false; Aim.Target = null;
            MovementModule.Flight = MovementModule.NoClip = MovementModule.AlwaysSprint = MovementModule.JumpPack = false;
            ItemsModule.NoEquipmentCooldown = false;
            RenderModule.EspMobs = RenderModule.EspInteractables = RenderModule.EspTeleporter = false;
            StatsModule.DisableAll();
            WorldModule.FreezeMatch = false; WorldModule.FreezeTimer = false; WorldModule.TimeScale = 1f;
            Notify.Push("All features disabled");
        }

        internal static Preset Capture(string name) => new Preset
        {
            Name = name,
            God = PlayerModule.GodMode,
            Skills = PlayerModule.InfiniteSkills,
            SilentAim = Aim.Enabled,
            Flight = MovementModule.Flight,
            Sprint = MovementModule.AlwaysSprint,
            JumpPack = MovementModule.JumpPack,
            NoEquipCd = ItemsModule.NoEquipmentCooldown,
            EspMobs = RenderModule.EspMobs,
            EspInteractables = RenderModule.EspInteractables,
            EspTeleporter = RenderModule.EspTeleporter,
            DmgOn = StatsModule.DamageOn, AtkOn = StatsModule.AttackSpeedOn, MoveOn = StatsModule.MoveSpeedOn,
            ArmorOn = StatsModule.ArmorOn, CritOn = StatsModule.CritOn, HpOn = StatsModule.MaxHealthOn,
            DmgMul = StatsModule.DamageMult, AtkMul = StatsModule.AttackSpeedMult, MoveMul = StatsModule.MoveSpeedMult,
            ArmorMul = StatsModule.ArmorMult, CritMul = StatsModule.CritMult, HpMul = StatsModule.MaxHealthMult,
            Money = ModConfig.GiveMoneyAmount.Value, Xp = ModConfig.GiveXpAmount.Value, Coins = ModConfig.GiveCoinsAmount.Value,
        };

        internal static void ApplyToggles(Preset p)
        {
            if (p == null) return;
            PlayerModule.GodMode = p.God;
            PlayerModule.InfiniteSkills = p.Skills;
            Aim.Enabled = p.SilentAim;
            MovementModule.Flight = p.Flight;
            MovementModule.AlwaysSprint = p.Sprint;
            MovementModule.JumpPack = p.JumpPack;
            ItemsModule.NoEquipmentCooldown = p.NoEquipCd;
            RenderModule.EspMobs = p.EspMobs;
            RenderModule.EspInteractables = p.EspInteractables;
            RenderModule.EspTeleporter = p.EspTeleporter;
            StatsModule.DamageOn = p.DmgOn; StatsModule.AttackSpeedOn = p.AtkOn; StatsModule.MoveSpeedOn = p.MoveOn;
            StatsModule.ArmorOn = p.ArmorOn; StatsModule.CritOn = p.CritOn; StatsModule.MaxHealthOn = p.HpOn;
            StatsModule.DamageMult = p.DmgMul; StatsModule.AttackSpeedMult = p.AtkMul; StatsModule.MoveSpeedMult = p.MoveMul;
            StatsModule.ArmorMult = p.ArmorMul; StatsModule.CritMult = p.CritMul; StatsModule.MaxHealthMult = p.HpMul;
        }

        internal static void ApplyGrants(Preset p)
        {
            if (p == null) return;
            if (p.Items != null)
                foreach (GrantItem g in p.Items)
                {
                    if (g == null || string.IsNullOrEmpty(g.Name)) continue;
                    ItemIndex idx = ItemCatalog.FindItemIndex(g.Name);
                    if (idx != ItemIndex.None) NetUtil.Do(PoppyOp.GiveItem, (int)idx, Mathf.Max(1, g.Count));
                }
            if (p.Equipment != null)
                foreach (string e in p.Equipment)
                {
                    if (string.IsNullOrEmpty(e)) continue;
                    EquipmentIndex ei = EquipmentCatalog.FindEquipmentIndex(e);
                    if (ei != EquipmentIndex.None) NetUtil.Do(PoppyOp.GiveEquipment, (int)ei);
                }
            if (p.GiveAllItems) NetUtil.Do(PoppyOp.GiveAllItems, i2: 1);
            if (p.Money > 0) NetUtil.Do(PoppyOp.GiveMoney, i1: p.Money);
            if (p.Xp > 0) NetUtil.Do(PoppyOp.GiveXp, i1: p.Xp);
            if (p.Coins > 0) NetUtil.Do(PoppyOp.GiveLunar, i1: p.Coins);
        }

        internal static void Apply(Preset p)
        {
            if (p == null) return;
            ApplyToggles(p);
            ApplyGrants(p);
            Notify.Push("Applied preset: " + p.Name);
        }
    }
}
