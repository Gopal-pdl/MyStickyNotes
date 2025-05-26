using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using IWshRuntimeLibrary;

namespace MyStickyNotes
{
    public partial class GPTools : Form
    {
        public const string RootFolderPath = @"D:\StickyNotes";
        private ListBox savedFilesListBox;
        private Panel shortcutsPanel;
        private List<string> loadedShortcuts = new List<string>();
        private TableLayoutPanel mainLayout;
        private FlowLayoutPanel buttonPanel;
        private int hoveredNoteIndex = -1;
        public GPTools()
        {
            InitializeComponent();
        }

        private void GPTools_Load(object sender, EventArgs e)
        {
            // Set form properties for a modern look
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);
            this.Text = "Sticky Notes Tools";
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimumSize = new Size(700, 500);

            // Header panel (add first!)
           

            // Main layout panel
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.White
            };
            // Set a fixed height for the button row
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Buttons row: 60px
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));  // Notes list
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));  // Shortcuts panel
            this.Controls.Add(mainLayout);

            // Button panel (responsive)
            buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = false,
                MinimumSize = new Size(0, 60),
                Height = 60,
                Padding = new Padding(10, 10, 10, 10),
                BackColor = Color.White
            };

            // Add Note button
            Button addNoteButton = new Button
            {
                Text = "Add Note",
                Size = new Size(120, 36),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0, 0, 10, 0)
            };
            addNoteButton.FlatAppearance.BorderSize = 0;
            addNoteButton.Click += AddNoteButton_Click;
            buttonPanel.Controls.Add(addNoteButton);

            // Edit File Name button
            Button editFileNameButton = new Button
            {
                Text = "Edit File Name",
                Size = new Size(120, 36),
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0, 0, 10, 0)
            };
            editFileNameButton.FlatAppearance.BorderSize = 0;
            editFileNameButton.Click += EditFileNameButton_Click;
            buttonPanel.Controls.Add(editFileNameButton);

            // Delete Note button
            Button deleteNoteButton = new Button
            {
                Text = "Delete Note",
                Size = new Size(120, 36),
                BackColor = Color.FromArgb(244, 67, 54), // Red color
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0, 0, 10, 0)
            };
            deleteNoteButton.FlatAppearance.BorderSize = 0;
            deleteNoteButton.Click += DeleteNoteButton_Click;
            buttonPanel.Controls.Add(deleteNoteButton);

            // Add Shortcut button
            Button addShortcutButton = new Button
            {
                Text = "Add Shortcut",
                Size = new Size(120, 36),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0, 0, 10, 0)
            };
            addShortcutButton.FlatAppearance.BorderSize = 0;
            addShortcutButton.Click += AddShortcutButton_Click;
            buttonPanel.Controls.Add(addShortcutButton);

            // Edit Shortcuts button
            Button editShortcutsButton = new Button
            {
                Text = "Edit Shortcuts",
                Size = new Size(120, 36),
                BackColor = Color.FromArgb(121, 85, 72), // Brownish color
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0, 0, 10, 0)
            };
            editShortcutsButton.FlatAppearance.BorderSize = 0;
            editShortcutsButton.Click += EditShortcutsButton_Click;
            buttonPanel.Controls.Add(editShortcutsButton);


            mainLayout.Controls.Add(buttonPanel, 0, 0);

            Panel headerPanel = new Panel
            {
                BackColor = Color.FromArgb(33, 150, 243),
                Height = 50,
                Dock = DockStyle.Top
            };
            Label headerLabel = new Label
            {
                Text = "Sticky Notes Tools",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 0, 0, 0)
            };
            headerPanel.Controls.Add(headerLabel);
            this.Controls.Add(headerPanel);
            // ListBox for saved files
            savedFilesListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };
            savedFilesListBox.DoubleClick += SavedFilesListBox_DoubleClick;
            mainLayout.Controls.Add(savedFilesListBox, 0, 1);
            savedFilesListBox.DrawMode = DrawMode.OwnerDrawFixed;
            savedFilesListBox.DrawItem += SavedFilesListBox_DrawItem;
            savedFilesListBox.MouseMove += SavedFilesListBox_MouseMove;
            savedFilesListBox.MouseLeave += SavedFilesListBox_MouseLeave;

            // Panel to hold shortcut buttons
            shortcutsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            shortcutsPanel.Name = "shortcutsPanel";
            mainLayout.Controls.Add(shortcutsPanel, 0, 2);

            // Ensure the root folder exists
            if (!Directory.Exists(RootFolderPath))
            {
                Directory.CreateDirectory(RootFolderPath);
            }

            // Load existing notes into the ListBox
            LoadSavedNotes();

            // Load shortcuts after the panel is created
            LoadShortcutsFromStorage();
        }




        private void LoadShortcutsFromStorage()
        {
            string shortcutFile = Path.Combine(RootFolderPath, "shortcuts.txt");
            loadedShortcuts.Clear();
            if (System.IO.File.Exists(shortcutFile))
            {
                loadedShortcuts.AddRange(System.IO.File.ReadAllLines(shortcutFile));
            }
            AddShortcutsToPanel(loadedShortcuts);
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
            // fallback: use default icon for .lnk
            return Icon.ExtractAssociatedIcon(shortcutPath);
        }


        private void AddShortcutButton_Click(object sender, EventArgs e)
        {
            using (var selector = new DesktopShortcutSelector())
            {
                if (selector.ShowDialog() == DialogResult.OK)
                {
                    LoadShortcutsFromStorage();
                }
            }
        }

        private void AddShortcutsToPanel(List<string> shortcutPaths)
        {
            if (shortcutsPanel == null) return;

            shortcutsPanel.Controls.Clear();
            int x = 10, y = 10;
            int btnSize = 48, margin = 10;

            foreach (var shortcutPath in shortcutPaths)
            {
                if (!System.IO.File.Exists(shortcutPath)) continue;

                // Get the icon
                Icon icon = GetShortcutIcon(shortcutPath);
                Image img = icon?.ToBitmap();

                Button btn = new Button
                {
                    Size = new System.Drawing.Size(btnSize, btnSize),
                    Location = new System.Drawing.Point(x, y),
                    Tag = shortcutPath,
                    BackColor = System.Drawing.Color.White,
                    FlatStyle = FlatStyle.Flat,
                    ImageAlign = ContentAlignment.MiddleCenter
                };
                btn.FlatAppearance.BorderSize = 0;
                if (img != null)
                {
                    btn.Image = new Bitmap(img, new Size(32, 32));
                }
                btn.Click += ShortcutButton_Click;
                btn.Cursor = Cursors.Hand;
                btn.Padding = new Padding(0);
                btn.Margin = new Padding(0);

                // Tooltip for accessibility
                var toolTip = new ToolTip();
                toolTip.SetToolTip(btn, Path.GetFileNameWithoutExtension(shortcutPath));

                shortcutsPanel.Controls.Add(btn);

                x += btnSize + margin;
            }
        }

        private void SavedFilesListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= savedFilesListBox.Items.Count)
                return;

            bool isHovered = (e.Index == hoveredNoteIndex);
            Color backColor = isHovered ? Color.FromArgb(232, 240, 254) : Color.White;
            Color textColor = isHovered ? Color.FromArgb(33, 150, 243) : Color.Black;

            using (SolidBrush backgroundBrush = new SolidBrush(backColor))
            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
                string text = savedFilesListBox.Items[e.Index].ToString();
                e.Graphics.DrawString(text, e.Font, textBrush, e.Bounds.Left + 4, e.Bounds.Top + 2);
            }

            // Draw focus rectangle if needed
            e.DrawFocusRectangle();
        }

        private void SavedFilesListBox_MouseMove(object sender, MouseEventArgs e)
        {
            int index = savedFilesListBox.IndexFromPoint(e.Location);
            if (index != hoveredNoteIndex)
            {
                hoveredNoteIndex = index;
                savedFilesListBox.Invalidate();
            }
        }

        private void SavedFilesListBox_MouseLeave(object sender, EventArgs e)
        {
            if (hoveredNoteIndex != -1)
            {
                hoveredNoteIndex = -1;
                savedFilesListBox.Invalidate();
            }
        }



        private void ShortcutButton_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is string shortcutPath)
            {
                try
                {
                    System.Diagnostics.Process.Start(shortcutPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to launch shortcut: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void EditShortcutsButton_Click(object sender, EventArgs e)
        {
            using (var selector = new DesktopShortcutSelector())
            {
                if (selector.ShowDialog() == DialogResult.OK)
                {
                    LoadShortcutsFromStorage();
                }
            }
        }



        private void AddNoteButton_Click(object sender, EventArgs e)
        {
            CreateStickyNote();
        }

        private void CreateStickyNote(string content = "", string filePath = null)
        {
            StickyNote note = new StickyNote(content, filePath);
            note.FormClosed += (s, args) =>
            {
                // Reload the list after saving a note
                LoadSavedNotes();
            };
            note.Show();
        }

        private void ShortcutsButton_Click(object sender, EventArgs e)
        {
            ShortcutForm shortcutForm = new ShortcutForm();
            shortcutForm.ShowDialog();
        }



        private void LoadSavedNotes()
        {
            savedFilesListBox.Items.Clear();
            if (Directory.Exists(RootFolderPath))
            {
                var files = Directory.GetFiles(RootFolderPath, "*.txt");
                foreach (var file in files)
                {
                    savedFilesListBox.Items.Add(Path.GetFileName(file));
                }
            }
        }

        private void SavedFilesListBox_DoubleClick(object sender, EventArgs e)
        {
            if (savedFilesListBox.SelectedItem is string fileName)
            {
                string filePath = Path.Combine(RootFolderPath, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    string content = System.IO.File.ReadAllText(filePath);
                    CreateStickyNote(content, filePath);
                }
            }
        }


        private void EditFileNameButton_Click(object sender, EventArgs e)
        {
            if (savedFilesListBox.SelectedItem is string selectedFileName)
            {
                string oldFilePath = Path.Combine(RootFolderPath, selectedFileName);

                if (System.IO.File.Exists(oldFilePath))
                {
                    string newFileName = PromptForNewFileName(selectedFileName);
                    if (!string.IsNullOrEmpty(newFileName))
                    {
                        string newFilePath = Path.Combine(RootFolderPath, newFileName);

                        // Ensure the new file name does not already exist
                        if (!System.IO.File.Exists(newFilePath))
                        {
                            System.IO.File.Move(oldFilePath, newFilePath);
                            LoadSavedNotes(); // Refresh the ListBox
                        }
                        else
                        {
                            MessageBox.Show("A file with the new name already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a file to rename.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        private void DeleteNoteButton_Click(object sender, EventArgs e)
        {
            if (savedFilesListBox.SelectedItem is string selectedFileName)
            {
                string filePath = Path.Combine(RootFolderPath, selectedFileName);

                if (System.IO.File.Exists(filePath))
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete '{selectedFileName}'?",
                        "Delete Note",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            System.IO.File.Delete(filePath);
                            LoadSavedNotes();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to delete note: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a note to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }



        private string PromptForNewFileName(string currentFileName)
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 300;
                prompt.Height = 150;
                prompt.Text = "Rename File";

                Label textLabel = new Label { Left = 10, Top = 20, Text = "New File Name:" };
                TextBox inputBox = new TextBox { Left = 10, Top = 50, Width = 260, Text = currentFileName };
                Button confirmation = new Button { Text = "OK", Left = 200, Width = 70, Top = 80, DialogResult = DialogResult.OK };

                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(inputBox);
                prompt.Controls.Add(confirmation);
                prompt.AcceptButton = confirmation;

                return prompt.ShowDialog() == DialogResult.OK ? inputBox.Text : null;
            }
        }

        public class StickyNote : Form
        {
            public string SavedFilePath { get; private set; }

            private TextBox noteTextBox;

            public StickyNote(string content = "", string filePath = null)
            {
                SavedFilePath = filePath;
                InitializeNote();
                noteTextBox.Text = content;
            }

            private void InitializeNote()
            {
                this.Text = "Sticky Note";
                this.Size = new System.Drawing.Size(300, 300);
                this.FormBorderStyle = FormBorderStyle.SizableToolWindow;

                // Set the form to always stay on top
                this.TopMost = true;

                noteTextBox = new TextBox
                {
                    Multiline = true,
                    Dock = DockStyle.Fill,
                    Font = new System.Drawing.Font("Arial", 12)
                };
                this.Controls.Add(noteTextBox);

                this.FormClosing += StickyNote_FormClosing;
            }

            private void StickyNote_FormClosing(object sender, FormClosingEventArgs e)
            {
                // Ask user to save the note
                if (string.IsNullOrEmpty(SavedFilePath))
                {
                    SavedFilePath = PromptSaveFilePath();
                    if (SavedFilePath == null)
                    {
                        e.Cancel = true; // Cancel closing if no path selected
                        return;
                    }
                }

                // Ensure the file has a .txt extension
                if (Path.GetExtension(SavedFilePath).ToLower() != ".txt")
                {
                    SavedFilePath += ".txt";
                }

                // Save the note content to the file
                System.IO.File.WriteAllText(SavedFilePath, noteTextBox.Text);

                // Ask the user if they want to rename the file
                DialogResult result = MessageBox.Show("Do you want to rename this note?", "Rename Note", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    string newFileName = PromptForNewFileName(Path.GetFileName(SavedFilePath));
                    if (!string.IsNullOrEmpty(newFileName))
                    {
                        string newFilePath = Path.Combine(GPTools.RootFolderPath, newFileName);

                        // Ensure the new file name does not already exist
                        if (!System.IO.File.Exists(newFilePath))
                        {
                            System.IO.File.Move(SavedFilePath, newFilePath);
                            SavedFilePath = newFilePath; // Update the file path
                        }
                        else
                        {
                            MessageBox.Show("A file with the new name already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            private string PromptForNewFileName(string currentFileName)
            {
                using (Form prompt = new Form())
                {
                    prompt.Width = 300;
                    prompt.Height = 150;
                    prompt.Text = "Rename File";

                    Label textLabel = new Label { Left = 10, Top = 20, Text = "New File Name:" };
                    TextBox inputBox = new TextBox { Left = 10, Top = 50, Width = 260, Text = currentFileName };
                    Button confirmation = new Button { Text = "OK", Left = 200, Width = 70, Top = 80, DialogResult = DialogResult.OK };

                    prompt.Controls.Add(textLabel);
                    prompt.Controls.Add(inputBox);
                    prompt.Controls.Add(confirmation);
                    prompt.AcceptButton = confirmation;

                    return prompt.ShowDialog() == DialogResult.OK ? inputBox.Text : null;
                }
            }
            private string PromptSaveFilePath()
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.InitialDirectory = GPTools.RootFolderPath;
                    saveFileDialog.Filter = "Text Files (*.txt)|*.txt";
                    saveFileDialog.DefaultExt = "txt";
                    saveFileDialog.FileName = $"Note_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = saveFileDialog.FileName;

                        // Explicitly enforce the .txt extension
                        if (!filePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                        {
                            filePath += ".txt";
                        }

                        return filePath;
                    }
                }
                return null; // Return null if the user cancels the dialog
            }
        }
    }
}
