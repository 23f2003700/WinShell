using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using WinShell.Core;

namespace WinShell.GUI
{
    public partial class MainWindow : Form
    {
        private TerminalControl _terminal;
        private MenuStrip _menuStrip;
        private ToolStrip _toolStrip;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;
        private ToolStripStatusLabel _directoryLabel;
        private ThemeManager _themeManager;
        private Panel _sidePanel;

        public MainWindow()
        {
            InitializeComponent();
            SetupWindow();
            _themeManager = new ThemeManager();
            ApplyTheme(_themeManager.CurrentTheme);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form settings
            this.Text = "🖥️ WinShell - Modern Terminal";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = SystemIcons.Application;
            this.MinimumSize = new Size(1000, 600);
            this.BackColor = Color.FromArgb(30, 30, 30);
            
            // Create menu
            CreateMenuStrip();
            
            // Create modern button panel (side panel)
            CreateModernButtonPanel();
            
            // Create toolbar
            CreateToolStrip();
            
            // Create terminal
            _terminal = new TerminalControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(12, 12, 12),
                ForeColor = Color.FromArgb(204, 204, 204),
                Font = new Font("Cascadia Code", 10F, FontStyle.Regular),
            };
            
            // Create status bar
            CreateStatusStrip();
            
            // Add controls in correct z-order
            this.Controls.Add(_terminal);
            this.Controls.Add(_sidePanel);
            this.Controls.Add(_statusStrip);
            this.Controls.Add(_toolStrip);
            this.Controls.Add(_menuStrip);
            
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void CreateMenuStrip()
        {
            _menuStrip = new MenuStrip
            {
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White
            };

            // File menu
            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("New Terminal", null, (s, e) => NewTerminal());
            fileMenu.DropDownItems.Add("Open Script", null, (s, e) => OpenScript());
            fileMenu.DropDownItems.Add("Save Output", null, (s, e) => SaveOutput());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Exit", null, (s, e) => Application.Exit());

            // Edit menu
            var editMenu = new ToolStripMenuItem("Edit");
            editMenu.DropDownItems.Add("Copy", null, (s, e) => _terminal.Copy());
            editMenu.DropDownItems.Add("Paste", null, (s, e) => _terminal.Paste());
            editMenu.DropDownItems.Add("Clear", null, (s, e) => _terminal.Clear());
            editMenu.DropDownItems.Add(new ToolStripSeparator());
            editMenu.DropDownItems.Add("Find", null, (s, e) => ShowFindDialog());

            // View menu
            var viewMenu = new ToolStripMenuItem("View");
            viewMenu.DropDownItems.Add("Zoom In", null, (s, e) => _terminal.ZoomIn());
            viewMenu.DropDownItems.Add("Zoom Out", null, (s, e) => _terminal.ZoomOut());
            viewMenu.DropDownItems.Add("Reset Zoom", null, (s, e) => _terminal.ResetZoom());
            viewMenu.DropDownItems.Add(new ToolStripSeparator());
            viewMenu.DropDownItems.Add("Toggle Fullscreen", null, (s, e) => ToggleFullscreen());

            // Tools menu
            var toolsMenu = new ToolStripMenuItem("Tools");
            toolsMenu.DropDownItems.Add("Environment Variables", null, (s, e) => ShowEnvironmentVariables());
            toolsMenu.DropDownItems.Add("Process Manager", null, (s, e) => ShowProcessManager());
            toolsMenu.DropDownItems.Add("Settings", null, (s, e) => ShowSettings());

            // Help menu
            var helpMenu = new ToolStripMenuItem("Help");
            helpMenu.DropDownItems.Add("Commands", null, (s, e) => _terminal.ExecuteCommand("help"));
            helpMenu.DropDownItems.Add("About", null, (s, e) => ShowAbout());

            _menuStrip.Items.AddRange(new[] { fileMenu, editMenu, viewMenu, toolsMenu, helpMenu });
        }

        private void CreateToolStrip()
        {
            _toolStrip = new ToolStrip
            {
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                GripStyle = ToolStripGripStyle.Hidden
            };

            var newButton = new ToolStripButton("New", null, (s, e) => NewTerminal());
            var openButton = new ToolStripButton("Open", null, (s, e) => OpenScript());
            var saveButton = new ToolStripButton("Save", null, (s, e) => SaveOutput());
            var separator1 = new ToolStripSeparator();
            var copyButton = new ToolStripButton("Copy", null, (s, e) => _terminal.Copy());
            var pasteButton = new ToolStripButton("Paste", null, (s, e) => _terminal.Paste());
            var clearButton = new ToolStripButton("Clear", null, (s, e) => _terminal.Clear());
            var separator2 = new ToolStripSeparator();
            var stopButton = new ToolStripButton("Stop", null, (s, e) => _terminal.StopCurrentCommand());

            _toolStrip.Items.AddRange(new ToolStripItem[] 
            { 
                newButton, openButton, saveButton, separator1,
                copyButton, pasteButton, clearButton, separator2,
                stopButton
            });
        }

        private void CreateStatusStrip()
        {
            _statusStrip = new StatusStrip
            {
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White
            };

            _statusLabel = new ToolStripStatusLabel("Ready");
            _directoryLabel = new ToolStripStatusLabel();
            
            _statusStrip.Items.AddRange(new[] { _statusLabel, new ToolStripStatusLabel { Spring = true }, _directoryLabel });
            
            _terminal.StatusChanged += (s, e) => 
            {
                if (InvokeRequired)
                    Invoke(new Action(() => _statusLabel.Text = e));
                else
                    _statusLabel.Text = e;
            };
            
            _terminal.DirectoryChanged += (s, e) =>
            {
                if (InvokeRequired)
                    Invoke(new Action(() => _directoryLabel.Text = e));
                else
                    _directoryLabel.Text = e;
            };
        }

        private void SetupWindow()
        {
            // Apply modern flat style
            foreach (Control control in Controls)
            {
                if (control is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                }
            }
        }

        private void NewTerminal()
        {
            var newWindow = new MainWindow();
            newWindow.Show();
        }

        private void OpenScript()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Script files (*.ps1;*.bat;*.cmd)|*.ps1;*.bat;*.cmd|All files (*.*)|*.*";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _terminal.ExecuteScript(dialog.FileName);
                }
            }
        }

        private void SaveOutput()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _terminal.SaveOutput(dialog.FileName);
                }
            }
        }

        private void ToggleFullscreen()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.Sizable;
                _menuStrip.Visible = true;
                _toolStrip.Visible = true;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                _menuStrip.Visible = false;
                _toolStrip.Visible = false;
            }
        }

        private void ShowEnvironmentVariables()
        {
            _terminal.ExecuteCommand("env");
        }

        private void ShowProcessManager()
        {
            var processForm = new ProcessMonitorForm(_terminal.GetProcessManager());
            processForm.ShowDialog();
        }

        private void ShowSettings()
        {
            var settingsForm = new SettingsForm(_terminal);
            settingsForm.ShowDialog();
        }

        private void ShowAbout()
        {
            var about = @"🖥️ WinShell v1.0.0

Advanced Modern Windows Terminal
Built with ❤️ for power users

FEATURES:
✨ Multiple Theme Support (Dark, Light, Matrix, Cyberpunk, Solarized, Monokai)
⚡ Real-time Process Management
📜 Command History with Search
🎯 Interactive Button Controls
🔧 Full PowerShell Integration
💻 Modern UI/UX Design

© 2025 WinShell Project
Licensed under MIT License";

            MessageBox.Show(about, "About WinShell", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // New methods for modern GUI
        private void CreateModernButtonPanel()
        {
            _sidePanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 200,
                BackColor = Color.FromArgb(25, 25, 25),
                Padding = new Padding(10)
            };

            var buttons = new System.Collections.Generic.List<ModernButton>
            {
                CreateModernButton("⚡ Kill All Jobs", "Stop all running processes", KillAllJobs),
                CreateModernButton("📜 Command History", "View and search command history", ShowCommandHistory),
                CreateModernButton("🎨 Change Theme", "Switch terminal theme", ShowThemeSelector),
                CreateModernButton("🗑️ Clear Terminal", "Clear all output", ClearTerminal),
                CreateModernButton("📊 Process Monitor", "View running processes", ShowProcessMonitor),
                CreateModernButton("📁 Quick Navigate", "Navigate to common folders", ShowQuickNavigate),
                CreateModernButton("💾 Export Output", "Save terminal output", ExportOutput),
                CreateModernButton("⚙️ Settings", "Configure WinShell", ShowAdvancedSettings)
            };

            int yPos = 10;
            foreach (var btn in buttons)
            {
                btn.Location = new Point(10, yPos);
                btn.Size = new Size(_sidePanel.Width - 20, 45);
                _sidePanel.Controls.Add(btn);
                yPos += 55;
            }

            var brandLabel = new Label
            {
                Text = "WinShell v1.0\n💻 Modern Terminal",
                Dock = DockStyle.Bottom,
                Height = 60,
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8F, FontStyle.Italic)
            };
            _sidePanel.Controls.Add(brandLabel);
        }

        private ModernButton CreateModernButton(string text, string tooltip, Action clickAction)
        {
            var btn = new ModernButton
            {
                Text = text,
                ToolTipText = tooltip,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(62, 62, 66);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 122, 204);

            btn.Click += (s, e) => clickAction?.Invoke();

            var toolTip = new ToolTip();
            toolTip.SetToolTip(btn, tooltip);

            return btn;
        }

        private void ApplyTheme(Theme theme)
        {
            this.BackColor = theme.BackgroundColor;
            if (_sidePanel != null)
            {
                _sidePanel.BackColor = Color.FromArgb(Math.Max(0, theme.BackgroundColor.R - 5), 
                                                      Math.Max(0, theme.BackgroundColor.G - 5), 
                                                      Math.Max(0, theme.BackgroundColor.B - 5));
            }
            _menuStrip.BackColor = theme.MenuBackground;
            _menuStrip.ForeColor = theme.MenuForeground;
            _toolStrip.BackColor = theme.MenuBackground;
            _toolStrip.ForeColor = theme.MenuForeground;
            _statusStrip.BackColor = theme.StatusBarBackground;
            _statusStrip.ForeColor = theme.StatusBarForeground;
            
            _terminal.ApplyTheme(theme);

            // Update button panel colors
            if (_sidePanel != null)
            {
                foreach (Control ctrl in _sidePanel.Controls)
                {
                    if (ctrl is ModernButton btn)
                    {
                        btn.BackColor = theme.ButtonBackground;
                        btn.ForeColor = theme.ButtonForeground;
                        btn.FlatAppearance.MouseOverBackColor = theme.ButtonHoverBackground;
                        btn.FlatAppearance.MouseDownBackColor = theme.AccentColor;
                    }
                }
            }
        }

        private void KillAllJobs()
        {
            var result = MessageBox.Show(
                "Are you sure you want to kill all running background processes?",
                "⚠️ Confirm Kill All Jobs",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                _terminal.KillAllProcesses();
                MessageBox.Show("All background processes have been terminated.", 
                              "✅ Jobs Killed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ShowCommandHistory()
        {
            var historyForm = new CommandHistoryForm(_terminal.GetCommandHistory());
            if (historyForm.ShowDialog() == DialogResult.OK && historyForm.SelectedCommand != null)
            {
                _terminal.ExecuteCommand(historyForm.SelectedCommand);
            }
        }

        private void ShowThemeSelector()
        {
            var themeForm = new ThemeSelectorForm(_themeManager);
            if (themeForm.ShowDialog() == DialogResult.OK)
            {
                ApplyTheme(_themeManager.CurrentTheme);
            }
        }

        private void ClearTerminal()
        {
            _terminal.Clear();
        }

        private void ShowProcessMonitor()
        {
            var processInfoForm = new ProcessInfoForm();
            processInfoForm.ShowDialog();
        }

        private void ShowQuickNavigate()
        {
            var quickNav = new QuickNavigateForm();
            if (quickNav.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(quickNav.SelectedPath))
            {
                _terminal.ExecuteCommand($"cd \"{quickNav.SelectedPath}\"");
            }
        }

        private void ExportOutput()
        {
            SaveOutput();
        }

        private void ShowAdvancedSettings()
        {
            var settingsForm = new SettingsForm(_terminal);
            settingsForm.ShowDialog();
        }

        private void ShowFindDialog()
        {
            var findForm = new Form
            {
                Text = "Find in Output",
                Size = new Size(400, 150),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var searchBox = new TextBox
            {
                Location = new Point(20, 20),
                Size = new Size(340, 25)
            };

            var findButton = new Button
            {
                Text = "🔍 Find",
                Location = new Point(20, 60),
                Size = new Size(100, 30)
            };

            findButton.Click += (s, e) =>
            {
                _terminal.FindText(searchBox.Text);
                findForm.Close();
            };

            findForm.Controls.AddRange(new Control[] { searchBox, findButton });
            findForm.ShowDialog();
        }

        private void ToggleSidePanel()
        {
            _sidePanel.Visible = !_sidePanel.Visible;
        }

        private void ShowKeyboardShortcuts()
        {
            var shortcuts = @"⌨️ WinShell Keyboard Shortcuts

GENERAL:
  F11                  - Toggle Fullscreen
  Ctrl+N               - New Terminal Window
  Ctrl+W               - Close Window

EDITING:
  Ctrl+C               - Copy Selected Text (or Cancel Command)
  Ctrl+V               - Paste from Clipboard
  Ctrl+A               - Select All
  Ctrl+L               - Clear Terminal
  Ctrl+F               - Find in Output

HISTORY:
  Up Arrow             - Previous Command
  Down Arrow           - Next Command
  Ctrl+Shift+H         - Show Command History

THEME & VIEW:
  Ctrl+Shift+T         - Theme Selector
  Ctrl+ +              - Zoom In
  Ctrl+ -              - Zoom Out

PROCESS MANAGEMENT:
  Ctrl+Shift+K         - Kill All Background Jobs";

            MessageBox.Show(shortcuts, "⌨️ Keyboard Shortcuts", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                e.Handled = true;
                ToggleFullscreen();
            }
            else if (e.Control && e.Shift && e.KeyCode == Keys.H)
            {
                e.Handled = true;
                ShowCommandHistory();
            }
            else if (e.Control && e.Shift && e.KeyCode == Keys.T)
            {
                e.Handled = true;
                ShowThemeSelector();
            }
            else if (e.Control && e.Shift && e.KeyCode == Keys.K)
            {
                e.Handled = true;
                KillAllJobs();
            }
        }
    }

    // Modern Button Class with Enhanced Visual Effects
    public class ModernButton : Button
    {
        public string ToolTipText { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Add subtle gradient effect
            if (this.ClientRectangle.Width > 0 && this.ClientRectangle.Height > 0)
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    this.ClientRectangle,
                    Color.FromArgb(20, 255, 255, 255),
                    Color.FromArgb(0, 255, 255, 255),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
            }
        }
    }
}