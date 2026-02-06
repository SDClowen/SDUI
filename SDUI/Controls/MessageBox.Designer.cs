using SkiaSharp;

namespace SDUI.Controls
{
    partial class MessageBox
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblIcon = new Label();
            lblMessage = new Label();
            btnButton1 = new Button();
            btnButton2 = new Button();
            btnButton3 = new Button();
            panelButtons = new Panel();
            panelButtons.SuspendLayout();
            SuspendLayout();
            // 
            // lblIcon
            // 
            lblIcon.Font = new Font("Segoe UI", 24F);
            lblIcon.Location = new SKPointI(20, 30);
            lblIcon.Name = "lblIcon";
            lblIcon.Size = new SKSizeI(60, 60);
            lblIcon.TabIndex = 0;
            lblIcon.Text = "?";
            lblIcon.TextAlign = ContentAlignment.MiddleCenter;
            lblIcon.Visible = false;
            // 
            // lblMessage
            // 
            lblMessage.AutoSize = true;
            lblMessage.Font = new Font("Segoe UI", 10F);
            lblMessage.Location = new SKPointI(100, 30);
            lblMessage.Name = "lblMessage";
            lblMessage.Size = new SKSizeI(300, 60);
            lblMessage.TabIndex = 1;
            lblMessage.Text = "Message text";
            lblMessage.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnButton1
            // 
            btnButton1.Color = SKColors.DodgerBlue;
            btnButton1.DialogResult = DialogResult.OK;
            btnButton1.ForeColor = SKColors.White;
            btnButton1.Location = new SKPointI(10, 10);
            btnButton1.Name = "btnButton1";
            btnButton1.Radius = 6;
            btnButton1.ShadowDepth = 4F;
            btnButton1.Size = new SKSizeI(100, 32);
            btnButton1.TabIndex = 0;
            btnButton1.Text = "Button1";
            btnButton1.Visible = false;
            btnButton1.Click += Button_Click;
            // 
            // btnButton2
            // 
            btnButton2.Color = SKColors.DodgerBlue;
            btnButton2.DialogResult = DialogResult.Cancel;
            btnButton2.ForeColor = SKColors.White;
            btnButton2.Location = new SKPointI(120, 10);
            btnButton2.Name = "btnButton2";
            btnButton2.Radius = 6;
            btnButton2.ShadowDepth = 4F;
            btnButton2.Size = new SKSizeI(100, 32);
            btnButton2.TabIndex = 1;
            btnButton2.Text = "Button2";
            btnButton2.Visible = false;
            btnButton2.Click += Button_Click;
            // 
            // btnButton3
            // 
            btnButton3.Color = SKColors.DodgerBlue;
            btnButton3.DialogResult = DialogResult.None;
            btnButton3.ForeColor = SKColors.White;
            btnButton3.Location = new SKPointI(230, 10);
            btnButton3.Name = "btnButton3";
            btnButton3.Radius = 6;
            btnButton3.ShadowDepth = 4F;
            btnButton3.Size = new SKSizeI(100, 32);
            btnButton3.TabIndex = 2;
            btnButton3.Text = "Button3";
            btnButton3.Visible = false;
            btnButton3.Click += Button_Click;
            // 
            // panelButtons
            // 
            panelButtons.BackColor = SKColors.Transparent;
            panelButtons.Controls.Add(btnButton1);
            panelButtons.Controls.Add(btnButton2);
            panelButtons.Controls.Add(btnButton3);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new SKPointI(0, 110);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new SKSizeI(420, 50);
            panelButtons.TabIndex = 2;
            // 
            // MessageBox
            // 
            AutoScaleDimensions = new SKSize(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = ColorScheme.BackColor;
            ClientSize = new SKSizeI(420, 160);
            Controls.Add(panelButtons);
            Controls.Add(lblMessage);
            Controls.Add(lblIcon);
            ForeColor = ColorScheme.ForeColor;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MessageBox";
            Padding = new Thickness(10);
            FormStartPosition = FormStartPosition.CenterScreen;
            Text = "MessageBox";
            panelButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Label lblIcon;
        private Label lblMessage;
        private Button btnButton1;
        private Button btnButton2;
        private Button btnButton3;
        private Panel panelButtons;
    }
}
