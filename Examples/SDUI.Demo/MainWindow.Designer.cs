namespace SDUI.Demo
{
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
            button1 = new Controls.Button();
            checkBox1 = new Controls.CheckBox();
            comboBox1 = new Controls.ComboBox();
            separator1 = new Controls.Separator();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Color = Color.Transparent;
            button1.Location = new Point(15, 43);
            button1.Name = "button1";
            button1.Radius = 6;
            button1.ShadowDepth = 4F;
            button1.Size = new Size(94, 29);
            button1.TabIndex = 0;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Depth = 0;
            checkBox1.Location = new Point(15, 87);
            checkBox1.Margin = new Padding(0);
            checkBox1.MouseLocation = new Point(-1, -1);
            checkBox1.Name = "checkBox1";
            checkBox1.Ripple = true;
            checkBox1.Size = new Size(105, 30);
            checkBox1.TabIndex = 1;
            checkBox1.Text = "checkBox1";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            comboBox1.DrawMode = DrawMode.OwnerDrawVariable;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "dafgadfg", "asdfasdfasdf", "asdfasdfasd" });
            comboBox1.Location = new Point(15, 135);
            comboBox1.Name = "comboBox1";
            comboBox1.Radius = 5;
            comboBox1.ShadowDepth = 4F;
            comboBox1.Size = new Size(151, 28);
            comboBox1.TabIndex = 2;
            // 
            // separator1
            // 
            separator1.IsVertical = false;
            separator1.Location = new Point(83, 187);
            separator1.Name = "separator1";
            separator1.Size = new Size(150, 8);
            separator1.TabIndex = 3;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(1279, 604);
            Controls.Add(separator1);
            Controls.Add(comboBox1);
            Controls.Add(checkBox1);
            Controls.Add(button1);
            DwmMargin = -1;
            Location = new Point(0, 0);
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainWindow";
            Text = "MainWindow";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Controls.Button button1;
        private Controls.CheckBox checkBox1;
        private Controls.ComboBox comboBox1;
        private Controls.Separator separator1;
    }
}