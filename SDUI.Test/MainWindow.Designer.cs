using System.Windows.Forms;
using SDUI;
using SDUI.Controls;

namespace SDUI.Demo;

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
        contextMenuStrip1 = new SDUI.Controls.ContextMenuStrip();
        toolStripMenuItem1 = new MenuItem();
        toolStripMenuItem2 = new MenuItem();
        toolStripMenuItem3 = new MenuItem();
        toolStripMenuItem8 = new MenuItem();
        toolStripMenuItem9 = new MenuItem();
        toolStripSeparator7 = new MenuItemSeparator();
        toolStripMenuItem10 = new MenuItem();
        toolStripMenuItem11 = new MenuItem();
        toolStripMenuItem12 = new MenuItem();
        toolStripMenuItem13 = new MenuItem();
        toolStripMenuItem14 = new MenuItem();
        toolStripMenuItem4 = new MenuItem();
        toolStripMenuItem5 = new MenuItem();
        toolStripMenuItem6 = new MenuItem();
        toolStripMenuItem7 = new MenuItem();
        toolStripSeparator6 = new MenuItemSeparator();
        menuStrip1 = new SDUI.Controls.MenuStrip();
        statusStrip1 = new SDUI.Controls.StatusStrip();
        statusStripLabel1 = new MenuItem();
        fileToolStripMenuItem = new MenuItem();
        newToolStripMenuItem = new MenuItem();
        openToolStripMenuItem = new MenuItem();
        toolStripSeparator = new MenuItemSeparator();
        saveToolStripMenuItem = new MenuItem();
        saveAsToolStripMenuItem = new MenuItem();
        toolStripSeparator1 = new MenuItemSeparator();
        printToolStripMenuItem = new MenuItem();
        printPreviewToolStripMenuItem = new MenuItem();
        toolStripSeparator2 = new MenuItemSeparator();
        exitToolStripMenuItem = new MenuItem();
        editToolStripMenuItem = new MenuItem();
        undoToolStripMenuItem = new MenuItem();
        redoToolStripMenuItem = new MenuItem();
        toolStripSeparator3 = new MenuItemSeparator();
        cutToolStripMenuItem = new MenuItem();
        copyToolStripMenuItem = new MenuItem();
        pasteToolStripMenuItem = new MenuItem();
        toolStripSeparator4 = new MenuItemSeparator();
        selectAllToolStripMenuItem = new MenuItem();
        viewToolStripMenuItem = new MenuItem();
        toolbarToolStripMenuItem = new MenuItem();
        statusBarToolStripMenuItem = new MenuItem();
        wordWrapToolStripMenuItem = new MenuItem();
        toolsToolStripMenuItem = new MenuItem();
        customizeToolStripMenuItem = new MenuItem();
        optionsToolStripMenuItem = new MenuItem();
        helpToolStripMenuItem = new MenuItem();
        contentsToolStripMenuItem = new MenuItem();
        indexToolStripMenuItem = new MenuItem();
        searchToolStripMenuItem = new MenuItem();
        toolStripSeparator5 = new MenuItemSeparator();
        aboutToolStripMenuItem = new MenuItem();
        toolTip1 = new SDUI.Controls.ToolTip();
        windowPageControl = new WindowPageControl();
        contextMenuStrip1.SuspendLayout();
        menuStrip1.SuspendLayout();
        SuspendLayout();
        // 
        // contextMenuStrip1
        // 
        contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
        contextMenuStrip1.Items.AddRange(new MenuItem[] { toolStripMenuItem1, toolStripMenuItem2, toolStripMenuItem3, toolStripMenuItem4, toolStripMenuItem5, toolStripMenuItem6, toolStripMenuItem7, toolStripSeparator6 });
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
        toolStripMenuItem3.DropDownItems.AddRange(new MenuItem[] { toolStripMenuItem8, toolStripMenuItem9, toolStripSeparator7, toolStripMenuItem10 });
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
        toolStripMenuItem10.DropDownItems.AddRange(new MenuItem[] { toolStripMenuItem11, toolStripMenuItem12, toolStripMenuItem13, toolStripMenuItem14 });
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
        menuStrip1.Dock = DockStyle.Top;
        menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
        menuStrip1.Items.AddRange(new MenuItem[] { fileToolStripMenuItem, editToolStripMenuItem, viewToolStripMenuItem, toolsToolStripMenuItem, helpToolStripMenuItem });
        menuStrip1.Location = new System.Drawing.Point(1, 737);
        menuStrip1.Name = "menuStrip1";
        menuStrip1.Padding = new Padding(6, 2, 0, 2);
        menuStrip1.Size = new System.Drawing.Size(1239, 28);
        menuStrip1.Stretch = false;
        menuStrip1.TabIndex = 1;
        menuStrip1.Text = "menuStrip1";
        // 
        // statusStripLabel1
        // 
        statusStripLabel1 = new MenuItem();
        statusStripLabel1.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        statusStripLabel1.Name = "statusStripLabel1";
        statusStripLabel1.Size = new System.Drawing.Size(200, 24);
        statusStripLabel1.Text = "Ready";
        // 
        // statusStrip1
        // 
        statusStrip1.BackColor = System.Drawing.Color.Transparent;
        statusStrip1.Dock = DockStyle.Bottom;
        statusStrip1.Items.Add(statusStripLabel1);
        statusStrip1.Location = new System.Drawing.Point(1, 736);
        statusStrip1.Name = "statusStrip1";
        statusStrip1.Size = new System.Drawing.Size(1239, 28);
        statusStrip1.TabIndex = 2;
        statusStrip1.Text = "statusStrip1";
        // 
        // fileToolStripMenuItem
        // 
        fileToolStripMenuItem.DropDownItems.AddRange(new MenuItem[] { newToolStripMenuItem, openToolStripMenuItem, toolStripSeparator, saveToolStripMenuItem, saveAsToolStripMenuItem, toolStripSeparator1, printToolStripMenuItem, printPreviewToolStripMenuItem, toolStripSeparator2, exitToolStripMenuItem });
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
        editToolStripMenuItem.DropDownItems.AddRange(new MenuItem[] { undoToolStripMenuItem, redoToolStripMenuItem, toolStripSeparator3, cutToolStripMenuItem, copyToolStripMenuItem, pasteToolStripMenuItem, toolStripSeparator4, selectAllToolStripMenuItem });
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
        // viewToolStripMenuItem
        // 
        viewToolStripMenuItem.DropDownItems.AddRange(new MenuItem[] { toolbarToolStripMenuItem, statusBarToolStripMenuItem, wordWrapToolStripMenuItem });
        viewToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        viewToolStripMenuItem.Name = "viewToolStripMenuItem";
        viewToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
        viewToolStripMenuItem.Text = "&View";
        // 
        // toolbarToolStripMenuItem
        // 
        toolbarToolStripMenuItem.CheckOnClick = true;
        toolbarToolStripMenuItem.Checked = true;
        toolbarToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        toolbarToolStripMenuItem.Name = "toolbarToolStripMenuItem";
        toolbarToolStripMenuItem.Size = new System.Drawing.Size(155, 26);
        toolbarToolStripMenuItem.Text = "&Toolbar";
        toolbarToolStripMenuItem.CheckedChanged += toolbarToolStripMenuItem_CheckedChanged;
        // 
        // statusBarToolStripMenuItem
        // 
        statusBarToolStripMenuItem.CheckOnClick = true;
        statusBarToolStripMenuItem.Checked = true;
        statusBarToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        statusBarToolStripMenuItem.Name = "statusBarToolStripMenuItem";
        statusBarToolStripMenuItem.Size = new System.Drawing.Size(155, 26);
        statusBarToolStripMenuItem.Text = "&Status Bar";
        statusBarToolStripMenuItem.CheckedChanged += statusBarToolStripMenuItem_CheckedChanged;
        // 
        // wordWrapToolStripMenuItem
        // 
        wordWrapToolStripMenuItem.CheckOnClick = true;
        wordWrapToolStripMenuItem.CheckState = SDUI.Enums.CheckState.Indeterminate;
        wordWrapToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
        wordWrapToolStripMenuItem.Name = "wordWrapToolStripMenuItem";
        wordWrapToolStripMenuItem.Size = new System.Drawing.Size(155, 26);
        wordWrapToolStripMenuItem.Text = "&Word Wrap";
        wordWrapToolStripMenuItem.CheckedChanged += wordWrapToolStripMenuItem_CheckedChanged;
        // 
        // toolsToolStripMenuItem
        // 
        toolsToolStripMenuItem.DropDownItems.AddRange(new MenuItem[] { customizeToolStripMenuItem, optionsToolStripMenuItem });
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
        helpToolStripMenuItem.DropDownItems.AddRange(new MenuItem[] { contentsToolStripMenuItem, indexToolStripMenuItem, searchToolStripMenuItem, toolStripSeparator5, aboutToolStripMenuItem });
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
        // 
        // windowPageControl
        // 
        windowPageControl.BackColor = System.Drawing.Color.Transparent;
        windowPageControl.Dock = DockStyle.Fill;
        windowPageControl.Location = new System.Drawing.Point(1, 45);
        windowPageControl.Margin = new Padding(2, 3, 2, 3);
        windowPageControl.Name = "windowPageControl";
        windowPageControl.SelectedIndex = -1;
        windowPageControl.Size = new System.Drawing.Size(1239, 692);
        windowPageControl.TabIndex = 2;
        // 
        // MainWindow
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = System.Drawing.Color.Transparent;
        ClientSize = new System.Drawing.Size(1241, 768);
        ContextMenuStrip = contextMenuStrip1;
        Controls.Add(windowPageControl);
        Controls.Add(menuStrip1);
        Controls.Add(statusStrip1);
        DwmMargin = -1;
        ExtendBox = true;
        ExtendMenu = contextMenuStrip1;
        ForeColor = System.Drawing.Color.Black;
        Location = new System.Drawing.Point(0, 0);
        Margin = new Padding(5, 3, 5, 3);
        Name = "MainWindow";
        Padding = new Padding(1, 40, 1, 1);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "MainWindow";
        TitleTabDesingMode = TabDesingMode.Rectangle;
        WindowPageControl = windowPageControl;
        Load += MainWindow_Load;
        contextMenuStrip1.ResumeLayout(false);
        menuStrip1.ResumeLayout(false);
        menuStrip1.PerformLayout();
        statusStrip1?.ResumeLayout(false);
        statusStrip1?.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
    private Controls.MenuStrip menuStrip1;
    private MenuItem fileToolStripMenuItem;
    private MenuItem newToolStripMenuItem;
    private MenuItem openToolStripMenuItem;
    private MenuItemSeparator toolStripSeparator;
    private MenuItem saveToolStripMenuItem;
    private MenuItem saveAsToolStripMenuItem;
    private MenuItemSeparator toolStripSeparator1;
    private MenuItem printToolStripMenuItem;
    private MenuItem printPreviewToolStripMenuItem;
    private MenuItemSeparator toolStripSeparator2;
    private MenuItem exitToolStripMenuItem;
    private MenuItem editToolStripMenuItem;
    private MenuItem undoToolStripMenuItem;
    private MenuItem redoToolStripMenuItem;
    private MenuItemSeparator toolStripSeparator3;
    private MenuItem cutToolStripMenuItem;
    private MenuItem copyToolStripMenuItem;
    private MenuItem pasteToolStripMenuItem;
    private MenuItemSeparator toolStripSeparator4;
    private MenuItem selectAllToolStripMenuItem;
    private MenuItem viewToolStripMenuItem;
    private MenuItem toolbarToolStripMenuItem;
    private MenuItem statusBarToolStripMenuItem;
    private MenuItem wordWrapToolStripMenuItem;
    private MenuItem toolsToolStripMenuItem;
    private MenuItem customizeToolStripMenuItem;
    private MenuItem optionsToolStripMenuItem;
    private MenuItem helpToolStripMenuItem;
    private MenuItem contentsToolStripMenuItem;
    private MenuItem indexToolStripMenuItem;
    private MenuItem searchToolStripMenuItem;
    private MenuItemSeparator toolStripSeparator5;
    private MenuItem aboutToolStripMenuItem;
    private Controls.ContextMenuStrip contextMenuStrip1;
    private MenuItem toolStripMenuItem1;
    private MenuItem toolStripMenuItem2;
    private MenuItem toolStripMenuItem3;
    private MenuItem toolStripMenuItem8;
    private MenuItem toolStripMenuItem9;
    private MenuItem toolStripMenuItem10;
    private MenuItem toolStripMenuItem11;
    private MenuItem toolStripMenuItem12;
    private MenuItem toolStripMenuItem13;
    private MenuItem toolStripMenuItem14;
    private MenuItem toolStripMenuItem4;
    private MenuItem toolStripMenuItem5;
    private MenuItem toolStripMenuItem6;
    private MenuItem toolStripMenuItem7;
    private MenuItemSeparator toolStripSeparator7;
    private MenuItemSeparator toolStripSeparator6;
    private SDUI.Controls.ToolTip toolTip1;
    private WindowPageControl windowPageControl;
    private Controls.StatusStrip statusStrip1;
    private MenuItem statusStripLabel1;
}