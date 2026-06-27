using RoR2;
using UnityEngine;

namespace PoppyMenu
{
    internal class RenderModule : PoppyModule
    {
        internal override string Name => "ESP";

        internal static bool EspMobs;
        internal static bool EspInteractables;
        internal static bool EspTeleporter;

        internal static bool ShowNames = true;
        internal static bool ShowDistance = true;
        internal static bool ShowEnemyHealth = true;
        internal static float MaxDistance;
        internal static float MarkerSize = 6f;

        internal static Color EnemyColor = Color.red;
        internal static Color InteractableColor = Color.cyan;
        internal static Color TeleporterColor = Color.yellow;

        private static GUIStyle _labelStyle;
        private static Color _styleColor = Color.clear;

        private static PurchaseInteraction[] _interactables = new PurchaseInteraction[0];
        private static float _nextScan;

        internal override void Tick()
        {
            if (EspInteractables && Time.realtimeSinceStartup >= _nextScan)
            {
                _interactables = Object.FindObjectsOfType<PurchaseInteraction>();
                _nextScan = Time.realtimeSinceStartup + 0.5f;
            }
        }

        internal override void DrawMenu()
        {
            Widgets.SectionBegin("ESP / Wallhack");
            EspMobs = Widgets.Toggle("Enemies", EspMobs);
            EspInteractables = Widgets.Toggle("Interactables", EspInteractables);
            EspTeleporter = Widgets.Toggle("Teleporter", EspTeleporter);
            Widgets.Hint("Markers draw through walls while in a run.");
            Widgets.SectionEnd();

            Widgets.SectionBegin("Display");
            ShowNames = Widgets.Toggle("Show names", ShowNames);
            ShowDistance = Widgets.Toggle("Show distance", ShowDistance);
            ShowEnemyHealth = Widgets.Toggle("Show enemy health", ShowEnemyHealth);
            MaxDistance = Widgets.Slider("Max distance (0 = unlimited)", MaxDistance, 0f, 500f);
            MarkerSize = Widgets.Slider("Marker size", MarkerSize, 2f, 16f);
            Widgets.SectionEnd();

            Widgets.SectionBegin("Colors");
            EnemyColor = ColorRow("Enemies", EnemyColor);
            InteractableColor = ColorRow("Interactables", InteractableColor);
            TeleporterColor = ColorRow("Teleporter", TeleporterColor);
            Widgets.SectionEnd();
        }

        private static Color ColorRow(string label, Color c)
        {
            Widgets.Label(label);
            c.r = Widgets.Slider("  R", c.r, 0f, 1f);
            c.g = Widgets.Slider("  G", c.g, 0f, 1f);
            c.b = Widgets.Slider("  B", c.b, 0f, 1f);
            return c;
        }

        internal override void DrawOverlay()
        {
            if (!PlayerContext.InGame) return;
            if (!EspMobs && !EspInteractables && !EspTeleporter) return;
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 origin = PlayerContext.HasBody ? PlayerContext.Body.corePosition : cam.transform.position;

            try
            {
                if (EspMobs)
                {
                    foreach (CharacterBody body in CharacterBody.readOnlyInstancesList)
                    {
                        if (body == null || body == PlayerContext.Body || body.teamComponent == null) continue;
                        TeamIndex t = body.teamComponent.teamIndex;
                        if (t != TeamIndex.Monster && t != TeamIndex.Void) continue;
                        if (Culled(origin, body.corePosition, out float dist)) continue;
                        DrawMarker(cam, body.corePosition, EnemyLabel(body, dist), EnemyColor);
                    }
                }

                if (EspInteractables)
                {
                    foreach (PurchaseInteraction pi in _interactables)
                    {
                        if (pi == null) continue;
                        if (Culled(origin, pi.transform.position, out float dist)) continue;
                        DrawMarker(cam, pi.transform.position, Label("Interactable", dist), InteractableColor);
                    }
                }

                if (EspTeleporter && TeleporterInteraction.instance != null)
                {
                    Vector3 tp = TeleporterInteraction.instance.transform.position;
                    if (!Culled(origin, tp, out float dist))
                        DrawMarker(cam, tp, Label("Teleporter", dist), TeleporterColor);
                }
            }
            catch {  }
        }

        private static bool Culled(Vector3 origin, Vector3 target, out float dist)
        {
            dist = Vector3.Distance(origin, target);
            return MaxDistance > 0.5f && dist > MaxDistance;
        }

        private static string EnemyLabel(CharacterBody body, float dist)
        {
            string s = ShowNames ? body.GetDisplayName() : "";
            if (ShowEnemyHealth && body.healthComponent != null)
                s = Append(s, Mathf.CeilToInt(body.healthComponent.combinedHealth) + " hp");
            if (ShowDistance) s = Append(s, Mathf.RoundToInt(dist) + "m");
            return s;
        }

        private static string Label(string name, float dist)
        {
            string s = ShowNames ? name : "";
            if (ShowDistance) s = Append(s, Mathf.RoundToInt(dist) + "m");
            return s;
        }

        private static string Append(string a, string b) => a.Length > 0 ? a + "  " + b : b;

        private void DrawMarker(Camera cam, Vector3 worldPos, string label, Color color)
        {
            Vector3 sp = cam.WorldToScreenPoint(worldPos);
            if (sp.z <= 0f) return;
            float y = Screen.height - sp.y;
            float half = MarkerSize * 0.5f;

            Theme.Fill(new Rect(sp.x - half, y - half, MarkerSize, MarkerSize), color);

            if (string.IsNullOrEmpty(label)) return;
            if (_labelStyle == null) _labelStyle = new GUIStyle(GUI.skin.label);
            if (_styleColor != color) { _labelStyle.normal.textColor = color; _styleColor = color; }
            GUI.Label(new Rect(sp.x + half + 3f, y - 8f, 260f, 20f), label, _labelStyle);
        }
    }
}
