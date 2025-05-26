using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using IWshRuntimeLibrary;
using System;

namespace MyStickyNotes
{
    public partial class DesktopShortcutSelector : Form
    {
        private ListView listView;
        private Button okButton;
        private Button cancelButton;
        private ImageList imageList;
        private Panel headerPanel;
        private Label headerLabel;
        private TableLayoutPanel mainLayout;
        public List<string> SelectedShortcuts { get; private set; } = new List<string>();
        private HashSet<string> existingShortcuts;

        public DesktopShortcutSelector()
        {
            this.Text = "Select Desktop Shortcuts";
            this.Size = new Size(650, 540);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            

            // Main layout
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            this.Controls.Add(mainLayout);

            // ListView for shortcuts
            imageList = new ImageList
            {
                ImageSize = new Size(32, 32),
                ColorDepth = ColorDepth.Depth32Bit
            };
            listView = new ListView
            {
                View = View.LargeIcon,
                CheckBoxes = true,
                Dock = DockStyle.Fill,
                LargeImageList = imageList,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11F),
                Margin = new Padding(0, 10, 0, 10)
            };
            mainLayout.Controls.Add(listView, 0, 0);

            // Button panel
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 60,
                BackColor = Color.White
            };

            okButton = new Button
            {
                Text = "Save Shortcuts",
                Size = new Size(160, 40),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(220, 10),
                Cursor = Cursors.Hand
            };
            okButton.FlatAppearance.BorderSize = 0;
            okButton.MouseEnter += (s, e) => okButton.BackColor = Color.FromArgb(25, 118, 210);
            okButton.MouseLeave += (s, e) => okButton.BackColor = Color.FromArgb(33, 150, 243);
            okButton.Click += OkButton_Click;

            cancelButton = new Button
            {
                Text = "Cancel",
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(224, 224, 224),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(400, 10),
                Cursor = Cursors.Hand
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.MouseEnter += (s, e) => cancelButton.BackColor = Color.FromArgb(200, 200, 200);
            cancelButton.MouseLeave += (s, e) => cancelButton.BackColor = Color.FromArgb(224, 224, 224);
            cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(cancelButton);
            mainLayout.Controls.Add(buttonPanel, 0, 1);

            // Header panel
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(33, 150, 243)
            };
            headerLabel = new Label
            {
                Text = "Select Desktop Shortcuts",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(60, 0, 0, 0)
            };
            // Optional: Add an icon to the header
            PictureBox headerIcon = new PictureBox
            {
                Image = SystemIcons.Application.ToBitmap(),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Size = new Size(48, 48),
                Location = new Point(10, 6),
                BackColor = Color.Transparent
            };
            headerPanel.Controls.Add(headerIcon);
            headerPanel.Controls.Add(headerLabel);
            this.Controls.Add(headerPanel);

            LoadDesktopShortcuts();
        }

        private void LoadDesktopShortcuts()
        {
            // Load existing shortcuts from storage
            string shortcutFile = Path.Combine(GPTools.RootFolderPath, "shortcuts.txt");
            existingShortcuts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (System.IO.File.Exists(shortcutFile))
            {
                foreach (var line in System.IO.File.ReadAllLines(shortcutFile))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        existingShortcuts.Add(line.Trim());
                }
            }

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var shortcuts = Directory.GetFiles(desktopPath, "*.lnk");
            int iconIndex = 0;
            foreach (var shortcut in shortcuts)
            {
                Icon icon = GetShortcutIcon(shortcut);
                if (icon != null)
                {
                    imageList.Images.Add(icon);
                }
                else
                {
                    imageList.Images.Add(SystemIcons.Application);
                }
                var item = new ListViewItem
                {
                    Text = Path.GetFileNameWithoutExtension(shortcut),
                    ImageIndex = iconIndex,
                    Tag = shortcut,
                    Checked = existingShortcuts.Contains(shortcut)
                };
                listView.Items.Add(item);
                iconIndex++;
            }
        }

        private Icon GetShortcutIcon(string shortcutPath)
        {
            try
            {
                var shell = new WshShell();
                IWshShortcut link = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                string targetPath = link.TargetPath;

                if (!string.IsNullOrEmpty(targetPath) && System.IO.File.Exists(targetPath))
                {
                    return Icon.ExtractAssociatedIcon(targetPath);
                }
            }
            catch
            {
                // fallback below
            }
            return SystemIcons.Application;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            List<string> checkedShortcuts = new List<string>();
            foreach (ListViewItem item in listView.Items)
            {
                if (item.Checked && item.Tag is string path)
                {
                    checkedShortcuts.Add(path);
                }
            }

            // Save only checked shortcuts to storage (removes unchecked)
            string shortcutFile = Path.Combine(GPTools.RootFolderPath, "shortcuts.txt");
            System.IO.File.WriteAllLines(shortcutFile, checkedShortcuts);

            SelectedShortcuts = checkedShortcuts;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void DesktopShortcutSelector_Load(object sender, EventArgs e)
        {
            // Any additional initialization can be done here
        }
    }
}
