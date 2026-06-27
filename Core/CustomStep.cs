using RoR2;

namespace PoppyMenu
{
    internal enum StepKind { Feature, Item, Equipment, Buff, Money, Xp, Lunar, Spawn, Become, Heal, Command, Macro }

    internal class CustomStep
    {
        public StepKind Kind;
        public string Id = "";
        public int Index = -1;
        public int Amount = 1;
        public float Duration;
        public int Team;
        public string Label = "";
    }

    internal static class Steps
    {
        internal static CustomStep Feature(string id)
        {
            PoppyAction a = ActionRegistry.Get(id);
            return new CustomStep { Kind = StepKind.Feature, Id = id, Label = a != null ? a.Name : id };
        }

        internal static CustomStep Item(int index, int count)
            => new CustomStep { Kind = StepKind.Item, Index = index, Amount = count, Label = $"Give {ItemName(index)} x{count}" };

        internal static CustomStep Equipment(int index)
            => new CustomStep { Kind = StepKind.Equipment, Index = index, Label = "Equip " + EquipName(index) };

        internal static CustomStep Buff(int index, float duration)
            => new CustomStep { Kind = StepKind.Buff, Index = index, Duration = duration, Label = duration > 0f ? $"Buff {BuffName(index)} ({duration:0}s)" : "Buff " + BuffName(index) };

        internal static CustomStep Money(int amount) => new CustomStep { Kind = StepKind.Money, Amount = amount, Label = "Money +" + amount };
        internal static CustomStep Xp(int amount) => new CustomStep { Kind = StepKind.Xp, Amount = amount, Label = "XP +" + amount };
        internal static CustomStep Lunar(int amount) => new CustomStep { Kind = StepKind.Lunar, Amount = amount, Label = "Lunar +" + amount };

        internal static CustomStep Spawn(string card, int count, int team, string display)
            => new CustomStep { Kind = StepKind.Spawn, Id = card, Amount = count, Team = team, Label = $"Spawn {display} x{count}" };

        internal static CustomStep Become(string body, string display)
            => new CustomStep { Kind = StepKind.Become, Id = body, Label = "Become " + display };

        internal static CustomStep Heal(int amount) => new CustomStep { Kind = StepKind.Heal, Amount = amount, Label = amount > 0 ? "Heal " + amount : "Heal to full" };
        internal static CustomStep Command(string cmd) => new CustomStep { Kind = StepKind.Command, Id = cmd, Label = "> " + cmd };
        internal static CustomStep Macro(string name) => new CustomStep { Kind = StepKind.Macro, Id = name, Label = "Macro: " + name };

        private static string ItemName(int idx) { foreach (var e in Catalogs.Items) if ((int)e.Index == idx) return e.Name; return "item"; }
        private static string BuffName(int idx) { foreach (var e in Catalogs.Buffs) if ((int)e.Index == idx) return e.Name; return "buff"; }
        private static string EquipName(int idx) { foreach (var e in Catalogs.Equipment) if ((int)e.Index == idx) return e.Name; return "equipment"; }
    }
}
