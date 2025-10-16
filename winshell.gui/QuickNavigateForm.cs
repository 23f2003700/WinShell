using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace WinShell.GUI
{
    public class QuickNavigateForm : Form
    {
        private ListBox _locationListBox;
        public string SelectedPath { get; private set; }

        public QuickNavigateForm()
        {
            InitializeComponents();
            LoadCommonLocations();
        }

        private void InitializeComponents()
        {
            this.Text = "📁 Quick Navigate";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);

            var titleLabel = new Label
            {
                Text = "Common Locations:",
                Location = new Point(20, 20),
                Size = new Size(200, 25),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };

            _locationListBox = new ListBox
            {
                Location = new Point(20, 55),
                Size = new Size(440, 320),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Font = new Font("Cascadia Code", 10F)
            };
            _locationListBox.DoubleClick += (s, e) => NavigateToSelected();

            var navigateButton = new Button
            {
                Text = "✅ Navigate",
                Location = new Point(260, 390),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            navigateButton.Click += (s, e) => NavigateToSelected();

            var cancelButton = new Button
            {
                Text = "❌ Cancel",
                Location = new Point(380, 390),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cancelButton.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { titleLabel, _locationListBox, navigateButton, cancelButton });
        }

        private void LoadCommonLocations()
        {
            var locations = new[]
            {
                $"🏠 Home: {Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}",
                $"💾 Desktop: {Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}",
                $"📄 Documents: {Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}",
                $"📥 Downloads: {Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")}",
                $"🎵 Music: {Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)}",
                $"🎬 Videos: {Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)}",
                $"🖼️ Pictures: {Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}",
                $"💻 Program Files: {Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}",
                $"🪟 Windows: {Environment.GetFolderPath(Environment.SpecialFolder.Windows)}",
                $"⚙️ System32: {Environment.GetFolderPath(Environment.SpecialFolder.System)}",
                $"🔧 AppData: {Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}",
                $"📁 Local AppData: {Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}",
                $"🌐 Program Data: {Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}",
                $"🗂️ Temp: {Path.GetTempPath()}",
                $"💽 C:\\ Drive: C:\\"
            };

            foreach (var location in locations)
            {
                _locationListBox.Items.Add(location);
            }
        }

        private void NavigateToSelected()
        {
            if (_locationListBox.SelectedItem != null)
            {
                var selected = _locationListBox.SelectedItem.ToString();
                var parts = selected.Split(new[] { ": " }, StringSplitOptions.None);
                if (parts.Length > 1)
                {
                    SelectedPath = parts[1];
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }
    }
}
