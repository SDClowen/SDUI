namespace SDUI.Test
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
            buttonSelectColor = new Controls.Button();
            comboBoxHatchType = new Controls.ComboBox();
            checkBoxDrawFullHatch = new Controls.CheckBox();
            numTitleHeight = new Controls.NumUpDown();
            numIconWidth = new Controls.NumUpDown();
            label5 = new Controls.Label();
            label6 = new Controls.Label();
            buttonSelectFont = new Controls.Button();
            label1 = new Controls.Label();
            buttonBorderColor = new Controls.Button();
            checkBoxTitleBorder = new Controls.CheckBox();
            SuspendLayout();
            // 
            // buttonSelectColor
            // 
            buttonSelectColor.Color = System.Drawing.Color.Navy;
            buttonSelectColor.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            buttonSelectColor.ForeColor = System.Drawing.Color.White;
            buttonSelectColor.Location = new System.Drawing.Point(252, 124);
            buttonSelectColor.Name = "buttonSelectColor";
            buttonSelectColor.Radius = 6;
            buttonSelectColor.ShadowDepth = 4F;
            buttonSelectColor.Size = new System.Drawing.Size(103, 27);
            buttonSelectColor.TabIndex = 46;
            buttonSelectColor.Text = "Select Color";
            buttonSelectColor.UseVisualStyleBackColor = true;
            buttonSelectColor.Click += buttonSelectColor_Click;
            // 
            // comboBoxHatchType
            // 
            comboBoxHatchType.BackColor = System.Drawing.Color.Transparent;
            comboBoxHatchType.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            comboBoxHatchType.DropDownHeight = 100;
            comboBoxHatchType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxHatchType.ForeColor = System.Drawing.Color.FromArgb(75, 85, 91);
            comboBoxHatchType.FormattingEnabled = true;
            comboBoxHatchType.IntegralHeight = false;
            comboBoxHatchType.ItemHeight = 16;
            comboBoxHatchType.Location = new System.Drawing.Point(171, 44);
            comboBoxHatchType.Name = "comboBoxHatchType";
            comboBoxHatchType.Radius = 5;
            comboBoxHatchType.ShadowDepth = 4F;
            comboBoxHatchType.Size = new System.Drawing.Size(121, 22);
            comboBoxHatchType.TabIndex = 42;
            comboBoxHatchType.SelectedIndexChanged += comboBoxHatchType_SelectedIndexChanged;
            // 
            // checkBoxDrawFullHatch
            // 
            checkBoxDrawFullHatch.AutoSize = true;
            checkBoxDrawFullHatch.BackColor = System.Drawing.Color.Transparent;
            checkBoxDrawFullHatch.Depth = 0;
            checkBoxDrawFullHatch.Location = new System.Drawing.Point(295, 36);
            checkBoxDrawFullHatch.Margin = new System.Windows.Forms.Padding(0);
            checkBoxDrawFullHatch.MouseLocation = new System.Drawing.Point(-1, -1);
            checkBoxDrawFullHatch.Name = "checkBoxDrawFullHatch";
            checkBoxDrawFullHatch.Ripple = true;
            checkBoxDrawFullHatch.Size = new System.Drawing.Size(111, 30);
            checkBoxDrawFullHatch.TabIndex = 45;
            checkBoxDrawFullHatch.Text = "Draw full hatch";
            checkBoxDrawFullHatch.UseVisualStyleBackColor = true;
            checkBoxDrawFullHatch.CheckedChanged += checkBoxDrawFullHatch_CheckedChanged;
            // 
            // numTitleHeight
            // 
            numTitleHeight.BackColor = System.Drawing.Color.Transparent;
            numTitleHeight.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            numTitleHeight.ForeColor = System.Drawing.Color.FromArgb(76, 76, 76);
            numTitleHeight.Location = new System.Drawing.Point(468, 84);
            numTitleHeight.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numTitleHeight.Minimum = new decimal(new int[] { 31, 0, 0, 0 });
            numTitleHeight.MinimumSize = new System.Drawing.Size(80, 25);
            numTitleHeight.Name = "numTitleHeight";
            numTitleHeight.Size = new System.Drawing.Size(80, 25);
            numTitleHeight.TabIndex = 40;
            numTitleHeight.Text = "rsNumericUpDown1";
            numTitleHeight.Value = new decimal(new int[] { 31, 0, 0, 0 });
            numTitleHeight.ValueChanged += numTitleHeight_ValueChanged;
            // 
            // numIconWidth
            // 
            numIconWidth.BackColor = System.Drawing.Color.Transparent;
            numIconWidth.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            numIconWidth.ForeColor = System.Drawing.Color.FromArgb(76, 76, 76);
            numIconWidth.Location = new System.Drawing.Point(251, 84);
            numIconWidth.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numIconWidth.Minimum = new decimal(new int[] { 32, 0, 0, 0 });
            numIconWidth.MinimumSize = new System.Drawing.Size(80, 25);
            numIconWidth.Name = "numIconWidth";
            numIconWidth.Size = new System.Drawing.Size(80, 25);
            numIconWidth.TabIndex = 41;
            numIconWidth.Text = "rsNumericUpDown1";
            numIconWidth.Value = new decimal(new int[] { 41, 0, 0, 0 });
            numIconWidth.ValueChanged += numIconWidth_ValueChanged;
            // 
            // label5
            // 
            label5.ApplyGradient = false;
            label5.AutoSize = true;
            label5.BackColor = System.Drawing.Color.Transparent;
            label5.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            label5.Gradient = (new System.Drawing.Color[] { System.Drawing.Color.Gray, System.Drawing.Color.Black });
            label5.Location = new System.Drawing.Point(171, 90);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(65, 15);
            label5.TabIndex = 43;
            label5.Text = "IconWidth:";
            // 
            // label6
            // 
            label6.ApplyGradient = false;
            label6.AutoSize = true;
            label6.BackColor = System.Drawing.Color.Transparent;
            label6.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            label6.Gradient = (new System.Drawing.Color[] { System.Drawing.Color.Gray, System.Drawing.Color.Black });
            label6.Location = new System.Drawing.Point(391, 90);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(71, 15);
            label6.TabIndex = 44;
            label6.Text = "Title Height:";
            // 
            // buttonSelectFont
            // 
            buttonSelectFont.Color = System.Drawing.Color.RosyBrown;
            buttonSelectFont.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            buttonSelectFont.ForeColor = System.Drawing.Color.White;
            buttonSelectFont.Location = new System.Drawing.Point(155, 124);
            buttonSelectFont.Name = "buttonSelectFont";
            buttonSelectFont.Radius = 8;
            buttonSelectFont.ShadowDepth = 0F;
            buttonSelectFont.Size = new System.Drawing.Size(91, 27);
            buttonSelectFont.TabIndex = 47;
            buttonSelectFont.Text = "Select Font";
            buttonSelectFont.UseVisualStyleBackColor = true;
            buttonSelectFont.Click += buttonSelectFont_Click;
            // 
            // label1
            // 
            label1.ApplyGradient = true;
            label1.Dock = System.Windows.Forms.DockStyle.Bottom;
            label1.Font = new System.Drawing.Font("Segoe UI Semibold", 129.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label1.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
            label1.Gradient = (new System.Drawing.Color[] { System.Drawing.Color.FromArgb(0, 165, 178), System.Drawing.Color.FromArgb(40, 50, 212), System.Drawing.Color.FromArgb(114, 9, 212) });
            label1.Location = new System.Drawing.Point(0, 355);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(791, 229);
            label1.TabIndex = 48;
            label1.Text = "SDUI";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonBorderColor
            // 
            buttonBorderColor.Color = System.Drawing.Color.Crimson;
            buttonBorderColor.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            buttonBorderColor.ForeColor = System.Drawing.Color.White;
            buttonBorderColor.Location = new System.Drawing.Point(361, 124);
            buttonBorderColor.Name = "buttonBorderColor";
            buttonBorderColor.Radius = 6;
            buttonBorderColor.ShadowDepth = 4F;
            buttonBorderColor.Size = new System.Drawing.Size(140, 27);
            buttonBorderColor.TabIndex = 49;
            buttonBorderColor.Text = "Select Border Color";
            buttonBorderColor.UseVisualStyleBackColor = true;
            buttonBorderColor.Click += buttonBorderColor_Click;
            // 
            // checkBoxTitleBorder
            // 
            checkBoxTitleBorder.AutoSize = true;
            checkBoxTitleBorder.BackColor = System.Drawing.Color.Transparent;
            checkBoxTitleBorder.Depth = 0;
            checkBoxTitleBorder.Location = new System.Drawing.Point(419, 36);
            checkBoxTitleBorder.Margin = new System.Windows.Forms.Padding(0);
            checkBoxTitleBorder.MouseLocation = new System.Drawing.Point(-1, -1);
            checkBoxTitleBorder.Name = "checkBoxTitleBorder";
            checkBoxTitleBorder.Ripple = true;
            checkBoxTitleBorder.Size = new System.Drawing.Size(124, 30);
            checkBoxTitleBorder.TabIndex = 45;
            checkBoxTitleBorder.Text = "Draw Title Border";
            checkBoxTitleBorder.UseVisualStyleBackColor = true;
            checkBoxTitleBorder.CheckedChanged += checkBoxTitleBorder_CheckedChanged;
            // 
            // ConfigPage
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(buttonBorderColor);
            Controls.Add(label1);
            Controls.Add(buttonSelectFont);
            Controls.Add(buttonSelectColor);
            Controls.Add(comboBoxHatchType);
            Controls.Add(checkBoxTitleBorder);
            Controls.Add(checkBoxDrawFullHatch);
            Controls.Add(numTitleHeight);
            Controls.Add(numIconWidth);
            Controls.Add(label5);
            Controls.Add(label6);
            Name = "ConfigPage";
            Size = new System.Drawing.Size(791, 584);
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
    }
}
