using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace PoppyMenu
{
    internal static class MenuRoot
    {
        private const int WindowId = 0x5B7A00;
        internal static bool Visible;
        internal static int ActiveTab;

        private static Rect _rect = new Rect(40, 60, 540, 600);
        private static Vector2 _scroll;
        private static List<TabGroup> _groups;
        private static bool _posLoaded;

        private static GUIStyle _headerBar, _sidebar, _hudBox, _hudTitle, _hudLine;
        private static bool _resizing;

        private const float MinW = 400f, MinH = 340f, MaxW = 1100f, MaxH = 960f;

        internal static void Draw(List<TabGroup> groups)
        {
            _groups = groups;
            EnsureLocalStyles();

            if (!_posLoaded)
            {
                _rect.x = ModConfig.WindowX.Value;
                _rect.y = ModConfig.WindowY.Value;
                _rect.width = Mathf.Clamp(ModConfig.WindowW.Value, MinW, MaxW);
                _rect.height = Mathf.Clamp(ModConfig.WindowH.Value, MinH, MaxH);
                _posLoaded = true;
            }

            if (Visible)
            {
                _rect = GUI.Window(WindowId, _rect, DrawWindow, "", Theme.Window);
            }
            else if (ModConfig.ShowHud.Value && PlayerContext.InGame)
            {
                DrawHud();
            }

            ListPicker.Draw();
        }

        internal static void SaveLayout()
        {
            ModConfig.WindowX.Value = _rect.x;
            ModConfig.WindowY.Value = _rect.y;
            ModConfig.WindowW.Value = _rect.width;
            ModConfig.WindowH.Value = _rect.height;
        }

        internal static void ResetPosition()
        {
            _rect = new Rect(40, 60, 540, 600);
            SaveLayout();
        }

        internal static string CurrentTabName =>
            _groups != null && _groups.Count > 0 ? _groups[Mathf.Clamp(ActiveTab, 0, _groups.Count - 1)].Name : "";

        internal static void SelectTabByName(string name)
        {
            if (_groups == null) return;
            for (int i = 0; i < _groups.Count; i++)
                if (_groups[i].Name == name) { ActiveTab = i; Visible = true; return; }
            for (int i = 0; i < _groups.Count; i++)
                for (int j = 0; j < _groups[i].Pages.Count; j++)
                    if (_groups[i].Pages[j].Name == name) { ActiveTab = i; _groups[i].Page = j; Visible = true; return; }
        }

        private static void EnsureLocalStyles()
        {
            if (_headerBar != null) return;
            _headerBar = new GUIStyle(GUI.skin.box) { padding = new RectOffset(10, 8, 0, 0) };
            _headerBar.normal.background = Theme.Solid(Theme.HeaderBg);
            _sidebar = new GUIStyle(GUI.skin.box) { padding = new RectOffset(6, 6, 8, 8) };
            _sidebar.normal.background = Theme.Solid(Theme.SidebarBg);
            _hudBox = new GUIStyle(GUI.skin.box) { padding = new RectOffset(10, 10, 8, 8), alignment = TextAnchor.UpperLeft };
            _hudBox.normal.background = Theme.Solid(Theme.WindowBg);
        }

        private static void DrawWindow(int id)
        {
            if (_groups == null || _groups.Count == 0) { GUI.DragWindow(); return; }
            ActiveTab = Mathf.Clamp(ActiveTab, 0, _groups.Count - 1);

            if (Event.current.type == EventType.MouseDown
                && new Rect(_rect.width - 26f, _rect.height - 26f, 26f, 26f).Contains(Event.current.mousePosition))
            {
                _resizing = true;
                Event.current.Use();
            }

            DrawHeaderBar();

            GUILayout.BeginHorizontal();
            DrawSidebar();
            DrawContent();
            GUILayout.EndHorizontal();

            DrawFooter();

            HandleResize();
            GUI.DragWindow(new Rect(0, 0, _rect.width, 40));
        }

        private static void HandleResize()
        {
            var grip = new Rect(_rect.width - 18f, _rect.height - 18f, 16f, 16f);
            Theme.Fill(grip, Theme.Accent);
            Theme.Fill(new Rect(grip.x + 5f, grip.y + 10f, 9f, 2f), new Color(1, 1, 1, 0.55f));
            Theme.Fill(new Rect(grip.x + 10f, grip.y + 5f, 2f, 9f), new Color(1, 1, 1, 0.55f));

            if (!_resizing) return;

            float scale = Mathf.Max(0.1f, ModConfig.UiScale.Value);
            float mouseX = Input.mousePosition.x / scale;
            float mouseY = (Screen.height - Input.mousePosition.y) / scale;
            _rect.width = Mathf.Clamp(mouseX - _rect.x + 3f, MinW, MaxW);
            _rect.height = Mathf.Clamp(mouseY - _rect.y + 3f, MinH, MaxH);

            if (!Input.GetMouseButton(0)) { _resizing = false; SaveLayout(); }
        }

        private static void DrawHeaderBar()
        {
            GUILayout.BeginHorizontal(_headerBar, GUILayout.Height(40));

            GUILayout.Label("<b><color=#" + Theme.Hex(Theme.Accent2) + ">P O P P Y</color></b>", Theme.Header, GUILayout.Width(140));
            var verStyle = new GUIStyle(Theme.Hint) { alignment = TextAnchor.MiddleLeft };
            GUILayout.Label("v" + PoppyPlugin.Version, verStyle, GUILayout.Width(46));

            GUILayout.FlexibleSpace();

            DrawStatusPill();

            if (GUILayout.Button("-", Theme.IconBtn, GUILayout.Width(26), GUILayout.Height(22)))
                ModConfig.UiScale.Value = Mathf.Clamp(ModConfig.UiScale.Value - 0.1f, 0.6f, 2f);
            if (GUILayout.Button("+", Theme.IconBtn, GUILayout.Width(26), GUILayout.Height(22)))
                ModConfig.UiScale.Value = Mathf.Clamp(ModConfig.UiScale.Value + 0.1f, 0.6f, 2f);
            if (GUILayout.Button("X", Theme.IconBtn, GUILayout.Width(26), GUILayout.Height(22)))
            {
                Visible = false;
                SaveLayout();
                ListPicker.Close();
            }

            GUILayout.EndHorizontal();
        }

        private static void DrawStatusPill()
        {
            string text; Color col;
            if (!PlayerContext.InGame) { text = "MAIN MENU"; col = Theme.TextDim; }
            else if (NetUtil.IsServer) { text = "IN RUN · HOST"; col = Theme.On; }
            else { text = "IN RUN · CLIENT"; col = new Color(0.95f, 0.8f, 0.3f); }

            Theme.Pill.normal.textColor = col;
            GUILayout.Label("* " + text, Theme.Pill, GUILayout.Height(22));
        }

        private static Vector2 _sideScroll;

        private static void DrawSidebar()
        {
            GUILayout.BeginVertical(_sidebar, GUILayout.Width(132), GUILayout.Height(_rect.height - 86));
            _sideScroll = GUILayout.BeginScrollView(_sideScroll, GUILayout.Width(132));
            for (int i = 0; i < _groups.Count; i++)
            {
                bool active = i == ActiveTab;
                if (GUILayout.Button(_groups[i].Name, active ? Theme.SideItemActive : Theme.SideItem, GUILayout.Height(30)))
                    ActiveTab = i;
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private static void DrawContent()
        {
            GUILayout.BeginVertical();
            if (!PlayerContext.InGame)
                GUILayout.Label("Start a run to use most features.", Theme.Hint);

            TabGroup group = _groups[ActiveTab];
            group.Page = Mathf.Clamp(group.Page, 0, group.Pages.Count - 1);

            bool hasSubNav = group.Pages.Count > 1;
            if (hasSubNav)
            {
                GUILayout.BeginHorizontal();
                for (int i = 0; i < group.Pages.Count; i++)
                {
                    bool act = i == group.Page;
                    if (GUILayout.Button(group.Pages[i].Name, act ? Theme.Primary : Theme.Button, GUILayout.Height(24)))
                        group.Page = i;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(2);
            }

            PoppyModule page = group.Pages[group.Page];
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(_rect.height - 86 - (hasSubNav ? 30 : 0)));
            Widgets.OpenSections = 0;
            try
            {
                page.DrawMenu();
            }
            catch (System.Exception e)
            {
                while (Widgets.OpenSections > 0) Widgets.SectionEnd();
                GUILayout.Label("This tab hit an error, see console.", Theme.Label);
                Log.Error($"{page.Name}.DrawMenu: {e}");
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private static void DrawFooter()
        {
            GUILayout.BeginHorizontal();
            int n = ActiveEffects().Count;
            GUILayout.Label(n > 0 ? $"<color=#4FC76B>* {n} active</color>" : "* idle",
                new GUIStyle(Theme.Hint) { richText = true });
            GUILayout.FlexibleSpace();
            GUILayout.Label($"[{ModConfig.ToggleMenuKey.Value}] toggle · drag header to move", Theme.Hint);
            GUILayout.EndHorizontal();
        }

        private static void DrawHud()
        {
            if (_hudTitle == null)
            {
                _hudTitle = new GUIStyle(Theme.Label) { richText = true, fontSize = 12, wordWrap = false };
                _hudLine = new GUIStyle(Theme.Hint) { richText = true, fontSize = 11, wordWrap = false };
            }

            List<string> active = ActiveEffects();
            const float pad = 8f, titleH = 20f, lineH = 16f, width = 220f;
            float h = pad * 2f + titleH + active.Count * lineH;

            GUILayout.BeginArea(new Rect(10, 10, width, h), _hudBox);
            GUILayout.Label("<b><color=#F0584E>POPPY</color></b>  <size=10><color=#8A8A99>[" + ModConfig.ToggleMenuKey.Value + "]</color></size>",
                _hudTitle, GUILayout.Height(titleH));
            foreach (string e in active)
                GUILayout.Label("<color=#4FC76B>*</color> " + e, _hudLine, GUILayout.Height(lineH));
            GUILayout.EndArea();
        }

        internal static List<string> ActiveEffects()
        {
            var list = new List<string>();
            if (PlayerModule.GodMode) list.Add("God Mode");
            if (PlayerModule.InfiniteSkills) list.Add("Infinite Skills");
            if (Aim.Enabled) list.Add("Aimbot");
            if (Aim.MagicBullet) list.Add("Magic Bullet");
            if (MovementModule.Flight) list.Add("Flight");
            if (MovementModule.NoClip) list.Add("No-Clip");
            if (MovementModule.AlwaysSprint) list.Add("Always Sprint");
            if (MovementModule.JumpPack) list.Add("Jump Pack");
            if (StatsModule.Active) list.Add("Stat Mods");
            if (ItemsModule.NoEquipmentCooldown) list.Add("No Equip Cooldown");
            if (RenderModule.EspMobs || RenderModule.EspInteractables || RenderModule.EspTeleporter) list.Add("ESP");
            if (WorldModule.FreezeMatch) list.Add("Match Frozen");
            if (WorldModule.FreezeTimer) list.Add("Timer Frozen");
            if (System.Math.Abs(WorldModule.TimeScale - 1f) > 0.001f && !WorldModule.FreezeMatch)
                list.Add($"Time {WorldModule.TimeScale:0.##}x");
            return list;
        }
    }
}
