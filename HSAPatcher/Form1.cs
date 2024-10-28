using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace HSAPatcher;
public class MainForm : Form
{
    private UpdateClient updateClient = null!;
    private ReleaseChannel[] releaseChannels = new ReleaseChannel[5];
    private TextBox directoryBox = null!;
    private ListView channelsList = null!;
    private Button btnStart = null!;
    private FlowLayoutPanel mainPanel = null!;
    private OperationPanel operationPanel = null!;

    public MainForm()
    {
        // ensure form is disabled until it's done loading, reenable after it loads.
        this.Enabled = false;
        this.Visible = false;
        InitializeComponent();
        this.Load += onFormLoad;
    }

    private void InitializeComponent()
    {
        this.Text = "HSAPatcher";
        this.AutoSize = true;
        this.StartPosition = FormStartPosition.CenterScreen;
        mainPanel = new FlowLayoutPanel();
        mainPanel.Dock = DockStyle.Fill;
        mainPanel.FlowDirection = FlowDirection.TopDown;
        mainPanel.Padding = new Padding(10);
        mainPanel.AutoSize = true;
        FlowLayoutPanel directoryPanel = new FlowLayoutPanel();
        directoryPanel.AutoSize = true;
        directoryPanel.FlowDirection = FlowDirection.LeftToRight;

        Label lblPath = new Label();
        lblPath.Text = "Select Folder:";
        lblPath.AutoSize = true;

        directoryBox = new TextBox();
        directoryBox.ReadOnly = true;
        directoryBox.AutoSize = true;
        directoryBox.Width = 200;

        Button btnBrowse = new Button();
        btnBrowse.Text = "Change:";
        btnBrowse.AutoSize = true;
        btnBrowse.Click += onBrowse;

        directoryPanel.Controls.Add(lblPath);
        directoryPanel.Controls.Add(directoryBox);
        directoryPanel.Controls.Add(btnBrowse);

        mainPanel.Controls.Add(directoryPanel);


        FlowLayoutPanel channelPanel = new FlowLayoutPanel();
        channelPanel.FlowDirection = FlowDirection.LeftToRight;
        channelPanel.AutoSize = true;

        Label lblSelect = new Label()
        {
            Text = "Select Channel:",
            AutoSize = true,
        };


        channelsList = new ListView()
        {
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            MultiSelect = false,
            Size = new System.Drawing.Size(400, 400),
        };
        channelsList.Columns.Add("Channel:", -2);
        channelsList.Columns.Add("Description:", -2);
        channelsList.Columns.Add("Latest Version:", -2);
        channelsList.Columns.Add("Released at:", -2);

        channelPanel.Controls.Add(lblSelect);
        channelPanel.Controls.Add(channelsList);
        mainPanel.Controls.Add(channelPanel);

        btnStart = new Button()
        {
            Text = "Start",
            AutoSize = true,
        };
        btnStart.Click += BtnStart_Click;
        mainPanel.Controls.Add(btnStart);
        operationPanel = new OperationPanel();

        mainPanel.Controls.Add(operationPanel);
        this.Controls.Add(mainPanel);

    }

    private async void onFormLoad(object? sender, EventArgs e)
    {
        updateClient = new UpdateClient();
        releaseChannels = await updateClient.GetReleaseChannels();
        if (releaseChannels == null)
        {
            MessageBox.Show("Unable to communicate with the update server", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
            return;
        }

        foreach (ReleaseChannel? channel in releaseChannels)
        {
            ListViewItem item = channelsList.Items.Add(channel.Name);
            item.SubItems.Add(channel.Description);
            item.SubItems.Add(channel.Latest_Release.Accessibility_Version.ToString());
            DateTimeOffset? uploadTime = channel.Latest_Release.Upload_Time;
            if (uploadTime == null) continue;
            item.SubItems.Add(uploadTime.Value.Date.ToString("d"));
        }
        channelsList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

        string? path = Patcher.LocateHearthstone();
        if (!string.IsNullOrWhiteSpace(path))
        {
            directoryBox.Text = path;
        }
        else
        {
            MessageBox.Show(this, "Could not automatically locate where Hearthstone is installed to apply the patch. On the next screen, please press on the 'change' button and pick where you've installed Hearthstone by choosing the Hearthstone folder.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        channelsList.Items[0].Selected = true;

        this.Enabled = true;
        this.Visible = true;
    }

    private void onBrowse(object? sender, EventArgs e)
    {
        using (FolderBrowserDialog dialog = new FolderBrowserDialog())
        {
            dialog.Description = "Select the folder where Hearthstone is installed:";
            dialog.ShowNewFolderButton = false;
            dialog.OkRequiresInteraction = true;
            dialog.UseDescriptionForTitle = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                directoryBox.Text = dialog.SelectedPath;
            }
        }
    }
    private async void BtnStart_Click(object? sender, EventArgs e)
    {
        string directory = directoryBox.Text;
        if (!Patcher.IsHsDirectory(directory))
        {
            MessageBox.Show(this, "The provided path is not a valid directory to a hearthstone installation.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (channelsList.SelectedIndices.Count < 1)
        {
            MessageBox.Show(this, "No release channel is  selected. Please select a channel.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        ReleaseChannel channel = releaseChannels[channelsList.SelectedIndices[0]];

        // Disable Start button to prevent multiple clicks
        btnStart.Enabled = false;
        operationPanel.AddHistoryItem($"Selected HearthstoneDirectory: {directory}");
        operationPanel.AddHistoryItem($"Selected channel: {channel.Name}, at {channel.Latest_Release.Url}");
        operationPanel.LabelText = "Downloading...";
        Downloader downloader = new Downloader(channel.Latest_Release.Url);

        downloader.ProgressChanged += (sender, progress) =>
        {
            operationPanel.UpdateProgress(progress, "Downloading...");
        };
        using Stream stream = await downloader.Download();
        operationPanel.LabelText = "Patching...";
        await Task.Yield();
        Patcher.UnpackAndPatch(stream, directory);

        operationPanel.LabelText = "Done.";
        // Re-enable Start button
        btnStart.Enabled = true;

        MessageBox.Show("Hearthstone patched!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
