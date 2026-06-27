using HarmonyLib;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace PoppyMenu
{
    internal static class Aim
    {
        internal static bool Enabled;
        internal static bool Active;
        internal static HurtBox Target;

        internal static int Sorting;
        internal static bool UseFov;
        internal static float Fov = 60f;
        internal static float MaxRange = 300f;
        internal static bool RequireLoS;
        internal static bool PrioritizeBosses;
        internal static bool Sticky;
        internal static bool Highlight = true;
        internal static bool ShowFovCircle;
        internal static bool MagicBullet;

        private static Harmony _h;
        private static GUIStyle _label;

        internal static void Init()
        {
            _h = new Harmony("poppy.aimbot");
            var bulletFire = AccessTools.Method(typeof(BulletAttack), nameof(BulletAttack.Fire), new System.Type[0]);
            if (bulletFire != null) _h.Patch(bulletFire, prefix: new HarmonyMethod(typeof(Aim), nameof(BulletFirePrefix)));
            var fireProj = AccessTools.Method(typeof(ProjectileManager), nameof(ProjectileManager.FireProjectile), new[] { typeof(FireProjectileInfo) });
            if (fireProj != null) _h.Patch(fireProj, prefix: new HarmonyMethod(typeof(Aim), nameof(FireProjectilePrefix)));

            var initProj = AccessTools.Method(typeof(ProjectileManager), nameof(ProjectileManager.InitializeProjectile));
            if (initProj != null) _h.Patch(initProj, postfix: new HarmonyMethod(typeof(Aim), nameof(InitProjectilePostfix)));
        }

        internal static void Shutdown() { Target = null; _h?.UnpatchSelf(); _h = null; }

        internal static void Tick()
        {
            KeyCode hold = ModConfig.SilentAimKey.Value;
            Active = Enabled && (hold == KeyCode.None || Input.GetKey(hold));
            if (!Active || !PlayerContext.HasBody) { Target = null; return; }

            UpdateTarget();
        }

        private static void UpdateTarget()
        {
            CharacterBody me = PlayerContext.Body;
            if (me == null || me.teamComponent == null) { Target = null; return; }

            Ray aim = PlayerContext.AimRay();
            if (Sticky && IsValidTarget(aim)) return;

            Target = null;
            TeamIndex myTeam = me.teamComponent.teamIndex;
            float rangeSqr = MaxRange * MaxRange;
            float bestScore = float.MaxValue;
            bool bestIsBoss = false;

            foreach (CharacterBody body in CharacterBody.readOnlyInstancesList)
            {
                if (!IsHostile(me, myTeam, body)) continue;
                HurtBox hb = Util.FindBodyMainHurtBox(body);
                if (hb == null) continue;

                Vector3 to = hb.transform.position - aim.origin;
                if (to.sqrMagnitude > rangeSqr) continue;
                float angle = Vector3.Angle(aim.direction, to);
                if (UseFov && angle > Fov) continue;
                if (RequireLoS && Physics.Linecast(aim.origin, hb.transform.position, LayerIndex.world.mask)) continue;

                bool boss = body.isBoss;
                if (PrioritizeBosses)
                {
                    if (bestIsBoss && !boss) continue;
                    if (boss && !bestIsBoss) { Target = hb; bestScore = Score(body, angle, to.magnitude); bestIsBoss = true; continue; }
                }

                float score = Score(body, angle, to.magnitude);
                if (score < bestScore) { bestScore = score; bestIsBoss = boss; Target = hb; }
            }
        }

        private static float Score(CharacterBody b, float angle, float dist)
        {
            switch (Sorting)
            {
                case 1: return dist;
                case 2: return b.healthComponent != null ? b.healthComponent.health : float.MaxValue;
                case 3: return b.healthComponent != null ? -b.healthComponent.health : float.MaxValue;
                default: return angle;
            }
        }

        private static bool IsHostile(CharacterBody me, TeamIndex myTeam, CharacterBody body)
        {
            if (body == null || body == me) return false;
            if (body.healthComponent == null || !body.healthComponent.alive) return false;
            if (body.teamComponent == null) return false;
            TeamIndex t = body.teamComponent.teamIndex;
            return t != myTeam && t != TeamIndex.None && t != TeamIndex.Neutral && t != TeamIndex.Player;
        }

        private static bool IsValidTarget(Ray aim)
        {
            if (Target == null || Target.healthComponent == null || !Target.healthComponent.alive) return false;
            Vector3 to = Target.transform.position - aim.origin;
            if (to.sqrMagnitude > MaxRange * MaxRange) return false;
            if (UseFov && Vector3.Angle(aim.direction, to) > Fov) return false;
            return true;
        }

        private static bool IsLocalOwner(GameObject owner)
        {
            CharacterBody me = PlayerContext.Body;
            return me != null && owner != null && owner == me.gameObject;
        }

        private static void BulletFirePrefix(BulletAttack __instance)
        {
            if (!IsLocalOwner(__instance.owner)) return;
            if (MagicBullet)
                __instance.stopperMask = LayerIndex.entityPrecise.mask;
            if (Active && Target != null)
            {
                Vector3 dir = Target.transform.position - __instance.origin;
                if (dir.sqrMagnitude > 0.001f) __instance.aimVector = dir.normalized;
            }
        }

        private static void FireProjectilePrefix(ref FireProjectileInfo fireProjectileInfo)
        {
            if (!Active || Target == null || !IsLocalOwner(fireProjectileInfo.owner)) return;
            Vector3 dir = Target.transform.position - fireProjectileInfo.position;
            if (dir.sqrMagnitude > 0.001f) fireProjectileInfo.rotation = Quaternion.LookRotation(dir.normalized);
        }

        private static void InitProjectilePostfix(ProjectileController projectileController, FireProjectileInfo fireProjectileInfo)
        {
            if (projectileController == null || projectileController.isPrediction || !NetUtil.IsServer) return;
            if (!IsLocalOwner(fireProjectileInfo.owner)) return;

            if (MagicBullet) projectileController.gameObject.AddComponent<PoppyGhost>();

            if (Active && Target != null)
            {
                CharacterBody me = PlayerContext.Body;
                if (me != null && me.teamComponent != null)
                    projectileController.gameObject.AddComponent<PoppyHoming>().Init(Target.transform, me.teamComponent.teamIndex);
            }
        }

        internal static void DrawOverlay()
        {
            if (!Active) return;
            Camera cam = Camera.main;
            if (cam == null) return;
            if (ShowFovCircle && UseFov) DrawFovCircle(cam);
            if (Highlight && Target != null) DrawLock(cam);
        }

        private static void DrawLock(Camera cam)
        {
            Vector3 sp = cam.WorldToScreenPoint(Target.transform.position);
            if (sp.z <= 0f) return;
            float y = Screen.height - sp.y;
            Color c = Theme.Accent;
            const float s = 16f, t = 2f, len = 8f;
            Theme.Fill(new Rect(sp.x - s, y - s, len, t), c); Theme.Fill(new Rect(sp.x - s, y - s, t, len), c);
            Theme.Fill(new Rect(sp.x + s - len, y - s, len, t), c); Theme.Fill(new Rect(sp.x + s - t, y - s, t, len), c);
            Theme.Fill(new Rect(sp.x - s, y + s - t, len, t), c); Theme.Fill(new Rect(sp.x - s, y + s - len, t, len), c);
            Theme.Fill(new Rect(sp.x + s - len, y + s - t, len, t), c); Theme.Fill(new Rect(sp.x + s - t, y + s - len, t, len), c);

            if (_label == null) _label = new GUIStyle(GUI.skin.label) { fontSize = 10, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _label.normal.textColor = c;
            GUI.Label(new Rect(sp.x - 40f, y - s - 16f, 80f, 14f), "LOCKED", _label);
        }

        private static void DrawFovCircle(Camera cam)
        {
            float r = Screen.height * 0.5f * Mathf.Tan(Fov * Mathf.Deg2Rad) / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            r = Mathf.Clamp(r, 8f, Screen.height);
            float cx = Screen.width * 0.5f, cy = Screen.height * 0.5f;
            Color col = new Color(Theme.Accent.r, Theme.Accent.g, Theme.Accent.b, 0.5f);
            const int seg = 48;
            for (int i = 0; i < seg; i++)
            {
                float a = i / (float)seg * Mathf.PI * 2f;
                Theme.Fill(new Rect(cx + Mathf.Cos(a) * r - 1f, cy + Mathf.Sin(a) * r - 1f, 2f, 2f), col);
            }
        }
    }
}
