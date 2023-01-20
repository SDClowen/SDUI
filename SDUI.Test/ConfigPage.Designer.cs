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
            this.buttonSelectColor = new SDUI.Controls.Button();
            this.comboBoxHatchType = new SDUI.Controls.ComboBox();
            this.checkBoxDrawFullHatch = new SDUI.Controls.CheckBox();
            this.numTitleHeight = new SDUI.Controls.NumUpDown();
            this.numIconWidth = new SDUI.Controls.NumUpDown();
            this.label5 = new SDUI.Controls.Label();
            this.label6 = new SDUI.Controls.Label();
            this.SuspendLayout();
            // 
            // buttonSelectColor
            // 
            this.buttonSelectColor.Color = System.Drawing.Color.Transparent;
            this.buttonSelectColor.Location = new System.Drawing.Point(453, 125);
            this.buttonSelectColor.Name = "buttonSelectColor";
            this.buttonSelectColor.Radius = 6;
            this.buttonSelectColor.ShadowDepth = 4F;
            this.buttonSelectColor.Size = new System.Drawing.Size(95, 23);
            this.buttonSelectColor.TabIndex = 46;
            this.buttonSelectColor.Text = "Select Color";
            this.buttonSelectColor.UseVisualStyleBackColor = true;
            this.buttonSelectColor.Click += new System.EventHandler(this.buttonSelectColor_Click);
            // 
            // comboBoxHatchType
            // 
            this.comboBoxHatchType.BackColor = System.Drawing.Color.Transparent;
            this.comboBoxHatchType.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBoxHatchType.DropDownHeight = 100;
            this.comboBoxHatchType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxHatchType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(91)))));
            this.comboBoxHatchType.FormattingEnabled = true;
            this.comboBoxHatchType.IntegralHeight = false;
            this.comboBoxHatchType.ItemHeight = 16;
            this.comboBoxHatchType.Location = new System.Drawing.Point(171, 44);
            this.comboBoxHatchType.Name = "comboBoxHatchType";
            this.comboBoxHatchType.Radius = 5;
            this.comboBoxHatchType.ShadowDepth = 4F;
            this.comboBoxHatchType.Size = new System.Drawing.Size(121, 22);
            this.comboBoxHatchType.TabIndex = 42;
            this.comboBoxHatchType.SelectedIndexChanged += new System.EventHandler(this.comboBoxHatchType_SelectedIndexChanged);
            // 
            // checkBoxDrawFullHatch
            // 
            this.checkBoxDrawFullHatch.AutoSize = true;
            this.checkBoxDrawFullHatch.BackColor = System.Drawing.Color.Transparent;
            this.checkBoxDrawFullHatch.Depth = 0;
            this.checkBoxDrawFullHatch.Location = new System.Drawing.Point(295, 40);
            this.checkBoxDrawFullHatch.Margin = new System.Windows.Forms.Padding(0);
            this.checkBoxDrawFullHatch.MouseLocation = new System.Drawing.Point(-1, -1);
            this.checkBoxDrawFullHatch.Name = "checkBoxDrawFullHatch";
            this.checkBoxDrawFullHatch.Ripple = true;
            this.checkBoxDrawFullHatch.Size = new System.Drawing.Size(111, 30);
            this.checkBoxDrawFullHatch.TabIndex = 45;
            this.checkBoxDrawFullHatch.Text = "Draw full hatch";
            this.checkBoxDrawFullHatch.UseVisualStyleBackColor = true;
            this.checkBoxDrawFullHatch.CheckedChanged += new System.EventHandler(this.checkBoxDrawFullHatch_CheckedChanged);
            // 
            // numTitleHeight
            // 
            this.numTitleHeight.BackColor = System.Drawing.Color.Transparent;
            this.numTitleHeight.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.numTitleHeight.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(76)))), ((int)(((byte)(76)))));
            this.numTitleHeight.Location = new System.Drawing.Point(468, 84);
            this.numTitleHeight.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numTitleHeight.Minimum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.numTitleHeight.MinimumSize = new System.Drawing.Size(80, 25);
            this.numTitleHeight.Name = "numTitleHeight";
            this.numTitleHeight.Size = new System.Drawing.Size(80, 25);
            this.numTitleHeight.TabIndex = 40;
            this.numTitleHeight.Text = "rsNumericUpDown1";
            this.numTitleHeight.Value = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.numTitleHeight.ValueChanged += new System.EventHandler(this.numTitleHeight_ValueChanged);
            // 
            // numIconWidth
            // 
            this.numIconWidth.BackColor = System.Drawing.Color.Transparent;
            this.numIconWidth.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.numIconWidth.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(76)))), ((int)(((byte)(76)))));
            this.numIconWidth.Location = new System.Drawing.Point(251, 84);
            this.numIconWidth.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numIconWidth.Minimum = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.numIconWidth.MinimumSize = new System.Drawing.Size(80, 25);
            this.numIconWidth.Name = "numIconWidth";
            this.numIconWidth.Size = new System.Drawing.Size(80, 25);
            this.numIconWidth.TabIndex = 41;
            this.numIconWidth.Text = "rsNumericUpDown1";
            this.numIconWidth.Value = new decimal(new int[] {
            41,
            0,
            0,
            0});
            this.numIconWidth.ValueChanged += new System.EventHandler(this.numIconWidth_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label5.Location = new System.Drawing.Point(171, 90);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 15);
            this.label5.TabIndex = 43;
            this.label5.Text = "IconWidth:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label6.Location = new System.Drawing.Point(391, 90);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 15);
            this.label6.TabIndex = 44;
            this.label6.Text = "Title Height:";
            // 
            // ConfigPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonSelectColor);
            this.Controls.Add(this.comboBoxHatchType);
            this.Controls.Add(this.checkBoxDrawFullHatch);
            this.Controls.Add(this.numTitleHeight);
            this.Controls.Add(this.numIconWidth);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Name = "ConfigPage";
            this.Size = new System.Drawing.Size(791, 584);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.Button buttonSelectColor;
        private Controls.ComboBox comboBoxHatchType;
        private Controls.CheckBox checkBoxDrawFullHatch;
        private Controls.NumUpDown numTitleHeight;
        private Controls.NumUpDown numIconWidth;
        private Controls.Label label5;
        private Controls.Label label6;
    }
}
