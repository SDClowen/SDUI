namespace SDUI.Demo
{
    partial class ConfigPage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            buttonSelectColor = new SDUI.Controls.Button();
            labelRenderBackend = new SDUI.Controls.Label();
            comboBoxRenderBackend = new SDUI.Controls.ComboBox();
            checkBoxPerfOverlay = new SDUI.Controls.CheckBox();
            colorPicker = new SDUI.Controls.ColorPicker();
            comboBoxHatchType = new SDUI.Controls.ComboBox();
            checkBoxDrawFullHatch = new SDUI.Controls.CheckBox();
            numTitleHeight = new SDUI.Controls.NumUpDown();
            numIconWidth = new SDUI.Controls.NumUpDown();
            label5 = new SDUI.Controls.Label();
            label6 = new SDUI.Controls.Label();
            buttonSelectFont = new SDUI.Controls.Button();
            label1 = new SDUI.Controls.Label();
            buttonBorderColor = new SDUI.Controls.Button();
            checkBoxTitleBorder = new SDUI.Controls.CheckBox();
            checkBoxToggleTitle = new SDUI.Controls.CheckBox();
            SuspendLayout();
            // 
            // buttonSelectColor
            // 
            buttonSelectColor.AutoSize = true;
            buttonSelectColor.Color = SkiaSharp.SKColor.Navy;
            buttonSelectColor.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            buttonSelectColor.ForeColor = SkiaSharp.SKColor.White;
            buttonSelectColor.Location = new System.Drawing.SKPoint(140, 124);
            buttonSelectColor.Name = "buttonSelectColor";
            buttonSelectColor.Radius = 6;
            buttonSelectColor.ShadowDepth = 0F;
            buttonSelectColor.Size = new SkiaSharp.SKSize(117, 23);
            buttonSelectColor.TabIndex = 46;
            buttonSelectColor.Text = "Select Color";
            buttonSelectColor.UseVisualStyleBackColor = true;
            buttonSelectColor.Click += buttonSelectColor_Click;
                // 
                // labelRenderBackend
                // 
                labelRenderBackend.ApplyGradient = false;
                labelRenderBackend.AutoSize = true;
                labelRenderBackend.BackColor = SkiaSharp.SKColor.Transparent;
                labelRenderBackend.ForeColor = SkiaSharp.SKColor.FromArgb(0, 0, 0);
                labelRenderBackend.Gradient = new SkiaSharp.SKColor[]
            {
            SkiaSharp.SKColor.Gray,
            SkiaSharp.SKColor.Black
            };
                labelRenderBackend.GradientAnimation = false;
                labelRenderBackend.Location = new System.Drawing.SKPoint(20, 16);
                labelRenderBackend.Name = "labelRenderBackend";
                labelRenderBackend.Size = new SkiaSharp.SKSize(73, 20);
                labelRenderBackend.TabIndex = 50;
                labelRenderBackend.Text = "Renderer:";
                // 
                // comboBoxRenderBackend
                // 
                comboBoxRenderBackend.BackColor = SkiaSharp.SKColor.Transparent;
                comboBoxRenderBackend.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
                comboBoxRenderBackend.DropDownHeight = 120;
                comboBoxRenderBackend.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                comboBoxRenderBackend.ForeColor = SkiaSharp.SKColor.FromArgb(75, 85, 91);
                comboBoxRenderBackend.FormattingEnabled = true;
                comboBoxRenderBackend.IntegralHeight = false;
                comboBoxRenderBackend.ItemHeight = 16;
                comboBoxRenderBackend.Location = new System.Drawing.SKPoint(100, 12);
                comboBoxRenderBackend.Name = "comboBoxRenderBackend";
                comboBoxRenderBackend.Radius = 5;
                comboBoxRenderBackend.ShadowDepth = 4F;
                comboBoxRenderBackend.Size = new SkiaSharp.SKSize(160, 22);
                comboBoxRenderBackend.TabIndex = 51;
                comboBoxRenderBackend.SelectedIndexChanged += comboBoxRenderBackend_SelectedIndexChanged;
                // 
                // checkBoxPerfOverlay
                // 
                checkBoxPerfOverlay.AutoSize = true;
                checkBoxPerfOverlay.BackColor = SkiaSharp.SKColor.Transparent;
                checkBoxPerfOverlay.Depth = 0;
                checkBoxPerfOverlay.Location = new System.Drawing.SKPoint(280, 10);
                checkBoxPerfOverlay.Margin = new System.Windows.Forms.Padding(0);
                checkBoxPerfOverlay.MouseLocation = new System.Drawing.SKPoint(-1, -1);
                checkBoxPerfOverlay.Name = "checkBoxPerfOverlay";
                checkBoxPerfOverlay.Ripple = true;
                checkBoxPerfOverlay.Size = new SkiaSharp.SKSize(150, 30);
                checkBoxPerfOverlay.TabIndex = 52;
                checkBoxPerfOverlay.Text = "Show perf overlay";
            checkBoxPerfOverlay.UseVisualStyleBackColor = true;
            checkBoxPerfOverlay.CheckedChanged += checkBoxPerfOverlay_CheckedChanged;
            // 
            // colorPicker
            // 
            colorPicker.Location = new System.Drawing.SKPoint(20, 160);
            colorPicker.Name = "colorPicker";
            colorPicker.Size = new SkiaSharp.SKSize(180, 32);
            colorPicker.TabIndex = 53;
            colorPicker.Text = "Color Picker";
            colorPicker.SelectedItemChanged += colorPicker_SelectedColorChanged;
            // 
            // comboBoxHatchType
            // 
            comboBoxHatchType.BackColor = SkiaSharp.SKColor.Transparent;
            comboBoxHatchType.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            comboBoxHatchType.DropDownHeight = 100;
            comboBoxHatchType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxHatchType.ForeColor = SkiaSharp.SKColor.FromArgb(75, 85, 91);
            comboBoxHatchType.FormattingEnabled = true;
            comboBoxHatchType.IntegralHeight = false;
            comboBoxHatchType.ItemHeight = 16;
            comboBoxHatchType.Location = new System.Drawing.SKPoint(20, 52);
            comboBoxHatchType.Name = "comboBoxHatchType";
            comboBoxHatchType.Radius = 5;
            comboBoxHatchType.ShadowDepth = 4F;
            comboBoxHatchType.Size = new SkiaSharp.SKSize(160, 22);
            comboBoxHatchType.TabIndex = 42;
            comboBoxHatchType.SelectedIndexChanged += comboBoxHatchType_SelectedIndexChanged;
            // 
            // checkBoxDrawFullHatch
            // 
            checkBoxDrawFullHatch.AutoSize = true;
            checkBoxDrawFullHatch.BackColor = SkiaSharp.SKColor.Transparent;
            checkBoxDrawFullHatch.Depth = 0;
            checkBoxDrawFullHatch.Location = new System.Drawing.SKPoint(200, 48);
            checkBoxDrawFullHatch.Margin = new System.Windows.Forms.Padding(0);
            checkBoxDrawFullHatch.MouseLocation = new System.Drawing.SKPoint(-1, -1);
            checkBoxDrawFullHatch.Name = "checkBoxDrawFullHatch";
            checkBoxDrawFullHatch.Ripple = true;
            checkBoxDrawFullHatch.Size = new SkiaSharp.SKSize(135, 30);
            checkBoxDrawFullHatch.TabIndex = 45;
            checkBoxDrawFullHatch.Text = "Draw full hatch";
            checkBoxDrawFullHatch.UseVisualStyleBackColor = true;
            checkBoxDrawFullHatch.CheckedChanged += checkBoxDrawFullHatch_CheckedChanged;
            // 
            // numTitleHeight
            // 
            numTitleHeight.BackColor = SkiaSharp.SKColor.Transparent;
            numTitleHeight.Font = new System.Drawing.Font("Segoe UI", 9F);
            numTitleHeight.ForeColor = SkiaSharp.SKColor.FromArgb(76, 76, 76);
            numTitleHeight.Location = new System.Drawing.SKPoint(320, 84);
            numTitleHeight.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numTitleHeight.Minimum = new decimal(new int[] { 31, 0, 0, 0 });
            numTitleHeight.MinimumSize = new SkiaSharp.SKSize(80, 25);
            numTitleHeight.Name = "numTitleHeight";
            numTitleHeight.Size = new SkiaSharp.SKSize(80, 25);
            numTitleHeight.TabIndex = 40;
            numTitleHeight.Text = "rsNumericUpDown1";
            numTitleHeight.Value = new decimal(new int[] { 31, 0, 0, 0 });
            numTitleHeight.ValueChanged += numTitleHeight_ValueChanged;
            // 
            // numIconWidth
            // 
            numIconWidth.BackColor = SkiaSharp.SKColor.Transparent;
            numIconWidth.Font = new System.Drawing.Font("Segoe UI", 9F);
            numIconWidth.ForeColor = SkiaSharp.SKColor.FromArgb(76, 76, 76);
            numIconWidth.Location = new System.Drawing.SKPoint(100, 84);
            numIconWidth.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numIconWidth.Minimum = new decimal(new int[] { 32, 0, 0, 0 });
            numIconWidth.MinimumSize = new SkiaSharp.SKSize(80, 25);
            numIconWidth.Name = "numIconWidth";
            numIconWidth.Size = new SkiaSharp.SKSize(80, 25);
            numIconWidth.TabIndex = 41;
            numIconWidth.Text = "rsNumericUpDown1";
            numIconWidth.Value = new decimal(new int[] { 41, 0, 0, 0 });
            numIconWidth.ValueChanged += numIconWidth_ValueChanged;
            // 
            // label5
            // 
            label5.ApplyGradient = false;
            label5.AutoSize = true;
            label5.BackColor = SkiaSharp.SKColor.Transparent;
            label5.ForeColor = SkiaSharp.SKColor.FromArgb(0, 0, 0);
            label5.Gradient = new SkiaSharp.SKColor[]
    {
    SkiaSharp.SKColor.Gray,
    SkiaSharp.SKColor.Black
    };
            label5.GradientAnimation = false;
            label5.Location = new System.Drawing.SKPoint(20, 88);
            label5.Name = "label5";
            label5.Size = new SkiaSharp.SKSize(80, 20);
            label5.TabIndex = 43;
            label5.Text = "IconWidth:";
            // 
            // label6
            // 
            label6.ApplyGradient = false;
            label6.AutoSize = true;
            label6.BackColor = SkiaSharp.SKColor.Transparent;
            label6.ForeColor = SkiaSharp.SKColor.FromArgb(0, 0, 0);
            label6.Gradient = new SkiaSharp.SKColor[]
    {
    SkiaSharp.SKColor.Gray,
    SkiaSharp.SKColor.Black
    };
            label6.GradientAnimation = false;
            label6.Location = new System.Drawing.SKPoint(240, 88);
            label6.Name = "label6";
            label6.Size = new SkiaSharp.SKSize(90, 20);
            label6.TabIndex = 44;
            label6.Text = "Title Height:";
            // 
            // buttonSelectFont
            // 
            buttonSelectFont.AutoSize = true;
            buttonSelectFont.Color = SkiaSharp.SKColor.RosyBrown;
            buttonSelectFont.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            buttonSelectFont.ForeColor = SkiaSharp.SKColor.White;
            buttonSelectFont.Location = new System.Drawing.SKPoint(20, 124);
            buttonSelectFont.Name = "buttonSelectFont";
            buttonSelectFont.Radius = 8;
            buttonSelectFont.ShadowDepth = 0F;
            buttonSelectFont.Size = new SkiaSharp.SKSize(111, 23);
            buttonSelectFont.TabIndex = 47;
            buttonSelectFont.Text = "Select Font";
            buttonSelectFont.UseVisualStyleBackColor = true;
            buttonSelectFont.Click += buttonSelectFont_Click;
            // 
            // label1
            // 
            label1.ApplyGradient = true;
            label1.Dock = System.Windows.Forms.DockStyle.Bottom;
            label1.Font = new System.Drawing.Font("Segoe UI Semibold", 129.75F, System.Drawing.FontStyle.Bold);
            label1.ForeColor = SkiaSharp.SKColor.FromArgb(0, 0, 0);
            label1.Gradient = new SkiaSharp.SKColor[]
    {
    SkiaSharp.SKColor.FromArgb(0, 165, 178),
    SkiaSharp.SKColor.FromArgb(40, 50, 212),
    SkiaSharp.SKColor.FromArgb(114, 9, 212)
    };
            label1.GradientAnimation = false;
            label1.Location = new System.Drawing.SKPoint(0, 355);
            label1.Name = "label1";
            label1.Size = new SkiaSharp.SKSize(791, 229);
            label1.TabIndex = 48;
            label1.Text = "SDUI";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonBorderColor
            // 
            buttonBorderColor.AutoSize = true;
            buttonBorderColor.Color = SkiaSharp.SKColor.Crimson;
            buttonBorderColor.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold);
            buttonBorderColor.ForeColor = SkiaSharp.SKColor.White;
            buttonBorderColor.Location = new System.Drawing.SKPoint(380, 124);
            buttonBorderColor.Name = "buttonBorderColor";
            buttonBorderColor.Radius = 6;
            buttonBorderColor.ShadowDepth = 4F;
            buttonBorderColor.Size = new SkiaSharp.SKSize(173, 23);
            buttonBorderColor.TabIndex = 49;
            buttonBorderColor.Text = "Select Border Color";
            buttonBorderColor.UseVisualStyleBackColor = true;
            buttonBorderColor.Click += buttonBorderColor_Click;
            // 
            // checkBoxTitleBorder
            // 
            checkBoxTitleBorder.AutoSize = true;
            checkBoxTitleBorder.BackColor = SkiaSharp.SKColor.Transparent;
            checkBoxTitleBorder.Depth = 0;
            checkBoxTitleBorder.Location = new System.Drawing.SKPoint(360, 48);
            checkBoxTitleBorder.Margin = new System.Windows.Forms.Padding(0);
            checkBoxTitleBorder.MouseLocation = new System.Drawing.SKPoint(-1, -1);
            checkBoxTitleBorder.Name = "checkBoxTitleBorder";
            checkBoxTitleBorder.Ripple = true;
            checkBoxTitleBorder.Size = new SkiaSharp.SKSize(152, 30);
            checkBoxTitleBorder.TabIndex = 45;
            checkBoxTitleBorder.Text = "Draw Title Border";
            checkBoxTitleBorder.UseVisualStyleBackColor = true;
            checkBoxTitleBorder.CheckedChanged += checkBoxTitleBorder_CheckedChanged;
            // 
            // checkBoxToggleTitle
            // 
            checkBoxToggleTitle.AutoSize = true;
            checkBoxToggleTitle.BackColor = SkiaSharp.SKColor.Transparent;
            checkBoxToggleTitle.Depth = 0;
            checkBoxToggleTitle.Location = new System.Drawing.SKPoint(520, 48);
            checkBoxToggleTitle.Margin = new System.Windows.Forms.Padding(0);
            checkBoxToggleTitle.MouseLocation = new System.Drawing.SKPoint(-1, -1);
            checkBoxToggleTitle.Name = "checkBoxToggleTitle";
            checkBoxToggleTitle.Ripple = true;
            checkBoxToggleTitle.Size = new SkiaSharp.SKSize(114, 30);
            checkBoxToggleTitle.TabIndex = 45;
            checkBoxToggleTitle.Text = "Toggle Title";
            checkBoxToggleTitle.UseVisualStyleBackColor = true;
            checkBoxToggleTitle.CheckedChanged += checkBoxToggleTitle_CheckedChanged;
            // 
            // ConfigPage
            // 
            BackColor = System.Drawing.SystemColors.Control;
            Controls.Add(checkBoxPerfOverlay);
            Controls.Add(comboBoxRenderBackend);
            Controls.Add(labelRenderBackend);
            Controls.Add(colorPicker);
            Controls.Add(buttonBorderColor);
            Controls.Add(label1);
            Controls.Add(buttonSelectFont);
            Controls.Add(buttonSelectColor);
            Controls.Add(comboBoxHatchType);
            Controls.Add(checkBoxTitleBorder);
            Controls.Add(checkBoxToggleTitle);
            Controls.Add(checkBoxDrawFullHatch);
            Controls.Add(numTitleHeight);
            Controls.Add(numIconWidth);
            Controls.Add(label5);
            Controls.Add(label6);
            Name = "ConfigPage";
            Size = new SkiaSharp.SKSize(791, 584);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Controls.Button buttonSelectColor;
        private Controls.ComboBox comboBoxHatchType;
        private Controls.CheckBox checkBoxDrawFullHatch;
        private Controls.NumUpDown numTitleHeight;
        private Controls.NumUpDown numIconWidth;
        private Controls.Label label5;
        private Controls.Label label6;
        private Controls.Button buttonSelectFont;
        private Controls.Label label1;
        private Controls.Button buttonBorderColor;
        private Controls.CheckBox checkBoxTitleBorder;
        private Controls.CheckBox checkBoxToggleTitle;
        private Controls.Label labelRenderBackend;
        private Controls.ComboBox comboBoxRenderBackend;
        private Controls.CheckBox checkBoxPerfOverlay;
        private SDUI.Controls.ColorPicker colorPicker;
    }
}
