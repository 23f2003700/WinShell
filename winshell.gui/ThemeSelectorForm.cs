using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinShell.GUI
{
    public class ThemeSelectorForm : Form
    {
        private ThemeManager _themeManager;
        private ComboBox _themeComboBox;
        private Panel _previewPanel;
        private Label _previewLabel;

        public ThemeSelectorForm(ThemeManager themeManager)
        {
            _themeManager = themeManager;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "🎨 Theme Selector";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);

            var label = new Label
            {
                Text = "Select Theme:",
                Location = new Point(20, 20),
                Size = new Size(150, 25),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };

            _themeComboBox = new ComboBox
            {
                Location = new Point(20, 55),
                Size = new Size(440, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F)
            };

            _themeComboBox.Items.AddRange(new[] 
            { 
                "Dark (VS Code)", 
                "Light (VS)", 
                "Matrix (Hacker)", 
                "Cyberpunk (Neon)", 
                "Solarized Dark",
                "Monokai"
            });
            _themeComboBox.SelectedIndex = 0;
            _themeComboBox.SelectedIndexChanged += ThemeComboBox_SelectedIndexChanged;

            _previewPanel = new Panel
            {
                Location = new Point(20, 100),
                Size = new Size(440, 200),
                BorderStyle = BorderStyle.Fixed3D
            };

            _previewLabel = new Label
            {
                Text = "WS D:\\Internet\\winshell2>\nThis is a preview of the terminal\nError: Sample error message\nSuccess: Sample success message",
                Location = new Point(10, 10),
                Size = new Size(420, 180),
                Font = new Font("Cascadia Code", 10F)
            };

            _previewPanel.Controls.Add(_previewLabel);

            var applyButton = new Button
            {
                Text = "✅ Apply Theme",
                Location = new Point(260, 320),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            applyButton.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };

            var cancelButton = new Button
            {
                Text = "❌ Cancel",
                Location = new Point(390, 320),
                Size = new Size(70, 35),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cancelButton.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { label, _themeComboBox, _previewPanel, applyButton, cancelButton });

            UpdatePreview();
        }

        private void ThemeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedTheme = _themeComboBox.SelectedItem.ToString();
            Theme theme = selectedTheme switch
            {
                "Dark (VS Code)" => _themeManager.GetDarkTheme(),
                "Light (VS)" => _themeManager.GetLightTheme(),
                "Matrix (Hacker)" => _themeManager.GetMatrixTheme(),
                "Cyberpunk (Neon)" => _themeManager.GetCyberpunkTheme(),
                "Solarized Dark" => _themeManager.GetSolarizedDarkTheme(),
                "Monokai" => _themeManager.GetMonokaiTheme(),
                _ => _themeManager.GetDarkTheme()
            };

            _themeManager.SetTheme(theme);
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            var theme = _themeManager.CurrentTheme;
            _previewPanel.BackColor = theme.TerminalBackground;
            _previewLabel.ForeColor = theme.TerminalForeground;
            _previewLabel.BackColor = theme.TerminalBackground;
        }
    }
}
