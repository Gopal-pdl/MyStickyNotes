using System;
using System.IO;
using System.Windows.Forms;

namespace MyStickyNotes
{
    public partial class Form1 : Form
    {
        public const string RootFolderPath = @"D:\StickyNotes";
        private ListBox savedFilesListBox;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Ensure the root folder exists
            if (!Directory.Exists(RootFolderPath))
            {
                Directory.CreateDirectory(RootFolderPath);
            }

            // Add a button to create new sticky notes
            Button addNoteButton = new Button
            {
                Text = "Add Note",
                Size = new System.Drawing.Size(100, 30),
                Location = new System.Drawing.Point(10, 10)
            };
            addNoteButton.Click += AddNoteButton_Click;
            this.Controls.Add(addNoteButton);

            // Add a ListBox to show recently saved files
            savedFilesListBox = new ListBox
            {
                Size = new System.Drawing.Size(400, 300),
                Location = new System.Drawing.Point(10, 50)
            };
            savedFilesListBox.DoubleClick += SavedFilesListBox_DoubleClick;
            this.Controls.Add(savedFilesListBox);

            // Add a button to edit file name
            Button editFileNameButton = new Button
            {
                Text = "Edit File Name",
                Size = new System.Drawing.Size(100, 30),
                Location = new System.Drawing.Point(120, 10)
            };
            editFileNameButton.Click += EditFileNameButton_Click;
            this.Controls.Add(editFileNameButton);

            // Load existing notes into the ListBox
            LoadSavedNotes();
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
                if (File.Exists(filePath))
                {
                    string content = File.ReadAllText(filePath);
                    CreateStickyNote(content, filePath);
                }
            }
        }


        private void EditFileNameButton_Click(object sender, EventArgs e)
        {
            if (savedFilesListBox.SelectedItem is string selectedFileName)
            {
                string oldFilePath = Path.Combine(RootFolderPath, selectedFileName);

                if (File.Exists(oldFilePath))
                {
                    string newFileName = PromptForNewFileName(selectedFileName);
                    if (!string.IsNullOrEmpty(newFileName))
                    {
                        string newFilePath = Path.Combine(RootFolderPath, newFileName);

                        // Ensure the new file name does not already exist
                        if (!File.Exists(newFilePath))
                        {
                            File.Move(oldFilePath, newFilePath);
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
                File.WriteAllText(SavedFilePath, noteTextBox.Text);

                // Ask the user if they want to rename the file
                DialogResult result = MessageBox.Show("Do you want to rename this note?", "Rename Note", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    string newFileName = PromptForNewFileName(Path.GetFileName(SavedFilePath));
                    if (!string.IsNullOrEmpty(newFileName))
                    {
                        string newFilePath = Path.Combine(Form1.RootFolderPath, newFileName);

                        // Ensure the new file name does not already exist
                        if (!File.Exists(newFilePath))
                        {
                            File.Move(SavedFilePath, newFilePath);
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
                    saveFileDialog.InitialDirectory = Form1.RootFolderPath;
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
