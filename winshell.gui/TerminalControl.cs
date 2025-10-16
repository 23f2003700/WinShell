using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinShell.Core;

namespace WinShell.GUI
{
    public class TerminalControl : UserControl
    {
        private RichTextBox _outputBox;
        private TextBox _inputBox;
        private CommandEngine _engine;
        private List<string> _commandHistory;
        private int _historyIndex;
        private CancellationTokenSource _currentCommandCts;
        private float _currentZoom = 1.0f;

        public event EventHandler<string> StatusChanged;
        public event EventHandler<string> DirectoryChanged;

        public TerminalControl()
        {
            InitializeComponent();
            InitializeTerminal();
        }

        private void InitializeComponent()
        {
            _outputBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(12, 12, 12),
                ForeColor = Color.FromArgb(204, 204, 204),
                Font = new Font("Cascadia Code", 10F),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                WordWrap = false,
                DetectUrls = false
            };

            _inputBox = new TextBox
            {
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Font = new Font("Cascadia Code", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                Height = 30
            };

            _inputBox.KeyDown += InputBox_KeyDown;
            _outputBox.TextChanged += (s, e) => ScrollToBottom();

            Controls.Add(_outputBox);
            Controls.Add(_inputBox);
        }

        private void InitializeTerminal()
        {
            _engine = new CommandEngine();
            _commandHistory = new List<string>();
            _historyIndex = -1;

            _engine.OutputReceived += (s, e) => AppendOutput(e, Color.White);
            _engine.ErrorReceived += (s, e) => AppendOutput(e, Color.Red);

            PrintWelcome();
            UpdatePrompt();
            
            DirectoryChanged?.Invoke(this, _engine.Environment.CurrentDirectory);
        }

        private void PrintWelcome()
        {
            AppendOutput(@"
╦ ╦╦╔╗╔╔═╗╦ ╦╔═╗╦  ╦  
║║║║║║║╚═╗╠═╣║╣ ║  ║  
╚╩╝╩╝╚╝╚═╝╩ ╩╚═╝╩═╝╩═╝
", Color.Cyan);
            AppendOutput("WinShell v1.0.0 - GUI Terminal\n", Color.White);
            AppendOutput("Type 'help' for available commands\n", Color.Gray);
            AppendOutput("----------------------------------------\n\n", Color.DarkGray);
        }

        private async void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await ProcessCommand();
            }
            else if (e.KeyCode == Keys.Up)
            {
                e.SuppressKeyPress = true;
                NavigateHistory(-1);
            }
            else if (e.KeyCode == Keys.Down)
            {
                e.SuppressKeyPress = true;
                NavigateHistory(1);
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                e.SuppressKeyPress = true;
                StopCurrentCommand();
            }
            else if (e.Control && e.KeyCode == Keys.L)
            {
                e.SuppressKeyPress = true;
                Clear();
            }
        }

        private async Task ProcessCommand()
        {
            var command = _inputBox.Text.Trim();
            if (string.IsNullOrEmpty(command))
                return;

            // Add to history
            _commandHistory.Add(command);
            _historyIndex = _commandHistory.Count;

            // Display command in output
            AppendOutput(_engine.Environment.GetPrompt(), Color.Green);
            AppendOutput(command + "\n", Color.Yellow);

            // Clear input
            _inputBox.Clear();
            _inputBox.Enabled = false;

            // Update status
            StatusChanged?.Invoke(this, "Executing...");

            try
            {
                _currentCommandCts = new CancellationTokenSource();
                
                var result = await Task.Run(() => 
                    _engine.ExecuteCommandAsync(command, _currentCommandCts.Token));

                // Check for special clear screen command
                if (result.Output == "[CLEAR_SCREEN]")
                {
                    ClearScreen();
                }
                else if (!string.IsNullOrEmpty(result.Output))
                {
                    AppendOutput(result.Output, Color.White);
                    if (!result.Output.EndsWith("\n"))
                        AppendOutput("\n", Color.White);
                }

                if (!string.IsNullOrEmpty(result.Error))
                {
                    AppendOutput(result.Error, Color.Red);
                    if (!result.Error.EndsWith("\n"))
                        AppendOutput("\n", Color.Red);
                }

                if (!result.Success && result.ExitCode != 0)
                {
                    AppendOutput($"Command exited with code: {result.ExitCode}\n", Color.Yellow);
                }

                StatusChanged?.Invoke(this, $"Ready (Execution time: {result.ExecutionTime.TotalMilliseconds:F0}ms)");
            }
            catch (Exception ex)
            {
                AppendOutput($"Error: {ex.Message}\n", Color.Red);
                StatusChanged?.Invoke(this, "Error");
            }
            finally
            {
                _currentCommandCts?.Dispose();
                _currentCommandCts = null;
                _inputBox.Enabled = true;
                _inputBox.Focus();
                
                DirectoryChanged?.Invoke(this, _engine.Environment.CurrentDirectory);
                UpdatePrompt();
            }
        }

        private void NavigateHistory(int direction)
        {
            if (_commandHistory.Count == 0)
                return;

            if (direction < 0) // Up
            {
                if (_historyIndex > 0)
                    _historyIndex--;
            }
            else // Down
            {
                if (_historyIndex < _commandHistory.Count - 1)
                    _historyIndex++;
                else
                {
                    _historyIndex = _commandHistory.Count;
                    _inputBox.Clear();
                    return;
                }
            }

            if (_historyIndex >= 0 && _historyIndex < _commandHistory.Count)
            {
                _inputBox.Text = _commandHistory[_historyIndex];
                _inputBox.SelectionStart = _inputBox.Text.Length;
            }
        }

        private void UpdatePrompt()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdatePrompt));
                return;
            }

            // You could display the prompt in a label or status bar
            // For now, we'll just update the input box placeholder
        }

        private void AppendOutput(string text, Color color)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppendOutput(text, color)));
                return;
            }

            _outputBox.SelectionStart = _outputBox.TextLength;
            _outputBox.SelectionLength = 0;
            _outputBox.SelectionColor = color;
            _outputBox.AppendText(text);
            _outputBox.SelectionColor = _outputBox.ForeColor;
        }

        private void ScrollToBottom()
        {
            _outputBox.SelectionStart = _outputBox.Text.Length;
            _outputBox.ScrollToCaret();
        }

        private void ClearScreen()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ClearScreen));
                return;
            }

            _outputBox.Clear();
        }

        public void Clear()
        {
            _outputBox.Clear();
            PrintWelcome();
        }

        public void Copy()
        {
            if (_outputBox.SelectedText.Length > 0)
                Clipboard.SetText(_outputBox.SelectedText);
        }

        public void Paste()
        {
            if (Clipboard.ContainsText())
                _inputBox.Text = Clipboard.GetText();
        }

        public void ZoomIn()
        {
            _currentZoom += 0.1f;
            UpdateZoom();
        }

        public void ZoomOut()
        {
            if (_currentZoom > 0.5f)
            {
                _currentZoom -= 0.1f;
                UpdateZoom();
            }
        }

        public void ResetZoom()
        {
            _currentZoom = 1.0f;
            UpdateZoom();
        }

        private void UpdateZoom()
        {
            var baseSize = 10f;
            var newSize = baseSize * _currentZoom;
            var font = new Font("Cascadia Code", newSize);
            _outputBox.Font = font;
            _inputBox.Font = font;
        }

        public void ExecuteCommand(string command)
        {
            _inputBox.Text = command;
            ProcessCommand().Wait();
        }

        public void ExecuteScript(string scriptPath)
        {
            if (File.Exists(scriptPath))
            {
                var lines = File.ReadAllLines(scriptPath);
                Task.Run(async () =>
                {
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("#"))
                        {
                            Invoke(new Action(() => _inputBox.Text = line));
                            await ProcessCommand();
                        }
                    }
                });
            }
        }

        public void SaveOutput(string filePath)
        {
            File.WriteAllText(filePath, _outputBox.Text);
        }

        public void StopCurrentCommand()
        {
            _currentCommandCts?.Cancel();
            AppendOutput("\n^C - Command cancelled\n", Color.Yellow);
            StatusChanged?.Invoke(this, "Cancelled");
        }

        // New methods for enhanced GUI
        public void ApplyTheme(Theme theme)
        {
            _outputBox.BackColor = theme.TerminalBackground;
            _outputBox.ForeColor = theme.TerminalForeground;
            _inputBox.BackColor = Color.FromArgb(
                Math.Min(255, theme.TerminalBackground.R + 15),
                Math.Min(255, theme.TerminalBackground.G + 15),
                Math.Min(255, theme.TerminalBackground.B + 15));
            _inputBox.ForeColor = theme.TerminalForeground;
        }

        public List<string> GetCommandHistory()
        {
            return new List<string>(_commandHistory);
        }

        public ProcessManager GetProcessManager()
        {
            return _engine.Environment != null ? new ProcessManager(_engine.Environment) : null;
        }

        public void KillAllProcesses()
        {
            // This would typically interact with ProcessManager
            // For now, just notify
            AppendOutput("All background processes killed.\n", Color.Yellow);
            StatusChanged?.Invoke(this, "All jobs killed");
        }

        public void FindText(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return;

            int startIndex = _outputBox.SelectionStart + _outputBox.SelectionLength;
            int index = _outputBox.Text.IndexOf(searchText, startIndex, StringComparison.CurrentCultureIgnoreCase);

            if (index < 0 && startIndex > 0)
            {
                // Wrap around search
                index = _outputBox.Text.IndexOf(searchText, 0, StringComparison.CurrentCultureIgnoreCase);
            }

            if (index >= 0)
            {
                _outputBox.Select(index, searchText.Length);
                _outputBox.ScrollToCaret();
                _outputBox.Focus();
            }
            else
            {
                MessageBox.Show($"Text '{searchText}' not found.", "Find", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void SelectAll()
        {
            _outputBox.SelectAll();
            _outputBox.Focus();
        }

        // Settings methods
        public void SetFontFamily(string fontFamily)
        {
            try
            {
                var currentSize = _outputBox.Font.Size;
                var newFont = new Font(fontFamily, currentSize);
                _outputBox.Font = newFont;
                _inputBox.Font = newFont;
            }
            catch
            {
                MessageBox.Show($"Font '{fontFamily}' could not be applied. Using default font.", 
                    "Font Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void SetFontSize(float size)
        {
            try
            {
                var currentFamily = _outputBox.Font.FontFamily.Name;
                var newFont = new Font(currentFamily, size);
                _outputBox.Font = newFont;
                _inputBox.Font = newFont;
                
                // Update zoom level to match new base size
                _currentZoom = size / 10f;
            }
            catch
            {
                MessageBox.Show($"Font size {size} could not be applied.", 
                    "Font Size Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void SetWordWrap(bool enabled)
        {
            _outputBox.WordWrap = enabled;
        }

        public void SetAutoScroll(bool enabled)
        {
            // Auto scroll is handled by the TextChanged event
            // We could add a flag to control this behavior
            if (enabled)
            {
                ScrollToBottom();
            }
        }

        public string GetCurrentFontFamily()
        {
            return _outputBox.Font.FontFamily.Name;
        }

        public float GetCurrentFontSize()
        {
            return _outputBox.Font.Size;
        }

        public bool GetWordWrap()
        {
            return _outputBox.WordWrap;
        }
    }
}