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
        private ImageList imageList;
        public List<string> SelectedShortcuts { get; private set; } = new List<string>();

        public DesktopShortcutSelector()
        {
            this.Text = "Select Desktop Shortcuts";
            this.Size = new System.Drawing.Size(600, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            imageList = new ImageList
            {
                ImageSize = new Size(32, 32),
                ColorDepth = ColorDepth.Depth32Bit
            };

            listView = new ListView
            {
                View = View.LargeIcon,
                CheckBoxes = true,
                Dock = DockStyle.Top,
                Height = 380,
                LargeImageList = imageList,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(listView);

            okButton = new Button
            {
                Text = "Add Selected Shortcuts",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            okButton.FlatAppearance.BorderSize = 0;
            okButton.Click += OkButton_Click;
            this.Controls.Add(okButton);

            LoadDesktopShortcuts();
        }

        private void LoadDesktopShortcuts()
        {
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
                    Tag = shortcut
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
            // Load existing shortcuts from storage
            string shortcutFile = Path.Combine(GPTools.RootFolderPath, "shortcuts.txt");
            HashSet<string> allShortcuts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (System.IO.File.Exists(shortcutFile))
            {
                foreach (var line in System.IO.File.ReadAllLines(shortcutFile))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        allShortcuts.Add(line.Trim());
                }
            }

            // Add newly selected shortcuts
            foreach (ListViewItem item in listView.Items)
            {
                if (item.Checked && item.Tag is string path)
                {
                    allShortcuts.Add(path);
                }
            }

            // Save merged shortcuts back to storage
            System.IO.File.WriteAllLines(shortcutFile, allShortcuts);

            SelectedShortcuts = new List<string>(allShortcuts);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void DesktopShortcutSelector_Load(object sender, EventArgs e)
        {
            // Any additional initialization can be done here
        }
    }
}