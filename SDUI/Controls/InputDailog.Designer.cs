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
            this.lblTitle = new SDUI.Controls.Label();
            this.lblMessage = new SDUI.Controls.Label();
            this.txtValue = new SDUI.Controls.TextBox();
            this.btnOK = new SDUI.Controls.Button();
            this.btnCancel = new SDUI.Controls.Button();
            this.comboBox = new SDUI.Controls.ComboBox();
            this.numValue = new NumUpDown();
            this.panel1 = new SDUI.Controls.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.numValue)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(7, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(37, 15);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Input";
            // 
            // lblMessage
            // 
            this.lblMessage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage.Location = new System.Drawing.Point(7, 24);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(299, 42);
            this.lblMessage.TabIndex = 2;
            this.lblMessage.Text = "Please enter a value";
            // 
            // txtValue
            // 
            this.txtValue.Location = new System.Drawing.Point(11, 69);
            this.txtValue.MaxLength = 32767;
            this.txtValue.MultiLine = false;
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(299, 21);
            this.txtValue.TabIndex = 0;
            this.txtValue.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            this.txtValue.UseSystemPasswordChar = false;
            this.txtValue.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.txtValue_PreviewKeyDown);
            // 
            // btnOK
            // 
            this.btnOK.Color = System.Drawing.Color.DodgerBlue;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.ForeColor = System.Drawing.Color.White;
            this.btnOK.Location = new System.Drawing.Point(11, 8);
            this.btnOK.Name = "btnOK";
            this.btnOK.Radius = 2;
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Color = System.Drawing.Color.Firebrick;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(235, 8);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Radius = 2;
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // comboBox
            // 
            this.comboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBox.DropDownHeight = 100;
            this.comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox.FormattingEnabled = true;
            this.comboBox.IntegralHeight = false;
            this.comboBox.ItemHeight = 17;
            this.comboBox.Location = new System.Drawing.Point(12, 69);
            this.comboBox.Name = "comboBox";
            this.comboBox.Size = new System.Drawing.Size(298, 23);
            this.comboBox.TabIndex = 4;
            this.comboBox.Visible = false;
            // 
            // numValue
            // 
            this.numValue.Location = new System.Drawing.Point(12, 69);
            this.numValue.Maximum = new decimal(new int[] {
            32765,
            0,
            0,
            0});
            this.numValue.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numValue.Name = "numValue";
            this.numValue.Size = new System.Drawing.Size(156, 23);
            this.numValue.TabIndex = 5;
            this.numValue.ThousandsSeparator = true;
            this.numValue.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numValue.KeyUp += new System.Windows.Forms.KeyEventHandler(this.numValue_KeyUp);
            // 
            // panel1
            // 
            this.panel1.Border = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.panel1.Controls.Add(this.btnOK);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 98);
            this.panel1.Name = "panel1";
            this.panel1.Radius = 0;
            this.panel1.Size = new System.Drawing.Size(321, 39);
            this.panel1.TabIndex = 6;
            // 
            // InputDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(321, 137);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.numValue);
            this.Controls.Add(this.comboBox);
            this.Controls.Add(this.txtValue);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.lblTitle);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Black;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Input";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InputDialog_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.numValue)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SDUI.Controls.Label lblTitle;
        private SDUI.Controls.Label lblMessage;
        private TextBox txtValue;
        private SDUI.Controls.Button btnOK;
        private SDUI.Controls.Button btnCancel;
        private ComboBox comboBox;
        private System.Windows.Forms.NumericUpDown numValue;
        private Panel panel1;
    }
}