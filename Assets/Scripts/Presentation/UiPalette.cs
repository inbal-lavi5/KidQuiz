using UnityEngine;

namespace KidQuiz.Presentation
{
    // Colors and shared visual constants from the Wonder Quiz design spec.
    public static class UiPalette
    {
        public static readonly Color Ink = HexColor("#232323");
        public static readonly Color SkyBackground = HexColor("#EAF6FD");
        public static readonly Color DeepSky = HexColor("#CDEBFB");
        public static readonly Color GrassGreen = HexColor("#3FBE7A");
        public static readonly Color SkyBlue = HexColor("#29B6F6");
        public static readonly Color SunYellow = HexColor("#FFD34D");
        public static readonly Color Coral = HexColor("#FF7A59");
        public static readonly Color Correct = HexColor("#2ECC71");
        public static readonly Color CorrectBorder = HexColor("#1F8A4D");
        public static readonly Color Incorrect = HexColor("#FF5A5F");
        public static readonly Color IncorrectBorder = HexColor("#C93E42");
        public static readonly Color Cream = HexColor("#FFF3DC");
        public static readonly Color CardWhite = HexColor("#FFFFFF");
        public static readonly Color TextMuted = HexColor("#6B7A78");
        public static readonly Color PageBackground = HexColor("#F4F1EA");
        public static readonly Color Silver = HexColor("#D8D8D8");
        public static readonly Color Bronze = HexColor("#E0A458");

        public static Color HexColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }
    }
}
