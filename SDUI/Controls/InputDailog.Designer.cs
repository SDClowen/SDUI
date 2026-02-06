using SkiaSharp;

namespace SDUI.Controls
{
    partial class InputDialog
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
            lblTitle = new Label();
            lblMessage = new Label();
            txtValue = new TextBox();
            btnOK = new Button();
            btnCancel = new Button();
            comboBox = new ComboBox();
            numValue = new NumUpDown();
            panel1 = new DoubleBufferedControl();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.ApplyGradient = false;
            lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.SKPoint);
            lblTitle.ForeColor = SkiaSharp.SKColor.FromArgb(0, 0, 0);
            lblTitle.Gradient = new SkiaSharp.SKColor[]
    {
    SkiaSharp.SKColor.Gray,
    SkiaSharp.SKColor.Black
    };
            lblTitle.GradientAnimation = false;
            lblTitle.Location = new System.Drawing.SKPoint(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new SkiaSharp.SKSize(324, 20);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Input";
            // 
            // lblMessage
            // 
            lblMessage.ApplyGradient = false;
            lblMessage.Dock = System.Windows.Forms.DockStyle.Top;
            lblMessage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.SKPoint);
            lblMessage.ForeColor = SkiaSharp.SKColor.FromArgb(0, 0, 0);
            lblMessage.Gradient = new SkiaSharp.SKColor[]
    {
    SkiaSharp.SKColor.Gray,
    SkiaSharp.SKColor.Black
    };
            lblMessage.GradientAnimation = false;
            lblMessage.Location = new System.Drawing.SKPoint(0, 20);
            lblMessage.Name = "lblMessage";
            lblMessage.Size = new SkiaSharp.SKSize(324, 46);
            lblMessage.TabIndex = 2;
            lblMessage.Text = "Please enter a value";
            // 
            // txtValue
            // 
            txtValue.Location = new System.Drawing.SKPoint(11, 83);
            txtValue.MaxLength = 32767;
            txtValue.MultiLine = false;
            txtValue.Name = "txtValue";
            txtValue.PassFocusShow = false;
            txtValue.Radius = 2;
            txtValue.Size = new SkiaSharp.SKSize(299, 25);
            txtValue.TabIndex = 0;
            txtValue.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            txtValue.UseSystemPasswordChar = false;
            txtValue.KeyDown += TxtValue_KeyDown;
            // 
            // btnOK
            // 
            btnOK.Color = SkiaSharp.SKColor.DodgerBlue;
            btnOK.DialogResult = SDUI.DialogResult.OK;
            btnOK.ForeColor = SkiaSharp.SKColor.White;
            btnOK.Location = new System.Drawing.SKPoint(11, 6);
            btnOK.Name = "btnOK";
            btnOK.Radius = 6;
            btnOK.ShadowDepth = 4F;
            btnOK.Size = new SkiaSharp.SKSize(106, 29);
            btnOK.TabIndex = 0;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Color = SkiaSharp.SKColors.Firebrick;
            btnCancel.DialogResult = SDUI.DialogResult.Cancel;
            btnCancel.ForeColor = SkiaSharp.SKColors.White;
            btnCancel.Location = new System.Drawing.SKPoint(223, 6);
            btnCancel.Name = "btnCancel";
            btnCancel.Radius = 6;
            btnCancel.ShadowDepth = 4F;
            btnCancel.Size = new SKSize(91, 29);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // comboBox
            // 
            comboBox.DropDownHeight = 100;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.FormattingEnabled = true;
            comboBox.IntegralHeight = false;
            comboBox.ItemHeight = 23;
            comboBox.Location = new System.Drawing.SKPoint(12, 80);
            comboBox.Name = "comboBox";
            comboBox.Radius = 5;
            comboBox.ShadowDepth = 4F;
            comboBox.Size = new SkiaSharp.SKSize(277, 29);
            comboBox.TabIndex = 4;
            comboBox.Visible = false;
            // 
            // numValue
            // 
            numValue.BackColor = SkiaSharp.SKColor.Transparent;
            numValue.Font = new System.Drawing.Font("Segoe UI", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.SKPoint);
            numValue.ForeColor = SkiaSharp.SKColor.FromArgb(0, 0, 0);
            numValue.Location = new System.Drawing.SKPoint(12, 77);
            numValue.Maximum = new decimal(new int[] { 32765, 0, 0, 0 });
            numValue.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numValue.MinimumSize = new SkiaSharp.SKSize(80, 25);
            numValue.Name = "numValue";
            numValue.Size = new SkiaSharp.SKSize(156, 34);
            numValue.TabIndex = 5;
            numValue.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numValue.KeyUp += numValue_KeyUp;
            // 
            // panel1
            // 
            panel1.BackColor = SkiaSharp.SKColors.Transparent;
            panel1.Controls.Add(btnOK);
            panel1.Controls.Add(btnCancel);
            panel1.Dock = SDUI.DockStyle.Bottom;
            panel1.Location = new System.Drawing.SKPoint(0, 125);
            panel1.Name = "panel1";
            panel1.Size = new SkiaSharp.SKSize(324, 39);
            panel1.TabIndex = 6;
            // 
            // InputDialog
            // 
            AutoScaleDimensions = new SkiaSharp.SKSize(120F, 120F);
            AutoScaleMode = SDUI.AutoScaleMode.Dpi;
            ClientSize = new SkiaSharp.SKSize(324, 164);
            ControlBox = false;
            Controls.Add(panel1);
            Controls.Add(numValue);
            Controls.Add(comboBox);
            Controls.Add(txtValue);
            Controls.Add(lblMessage);
            Controls.Add(lblTitle);
            ForeColor = SkiaSharp.SKColors.Black;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InputDialog";
            Padding = new(10);
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            FormClosing += InputDialog_FormClosing;
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SDUI.Controls.Label lblTitle;
        private SDUI.Controls.Label lblMessage;
        private TextBox txtValue;
        private SDUI.Controls.Button btnOK;
        private SDUI.Controls.Button btnCancel;
        private ComboBox comboBox;
        private UIElementBase panel1;
        private NumUpDown numValue;
    }
}
