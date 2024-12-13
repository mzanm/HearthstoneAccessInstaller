using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace HearthstoneAccessInstaller;
public class MainForm : Form
{
    private UpdateClient updateClient = null!;
    private ReleaseChannel[] releaseChannels = new ReleaseChannel[5];
    private TextBox directoryBox = null!;
    private ListView channelsList = null!;
    private Button btnStart = null!;
    private FlowLayoutPanel mainPanel = null!;
    private OperationPanel operationPanel = null!;
    private DocumentPanel documentPanel = null!;

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
        this.Text = "Hearthstone Access Installer - V 1.0";
        this.AutoSize = true;
        this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        this.StartPosition = FormStartPosition.CenterScreen;
        mainPanel = new FlowLayoutPanel();
        mainPanel.Dock = DockStyle.Fill;
        mainPanel.FlowDirection = FlowDirection.TopDown;
        mainPanel.Padding = new Padding(10);
        mainPanel.AutoSize = true;
        mainPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        mainPanel.WrapContents = false;
        FlowLayoutPanel directoryPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            Padding = new Padding(10),
        };

        Label lblPath = new Label();
        lblPath.Text = "Select Folder:";
        lblPath.AutoSize = true;

        directoryBox = new TextBox();
        directoryBox.ReadOnly = true;
        directoryBox.AutoSize = true;
        directoryBox.Width = 200;

        Button btnBrowse = new Button
        {
            Text = "Change:",
            AutoSize = true
        };
        btnBrowse.Click += onBrowse;

        directoryPanel.Controls.Add(lblPath);
        directoryPanel.Controls.Add(directoryBox);
        directoryPanel.Controls.Add(btnBrowse);

        mainPanel.Controls.Add(directoryPanel);


        FlowLayoutPanel channelPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            Padding = new Padding(10),
        };

        Label lblSelect = new Label()
        {
            Text = "Select Channel:",
            AutoSize = true,
        };


        channelsList = new ListView()
        {
            View = View.Details,
            FullRowSelect = true,
            HeaderStyle = ColumnHeaderStyle.Nonclickable,
            GridLines = true,
            MultiSelect = false,
            AccessibleRole = AccessibleRole.List,
        };
        channelsList.Columns.Add("Channel:");
        channelsList.Columns.Add("Description:");
        channelsList.Columns.Add("Latest Version:");
        channelsList.Columns.Add("Released at:");
        channelsList.Columns.Add("Changes:");

        channelPanel.Controls.Add(lblSelect);
        channelPanel.Controls.Add(channelsList);
        mainPanel.Controls.Add(channelPanel);

        btnStart = new Button()
        {
            Text = "Start",
            AutoSize = true,
            Padding = new Padding(10),
        };
        btnStart.Click += BtnStart_Click;
        mainPanel.Controls.Add(btnStart);
        operationPanel = new OperationPanel();
        documentPanel = new DocumentPanel();
        mainPanel.Controls.Add(operationPanel);
        mainPanel.Controls.Add(documentPanel);
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

        channelsList.BeginUpdate();
        foreach (ReleaseChannel? channel in releaseChannels)
        {
            ListViewItem item = channelsList.Items.Add(channel.Name);
            item.SubItems.Add(channel.Description);
            item.SubItems.Add(channel.Latest_Release.Accessibility_Version.ToString());
            DateTimeOffset? uploadTime = channel.Latest_Release.Upload_Time;
            if (uploadTime == null)
            {
                item.SubItems.Add("Unknown");
            }
            else
            {
                DateTimeOffset offset = uploadTime.Value;
                string relativeString = Utils.GetElapsedTime(offset);
                item.SubItems.Add($"{offset.ToLocalTime().ToString("g")}, {relativeString}.");
            }
            string? changelog = channel.Latest_Release.Changelog;
            if (string.IsNullOrWhiteSpace(changelog))
            {
                item.SubItems.Add("Unknown");
            }
            else
            {
                item.SubItems.Add(changelog);
            }


        }
        channelsList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        int totalWidth = 0;
        foreach (ColumnHeader column in channelsList.Columns)
        {
            totalWidth += column.Width;
        }
        totalWidth += SystemInformation.VerticalScrollBarWidth;
        totalWidth += 20;
        channelsList.Width = totalWidth;
        channelsList.EndUpdate();
        channelsList.PerformLayout();
        this.PerformLayout();

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
        documentPanel.Reset();
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
        Document[] documents = Patcher.UnpackAndPatch(stream, directory);
        documentPanel.LoadDocuments(documents);

        operationPanel.UpdateProgress(100, "Patching Done. You can view the readme and changelog now.");
        // Re-enable Start button
        btnStart.Enabled = true;
        operationPanel.listBox.Focus();
    }

}
