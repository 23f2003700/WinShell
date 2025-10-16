using System;
using System.Drawing;

namespace WinShell.GUI
{
    public class Theme
    {
        public string Name { get; set; }
        public Color BackgroundColor { get; set; }
        public Color ForegroundColor { get; set; }
        public Color AccentColor { get; set; }
        public Color TerminalBackground { get; set; }
        public Color TerminalForeground { get; set; }
        public Color PromptColor { get; set; }
        public Color ErrorColor { get; set; }
        public Color SuccessColor { get; set; }
        public Color ButtonBackground { get; set; }
        public Color ButtonForeground { get; set; }
        public Color ButtonHoverBackground { get; set; }
        public Color MenuBackground { get; set; }
        public Color MenuForeground { get; set; }
        public Color StatusBarBackground { get; set; }
        public Color StatusBarForeground { get; set; }
    }

    public class ThemeManager
    {
        public Theme CurrentTheme { get; private set; }

        public ThemeManager()
        {
            CurrentTheme = GetDarkTheme();
        }

        public Theme GetDarkTheme()
        {
            return new Theme
            {
                Name = "Dark (VS Code)",
                BackgroundColor = Color.FromArgb(30, 30, 30),
                ForegroundColor = Color.FromArgb(204, 204, 204),
                AccentColor = Color.FromArgb(0, 122, 204),
                TerminalBackground = Color.FromArgb(12, 12, 12),
                TerminalForeground = Color.FromArgb(204, 204, 204),
                PromptColor = Color.FromArgb(76, 175, 80),
                ErrorColor = Color.FromArgb(244, 67, 54),
                SuccessColor = Color.FromArgb(139, 195, 74),
                ButtonBackground = Color.FromArgb(45, 45, 48),
                ButtonForeground = Color.White,
                ButtonHoverBackground = Color.FromArgb(62, 62, 66),
                MenuBackground = Color.FromArgb(45, 45, 48),
                MenuForeground = Color.White,
                StatusBarBackground = Color.FromArgb(0, 122, 204),
                StatusBarForeground = Color.White
            };
        }

        public Theme GetLightTheme()
        {
            return new Theme
            {
                Name = "Light (VS)",
                BackgroundColor = Color.FromArgb(240, 240, 240),
                ForegroundColor = Color.FromArgb(30, 30, 30),
                AccentColor = Color.FromArgb(0, 122, 204),
                TerminalBackground = Color.White,
                TerminalForeground = Color.Black,
                PromptColor = Color.FromArgb(0, 128, 0),
                ErrorColor = Color.FromArgb(204, 0, 0),
                SuccessColor = Color.FromArgb(0, 153, 0),
                ButtonBackground = Color.FromArgb(225, 225, 225),
                ButtonForeground = Color.Black,
                ButtonHoverBackground = Color.FromArgb(200, 200, 200),
                MenuBackground = Color.FromArgb(246, 246, 246),
                MenuForeground = Color.Black,
                StatusBarBackground = Color.FromArgb(0, 122, 204),
                StatusBarForeground = Color.White
            };
        }

        public Theme GetMatrixTheme()
        {
            return new Theme
            {
                Name = "Matrix (Hacker)",
                BackgroundColor = Color.Black,
                ForegroundColor = Color.FromArgb(0, 255, 65),
                AccentColor = Color.FromArgb(0, 255, 0),
                TerminalBackground = Color.Black,
                TerminalForeground = Color.FromArgb(0, 255, 65),
                PromptColor = Color.FromArgb(0, 255, 0),
                ErrorColor = Color.FromArgb(255, 0, 0),
                SuccessColor = Color.FromArgb(0, 255, 0),
                ButtonBackground = Color.FromArgb(0, 51, 0),
                ButtonForeground = Color.FromArgb(0, 255, 65),
                ButtonHoverBackground = Color.FromArgb(0, 102, 0),
                MenuBackground = Color.FromArgb(0, 20, 0),
                MenuForeground = Color.FromArgb(0, 255, 65),
                StatusBarBackground = Color.FromArgb(0, 153, 0),
                StatusBarForeground = Color.Black
            };
        }

        public Theme GetCyberpunkTheme()
        {
            return new Theme
            {
                Name = "Cyberpunk (Neon)",
                BackgroundColor = Color.FromArgb(15, 15, 35),
                ForegroundColor = Color.FromArgb(255, 20, 147),
                AccentColor = Color.FromArgb(0, 255, 255),
                TerminalBackground = Color.FromArgb(10, 10, 25),
                TerminalForeground = Color.FromArgb(255, 20, 147),
                PromptColor = Color.FromArgb(255, 0, 255),
                ErrorColor = Color.FromArgb(255, 0, 0),
                SuccessColor = Color.FromArgb(0, 255, 255),
                ButtonBackground = Color.FromArgb(40, 0, 80),
                ButtonForeground = Color.FromArgb(255, 20, 147),
                ButtonHoverBackground = Color.FromArgb(80, 0, 160),
                MenuBackground = Color.FromArgb(25, 0, 51),
                MenuForeground = Color.FromArgb(255, 20, 147),
                StatusBarBackground = Color.FromArgb(138, 43, 226),
                StatusBarForeground = Color.White
            };
        }

        public Theme GetSolarizedDarkTheme()
        {
            return new Theme
            {
                Name = "Solarized Dark",
                BackgroundColor = Color.FromArgb(0, 43, 54),
                ForegroundColor = Color.FromArgb(131, 148, 150),
                AccentColor = Color.FromArgb(42, 161, 152),
                TerminalBackground = Color.FromArgb(0, 43, 54),
                TerminalForeground = Color.FromArgb(131, 148, 150),
                PromptColor = Color.FromArgb(133, 153, 0),
                ErrorColor = Color.FromArgb(220, 50, 47),
                SuccessColor = Color.FromArgb(133, 153, 0),
                ButtonBackground = Color.FromArgb(7, 54, 66),
                ButtonForeground = Color.FromArgb(131, 148, 150),
                ButtonHoverBackground = Color.FromArgb(88, 110, 117),
                MenuBackground = Color.FromArgb(7, 54, 66),
                MenuForeground = Color.FromArgb(131, 148, 150),
                StatusBarBackground = Color.FromArgb(42, 161, 152),
                StatusBarForeground = Color.FromArgb(0, 43, 54)
            };
        }

        public Theme GetMonokaiTheme()
        {
            return new Theme
            {
                Name = "Monokai",
                BackgroundColor = Color.FromArgb(39, 40, 34),
                ForegroundColor = Color.FromArgb(248, 248, 242),
                AccentColor = Color.FromArgb(249, 38, 114),
                TerminalBackground = Color.FromArgb(39, 40, 34),
                TerminalForeground = Color.FromArgb(248, 248, 242),
                PromptColor = Color.FromArgb(166, 226, 46),
                ErrorColor = Color.FromArgb(249, 38, 114),
                SuccessColor = Color.FromArgb(166, 226, 46),
                ButtonBackground = Color.FromArgb(73, 72, 62),
                ButtonForeground = Color.FromArgb(248, 248, 242),
                ButtonHoverBackground = Color.FromArgb(102, 100, 86),
                MenuBackground = Color.FromArgb(39, 40, 34),
                MenuForeground = Color.FromArgb(248, 248, 242),
                StatusBarBackground = Color.FromArgb(102, 217, 239),
                StatusBarForeground = Color.Black
            };
        }

        public void SetTheme(Theme theme)
        {
            CurrentTheme = theme;
        }
    }
}
