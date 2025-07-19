using System;
using System.Windows.Forms;
using System.IO;
namespace HearthstoneAccessInstaller;

public class Document
{
    public readonly string Title;
    public readonly string Content;
    public Document(string title, string content)
    {
        (Title, Content) = (title, content);
    }

}


public class TextViewerDialog : Form
{
    private TextBox textBox;
    private Button saveButton;
    private Button copyButton;
    private Button closeButton;

    private Document document;

    public TextViewerDialog(Document document)
    {
        this.document = document;
        this.AutoSize = true;
        this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MinimizeBox = false;
        this.MaximizeBox = false;

        FlowLayoutPanel mainPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(11),
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowOnly,
        };

        textBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            MinimumSize = new System.Drawing.Size(500, 350),
            BorderStyle = BorderStyle.FixedSingle,
            AccessibleRole = AccessibleRole.Document, // So that NVDA doesn't grab the text from it and set it to the description of the dialog, which causes lag.
        };

        saveButton = new Button { Text = "Save", Margin = new Padding(6, 0, 0, 0) };
        copyButton = new Button { Text = "Copy", Margin = new Padding(6, 0, 0, 0) };
        closeButton = new Button
        {
            AutoSize = true,
            Margin = new Padding(6, 0, 0, 0),
            Text = "Close",
            DialogResult = DialogResult.Cancel,
        };

        saveButton.Click += SaveButton_Click;
        copyButton.Click += CopyButton_Click;
        closeButton.Click += (s, e) => this.Close();

        FlowLayoutPanel buttonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            Padding = new Padding(5),
        };

        buttonPanel.Controls.AddRange([saveButton, copyButton, closeButton]);

        mainPanel.Controls.Add(textBox);
        mainPanel.Controls.Add(buttonPanel);

        this.Controls.Add(mainPanel);

        this.CancelButton = closeButton;
        this.Text = document.Title;
        textBox.Text = document.Content.ReplaceLineEndings();
        textBox.Select(0, 0);
    }

    private void CopyButton_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(textBox.Text))
        {
            Clipboard.SetText(textBox.Text);
        }
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        using (SaveFileDialog saveDialog = new SaveFileDialog
        {
            FileName = document.Title,
            CheckPathExists = true,
            CheckWriteAccess = true,
            OkRequiresInteraction = true,
            OverwritePrompt = true,
            ValidateNames = true,
        })

        {
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(saveDialog.FileName, document.Content);
                    MessageBox.Show(
                        $"File successfully saved at:\n{saveDialog.FileName}",
                        "Save Successful",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error saving file: {ex.Message}",
                        "Save Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
    }
}

public class DocumentPanel : FlowLayoutPanel
{
    public DocumentPanel()
    {
        this.FlowDirection = FlowDirection.LeftToRight;
        this.AutoSize = true;
        this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        this.WrapContents = false;
        this.Padding = new Padding(10);
        this.Visible = false;
        this.Enabled = false;
    }


    public void Reset()
    {
        this.Visible = false;
        this.Enabled = false;
        while (this.Controls.Count > 0)
        {
            var control = this.Controls[0];
            this.Controls.Remove(control);
            control.Dispose();
        }
    }

    public void LoadDocuments(Document[] documents)
    {
        Reset();
        foreach (Document document in documents)
        {
            Button button = new Button
            {
                AutoSize = true,
                Text = $"View {document.Title}",
                Margin = new Padding(4, 0, 4, 0),
            };

            button.Click += (s, e) =>
            {
                using (TextViewerDialog dialog = new TextViewerDialog(document))
                {
                    dialog.ShowDialog();
                }
            };
            this.Controls.Add(button);
        }
        this.Enabled = true;
        this.Visible = true;
    }
}
