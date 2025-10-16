using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WinShell.GUI
{
    public class CommandHistoryForm : Form
    {
        private ListBox _historyListBox;
        private TextBox _searchBox;
        private Button _executeButton;
        private Button _cancelButton;
        private List<string> _fullHistory;

        public string SelectedCommand { get; private set; }

        public CommandHistoryForm(List<string> commandHistory)
        {
            _fullHistory = commandHistory ?? new List<string>();
            InitializeComponents();
            LoadHistory();
        }

        private void InitializeComponents()
        {
            this.Text = "📜 Command History";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Search box
            var searchLabel = new Label
            {
                Text = "🔍 Search:",
                Location = new Point(20, 20),
                Size = new Size(70, 20),
                ForeColor = Color.White
            };

            _searchBox = new TextBox
            {
                Location = new Point(95, 18),
                Size = new Size(465, 25),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White
            };
            _searchBox.TextChanged += SearchBox_TextChanged;

            // History ListBox
            _historyListBox = new ListBox
            {
                Location = new Point(20, 55),
                Size = new Size(540, 350),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Font = new Font("Cascadia Code", 10F),
                SelectionMode = SelectionMode.One
            };
            _historyListBox.DoubleClick += (s, e) => ExecuteSelected();

            // Execute button
            _executeButton = new Button
            {
                Text = "✅ Execute",
                Location = new Point(360, 420),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _executeButton.Click += (s, e) => ExecuteSelected();

            // Cancel button
            _cancelButton = new Button
            {
                Text = "❌ Cancel",
                Location = new Point(470, 420),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _cancelButton.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // Add info label
            var infoLabel = new Label
            {
                Text = $"Total Commands: {_fullHistory.Count}",
                Location = new Point(20, 425),
                Size = new Size(200, 20),
                ForeColor = Color.Gray
            };

            this.Controls.AddRange(new Control[] { searchLabel, _searchBox, _historyListBox, _executeButton, _cancelButton, infoLabel });
        }

        private void LoadHistory()
        {
            _historyListBox.Items.Clear();
            var reversedHistory = Enumerable.Reverse(_fullHistory).ToList();
            foreach (var cmd in reversedHistory)
            {
                _historyListBox.Items.Add(cmd);
            }
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            var searchText = _searchBox.Text.ToLower();
            _historyListBox.Items.Clear();

            var filtered = _fullHistory.Where(cmd => cmd.ToLower().Contains(searchText)).Reverse();
            foreach (var cmd in filtered)
            {
                _historyListBox.Items.Add(cmd);
            }
        }

        private void ExecuteSelected()
        {
            if (_historyListBox.SelectedItem != null)
            {
                SelectedCommand = _historyListBox.SelectedItem.ToString();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
