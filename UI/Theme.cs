using UnityEngine;

namespace PoppyMenu
{
    internal static class Theme
    {
        internal static Color Accent    = new Color32(0xE5, 0x38, 0x4A, 0xFF);
        internal static Color Accent2   = new Color32(0xF0, 0x58, 0x4E, 0xFF);
        internal static readonly Color On         = new Color32(0x4F, 0xC7, 0x6B, 0xFF);
        internal static readonly Color Danger     = new Color32(0x9E, 0x29, 0x29, 0xFF);
        internal static readonly Color Text       = new Color32(0xEC, 0xEC, 0xF3, 0xFF);
        internal static readonly Color TextDim    = new Color32(0x8A, 0x8A, 0x99, 0xFF);
        internal static readonly Color WindowBg   = new Color32(0x12, 0x0C, 0x0E, 0xF7);
        internal static readonly Color SidebarBg  = new Color32(0x0C, 0x08, 0x09, 0xFF);
        internal static readonly Color HeaderBg   = new Color32(0x22, 0x12, 0x15, 0xFF);
        internal static readonly Color CardBg      = new Color32(0x1E, 0x16, 0x18, 0xFF);
        internal static readonly Color RowBg       = new Color32(0x28, 0x1C, 0x1E, 0xFF);
        internal static readonly Color SlotOff     = new Color32(0x36, 0x2A, 0x2C, 0xFF);
        internal static readonly Color Hovered     = new Color32(0x4E, 0x24, 0x2A, 0xFF);

        internal static GUIStyle Window, Header, SubHeader, Label, Hint, Pill;
        internal static GUIStyle Button, Primary, Danger_, SideItem, SideItemActive;
        internal static GUIStyle SwitchOn, SwitchOff, Card, Search, RowButton, IconBtn, ChipLabel;

        private static bool _ready;
        private static Texture2D _trans;

        internal static Texture2D Solid(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            t.hideFlags = HideFlags.HideAndDontSave;
            return t;
        }

        internal static void EnsureInit()
        {
            if (_ready) return;
            _ready = true;

            _trans = Solid(new Color(0, 0, 0, 0));
            Texture2D win = Solid(WindowBg), side = Solid(SidebarBg), head = Solid(HeaderBg),
                      card = Solid(CardBg), row = Solid(RowBg), slot = Solid(SlotOff),
                      hov = Solid(Hovered), accent = Solid(Accent), accentDim = Solid(new Color(Accent.r, Accent.g, Accent.b, 0.35f)),
                      on = Solid(On), danger = Solid(Danger);

            Window = new GUIStyle(GUI.skin.box) { padding = new RectOffset(0, 0, 0, 0), border = new RectOffset(2, 2, 2, 2) };
            Window.normal.background = win;

            Header = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 15, alignment = TextAnchor.MiddleLeft };
            Header.normal.textColor = Text;

            SubHeader = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 12, alignment = TextAnchor.MiddleLeft };
            SubHeader.normal.textColor = Accent2;

            Label = new GUIStyle(GUI.skin.label) { fontSize = 12, wordWrap = true };
            Label.normal.textColor = Text;

            Hint = new GUIStyle(GUI.skin.label) { fontSize = 10, wordWrap = true };
            Hint.normal.textColor = TextDim;

            Pill = new GUIStyle(GUI.skin.label) { fontSize = 10, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, padding = new RectOffset(8, 8, 3, 3) };
            Pill.normal.textColor = Text;
            Pill.normal.background = row;

            Button = new GUIStyle(GUI.skin.button) { fontSize = 12, alignment = TextAnchor.MiddleCenter, padding = new RectOffset(8, 8, 6, 6) };
            Button.normal.background = slot; Button.normal.textColor = Text;
            Button.hover.background = hov;   Button.hover.textColor = Color.white;
            Button.active.background = accentDim;

            Primary = new GUIStyle(Button);
            Primary.normal.background = accent; Primary.normal.textColor = Color.white;
            Primary.hover.background = Solid(Accent2);

            Danger_ = new GUIStyle(Button);
            Danger_.normal.background = Solid(new Color(Danger.r, Danger.g, Danger.b, 0.85f)); Danger_.normal.textColor = Color.white;
            Danger_.hover.background = danger;

            SideItem = new GUIStyle(GUI.skin.button) { fontSize = 12, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(12, 6, 8, 8) };
            SideItem.normal.background = side;  SideItem.normal.textColor = TextDim;
            SideItem.hover.background = hov;    SideItem.hover.textColor = Text;

            SideItemActive = new GUIStyle(SideItem);
            SideItemActive.normal.background = card; SideItemActive.normal.textColor = Color.white;
            SideItemActive.fontStyle = FontStyle.Bold;

            SwitchOn = new GUIStyle(Button) { alignment = TextAnchor.MiddleLeft, padding = new RectOffset(10, 10, 7, 7) };
            SwitchOn.normal.background = Solid(new Color(On.r, On.g, On.b, 0.22f));
            SwitchOn.hover.background = Solid(new Color(On.r, On.g, On.b, 0.32f));
            SwitchOn.normal.textColor = Color.white;

            SwitchOff = new GUIStyle(SwitchOn);
            SwitchOff.normal.background = row;
            SwitchOff.hover.background = hov;
            SwitchOff.normal.textColor = TextDim;

            Card = new GUIStyle(GUI.skin.box) { padding = new RectOffset(10, 10, 8, 10), margin = new RectOffset(0, 0, 4, 4) };
            Card.normal.background = card;

            Search = new GUIStyle(GUI.skin.textField) { fontSize = 12, padding = new RectOffset(8, 8, 6, 6) };
            Search.normal.textColor = Text;

            RowButton = new GUIStyle(Button) { alignment = TextAnchor.MiddleLeft, fontSize = 12, padding = new RectOffset(26, 8, 6, 6) };
            RowButton.normal.background = row;
            RowButton.hover.background = hov;

            IconBtn = new GUIStyle(Button) { fontSize = 12, fontStyle = FontStyle.Bold, padding = new RectOffset(0, 0, 2, 2), alignment = TextAnchor.MiddleCenter };

            ChipLabel = new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.MiddleLeft };
            ChipLabel.normal.textColor = Text;

            Header.richText = SubHeader.richText = Label.richText = Hint.richText = Pill.richText = true;
        }

        internal static void ApplyAccent(Color c)
        {
            Accent = c;
            Accent2 = Color.Lerp(c, Color.white, 0.18f);
            _ready = false;
        }

        internal static string Hex(Color c) => ColorUtility.ToHtmlStringRGB(c);

        internal static void Fill(Rect r, Color c)
        {
            Color prev = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = prev;
        }
    }
}
