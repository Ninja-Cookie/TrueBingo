using UnityEngine;

namespace TrueBingo.BingoSync
{
    internal static class BingoSyncGUIConfig
    {
        private static bool stylesSetUp = false;

        public static void SetupStyles()
        {
            if (stylesSetUp)
                return;

            Styles.Button = GUI.skin.button;
            Styles.Button.normal.background     = Textures.Button_Background;
            Styles.Button.hover.background      = Textures.Button_Hover;
            Styles.Button.active.background     = Textures.Button_Click;

            Styles.Label = GUI.skin.label;

            stylesSetUp = true;
        }

        public static GUIStyle GetLabelStyle(bool field = false)
        {
            Styles.Label.fixedWidth     = field ? BingoSyncGUI.labelSpace : BingoSyncGUI.elementW;
            Styles.Label.alignment      = TextAnchor.MiddleCenter;
            Styles.Label.wordWrap       = false;
            Styles.Label.fontStyle      = FontStyle.Bold;

            return Styles.Label;
        }

        public static class Styles
        {
            public static GUIStyle Button;
            public static GUIStyle Label;
        }

        public static class Textures
        {
            public static readonly Texture2D Button_Background     = new Texture2D(1, 1);
            public static readonly Texture2D Button_Hover          = new Texture2D(1, 1);
            public static readonly Texture2D Button_Click          = new Texture2D(1, 1);
        }

        public static class Colors
        {
            public static readonly Color Orange     = new Color(254, 154, 20);
            public static readonly Color Red        = new Color(247, 72, 67);
            public static readonly Color Blue       = new Color(62, 157, 248);
            public static readonly Color Green      = new Color(39, 209, 16);
            public static readonly Color Purple     = new Color(128, 44, 189);
            public static readonly Color Navy       = new Color(11, 66, 168);
            public static readonly Color Teal       = new Color(62, 145, 144);
            public static readonly Color Brown      = new Color(162, 87, 33);
            public static readonly Color Pink       = new Color(232, 131, 166);
            public static readonly Color Yellow     = new Color(212, 205, 19);
        }
    }
}
