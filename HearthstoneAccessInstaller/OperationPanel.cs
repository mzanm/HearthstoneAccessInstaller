using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.Automation;
namespace HearthstoneAccessInstaller;
public class OperationPanel : FlowLayoutPanel
{
    private ProgressBar progressBar;
    private Label label;
    public ListBox listBox;
    private List<string> historyItems;

    public OperationPanel()
    {
        historyItems = new List<string>();

        this.FlowDirection = FlowDirection.LeftToRight;
        this.AutoSize = true;
        this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        this.Padding = new Padding(10);
        this.WrapContents = false;

        progressBar = new ProgressBar();

        label = new Label
        {
            Text = "Operation Status",
            AutoSize = true
        };

        listBox = new ListBox
        {
            IntegralHeight = true,
        };


        this.Controls.Add(progressBar);
        this.Controls.Add(label);
        this.Controls.Add(listBox);

        UpdateVisibility();
    }

    public void AddHistoryItem(string item)
    {
        historyItems.Add(item);
        UpdateVisibility();
    }

    public void ClearHistory()
    {
        historyItems.Clear();
        UpdateVisibility();
    }

    public ProgressBar ProgressBar
    {
        get { return progressBar; }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string LabelText
    {
        get { return label.Text; }
        set
        {
            if (label.Text != value)
            {
                label.Text = value;
                label.AccessibilityObject.RaiseAutomationNotification(AutomationNotificationKind.Other, AutomationNotificationProcessing.CurrentThenMostRecent, value);
                AddHistoryItem(value);
            }
        }
    }

    public List<string> HistoryItems
    {
        get { return historyItems; }
    }

    public void UpdateProgress(int progressValue, string? text = null)
    {
        if (text != null)
        {
            LabelText = text;
        }
        if (progressValue != progressBar.Value)
        {
            progressBar.Value = progressValue;
        }

    }

    private void UpdateVisibility()
    {
        listBox.BeginUpdate();
        bool shouldBeVisible = historyItems.Count > 0;
        if (this.Visible != shouldBeVisible)
        {
            this.Visible = shouldBeVisible;
        }
        listBox.Items.Clear();
        listBox.Items.AddRange(historyItems.ToArray());
        listBox.EndUpdate();
    }
}
