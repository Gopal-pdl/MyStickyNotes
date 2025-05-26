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
        private ListView savedFilesListView;
        private Panel shortcutsPanel;
        private List<string> loadedShortcuts = new List<string>();
        private TableLayoutPanel mainLayout;
        private FlowLayoutPanel buttonPanel;
        private int hoveredNoteIndex = -1;
        private Panel sidebarPanel;
        private Panel mainContentPanel;
        private string currentRootFolder = RootFolderPath;

        private Panel notePreviewPanel;
        private TextBox notePreviewTextBox;
        private Button togglePreviewButton;
        private bool isPreviewVisible = false;

        public GPTools()
        {
            InitializeComponent();
        }

        private void GPTools_Load(object sender, EventArgs e)
        {
            // Set form properties
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);
            this.Text = "Sticky Notes Tools";
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimumSize = new Size(900, 600);

            

           

            // Main layout: sidebar + main content
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F)); // Sidebar width
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.Controls.Add(mainLayout);

            // Sidebar panel
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            mainLayout.Controls.Add(sidebarPanel, 0, 0);

            // Header panel
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

            // Main content panel
            mainContentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            mainLayout.Controls.Add(mainContentPanel, 1, 0);

            // Sidebar buttons
            int btnHeight = 48;
            int btnWidth = 160;
            int btnTop = 30;

            // Notes button (first)
            Button notesButton = new Button
            {
                Text = "Notes",
                Size = new Size(btnWidth, btnHeight),
                Location = new Point(10, btnTop),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            notesButton.FlatAppearance.BorderSize = 0;
            notesButton.Click += (s, e2) => ShowNotesPanel();
            sidebarPanel.Controls.Add(notesButton);

            // Manage Shortcuts button (second)
            Button manageShortcutsButton = new Button
            {
                Text = "Manage Shortcuts",
                Size = new Size(btnWidth, btnHeight),
                Location = new Point(10, btnTop + btnHeight + 10), // Directly after Notes
                BackColor = Color.FromArgb(63, 81, 181),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            manageShortcutsButton.FlatAppearance.BorderSize = 0;
            manageShortcutsButton.Click += ManageShortcutsButton_Click;
            sidebarPanel.Controls.Add(manageShortcutsButton);

            // Settings button (third)
            Button settingsButton = new Button
            {
                Text = "Settings",
                Size = new Size(btnWidth, btnHeight),
                Location = new Point(10, btnTop + (btnHeight + 10) * 2), // After Manage Shortcuts
                BackColor = Color.FromArgb(120, 144, 156),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            settingsButton.FlatAppearance.BorderSize = 0;
            settingsButton.Click += (s, e2) => ShowSettingsPanel();
            sidebarPanel.Controls.Add(settingsButton);



            // Add more sidebar buttons as needed...

            // Ensure the root folder exists
            if (!Directory.Exists(RootFolderPath))
            {
                Directory.CreateDirectory(RootFolderPath);
            }

            // Show notes panel by default
            ShowNotesPanel();

            // Shortcuts panel (static, always at bottom)
            shortcutsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            shortcutsPanel.Name = "shortcutsPanel";
            this.Controls.Add(shortcutsPanel);

            // Load shortcuts after the panel is created
            LoadShortcutsFromStorage();
        }


        private void ManageShortcutsButton_Click(object sender, EventArgs e)
        {
            using (var selector = new DesktopShortcutSelector())
            {
                if (selector.ShowDialog() == DialogResult.OK)
                {
                    LoadShortcutsFromStorage();
                }
            }
        }



        private void ShowNotesPanel()
        {
            mainContentPanel.Controls.Clear();

            // TableLayoutPanel for button panel and notes list
            var notesLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White
            };
            notesLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Button panel height
            notesLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Notes list fills rest

            // Button panel (responsive)
            buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = false,
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
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0, 0, 10, 0)
            };
            deleteNoteButton.FlatAppearance.BorderSize = 0;
            deleteNoteButton.Click += DeleteNoteButton_Click;
            buttonPanel.Controls.Add(deleteNoteButton);

            notesLayout.Controls.Add(buttonPanel, 0, 0);

            // ListView for saved files
            savedFilesListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                HeaderStyle = ColumnHeaderStyle.None,
                Font = new Font("Segoe UI", 12F),
                BorderStyle = BorderStyle.FixedSingle,
                HideSelection = false,
                MultiSelect = false
            };
            savedFilesListView.Columns.Add("Note Name", -2, HorizontalAlignment.Left);
            savedFilesListView.DoubleClick += SavedFilesListView_DoubleClick;
            savedFilesListView.KeyDown += SavedFilesListView_KeyDown;
            savedFilesListView.SelectedIndexChanged += SavedFilesListView_SelectedIndexChanged;
            //savedFilesListView.SelectedIndexChanged += SavedFilesListView_SelectedIndexChanged;
            notesLayout.Controls.Add(savedFilesListView, 0, 1);

            // Preview panel (hidden by default)
            notePreviewPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 250,
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false // Hidden initially
            };

            // Preview textbox (read-only)
            notePreviewTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Vertical
            };
            notePreviewPanel.Controls.Add(notePreviewTextBox);

            // Toggle button
            togglePreviewButton = new Button
            {
                Text = "Show Preview ▶",
                Width = 120,
                Height = 30,
                Dock = DockStyle.Right,
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(0, 10, 10, 0)
            };
            togglePreviewButton.FlatAppearance.BorderSize = 0;
            togglePreviewButton.Click += TogglePreviewButton_Click;

            // Add the toggle button to the notesLayout (top right of the button panel)
            buttonPanel.Controls.Add(togglePreviewButton);

            // Add the preview panel to mainContentPanel (after notesLayout)
            // Add the preview panel to mainContentPanel (before notesLayout)
            mainContentPanel.Controls.Add(notePreviewPanel);
            mainContentPanel.Controls.Add(notesLayout);


            // Load notes
            LoadSavedNotes();
        }

        private void SavedFilesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (savedFilesListView.SelectedItems.Count > 0)
            {
                string fileName = savedFilesListView.SelectedItems[0].Text;
                string filePath = Path.Combine(RootFolderPath, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    notePreviewTextBox.Text = System.IO.File.ReadAllText(filePath);
                }
                else
                {
                    notePreviewTextBox.Text = "";
                }
            }
            else
            {
                notePreviewTextBox.Text = "";
            }
        }


        private void TogglePreviewButton_Click(object sender, EventArgs e)
        {
            isPreviewVisible = !isPreviewVisible;
            notePreviewPanel.Visible = isPreviewVisible;
            togglePreviewButton.Text = isPreviewVisible ? "Hide Preview ◀" : "Show Preview ▶";
        }



        private void SavedFilesListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteNoteButton_Click(sender, EventArgs.Empty);
                e.Handled = true;
            }
        }


        private void SavedFilesListBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            // Set the item height to fit the font and padding
            e.ItemHeight = 36; // Should match your DrawItem logic
        }


       private void ShowSettingsPanel()
{
    mainContentPanel.Controls.Clear();

    Label settingsLabel = new Label
    {
        Text = "Settings",
        Dock = DockStyle.Top,
        Font = new Font("Segoe UI", 14F, FontStyle.Bold),
        ForeColor = Color.Gray,
        Height = 40,
        TextAlign = ContentAlignment.MiddleLeft,
        Padding = new Padding(10, 20, 0, 0)
    };
    mainContentPanel.Controls.Add(settingsLabel);

    Label folderLabel = new Label
    {
        Text = "Notes Folder:",
        Dock = DockStyle.Top,
        Font = new Font("Segoe UI", 11F, FontStyle.Regular),
        Height = 30,
        Padding = new Padding(10, 10, 0, 0)
    };
    mainContentPanel.Controls.Add(folderLabel);

    TextBox folderPathBox = new TextBox
    {
        Text = currentRootFolder,
        Dock = DockStyle.Top,
        ReadOnly = true,
        Font = new Font("Segoe UI", 10F),
        Height = 28,
        Margin = new Padding(10, 0, 10, 0)
    };
    mainContentPanel.Controls.Add(folderPathBox);

    Button changeFolderButton = new Button
    {
        Text = "Change Folder...",
        Dock = DockStyle.Top,
        Height = 32,
        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
        BackColor = Color.FromArgb(33, 150, 243),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Margin = new Padding(10, 10, 10, 0)
    };
    changeFolderButton.FlatAppearance.BorderSize = 0;
    mainContentPanel.Controls.Add(changeFolderButton);

    changeFolderButton.Click += (s, e) =>
    {
        using (var dialog = new FolderBrowserDialog())
        {
            dialog.Description = "Select a folder to save notes";
            dialog.SelectedPath = currentRootFolder;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                currentRootFolder = dialog.SelectedPath;
                folderPathBox.Text = currentRootFolder;
                if (!Directory.Exists(currentRootFolder))
                {
                    Directory.CreateDirectory(currentRootFolder);
                }
                LoadSavedNotes();
            }
        }
    };
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

        //private void SavedFilesListBox_DrawItem(object sender, DrawItemEventArgs e)
        //{
        //    if (e.Index < 0 || e.Index >= savedFilesListBox.Items.Count)
        //        return;

        //    bool isHovered = (e.Index == hoveredNoteIndex);
        //    bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

        //    Color backColor;
        //    Color textColor;

        //    if (isSelected)
        //    {
        //        backColor = Color.FromArgb(204, 228, 247); // Windows selection blue
        //        textColor = Color.Black;
        //    }
        //    else if (isHovered)
        //    {
        //        backColor = Color.FromArgb(232, 240, 254); // Light hover blue
        //        textColor = Color.FromArgb(33, 150, 243);
        //    }
        //    else
        //    {
        //        backColor = Color.White;
        //        textColor = Color.Black;
        //    }

        //    // Fill background
        //    using (SolidBrush backgroundBrush = new SolidBrush(backColor))
        //    {
        //        e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
        //    }

        //    // Add padding (ensure it fits within ItemHeight)
        //    int paddingLeft = 12;
        //    int paddingTop = 8;
        //    Rectangle textRect = new Rectangle(
        //        e.Bounds.Left + paddingLeft,
        //        e.Bounds.Top + paddingTop,
        //        e.Bounds.Width - paddingLeft * 2,
        //        e.Bounds.Height - paddingTop * 2
        //    );

        //    // Draw text (Windows style)
        //    TextRenderer.DrawText(
        //        e.Graphics,
        //        savedFilesListBox.Items[e.Index].ToString(),
        //        e.Font,
        //        textRect,
        //        textColor,
        //        TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis
        //    );

        //    // Draw subtle bottom border for separation
        //    using (Pen borderPen = new Pen(Color.FromArgb(230, 230, 230)))
        //    {
        //        e.Graphics.DrawLine(borderPen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        //    }

        //    // Draw focus rectangle if needed
        //    if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
        //        e.DrawFocusRectangle();
        //}




        //private void SavedFilesListBox_MouseMove(object sender, MouseEventArgs e)
        //{
        //    int index = savedFilesListBox.IndexFromPoint(e.Location);
        //    if (index != hoveredNoteIndex)
        //    {
        //        hoveredNoteIndex = index;
        //        savedFilesListBox.Invalidate();
        //    }
        //}

        //private void SavedFilesListBox_MouseLeave(object sender, EventArgs e)
        //{
        //    if (hoveredNoteIndex != -1)
        //    {
        //        hoveredNoteIndex = -1;
        //        savedFilesListBox.Invalidate();
        //    }
        //}



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
            if (savedFilesListView == null)
                return;

            savedFilesListView.Items.Clear();
            if (Directory.Exists(RootFolderPath))
            {
                var files = Directory.GetFiles(RootFolderPath, "*.txt");
                foreach (var file in files)
                {
                    savedFilesListView.Items.Add(new ListViewItem(Path.GetFileName(file)));
                }
            }
            // Auto-size the column to fit content
            if (savedFilesListView.Columns.Count > 0)
                savedFilesListView.Columns[0].Width = -2;
        }


        private void SavedFilesListView_DoubleClick(object sender, EventArgs e)
        {
            if (savedFilesListView.SelectedItems.Count > 0)
            {
                string fileName = savedFilesListView.SelectedItems[0].Text;
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
            if (savedFilesListView.SelectedItems.Count > 0)
            {
                string selectedFileName = savedFilesListView.SelectedItems[0].Text;
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
                            LoadSavedNotes(); // Refresh the ListView
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
            if (savedFilesListView.SelectedItems.Count > 0)
            {
                string selectedFileName = savedFilesListView.SelectedItems[0].Text;
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
