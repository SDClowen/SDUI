namespace SDUI.Skia;

partial class MainWindow
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
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
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        button1 = new SDUI.SK.Button();
        button2 = new SDUI.SK.Button();
        SuspendLayout();
        // 
        // button1
        // 
        button1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        button1.BackColor = Color.Transparent;
        button1.Color = SystemColors.WindowFrame;
        button1.DialogResult = DialogResult.None;
        button1.ForeColor = Color.White;
        button1.Image = null;
        button1.Location = new Point(31, 61);
        button1.Name = "button1";
        button1.Radius = 6;
        button1.ShadowDepth = 4F;
        button1.Size = new Size(266, 108);
        button1.TabIndex = 0;
        button1.Text = "button1";
        button1.UseVisualStyleBackColor = true;
        // 
        // button2
        // 
        button2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        button2.BackColor = Color.Transparent;
        button2.Color = Color.Transparent;
        button2.DialogResult = DialogResult.None;
        button2.Image = null;
        button2.Location = new Point(31, 175);
        button2.Name = "button2";
        button2.Radius = 6;
        button2.ShadowDepth = 4F;
        button2.Size = new Size(266, 108);
        button2.TabIndex = 0;
        button2.Text = "button1";
        button2.UseVisualStyleBackColor = true;
        // 
        // MainWindow
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.Black;
        ClientSize = new Size(1279, 604);
        Controls.Add(button2);
        Controls.Add(button1);
        DwmMargin = -1;
        Location = new Point(0, 0);
        Margin = new Padding(3, 4, 3, 4);
        Name = "MainWindow";
        Text = "MainWindow";
        ResumeLayout(false);
    }

    #endregion

    private SK.Button button1;
    private SK.Button button2;
}