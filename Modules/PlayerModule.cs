using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace PoppyMenu
{
    internal class PlayerModule : PoppyModule
    {
        internal override string Name => "Combat";

        internal static bool GodMode;
        internal static bool InfiniteSkills;

        private static int _heal = 500;
        private static int _hurt = 100;
        private static float _buffDuration;

        internal static void ToggleGod() => GodMode = !GodMode;

        internal override void Tick()
        {
            if (!PlayerContext.InGame || !PlayerContext.HasBody) return;

            if (PlayerContext.Health != null) PlayerContext.Health.godMode = GodMode;
            if (PlayerContext.Master != null) PlayerContext.Master.godMode = GodMode;

            if (InfiniteSkills && PlayerContext.Skills != null)
                PlayerContext.Skills.ApplyAmmoPack();
        }

        internal override void DrawMenu()
        {
            Widgets.SectionBegin("Combat");
            GodMode = Widgets.Toggle("God Mode", GodMode);
            Safety.Buddha = Widgets.Toggle("Buddha (survive lethal hits)", Safety.Buddha);
            InfiniteSkills = Widgets.Toggle("Infinite Skills", InfiniteSkills);
            Widgets.SectionEnd();

            Widgets.SectionBegin("Health");
            Widgets.Button("Heal to Full", () => NetUtil.Do(PoppyOp.HealFull));
            GUILayout.BeginHorizontal();
            Widgets.Button("Heal " + _heal, () => NetUtil.Do(PoppyOp.HealAmount, f1: _heal));
            Widgets.Button("Hurt " + _hurt, () => NetUtil.Do(PoppyOp.HurtBody, f1: _hurt));
            GUILayout.EndHorizontal();
            _heal = Widgets.IntStepper("Heal amount", _heal, 100, 1, 1000000000);
            _hurt = Widgets.IntStepper("Hurt amount", _hurt, 50, 1, 1000000000);
            Widgets.Button("Respawn", () => NetUtil.Do(PoppyOp.Respawn));
            Widgets.SectionEnd();

            Widgets.SectionBegin("Grants");
            Widgets.Button("Give Money", () => NetUtil.Do(PoppyOp.GiveMoney, i1: ModConfig.GiveMoneyAmount.Value));
            Widgets.Button("Give XP", () => NetUtil.Do(PoppyOp.GiveXp, i1: ModConfig.GiveXpAmount.Value));
            Widgets.Button("Give Lunar Coins", () => NetUtil.Do(PoppyOp.GiveLunar, i1: ModConfig.GiveCoinsAmount.Value));
            Widgets.SectionEnd();

            Widgets.SectionBegin("Buffs & DoTs");
            _buffDuration = Widgets.Slider("Buff duration (0 = permanent)", _buffDuration, 0f, 60f);
            Widgets.PickerButton("Give Buff...", "Buffs", BuffRows());
            Widgets.Button("Remove All Buffs", () => NetUtil.Do(PoppyOp.RemoveAllBuffs));
            Widgets.PickerButton("Inflict DoT...", "Damage over time", DotRows());
            Widgets.SectionEnd();
        }

        private static List<ListPicker.Row> BuffRows()
        {
            var rows = new List<ListPicker.Row>(Catalogs.Buffs.Count);
            foreach (var entry in Catalogs.Buffs)
            {
                var e = entry;
                rows.Add(new ListPicker.Row(e.Name, e.IsDebuff ? Color.red : Color.cyan, () =>
                {
                    if (_buffDuration > 0.05f) NetUtil.Do(PoppyOp.GiveTimedBuff, i1: (int)e.Index, f1: _buffDuration);
                    else NetUtil.Do(PoppyOp.GiveBuff, (int)e.Index, 1);
                }));
            }
            return rows;
        }

        private static List<ListPicker.Row> DotRows()
        {
            var rows = new List<ListPicker.Row>();
            foreach (DotController.DotIndex dot in System.Enum.GetValues(typeof(DotController.DotIndex)))
            {
                if ((int)dot < 0) continue;
                DotController.DotIndex d = dot;
                rows.Add(new ListPicker.Row(d.ToString(), Color.red, () =>
                    NetUtil.Do(PoppyOp.InflictDot, i1: (int)d, f1: _buffDuration > 0.05f ? _buffDuration : 5f)));
            }
            return rows;
        }
    }
}
