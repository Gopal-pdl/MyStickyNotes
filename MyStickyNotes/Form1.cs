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

            // Save the note content to the file
            File.WriteAllText(SavedFilePath, noteTextBox.Text);
        }

        private string PromptSaveFilePath()
        {
            // Generate a default file name
            string defaultFileName = $"Note_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string filePath = Path.Combine(Form1.RootFolderPath, defaultFileName);
            return filePath;
        }
    }
}
