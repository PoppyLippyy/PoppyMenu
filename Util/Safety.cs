using HarmonyLib;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace PoppyMenu
{
    internal static class Safety
    {
        internal static bool NoEnemies;
        internal static bool Buddha;
        internal static bool LockExp;
        internal static bool PreventProfileWriting;

        private static Harmony _h;

        internal static void Init()
        {
            _h = new Harmony("poppy.safety");
            Patch(typeof(HealthComponent), "TakeDamage", nameof(BuddhaPrefix));
            Patch(typeof(ExperienceManager), "AwardExperience", nameof(LockExpPrefix));
            Patch(typeof(UserProfile), "RequestEventualSave", nameof(PreventSavePrefix));
            CharacterBody.onBodyStartGlobal += OnBodyStart;
        }

        internal static void Shutdown()
        {
            CharacterBody.onBodyStartGlobal -= OnBodyStart;
            _h?.UnpatchSelf();
            _h = null;
        }

        private static void Patch(System.Type type, string method, string prefix)
        {
            try
            {
                var m = AccessTools.Method(type, method);
                if (m != null) _h.Patch(m, prefix: new HarmonyMethod(typeof(Safety), prefix));
                else Log.Warning($"Safety: {type.Name}.{method} not found, that toggle will be inert.");
            }
            catch (System.Exception e) { Log.Error($"Safety patch {type.Name}.{method}: {e}"); }
        }

        private static void OnBodyStart(CharacterBody body)
        {
            if (!NoEnemies || !NetworkServer.active) return;
            if (body == null || body.teamComponent == null || body.master == null) return;
            TeamIndex team = body.teamComponent.teamIndex;
            if (team == TeamIndex.Monster || team == TeamIndex.Void) body.master.TrueKill();
        }

        private static void BuddhaPrefix(HealthComponent __instance, DamageInfo damageInfo)
        {
            if (!Buddha || __instance == null || damageInfo == null) return;
            if (__instance.body == null || !IsLocalPlayer(__instance.body)) return;
            float survivable = Mathf.Max(0f, __instance.combinedHealth - 1f);
            if (damageInfo.damage > survivable) damageInfo.damage = survivable;
        }

        private static bool LockExpPrefix() => !LockExp;

        private static bool PreventSavePrefix() => !PreventProfileWriting;

        private static bool IsLocalPlayer(CharacterBody body)
        {
            NetworkUser user = RoR2.Util.LookUpBodyNetworkUser(body);
            return user != null && NetworkUser.readOnlyLocalPlayersList.Contains(user);
        }
    }
}
