using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinShell.GUI
{
    public class SettingsForm : Form
    {
        private TerminalControl _terminal;
        private NumericUpDown _fontSizeInput;
        private ComboBox _fontFamilyInput;
        private CheckBox _wordWrapCheckBox;
        private CheckBox _autoScrollCheckBox;

        public SettingsForm(TerminalControl terminal)
        {
            _terminal = terminal;
            InitializeComponents();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            // Load current settings from terminal
            try
            {
                var currentFont = _terminal.GetCurrentFontFamily();
                var currentSize = _terminal.GetCurrentFontSize();
                var wordWrap = _terminal.GetWordWrap();

                // Set font family
                for (int i = 0; i < _fontFamilyInput.Items.Count; i++)
                {
                    if (_fontFamilyInput.Items[i].ToString() == currentFont)
                    {
                        _fontFamilyInput.SelectedIndex = i;
                        break;
                    }
                }

                // Set font size
                _fontSizeInput.Value = (decimal)currentSize;

                // Set word wrap
                _wordWrapCheckBox.Checked = wordWrap;
            }
            catch
            {
                // Use defaults if loading fails
            }
        }

        private void InitializeComponents()
        {
            this.Text = "⚙️ WinShell Settings";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);

            var titleLabel = new Label
            {
                Text = "Terminal Settings",
                Location = new Point(20, 20),
                Size = new Size(200, 30),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold)
            };

            // Font Family
            var fontFamilyLabel = new Label
            {
                Text = "Font Family:",
                Location = new Point(20, 70),
                Size = new Size(120, 25),
                ForeColor = Color.White
            };

            _fontFamilyInput = new ComboBox
            {
                Location = new Point(150, 68),
                Size = new Size(300, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White
            };
            _fontFamilyInput.Items.AddRange(new[] { "Cascadia Code", "Consolas", "Courier New", "Lucida Console", "Segoe UI Mono" });
            _fontFamilyInput.SelectedIndex = 0;

            // Font Size
            var fontSizeLabel = new Label
            {
                Text = "Font Size:",
                Location = new Point(20, 110),
                Size = new Size(120, 25),
                ForeColor = Color.White
            };

            _fontSizeInput = new NumericUpDown
            {
                Location = new Point(150, 108),
                Size = new Size(100, 25),
                Minimum = 8,
                Maximum = 24,
                Value = 10,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White
            };

            // Font Preview
            var previewLabel = new Label
            {
                Text = "Preview:",
                Location = new Point(20, 220),
                Size = new Size(80, 25),
                ForeColor = Color.White
            };

            var previewBox = new TextBox
            {
                Text = "The quick brown fox jumps over the lazy dog\n0123456789",
                Location = new Point(20, 245),
                Size = new Size(440, 60),
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(12, 12, 12),
                ForeColor = Color.FromArgb(204, 204, 204),
                Font = new Font("Cascadia Code", 10F)
            };

            // Add event handlers for live preview
            void UpdatePreview()
            {
                try
                {
                    var fontFamily = _fontFamilyInput.SelectedItem?.ToString() ?? "Cascadia Code";
                    var fontSize = (float)_fontSizeInput.Value;
                    previewBox.Font = new Font(fontFamily, fontSize);
                }
                catch { }
            }

            _fontFamilyInput.SelectedIndexChanged += (s, e) => UpdatePreview();
            _fontSizeInput.ValueChanged += (s, e) => UpdatePreview();

            // Word Wrap
            _wordWrapCheckBox = new CheckBox
            {
                Text = "Enable Word Wrap",
                Location = new Point(20, 150),
                Size = new Size(200, 25),
                ForeColor = Color.White,
                Checked = false
            };

            // Auto Scroll
            _autoScrollCheckBox = new CheckBox
            {
                Text = "Auto Scroll to Bottom",
                Location = new Point(20, 185),
                Size = new Size(200, 25),
                ForeColor = Color.White,
                Checked = true
            };

            // Apply Button
            var applyButton = new Button
            {
                Text = "✅ Apply Settings",
                Location = new Point(240, 320),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            applyButton.Click += ApplyButton_Click;

            // Cancel Button
            var cancelButton = new Button
            {
                Text = "❌ Cancel",
                Location = new Point(380, 320),
                Size = new Size(70, 35),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cancelButton.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] 
            { 
                titleLabel, 
                fontFamilyLabel, _fontFamilyInput,
                fontSizeLabel, _fontSizeInput,
                previewLabel, previewBox,
                _wordWrapCheckBox,
                _autoScrollCheckBox,
                applyButton, cancelButton 
            });
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Apply font family
                var selectedFont = _fontFamilyInput.SelectedItem.ToString();
                _terminal.SetFontFamily(selectedFont);

                // Apply font size
                var selectedSize = (float)_fontSizeInput.Value;
                _terminal.SetFontSize(selectedSize);

                // Apply word wrap
                _terminal.SetWordWrap(_wordWrapCheckBox.Checked);

                // Apply auto scroll
                _terminal.SetAutoScroll(_autoScrollCheckBox.Checked);

                MessageBox.Show(
                    "✅ Settings applied successfully!\n\n" +
                    $"Font: {selectedFont}\n" +
                    $"Size: {selectedSize}pt\n" +
                    $"Word Wrap: {(_wordWrapCheckBox.Checked ? "Enabled" : "Disabled")}\n" +
                    $"Auto Scroll: {(_autoScrollCheckBox.Checked ? "Enabled" : "Disabled")}",
                    "✅ Settings Applied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Error applying settings:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
