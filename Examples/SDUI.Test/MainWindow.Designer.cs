using System.Windows.Forms;
using SDUI;
using SDUI.Controls;

namespace SDUI.Test;

public partial class MainWindow
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
        contextMenuStrip1 = new Controls.ContextMenuStrip();
        toolStripMenuItem1 = new ToolStripMenuItem();
        toolStripMenuItem2 = new ToolStripMenuItem();
        toolStripMenuItem3 = new ToolStripMenuItem();
        toolStripMenuItem8 = new ToolStripMenuItem();
        toolStripMenuItem9 = new ToolStripMenuItem();
        toolStripSeparator7 = new ToolStripSeparator();
        toolStripMenuItem10 = new ToolStripMenuItem();
        toolStripMenuItem11 = new ToolStripMenuItem();
        toolStripMenuItem12 = new ToolStripMenuItem();
        toolStripMenuItem13 = new ToolStripMenuItem();
        toolStripMenuItem14 = new ToolStripMenuItem();
        toolStripMenuItem4 = new ToolStripMenuItem();
        toolStripMenuItem5 = new ToolStripMenuItem();
        toolStripMenuItem6 = new ToolStripMenuItem();
        toolStripMenuItem7 = new ToolStripMenuItem();
        toolStripSeparator6 = new ToolStripSeparator();
        menuStrip1 = new Controls.MenuStrip();
        fileToolStripMenuItem = new ToolStripMenuItem();
        newToolStripMenuItem = new ToolStripMenuItem();
        openToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator = new ToolStripSeparator();
        saveToolStripMenuItem = new ToolStripMenuItem();
        saveAsToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator1 = new ToolStripSeparator();
        printToolStripMenuItem = new ToolStripMenuItem();
        printPreviewToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator2 = new ToolStripSeparator();
        exitToolStripMenuItem = new ToolStripMenuItem();
        editToolStripMenuItem = new ToolStripMenuItem();
        undoToolStripMenuItem = new ToolStripMenuItem();
        redoToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator3 = new ToolStripSeparator();
        cutToolStripMenuItem = new ToolStripMenuItem();
        copyToolStripMenuItem = new ToolStripMenuItem();
        pasteToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator4 = new ToolStripSeparator();
        selectAllToolStripMenuItem = new ToolStripMenuItem();
        toolsToolStripMenuItem = new ToolStripMenuItem();
        customizeToolStripMenuItem = new ToolStripMenuItem();
        optionsToolStripMenuItem = new ToolStripMenuItem();
        helpToolStripMenuItem = new ToolStripMenuItem();
        contentsToolStripMenuItem = new ToolStripMenuItem();
        indexToolStripMenuItem = new ToolStripMenuItem();
        searchToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator5 = new ToolStripSeparator();
        aboutToolStripMenuItem = new ToolStripMenuItem();
        toolTip1 = new ToolTip(components);
        windowPageControl = new WindowPageControl();
        separator1 = new Separator();
        panel8 = new Controls.Panel();
        toggleButton1 = new ToggleButton();
        shapeProgressBar2 = new ShapeProgressBar();
        shapeProgressBar1 = new ShapeProgressBar();
        chatBubble2 = new ChatBubble();
        chatBubble1 = new ChatBubble();
        numUpDown1 = new NumUpDown();
        checkBox4 = new Controls.CheckBox();
        checkBox3 = new Controls.CheckBox();
        label3 = new Controls.Label();
        progressBar5 = new Controls.ProgressBar();
        contextMenuStrip1.SuspendLayout();
        menuStrip1.SuspendLayout();
        panel8.SuspendLayout();
        SuspendLayout();
        // 
        // contextMenuStrip1
        // 
        contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
        contextMenuStrip1.Items.AddRange(new ToolStripItem[] { toolStripMenuItem1, toolStripMenuItem2, toolStripMenuItem3, toolStripMenuItem4, toolStripMenuItem5, toolStripMenuItem6, toolStripMenuItem7, toolStripSeparator6 });
        contextMenuStrip1.Name = "contextMenuStrip1";
        contextMenuStrip1.Size = new System.Drawing.Size(212, 178);
        // 
        // toolStripMenuItem1
        // 
        toolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        toolStripMenuItem1.Name = "toolStripMenuItem1";
        toolStripMenuItem1.Size = new System.Drawing.Size(211, 24);
        toolStripMenuItem1.Text = "toolStripMenuItem1";
        // 
        // toolStripMenuItem2
        // 
        toolStripMenuItem2.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        toolStripMenuItem2.Name = "toolStripMenuItem2";
        toolStripMenuItem2.Size = new System.Drawing.Size(211, 24);
        toolStripMenuItem2.Text = "toolStripMenuItem2";
        // 
        // toolStripMenuItem3
        // 
        toolStripMenuItem3.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem8, toolStripMenuItem9, toolStripSeparator7, toolStripMenuItem10 });
        toolStripMenuItem3.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        toolStripMenuItem3.Name = "toolStripMenuItem3";
        toolStripMenuItem3.Size = new System.Drawing.Size(211, 24);
        toolStripMenuItem3.Text = "toolStripMenuItem3";
        // 
        // toolStripMenuItem8
        // 
        toolStripMenuItem8.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        toolStripMenuItem8.Name = "toolStripMenuItem8";
        toolStripMenuItem8.Size = new System.Drawing.Size(233, 26);
        toolStripMenuItem8.Text = "toolStripMenuItem8";
        // 
        // toolStripMenuItem9
        // 
        toolStripMenuItem9.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        toolStripMenuItem9.Name = "toolStripMenuItem9";
        toolStripMenuItem9.Size = new System.Drawing.Size(233, 26);
        toolStripMenuItem9.Text = "toolStripMenuItem9";
        // 
        // toolStripSeparator7
        // 
        toolStripSeparator7.Name = "toolStripSeparator7";
        toolStripSeparator7.Size = new System.Drawing.Size(230, 6);
        // 
        // toolStripMenuItem10
        // 
        toolStripMenuItem10.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem11, toolStripMenuItem12, toolStripMenuItem13, toolStripMenuItem14 });
        toolStripMenuItem10.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        toolStripMenuItem10.Name = "toolStripMenuItem10";
        toolStripMenuItem10.Size = new System.Drawing.Size(233, 26);
        toolStripMenuItem10.Text = "toolStripMenuItem10";
        // 
        // toolStripMenuItem11
        // 
        toolStripMenuItem11.Name = "toolStripMenuItem11";
        toolStripMenuItem11.Size = new System.Drawing.Size(233, 26);
        toolStripMenuItem11.Text = "toolStripMenuItem11";
        // 
        // toolStripMenuItem12
        // 
        toolStripMenuItem12.Name = "toolStripMenuItem12";
        toolStripMenuItem12.Size = new System.Drawing.Size(233, 26);
        toolStripMenuItem12.Text = "toolStripMenuItem12";
        // 
        // toolStripMenuItem13
        // 
        toolStripMenuItem13.Name = "toolStripMenuItem13";
        toolStripMenuItem13.Size = new System.Drawing.Size(233, 26);
        toolStripMenuItem13.Text = "toolStripMenuItem13";
        // 
        // toolStripMenuItem14
        // 
        toolStripMenuItem14.Name = "toolStripMenuItem14";
        toolStripMenuItem14.Size = new System.Drawing.Size(233, 26);
        toolStripMenuItem14.Text = "toolStripMenuItem14";
        // 
        // toolStripMenuItem4
        // 
        toolStripMenuItem4.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        toolStripMenuItem4.Name = "toolStripMenuItem4";
        toolStripMenuItem4.Size = new System.Drawing.Size(211, 24);
        toolStripMenuItem4.Text = "toolStripMenuItem4";
        // 
        // toolStripMenuItem5
        // 
        toolStripMenuItem5.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        toolStripMenuItem5.Name = "toolStripMenuItem5";
        toolStripMenuItem5.Size = new System.Drawing.Size(211, 24);
        toolStripMenuItem5.Text = "toolStripMenuItem5";
        // 
        // toolStripMenuItem6
        // 
        toolStripMenuItem6.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        toolStripMenuItem6.Name = "toolStripMenuItem6";
        toolStripMenuItem6.Size = new System.Drawing.Size(211, 24);
        toolStripMenuItem6.Text = "toolStripMenuItem6";
        // 
        // toolStripMenuItem7
        // 
        toolStripMenuItem7.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        toolStripMenuItem7.Name = "toolStripMenuItem7";
        toolStripMenuItem7.Size = new System.Drawing.Size(211, 24);
        toolStripMenuItem7.Text = "toolStripMenuItem7";
        // 
        // toolStripSeparator6
        // 
        toolStripSeparator6.Name = "toolStripSeparator6";
        toolStripSeparator6.Size = new System.Drawing.Size(208, 6);
        // 
        // menuStrip1
        // 
        menuStrip1.BackColor = System.Drawing.Color.Transparent;
        menuStrip1.Dock = DockStyle.Bottom;
        menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
        menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, toolsToolStripMenuItem, helpToolStripMenuItem });
        menuStrip1.Location = new System.Drawing.Point(1, 739);
        menuStrip1.Name = "menuStrip1";
        menuStrip1.Size = new System.Drawing.Size(1239, 28);
        menuStrip1.Stretch = false;
        menuStrip1.TabIndex = 1;
        menuStrip1.Text = "menuStrip1";
        // 
        // fileToolStripMenuItem
        // 
        fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newToolStripMenuItem, openToolStripMenuItem, toolStripSeparator, saveToolStripMenuItem, saveAsToolStripMenuItem, toolStripSeparator1, printToolStripMenuItem, printPreviewToolStripMenuItem, toolStripSeparator2, exitToolStripMenuItem });
        fileToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        fileToolStripMenuItem.Name = "fileToolStripMenuItem";
        fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
        fileToolStripMenuItem.Text = "&File";
        // 
        // newToolStripMenuItem
        // 
        newToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("newToolStripMenuItem.Image");
        newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
        newToolStripMenuItem.Name = "newToolStripMenuItem";
        newToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
        newToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
        newToolStripMenuItem.Text = "&New";
        // 
        // openToolStripMenuItem
        // 
        openToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("openToolStripMenuItem.Image");
        openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
        openToolStripMenuItem.Name = "openToolStripMenuItem";
        openToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
        openToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
        openToolStripMenuItem.Text = "&Open";
        // 
        // toolStripSeparator
        // 
        toolStripSeparator.Name = "toolStripSeparator";
        toolStripSeparator.Size = new System.Drawing.Size(178, 6);
        // 
        // saveToolStripMenuItem
        // 
        saveToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("saveToolStripMenuItem.Image");
        saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
        saveToolStripMenuItem.Name = "saveToolStripMenuItem";
        saveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
        saveToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
        saveToolStripMenuItem.Text = "&Save";
        // 
        // saveAsToolStripMenuItem
        // 
        saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
        saveAsToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
        saveAsToolStripMenuItem.Text = "Save &As";
        // 
        // toolStripSeparator1
        // 
        toolStripSeparator1.Name = "toolStripSeparator1";
        toolStripSeparator1.Size = new System.Drawing.Size(178, 6);
        // 
        // printToolStripMenuItem
        // 
        printToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("printToolStripMenuItem.Image");
        printToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
        printToolStripMenuItem.Name = "printToolStripMenuItem";
        printToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.P;
        printToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
        printToolStripMenuItem.Text = "&Print";
        // 
        // printPreviewToolStripMenuItem
        // 
        printPreviewToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("printPreviewToolStripMenuItem.Image");
        printPreviewToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
        printPreviewToolStripMenuItem.Name = "printPreviewToolStripMenuItem";
        printPreviewToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
        printPreviewToolStripMenuItem.Text = "Print Pre&view";
        // 
        // toolStripSeparator2
        // 
        toolStripSeparator2.Name = "toolStripSeparator2";
        toolStripSeparator2.Size = new System.Drawing.Size(178, 6);
        // 
        // exitToolStripMenuItem
        // 
        exitToolStripMenuItem.Name = "exitToolStripMenuItem";
        exitToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
        exitToolStripMenuItem.Text = "E&xit";
        // 
        // editToolStripMenuItem
        // 
        editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { undoToolStripMenuItem, redoToolStripMenuItem, toolStripSeparator3, cutToolStripMenuItem, copyToolStripMenuItem, pasteToolStripMenuItem, toolStripSeparator4, selectAllToolStripMenuItem });
        editToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        editToolStripMenuItem.Name = "editToolStripMenuItem";
        editToolStripMenuItem.Size = new System.Drawing.Size(49, 24);
        editToolStripMenuItem.Text = "&Edit";
        // 
        // undoToolStripMenuItem
        // 
        undoToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        undoToolStripMenuItem.Name = "undoToolStripMenuItem";
        undoToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Z;
        undoToolStripMenuItem.Size = new System.Drawing.Size(179, 26);
        undoToolStripMenuItem.Text = "&Undo";
        // 
        // redoToolStripMenuItem
        // 
        redoToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        redoToolStripMenuItem.Name = "redoToolStripMenuItem";
        redoToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Y;
        redoToolStripMenuItem.Size = new System.Drawing.Size(179, 26);
        redoToolStripMenuItem.Text = "&Redo";
        // 
        // toolStripSeparator3
        // 
        toolStripSeparator3.Name = "toolStripSeparator3";
        toolStripSeparator3.Size = new System.Drawing.Size(176, 6);
        // 
        // cutToolStripMenuItem
        // 
        cutToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        cutToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("cutToolStripMenuItem.Image");
        cutToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
        cutToolStripMenuItem.Name = "cutToolStripMenuItem";
        cutToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.X;
        cutToolStripMenuItem.Size = new System.Drawing.Size(179, 26);
        cutToolStripMenuItem.Text = "Cu&t";
        // 
        // copyToolStripMenuItem
        // 
        copyToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        copyToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("copyToolStripMenuItem.Image");
        copyToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
        copyToolStripMenuItem.Name = "copyToolStripMenuItem";
        copyToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.C;
        copyToolStripMenuItem.Size = new System.Drawing.Size(179, 26);
        copyToolStripMenuItem.Text = "&Copy";
        // 
        // pasteToolStripMenuItem
        // 
        pasteToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        pasteToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("pasteToolStripMenuItem.Image");
        pasteToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
        pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
        pasteToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.V;
        pasteToolStripMenuItem.Size = new System.Drawing.Size(179, 26);
        pasteToolStripMenuItem.Text = "&Paste";
        // 
        // toolStripSeparator4
        // 
        toolStripSeparator4.Name = "toolStripSeparator4";
        toolStripSeparator4.Size = new System.Drawing.Size(176, 6);
        // 
        // selectAllToolStripMenuItem
        // 
        selectAllToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
        selectAllToolStripMenuItem.Size = new System.Drawing.Size(179, 26);
        selectAllToolStripMenuItem.Text = "Select &All";
        // 
        // toolsToolStripMenuItem
        // 
        toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { customizeToolStripMenuItem, optionsToolStripMenuItem });
        toolsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
        toolsToolStripMenuItem.Size = new System.Drawing.Size(58, 24);
        toolsToolStripMenuItem.Text = "&Tools";
        // 
        // customizeToolStripMenuItem
        // 
        customizeToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        customizeToolStripMenuItem.Name = "customizeToolStripMenuItem";
        customizeToolStripMenuItem.Size = new System.Drawing.Size(161, 26);
        customizeToolStripMenuItem.Text = "&Customize";
        // 
        // optionsToolStripMenuItem
        // 
        optionsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
        optionsToolStripMenuItem.Size = new System.Drawing.Size(161, 26);
        optionsToolStripMenuItem.Text = "&Options";
        // 
        // helpToolStripMenuItem
        // 
        helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { contentsToolStripMenuItem, indexToolStripMenuItem, searchToolStripMenuItem, toolStripSeparator5, aboutToolStripMenuItem });
        helpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        helpToolStripMenuItem.Name = "helpToolStripMenuItem";
        helpToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
        helpToolStripMenuItem.Text = "&Help";
        // 
        // contentsToolStripMenuItem
        // 
        contentsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        contentsToolStripMenuItem.Name = "contentsToolStripMenuItem";
        contentsToolStripMenuItem.Size = new System.Drawing.Size(150, 26);
        contentsToolStripMenuItem.Text = "&Contents";
        // 
        // indexToolStripMenuItem
        // 
        indexToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        indexToolStripMenuItem.Name = "indexToolStripMenuItem";
        indexToolStripMenuItem.Size = new System.Drawing.Size(150, 26);
        indexToolStripMenuItem.Text = "&Index";
        // 
        // searchToolStripMenuItem
        // 
        searchToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        searchToolStripMenuItem.Name = "searchToolStripMenuItem";
        searchToolStripMenuItem.Size = new System.Drawing.Size(150, 26);
        searchToolStripMenuItem.Text = "&Search";
        // 
        // toolStripSeparator5
        // 
        toolStripSeparator5.Name = "toolStripSeparator5";
        toolStripSeparator5.Size = new System.Drawing.Size(147, 6);
        // 
        // aboutToolStripMenuItem
        // 
        aboutToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
        aboutToolStripMenuItem.Size = new System.Drawing.Size(150, 26);
        aboutToolStripMenuItem.Text = "&About...";
        // 
        // toolTip1
        // 
        toolTip1.BackColor = System.Drawing.Color.Black;
        toolTip1.ForeColor = System.Drawing.Color.White;
        toolTip1.ShowAlways = true;
        toolTip1.ToolTipIcon = ToolTipIcon.Info;
        toolTip1.ToolTipTitle = "Heuuuu";
        // 
        // windowPageControl
        // 
        windowPageControl.BackColor = System.Drawing.Color.Transparent;
        windowPageControl.Dock = DockStyle.Fill;
        windowPageControl.Location = new System.Drawing.Point(1, 46);
        windowPageControl.Margin = new Padding(2);
        windowPageControl.Name = "windowPageControl";
        windowPageControl.SelectedIndex = -1;
        windowPageControl.Size = new System.Drawing.Size(1007, 693);
        windowPageControl.TabIndex = 2;
        // 
        // separator1
        // 
        separator1.BackColor = System.Drawing.Color.Transparent;
        separator1.Dock = DockStyle.Top;
        separator1.IsVertical = false;
        separator1.Location = new System.Drawing.Point(1, 40);
        separator1.Margin = new Padding(2);
        separator1.Name = "separator1";
        separator1.Size = new System.Drawing.Size(1239, 6);
        separator1.TabIndex = 3;
        // 
        // panel8
        // 
        panel8.BackColor = System.Drawing.Color.Transparent;
        panel8.Border = new Padding(1, 0, 0, 0);
        panel8.BorderColor = System.Drawing.Color.Transparent;
        panel8.Controls.Add(toggleButton1);
        panel8.Controls.Add(shapeProgressBar2);
        panel8.Controls.Add(shapeProgressBar1);
        panel8.Controls.Add(chatBubble2);
        panel8.Controls.Add(chatBubble1);
        panel8.Controls.Add(numUpDown1);
        panel8.Controls.Add(checkBox4);
        panel8.Controls.Add(checkBox3);
        panel8.Controls.Add(label3);
        panel8.Controls.Add(progressBar5);
        panel8.Dock = DockStyle.Right;
        panel8.Location = new System.Drawing.Point(1008, 46);
        panel8.Margin = new Padding(2);
        panel8.Name = "panel8";
        panel8.Radius = 0;
        panel8.ShadowDepth = 0F;
        panel8.Size = new System.Drawing.Size(232, 693);
        panel8.TabIndex = 58;
        // 
        // toggleButton1
        // 
        toggleButton1.AutoSize = true;
        toggleButton1.BackColor = System.Drawing.Color.Transparent;
        toggleButton1.Location = new System.Drawing.Point(106, 380);
        toggleButton1.Margin = new Padding(2);
        toggleButton1.MinimumSize = new System.Drawing.Size(46, 22);
        toggleButton1.Name = "toggleButton1";
        toggleButton1.Size = new System.Drawing.Size(46, 22);
        toggleButton1.TabIndex = 31;
        toggleButton1.UseVisualStyleBackColor = false;
        // 
        // shapeProgressBar2
        // 
        shapeProgressBar2.BackColor = System.Drawing.Color.Transparent;
        shapeProgressBar2.DrawHatch = true;
        shapeProgressBar2.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        shapeProgressBar2.Gradient = new System.Drawing.Color[]
{
    System.Drawing.Color.Maroon,
    System.Drawing.Color.Red
};
        shapeProgressBar2.HatchType = System.Drawing.Drawing2D.HatchStyle.ZigZag;
        shapeProgressBar2.Location = new System.Drawing.Point(118, 452);
        shapeProgressBar2.Margin = new Padding(2);
        shapeProgressBar2.Maximum = 100L;
        shapeProgressBar2.Name = "shapeProgressBar2";
        shapeProgressBar2.Size = new System.Drawing.Size(64, 64);
        shapeProgressBar2.TabIndex = 30;
        shapeProgressBar2.Text = "shapeProgressBar1";
        shapeProgressBar2.Value = 52L;
        shapeProgressBar2.Weight = 12F;
        // 
        // shapeProgressBar1
        // 
        shapeProgressBar1.BackColor = System.Drawing.Color.Transparent;
        shapeProgressBar1.DrawHatch = false;
        shapeProgressBar1.Font = new System.Drawing.Font("Impact", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        shapeProgressBar1.ForeColor = System.Drawing.Color.Silver;
        shapeProgressBar1.Gradient = new System.Drawing.Color[]
{
    System.Drawing.Color.Maroon,
    System.Drawing.Color.Red
};
        shapeProgressBar1.HatchType = System.Drawing.Drawing2D.HatchStyle.ZigZag;
        shapeProgressBar1.Location = new System.Drawing.Point(16, 452);
        shapeProgressBar1.Margin = new Padding(2);
        shapeProgressBar1.Maximum = 100L;
        shapeProgressBar1.Name = "shapeProgressBar1";
        shapeProgressBar1.Size = new System.Drawing.Size(48, 48);
        shapeProgressBar1.TabIndex = 30;
        shapeProgressBar1.Text = "shapeProgressBar1";
        shapeProgressBar1.Value = 52L;
        shapeProgressBar1.Weight = 8F;
        // 
        // chatBubble2
        // 
        chatBubble2.ArrowPosition = AnchorStyles.Right;
        chatBubble2.BackColor = System.Drawing.Color.Transparent;
        chatBubble2.BubbleColor = System.Drawing.Color.FromArgb(192, 192, 255);
        chatBubble2.DrawBubbleArrow = true;
        chatBubble2.ForeColor = System.Drawing.Color.FromArgb(52, 52, 52);
        chatBubble2.Location = new System.Drawing.Point(16, 326);
        chatBubble2.Margin = new Padding(2);
        chatBubble2.Name = "chatBubble2";
        chatBubble2.Size = new System.Drawing.Size(175, 38);
        chatBubble2.TabIndex = 28;
        chatBubble2.Text = "Hello Bob i am fine, Thanks";
        // 
        // chatBubble1
        // 
        chatBubble1.ArrowPosition = AnchorStyles.Left;
        chatBubble1.BackColor = System.Drawing.Color.Transparent;
        chatBubble1.BubbleColor = System.Drawing.Color.FromArgb(217, 217, 217);
        chatBubble1.DrawBubbleArrow = true;
        chatBubble1.ForeColor = System.Drawing.Color.FromArgb(52, 52, 52);
        chatBubble1.Location = new System.Drawing.Point(42, 279);
        chatBubble1.Margin = new Padding(2);
        chatBubble1.Name = "chatBubble1";
        chatBubble1.Size = new System.Drawing.Size(175, 38);
        chatBubble1.TabIndex = 28;
        chatBubble1.Text = "Hello Jean How are you?";
        // 
        // numUpDown1
        // 
        numUpDown1.BackColor = System.Drawing.Color.Transparent;
        numUpDown1.Font = new System.Drawing.Font("Segoe UI", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        numUpDown1.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        numUpDown1.Location = new System.Drawing.Point(16, 379);
        numUpDown1.Margin = new Padding(2);
        numUpDown1.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
        numUpDown1.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
        numUpDown1.MinimumSize = new System.Drawing.Size(80, 25);
        numUpDown1.Name = "numUpDown1";
        numUpDown1.Size = new System.Drawing.Size(80, 25);
        numUpDown1.TabIndex = 28;
        numUpDown1.Value = new decimal(new int[] { 0, 0, 0, 0 });
        // 
        // checkBox4
        // 
        checkBox4.AutoSize = true;
        checkBox4.BackColor = System.Drawing.Color.Transparent;
        checkBox4.Checked = true;
        checkBox4.CheckState = CheckState.Checked;
        checkBox4.Depth = 0;
        checkBox4.Location = new System.Drawing.Point(16, 238);
        checkBox4.Margin = new Padding(0);
        checkBox4.MouseLocation = new System.Drawing.Point(-1, -1);
        checkBox4.Name = "checkBox4";
        checkBox4.Ripple = true;
        checkBox4.Size = new System.Drawing.Size(105, 30);
        checkBox4.TabIndex = 13;
        checkBox4.Text = "checkBox1";
        checkBox4.UseVisualStyleBackColor = false;
        // 
        // checkBox3
        // 
        checkBox3.AutoSize = true;
        checkBox3.BackColor = System.Drawing.Color.Transparent;
        checkBox3.Depth = 0;
        checkBox3.Dock = DockStyle.Top;
        checkBox3.Location = new System.Drawing.Point(0, 0);
        checkBox3.Margin = new Padding(0);
        checkBox3.MouseLocation = new System.Drawing.Point(-1, -1);
        checkBox3.Name = "checkBox3";
        checkBox3.Ripple = true;
        checkBox3.Size = new System.Drawing.Size(232, 30);
        checkBox3.TabIndex = 13;
        checkBox3.Text = "Toggle Title";
        checkBox3.UseVisualStyleBackColor = false;
        // 
        // label3
        // 
        label3.ApplyGradient = false;
        label3.AutoSize = true;
        label3.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        label3.Gradient = new System.Drawing.Color[]
{
    System.Drawing.Color.Gray,
    System.Drawing.Color.Black
};
        label3.GradientAnimation = false;
        label3.Location = new System.Drawing.Point(10, 45);
        label3.Margin = new Padding(2, 0, 2, 0);
        label3.Name = "label3";
        label3.Size = new System.Drawing.Size(274, 40);
        label3.TabIndex = 11;
        label3.Text = "Lorem Ipsum is simply dummy text \r\nof the printing and typesetting industry. ";
        // 
        // progressBar5
        // 
        progressBar5.BackColor = System.Drawing.Color.Transparent;
        progressBar5.DrawHatch = false;
        progressBar5.Gradient = new System.Drawing.Color[]
{
    System.Drawing.Color.Maroon,
    System.Drawing.Color.Red
};
        progressBar5.HatchType = System.Drawing.Drawing2D.HatchStyle.Percent10;
        progressBar5.Location = new System.Drawing.Point(16, 418);
        progressBar5.Margin = new Padding(4, 2, 4, 2);
        progressBar5.Maximum = 100L;
        progressBar5.MaxPercentShowValue = 100F;
        progressBar5.Name = "progressBar5";
        progressBar5.PercentIndices = 2;
        progressBar5.Radius = 4;
        progressBar5.ShowAsPercent = true;
        progressBar5.ShowValue = true;
        progressBar5.Size = new System.Drawing.Size(179, 16);
        progressBar5.TabIndex = 3;
        progressBar5.Text = "70,00%";
        progressBar5.Value = 70L;
        // 
        // MainWindow
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = System.Drawing.Color.Transparent;
        ClientSize = new System.Drawing.Size(1241, 768);
        ContextMenuStrip = contextMenuStrip1;
        Controls.Add(windowPageControl);
        Controls.Add(panel8);
        Controls.Add(separator1);
        Controls.Add(menuStrip1);
        DwmMargin = -1;
        ExtendBox = true;
        ExtendMenu = contextMenuStrip1;
        ForeColor = System.Drawing.Color.Black;
        Location = new System.Drawing.Point(0, 0);
        Margin = new Padding(4, 2, 4, 2);
        Name = "MainWindow";
        Padding = new Padding(1, 40, 1, 1);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "MainWindow";
        WindowPageControl = windowPageControl;
        Load += MainWindow_Load;
        contextMenuStrip1.ResumeLayout(false);
        menuStrip1.ResumeLayout(false);
        menuStrip1.PerformLayout();
        panel8.ResumeLayout(false);
        panel8.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
    private Controls.MenuStrip menuStrip1;
    private ToolStripMenuItem fileToolStripMenuItem;
    private ToolStripMenuItem newToolStripMenuItem;
    private ToolStripMenuItem openToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator;
    private ToolStripMenuItem saveToolStripMenuItem;
    private ToolStripMenuItem saveAsToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripMenuItem printToolStripMenuItem;
    private ToolStripMenuItem printPreviewToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator2;
    private ToolStripMenuItem exitToolStripMenuItem;
    private ToolStripMenuItem editToolStripMenuItem;
    private ToolStripMenuItem undoToolStripMenuItem;
    private ToolStripMenuItem redoToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator3;
    private ToolStripMenuItem cutToolStripMenuItem;
    private ToolStripMenuItem copyToolStripMenuItem;
    private ToolStripMenuItem pasteToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator4;
    private ToolStripMenuItem selectAllToolStripMenuItem;
    private ToolStripMenuItem toolsToolStripMenuItem;
    private ToolStripMenuItem customizeToolStripMenuItem;
    private ToolStripMenuItem optionsToolStripMenuItem;
    private ToolStripMenuItem helpToolStripMenuItem;
    private ToolStripMenuItem contentsToolStripMenuItem;
    private ToolStripMenuItem indexToolStripMenuItem;
    private ToolStripMenuItem searchToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator5;
    private ToolStripMenuItem aboutToolStripMenuItem;
    private Controls.ContextMenuStrip contextMenuStrip1;
    private ToolStripMenuItem toolStripMenuItem1;
    private ToolStripMenuItem toolStripMenuItem2;
    private ToolStripMenuItem toolStripMenuItem3;
    private ToolStripMenuItem toolStripMenuItem8;
    private ToolStripMenuItem toolStripMenuItem9;
    private ToolStripMenuItem toolStripMenuItem10;
    private ToolStripMenuItem toolStripMenuItem11;
    private ToolStripMenuItem toolStripMenuItem12;
    private ToolStripMenuItem toolStripMenuItem13;
    private ToolStripMenuItem toolStripMenuItem14;
    private ToolStripMenuItem toolStripMenuItem4;
    private ToolStripMenuItem toolStripMenuItem5;
    private ToolStripMenuItem toolStripMenuItem6;
    private ToolStripMenuItem toolStripMenuItem7;
    private ToolStripSeparator toolStripSeparator7;
    private ToolStripSeparator toolStripSeparator6;
    private ToolTip toolTip1;
    private WindowPageControl windowPageControl;
    private Separator separator1;
    private Controls.Panel panel8;
    private ToggleButton toggleButton1;
    private ShapeProgressBar shapeProgressBar2;
    private ShapeProgressBar shapeProgressBar1;
    private ChatBubble chatBubble2;
    private ChatBubble chatBubble1;
    private NumUpDown numUpDown1;
    private Controls.CheckBox checkBox4;
    private Controls.CheckBox checkBox3;
    private Controls.Label label3;
    private Controls.ProgressBar progressBar5;
}