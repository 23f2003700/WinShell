using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using WinShell.Core;

namespace WinShell.GUI
{
    public class ProcessMonitorForm : Form
    {
        private ProcessManager _processManager;
        private ListBox _processListBox;
        private Timer _refreshTimer;
        private Button _killButton;
        private Button _refreshButton;

        public ProcessMonitorForm(ProcessManager processManager)
        {
            _processManager = processManager;
            InitializeComponents();
            RefreshProcessList();
        }

        private void InitializeComponents()
        {
            this.Text = "📊 Process Monitor";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 30);

            var titleLabel = new Label
            {
                Text = "⚡ Running Background Processes",
                Location = new Point(20, 20),
                Size = new Size(400, 30),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold)
            };

            _processListBox = new ListBox
            {
                Location = new Point(20, 60),
                Size = new Size(640, 350),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Font = new Font("Cascadia Code", 10F)
            };

            _killButton = new Button
            {
                Text = "⛔ Kill Selected Process",
                Location = new Point(20, 425),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _killButton.Click += KillButton_Click;

            _refreshButton = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(210, 425),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _refreshButton.Click += (s, e) => RefreshProcessList();

            var closeButton = new Button
            {
                Text = "❌ Close",
                Location = new Point(570, 425),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            closeButton.Click += (s, e) => this.Close();

            // Auto-refresh timer
            _refreshTimer = new Timer { Interval = 5000 };
            _refreshTimer.Tick += (s, e) => RefreshProcessList();
            _refreshTimer.Start();

            this.Controls.AddRange(new Control[] { titleLabel, _processListBox, _killButton, _refreshButton, closeButton });
        }

        private void RefreshProcessList()
        {
            _processListBox.Items.Clear();
            var processes = _processManager.GetRunningProcesses();
            
            if (!processes.Any())
            {
                _processListBox.Items.Add("No background processes running");
            }
            else
            {
                foreach (var process in processes)
                {
                    try
                    {
                        _processListBox.Items.Add($"PID: {process.Id} | {process.ProcessName}");
                    }
                    catch { }
                }
            }
        }

        private void KillButton_Click(object sender, EventArgs e)
        {
            if (_processListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a process to kill.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedItem = _processListBox.SelectedItem.ToString();
            if (selectedItem.Contains("PID:"))
            {
                var pidStr = selectedItem.Split('|')[0].Replace("PID:", "").Trim();
                if (int.TryParse(pidStr, out int pid))
                {
                    var confirm = MessageBox.Show(
                        $"Kill process {pid}?",
                        "Confirm Kill",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirm == DialogResult.Yes)
                    {
                        _processManager.KillProcess(pid);
                        RefreshProcessList();
                    }
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
