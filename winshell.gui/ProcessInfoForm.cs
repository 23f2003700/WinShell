using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WinShell.GUI
{
    public class ProcessInfoForm : Form
    {
        private DataGridView _processGrid;
        private System.Windows.Forms.Timer _refreshTimer;
        private Button _refreshButton;
        private Button _killButton;
        private Label _statusLabel;
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _memoryCounter;

        public ProcessInfoForm()
        {
            InitializeComponent();
            InitializePerformanceCounters();
            LoadProcesses();
            
            // Auto-refresh every 2 seconds
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 2000;
            _refreshTimer.Tick += (s, e) => LoadProcesses();
            _refreshTimer.Start();
        }

        private void InitializeComponent()
        {
            this.Text = "📊 Process Monitor - WinShell";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.MinimizeBox = true;
            this.MaximizeBox = true;

            // Top panel with buttons
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(45, 45, 45),
                Padding = new Padding(10)
            };

            _refreshButton = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(10, 10),
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _refreshButton.FlatAppearance.BorderSize = 0;
            _refreshButton.Click += (s, e) => LoadProcesses();

            _killButton = new Button
            {
                Text = "⚠️ Kill Process",
                Location = new Point(120, 10),
                Size = new Size(120, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(204, 50, 50),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _killButton.FlatAppearance.BorderSize = 0;
            _killButton.Click += KillSelectedProcess;

            _statusLabel = new Label
            {
                Location = new Point(250, 15),
                Size = new Size(600, 20),
                ForeColor = Color.LightGray,
                Text = "Monitoring processes..."
            };

            topPanel.Controls.Add(_refreshButton);
            topPanel.Controls.Add(_killButton);
            topPanel.Controls.Add(_statusLabel);

            // DataGridView for processes
            _processGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                GridColor = Color.FromArgb(60, 60, 60),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };

            // Style headers
            _processGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
            _processGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _processGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _processGrid.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            _processGrid.ColumnHeadersHeight = 35;

            // Style rows
            _processGrid.DefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            _processGrid.DefaultCellStyle.ForeColor = Color.White;
            _processGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            _processGrid.DefaultCellStyle.SelectionForeColor = Color.White;
            _processGrid.DefaultCellStyle.Padding = new Padding(5);
            _processGrid.RowTemplate.Height = 30;

            // Alternating row colors
            _processGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);

            // Columns
            _processGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PID",
                HeaderText = "PID",
                Width = 80
            });

            _processGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Process Name",
                Width = 200
            });

            _processGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Memory",
                HeaderText = "Memory (MB)",
                Width = 120
            });

            _processGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CPU",
                HeaderText = "CPU %",
                Width = 100
            });

            _processGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Threads",
                HeaderText = "Threads",
                Width = 100
            });

            _processGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Handles",
                HeaderText = "Handles",
                Width = 100
            });

            _processGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StartTime",
                HeaderText = "Start Time",
                Width = 150
            });

            _processGrid.SelectionChanged += (s, e) =>
            {
                _killButton.Enabled = _processGrid.SelectedRows.Count > 0;
            };

            this.Controls.Add(_processGrid);
            this.Controls.Add(topPanel);
        }

        private void InitializePerformanceCounters()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
            }
            catch
            {
                // Performance counters may not be available
            }
        }

        private void LoadProcesses()
        {
            try
            {
                var processes = Process.GetProcesses()
                    .OrderByDescending(p => p.WorkingSet64)
                    .ToList();

                _processGrid.Rows.Clear();

                foreach (var process in processes)
                {
                    try
                    {
                        var memoryMB = process.WorkingSet64 / 1024.0 / 1024.0;
                        var cpuPercent = GetProcessCpuUsage(process);
                        var startTime = "N/A";

                        try
                        {
                            startTime = process.StartTime.ToString("HH:mm:ss");
                        }
                        catch { }

                        _processGrid.Rows.Add(
                            process.Id,
                            process.ProcessName,
                            memoryMB.ToString("F2"),
                            cpuPercent.ToString("F1"),
                            process.Threads.Count,
                            process.HandleCount,
                            startTime
                        );
                    }
                    catch
                    {
                        // Process may have exited or access denied
                    }
                }

                // Update status
                var totalMemoryUsed = processes.Sum(p => p.WorkingSet64) / 1024.0 / 1024.0 / 1024.0;
                var availableMemory = _memoryCounter?.NextValue() ?? 0;
                var cpuUsage = _cpuCounter?.NextValue() ?? 0;

                _statusLabel.Text = $"Processes: {processes.Count} | " +
                                  $"Total Memory Used: {totalMemoryUsed:F2} GB | " +
                                  $"Available: {availableMemory:F2} GB | " +
                                  $"CPU: {cpuUsage:F1}%";
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"Error loading processes: {ex.Message}";
            }
        }

        private Dictionary<int, (DateTime lastTime, TimeSpan lastCpuTime)> _processCpuCache = 
            new Dictionary<int, (DateTime, TimeSpan)>();

        private double GetProcessCpuUsage(Process process)
        {
            try
            {
                var currentTime = DateTime.Now;
                var currentCpuTime = process.TotalProcessorTime;

                if (_processCpuCache.TryGetValue(process.Id, out var cached))
                {
                    var timeDiff = (currentTime - cached.lastTime).TotalMilliseconds;
                    var cpuDiff = (currentCpuTime - cached.lastCpuTime).TotalMilliseconds;

                    if (timeDiff > 0)
                    {
                        var cpuPercent = (cpuDiff / timeDiff) * 100.0 / Environment.ProcessorCount;
                        _processCpuCache[process.Id] = (currentTime, currentCpuTime);
                        return Math.Min(cpuPercent, 100.0);
                    }
                }

                _processCpuCache[process.Id] = (currentTime, currentCpuTime);
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private void KillSelectedProcess(object sender, EventArgs e)
        {
            if (_processGrid.SelectedRows.Count == 0)
                return;

            var row = _processGrid.SelectedRows[0];
            var pid = Convert.ToInt32(row.Cells["PID"].Value);
            var name = row.Cells["Name"].Value.ToString();

            var result = MessageBox.Show(
                $"Are you sure you want to kill process '{name}' (PID: {pid})?",
                "⚠️ Confirm Kill Process",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var process = Process.GetProcessById(pid);
                    process.Kill();
                    MessageBox.Show($"Process '{name}' (PID: {pid}) has been terminated.",
                        "✅ Process Killed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadProcesses();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to kill process: {ex.Message}",
                        "❌ Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
            _cpuCounter?.Dispose();
            _memoryCounter?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
