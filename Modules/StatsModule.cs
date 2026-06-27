using System;
using HarmonyLib;
using RoR2;

namespace PoppyMenu
{
    internal class StatsModule : PoppyModule
    {
        internal override string Name => "Stats";

        internal static bool DamageOn, AttackSpeedOn, MoveSpeedOn, ArmorOn, CritOn, MaxHealthOn;
        internal static float DamageMult = 1f, AttackSpeedMult = 1f, MoveSpeedMult = 1f,
                              ArmorMult = 1f, CritMult = 1f, MaxHealthMult = 1f;

        private static Harmony _harmony;
        private static bool _wasOn;

        private static bool AnyOn =>
            DamageOn || AttackSpeedOn || MoveSpeedOn || ArmorOn || CritOn || MaxHealthOn;

        internal static bool Active => AnyOn;

        internal static void DisableAll()
        {
            DamageOn = AttackSpeedOn = MoveSpeedOn = ArmorOn = CritOn = MaxHealthOn = false;
        }

        private static void EnsurePatched()
        {
            if (_harmony != null)
                return;
            _harmony = new Harmony("poppy.stats");
            var orig = AccessTools.Method(typeof(CharacterBody), nameof(CharacterBody.RecalculateStats));
            var post = AccessTools.Method(typeof(StatsModule), nameof(RecalcPostfix));
            _harmony.Patch(orig, postfix: new HarmonyMethod(post));
        }

        private static void RecalcPostfix(CharacterBody __instance)
        {
            if (__instance == null || __instance != PlayerContext.Body)
                return;
            if (DamageOn) __instance.damage *= DamageMult;
            if (AttackSpeedOn) __instance.attackSpeed *= AttackSpeedMult;
            if (MoveSpeedOn) __instance.moveSpeed *= MoveSpeedMult;
            if (ArmorOn) __instance.armor *= ArmorMult;
            if (CritOn) __instance.crit *= CritMult;
            if (MaxHealthOn) __instance.maxHealth *= MaxHealthMult;
        }

        internal override void Tick()
        {
            EnsurePatched();
            if ((AnyOn || _wasOn) && PlayerContext.HasBody)
                PlayerContext.Body.RecalculateStats();
            _wasOn = AnyOn;
        }

        internal override void DrawMenu()
        {
            Widgets.SectionBegin("Stat Multipliers");

            DamageOn = Widgets.Toggle("Damage", DamageOn);
            DamageMult = Widgets.Slider("Damage x", DamageMult, 1f, 50f);

            AttackSpeedOn = Widgets.Toggle("Attack Speed", AttackSpeedOn);
            AttackSpeedMult = Widgets.Slider("Atk Spd x", AttackSpeedMult, 1f, 50f);

            MoveSpeedOn = Widgets.Toggle("Move Speed", MoveSpeedOn);
            MoveSpeedMult = Widgets.Slider("Move x", MoveSpeedMult, 1f, 50f);

            ArmorOn = Widgets.Toggle("Armor", ArmorOn);
            ArmorMult = Widgets.Slider("Armor x", ArmorMult, 1f, 50f);

            CritOn = Widgets.Toggle("Crit", CritOn);
            CritMult = Widgets.Slider("Crit x", CritMult, 1f, 50f);

            MaxHealthOn = Widgets.Toggle("Max Health", MaxHealthOn);
            MaxHealthMult = Widgets.Slider("Health x", MaxHealthMult, 1f, 50f);
            Widgets.SectionEnd();

            Widgets.SectionBegin("Current");
            if (PlayerContext.HasBody)
            {
                CharacterBody b = PlayerContext.Body;
                Widgets.Label($"Max Health: {b.maxHealth:0}");
                Widgets.Label($"Damage: {b.damage:0.##}");
                Widgets.Label($"Attack Speed: {b.attackSpeed:0.##}");
                Widgets.Label($"Armor: {b.armor:0.##}");
                Widgets.Label($"Crit: {b.crit:0.##}%");
                Widgets.Label($"Move Speed: {b.moveSpeed:0.##}");
            }
            else
            {
                Widgets.Label("(no body)");
            }
            Widgets.SectionEnd();
        }

        internal override void OnUnload()
        {
            if (_harmony != null)
            {
                _harmony.UnpatchSelf();
                _harmony = null;
            }
        }
    }
}
