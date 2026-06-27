using UnityEngine;

namespace PoppyMenu
{
    internal class AimbotModule : PoppyModule
    {
        private static readonly string[] Priorities =
            { "Closest to Crosshair", "Closest Distance", "Lowest HP", "Highest HP" };

        internal override void Tick() => Aim.Tick();
        internal override void DrawOverlay() => Aim.DrawOverlay();

        internal override string Name => "Aimbot";

        internal override void DrawMenu()
        {
            Widgets.SectionBegin("Aimbot");
            Aim.Enabled = Widgets.Toggle("Enable Aimbot", Aim.Enabled, ModConfig.SilentAimKey.Value);
            Widgets.KeybindRow("Aim Hold Key", ModConfig.SilentAimKey);
            Widgets.Hint(ModConfig.SilentAimKey.Value == KeyCode.None
                ? "No hold key, so it's on whenever it's enabled. Click the box and press a key or mouse button."
                : $"Only active while you hold [{ModConfig.SilentAimKey.Value}]. X clears it.");
            Widgets.Hint("Silent aim. Your shots go to the target, your camera never moves.");
            Widgets.SectionEnd();

            Widgets.SectionBegin("Targeting");
            int p = Mathf.Clamp(Aim.Sorting, 0, Priorities.Length - 1);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Priority: " + Priorities[p], Theme.Label, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("<", Theme.Button, GUILayout.Width(28))) Aim.Sorting = (p + Priorities.Length - 1) % Priorities.Length;
            if (GUILayout.Button(">", Theme.Button, GUILayout.Width(28))) Aim.Sorting = (p + 1) % Priorities.Length;
            GUILayout.EndHorizontal();

            Aim.PrioritizeBosses = Widgets.Toggle("Prioritize Bosses", Aim.PrioritizeBosses);
            Aim.Sticky = Widgets.Toggle("Sticky Target", Aim.Sticky);
            Widgets.Hint("Keeps one target until it dies or leaves range.");
            Aim.RequireLoS = Widgets.Toggle("Require Line of Sight", Aim.RequireLoS);

            Aim.MaxRange = Widgets.Slider("Max Range", Aim.MaxRange, 20f, 600f);
            Aim.UseFov = Widgets.Toggle("Limit to FOV cone", Aim.UseFov);
            if (Aim.UseFov)
                Aim.Fov = Widgets.Slider("FOV (deg)", Aim.Fov, 1f, 180f);
            Widgets.SectionEnd();

            Widgets.SectionBegin("Projectiles");
            Widgets.Hint("Your projectiles automatically curve onto the locked target. Host or solo.");
            Aim.MagicBullet = Widgets.Toggle("Magic Bullet (shoot through walls)", Aim.MagicBullet);
            Widgets.Hint("Your shots pass through walls. Enemies don't. Host or solo.");
            Widgets.SectionEnd();

            Widgets.SectionBegin("Visuals");
            Aim.Highlight = Widgets.Toggle("Highlight Target", Aim.Highlight);
            Aim.ShowFovCircle = Widgets.Toggle("Show FOV Circle", Aim.ShowFovCircle);
            Widgets.SectionEnd();
        }
    }
}
