using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System;

namespace MyStickyNotes
{
    public partial class ShortcutForm : Form
    {
        private ListBox shortcutsListBox;
        private Button addShortcutButton;
        private Button launchShortcutButton;
        private List<ShortcutItem> shortcuts = new List<ShortcutItem>();

        public ShortcutForm()
        {
            InitializeComponent();
            this.Text = "Application Shortcuts";
            this.Size = new System.Drawing.Size(400, 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.BackColor = System.Drawing.Color.White;

            shortcutsListBox = new ListBox
            {
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(340, 200),
                Font = new System.Drawing.Font("Segoe UI", 10F)
            };
            this.Controls.Add(shortcutsListBox);

            addShortcutButton = new Button
            {
                Text = "Add Shortcut",
                Location = new System.Drawing.Point(20, 240),
                Size = new System.Drawing.Size(120, 36),
                BackColor = System.Drawing.Color.FromArgb(76, 175, 80),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            addShortcutButton.FlatAppearance.BorderSize = 0;
            addShortcutButton.Click += AddShortcutButton_Click;
            this.Controls.Add(addShortcutButton);

            launchShortcutButton = new Button
            {
                Text = "Launch",
                Location = new System.Drawing.Point(160, 240),
                Size = new System.Drawing.Size(120, 36),
                BackColor = System.Drawing.Color.FromArgb(33, 150, 243),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            launchShortcutButton.FlatAppearance.BorderSize = 0;
            launchShortcutButton.Click += LaunchShortcutButton_Click;
            this.Controls.Add(launchShortcutButton);

            // Preload some common shortcuts
            AddShortcut("VS Code", @"C:\Program Files\Microsoft VS Code\Code.exe");
            AddShortcut("Google Chrome", @"C:\Program Files\Google\Chrome\Application\chrome.exe");
            AddShortcut("Microsoft Edge", @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe");
            RefreshShortcutsList();
        }

        private void AddShortcutButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select Application";
                openFileDialog.Filter = "Executable Files (*.exe)|*.exe";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string appPath = openFileDialog.FileName;
                    string appName = Path.GetFileNameWithoutExtension(appPath);
                    AddShortcut(appName, appPath);
                    RefreshShortcutsList();
                }
            }
        }

        private void LaunchShortcutButton_Click(object sender, EventArgs e)
        {
            if (shortcutsListBox.SelectedItem is ShortcutItem item)
            {
                try
                {
                    Process.Start(item.Path);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to launch: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AddShortcut(string name, string path)
        {
            if (!shortcuts.Exists(s => s.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
            {
                shortcuts.Add(new ShortcutItem { Name = name, Path = path });
            }
        }

        private void RefreshShortcutsList()
        {
            shortcutsListBox.DataSource = null;
            shortcutsListBox.DataSource = shortcuts;
            shortcutsListBox.DisplayMember = "Name";
        }

        private class ShortcutItem
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public override string ToString() => Name;
        }

        private void ShortcutForm_Load(object sender, EventArgs e)
        {

        }
    }
}
