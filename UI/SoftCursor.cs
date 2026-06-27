using UnityEngine;

namespace PoppyMenu
{
    internal static class SoftCursor
    {
        private static Texture2D _tex;

        private static readonly string[] Art =
        {
            "O...........",
            "OO..........",
            "O#O.........",
            "O##O........",
            "O###O.......",
            "O####O......",
            "O#####O.....",
            "O######O....",
            "O#######O...",
            "O########O..",
            "O#########O.",
            "O######OOOOO",
            "O###O##O....",
            "O##OO##O....",
            "O#O.O##O....",
            "OO..O##O....",
            "O....O##O...",
            ".....O##O...",
            ".....OOOO...",
        };

        internal static void Draw()
        {
            if (_tex == null) Build();
            Vector2 m = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            Color prev = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(m.x, m.y, _tex.width, _tex.height), _tex);
            GUI.color = prev;
        }

        private static void Build()
        {
            int h = Art.Length, w = Art[0].Length;
            _tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, hideFlags = HideFlags.HideAndDontSave };
            Color fill = Color.white;
            Color outline = new Color(0.05f, 0.05f, 0.07f, 1f);
            Color clear = new Color(0, 0, 0, 0);
            for (int r = 0; r < h; r++)
            {
                string row = Art[r];
                for (int c = 0; c < w; c++)
                {
                    char ch = c < row.Length ? row[c] : '.';
                    Color col = ch == '#' ? fill : ch == 'O' ? outline : clear;
                    _tex.SetPixel(c, h - 1 - r, col);
                }
            }
            _tex.Apply();
        }
    }
}
