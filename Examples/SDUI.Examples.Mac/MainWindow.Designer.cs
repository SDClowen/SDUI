namespace SDUI.Examples.Mac
{
    partial class MainWindow
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
            tabControl = new Controls.TabControl();
            tabPage1 = new TabPage();
            textBox1 = new Controls.TextBox();
            button1 = new Controls.Button();
            panel3 = new Controls.Panel();
            panel6 = new Controls.Panel();
            panel5 = new Controls.Panel();
            panel4 = new Controls.Panel();
            numUpDown1 = new Controls.NumUpDown();
            label2 = new Controls.Label();
            label6 = new Controls.Label();
            label5 = new Controls.Label();
            label4 = new Controls.Label();
            label3 = new Controls.Label();
            separator2 = new Controls.Separator();
            separator1 = new Controls.Separator();
            panel2 = new Controls.Panel();
            radio3 = new Controls.Radio();
            radio2 = new Controls.Radio();
            radio1 = new Controls.Radio();
            label1 = new Controls.Label();
            tabPage2 = new TabPage();
            panel1 = new Controls.Panel();
            formControlBox1 = new Controls.FormControlBox();
            tabControl.SuspendLayout();
            tabPage1.SuspendLayout();
            panel3.SuspendLayout();
            panel2.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPage1);
            tabControl.Controls.Add(tabPage2);
            tabControl.Dock = DockStyle.Fill;
            tabControl.ItemSize = new Size(120, 30);
            tabControl.Location = new Point(248, 0);
            tabControl.Margin = new Padding(3, 4, 3, 4);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(720, 675);
            tabControl.SizeMode = TabSizeMode.Fixed;
            tabControl.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.BackColor = Color.White;
            tabPage1.Controls.Add(textBox1);
            tabPage1.Controls.Add(button1);
            tabPage1.Controls.Add(panel3);
            tabPage1.Controls.Add(panel2);
            tabPage1.Location = new Point(4, 34);
            tabPage1.Margin = new Padding(3, 4, 3, 4);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3, 4, 3, 4);
            tabPage1.Size = new Size(712, 637);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "tabPage1";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(36, 378);
            textBox1.MaxLength = 32767;
            textBox1.MultiLine = false;
            textBox1.Name = "textBox1";
            textBox1.PassFocusShow = true;
            textBox1.Radius = 2;
            textBox1.Size = new Size(135, 25);
            textBox1.TabIndex = 2;
            textBox1.TextAlignment = HorizontalAlignment.Left;
            textBox1.UseSystemPasswordChar = true;
            // 
            // button1
            // 
            button1.Color = Color.MediumSeaGreen;
            button1.ForeColor = Color.White;
            button1.Location = new Point(600, 351);
            button1.Name = "button1";
            button1.Radius = 8;
            button1.ShadowDepth = 4F;
            button1.Size = new Size(91, 29);
            button1.TabIndex = 1;
            button1.Text = "Save";
            button1.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            panel3.BackColor = Color.Transparent;
            panel3.Border = new Padding(0, 0, 0, 0);
            panel3.BorderColor = Color.Transparent;
            panel3.Controls.Add(panel6);
            panel3.Controls.Add(panel5);
            panel3.Controls.Add(panel4);
            panel3.Controls.Add(numUpDown1);
            panel3.Controls.Add(label2);
            panel3.Controls.Add(label6);
            panel3.Controls.Add(label5);
            panel3.Controls.Add(label4);
            panel3.Controls.Add(label3);
            panel3.Controls.Add(separator2);
            panel3.Controls.Add(separator1);
            panel3.Location = new Point(36, 8);
            panel3.Margin = new Padding(3, 4, 3, 4);
            panel3.Name = "panel3";
            panel3.Radius = 10;
            panel3.ShadowDepth = 0F;
            panel3.Size = new Size(655, 186);
            panel3.TabIndex = 0;
            // 
            // panel6
            // 
            panel6.BackColor = Color.Transparent;
            panel6.Border = new Padding(0, 0, 0, 0);
            panel6.BorderColor = Color.Yellow;
            panel6.Location = new Point(567, 24);
            panel6.Name = "panel6";
            panel6.Radius = 10;
            panel6.ShadowDepth = 4F;
            panel6.Size = new Size(64, 48);
            panel6.TabIndex = 5;
            // 
            // panel5
            // 
            panel5.BackColor = Color.Transparent;
            panel5.Border = new Padding(0, 0, 0, 0);
            panel5.BorderColor = Color.Black;
            panel5.Location = new Point(497, 24);
            panel5.Name = "panel5";
            panel5.Radius = 10;
            panel5.ShadowDepth = 4F;
            panel5.Size = new Size(64, 48);
            panel5.TabIndex = 5;
            // 
            // panel4
            // 
            panel4.BackColor = Color.Transparent;
            panel4.Border = new Padding(0, 0, 0, 0);
            panel4.BorderColor = Color.White;
            panel4.Location = new Point(427, 24);
            panel4.Name = "panel4";
            panel4.Radius = 10;
            panel4.ShadowDepth = 4F;
            panel4.Size = new Size(64, 48);
            panel4.TabIndex = 5;
            // 
            // numUpDown1
            // 
            numUpDown1.BackColor = Color.Transparent;
            numUpDown1.Font = new Font("Segoe UI", 9.25F, FontStyle.Regular, GraphicsUnit.Point);
            numUpDown1.Location = new Point(558, 123);
            numUpDown1.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numUpDown1.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numUpDown1.MinimumSize = new Size(80, 25);
            numUpDown1.Name = "numUpDown1";
            numUpDown1.Size = new Size(80, 25);
            numUpDown1.TabIndex = 4;
            numUpDown1.Text = "numUpDown1";
            numUpDown1.Value = new decimal(new int[] { 32, 0, 0, 0 });
            // 
            // label2
            // 
            label2.ApplyGradient = false;
            label2.AutoSize = true;
            label2.ForeColor = Color.FromArgb(0, 0, 0);
            label2.Gradient = (new Color[] { Color.Gray, Color.Black });
            label2.Location = new Point(19, 24);
            label2.Name = "label2";
            label2.Size = new Size(89, 20);
            label2.TabIndex = 3;
            label2.Text = "Appereance";
            // 
            // label6
            // 
            label6.ApplyGradient = false;
            label6.AutoSize = true;
            label6.ForeColor = Color.FromArgb(0, 0, 0);
            label6.Gradient = (new Color[] { Color.Gray, Color.Black });
            label6.Location = new Point(579, 75);
            label6.Name = "label6";
            label6.Size = new Size(41, 20);
            label6.TabIndex = 3;
            label6.Text = "Auto";
            // 
            // label5
            // 
            label5.ApplyGradient = false;
            label5.AutoSize = true;
            label5.ForeColor = Color.FromArgb(0, 0, 0);
            label5.Gradient = (new Color[] { Color.Gray, Color.Black });
            label5.Location = new Point(509, 75);
            label5.Name = "label5";
            label5.Size = new Size(40, 20);
            label5.TabIndex = 3;
            label5.Text = "Dark";
            // 
            // label4
            // 
            label4.ApplyGradient = false;
            label4.AutoSize = true;
            label4.ForeColor = Color.FromArgb(0, 0, 0);
            label4.Gradient = (new Color[] { Color.Gray, Color.Black });
            label4.Location = new Point(437, 75);
            label4.Name = "label4";
            label4.Size = new Size(42, 20);
            label4.TabIndex = 3;
            label4.Text = "Light";
            // 
            // label3
            // 
            label3.ApplyGradient = false;
            label3.AutoSize = true;
            label3.ForeColor = Color.FromArgb(0, 0, 0);
            label3.Gradient = (new Color[] { Color.Gray, Color.Black });
            label3.Location = new Point(19, 126);
            label3.Name = "label3";
            label3.Size = new Size(119, 20);
            label3.TabIndex = 3;
            label3.Text = "Header icon size";
            // 
            // separator2
            // 
            separator2.IsVertical = false;
            separator2.Location = new Point(19, 102);
            separator2.Margin = new Padding(3, 4, 3, 4);
            separator2.Name = "separator2";
            separator2.Size = new Size(619, 10);
            separator2.TabIndex = 0;
            separator2.Text = "separator1";
            // 
            // separator1
            // 
            separator1.IsVertical = false;
            separator1.Location = new Point(19, 155);
            separator1.Margin = new Padding(3, 4, 3, 4);
            separator1.Name = "separator1";
            separator1.Size = new Size(619, 10);
            separator1.TabIndex = 0;
            separator1.Text = "separator1";
            // 
            // panel2
            // 
            panel2.BackColor = Color.Transparent;
            panel2.Border = new Padding(0, 0, 0, 0);
            panel2.BorderColor = Color.Transparent;
            panel2.Controls.Add(radio3);
            panel2.Controls.Add(radio2);
            panel2.Controls.Add(radio1);
            panel2.Controls.Add(label1);
            panel2.Location = new Point(36, 212);
            panel2.Margin = new Padding(3, 4, 3, 4);
            panel2.Name = "panel2";
            panel2.Radius = 10;
            panel2.ShadowDepth = 0F;
            panel2.Size = new Size(655, 132);
            panel2.TabIndex = 0;
            // 
            // radio3
            // 
            radio3.AutoSize = true;
            radio3.Location = new Point(19, 96);
            radio3.Margin = new Padding(0);
            radio3.Name = "radio3";
            radio3.Ripple = true;
            radio3.Size = new Size(78, 30);
            radio3.TabIndex = 2;
            radio3.TabStop = true;
            radio3.Text = "Always";
            radio3.UseVisualStyleBackColor = true;
            // 
            // radio2
            // 
            radio2.AutoSize = true;
            radio2.Location = new Point(19, 66);
            radio2.Margin = new Padding(0);
            radio2.Name = "radio2";
            radio2.Ripple = true;
            radio2.Size = new Size(133, 30);
            radio2.TabIndex = 2;
            radio2.TabStop = true;
            radio2.Text = "When scrolling";
            radio2.UseVisualStyleBackColor = true;
            // 
            // radio1
            // 
            radio1.AutoSize = true;
            radio1.Location = new Point(19, 36);
            radio1.Margin = new Padding(0);
            radio1.Name = "radio1";
            radio1.Ripple = true;
            radio1.Size = new Size(324, 30);
            radio1.TabIndex = 2;
            radio1.TabStop = true;
            radio1.Text = "Automatically based on mouse or trackpad";
            radio1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.ApplyGradient = false;
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            label1.ForeColor = Color.FromArgb(0, 0, 0);
            label1.Gradient = (new Color[] { Color.Gray, Color.Black });
            label1.Location = new Point(19, 11);
            label1.Name = "label1";
            label1.Size = new Size(131, 20);
            label1.TabIndex = 1;
            label1.Text = "Show scroll bars...";
            // 
            // tabPage2
            // 
            tabPage2.BackColor = Color.White;
            tabPage2.Location = new Point(4, 34);
            tabPage2.Margin = new Padding(3, 4, 3, 4);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3, 4, 3, 4);
            tabPage2.Size = new Size(712, 637);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            // 
            // panel1
            // 
            panel1.BackColor = Color.Transparent;
            panel1.Border = new Padding(0, 0, 0, 0);
            panel1.BorderColor = Color.Transparent;
            panel1.Controls.Add(formControlBox1);
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(3, 4, 3, 4);
            panel1.Name = "panel1";
            panel1.Radius = 0;
            panel1.ShadowDepth = 0F;
            panel1.Size = new Size(248, 675);
            panel1.TabIndex = 1;
            // 
            // formControlBox1
            // 
            formControlBox1.BackColor = Color.Transparent;
            formControlBox1.EnableMaximize = true;
            formControlBox1.Font = new Font("Webdings", 9F, FontStyle.Regular, GraphicsUnit.Point);
            formControlBox1.IsVertical = false;
            formControlBox1.Location = new Point(3, 4);
            formControlBox1.Margin = new Padding(3, 4, 3, 4);
            formControlBox1.Name = "formControlBox1";
            formControlBox1.Size = new Size(74, 31);
            formControlBox1.TabIndex = 2;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(968, 675);
            Controls.Add(tabControl);
            Controls.Add(panel1);
            Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            Location = new Point(0, 0);
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainWindow";
            ShowTitle = false;
            Text = "SDUI Examples Mac";
            tabControl.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Controls.TabControl tabControl;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Controls.Panel panel1;
        private Controls.Button button1;
        private Controls.Panel panel3;
        private Controls.Panel panel6;
        private Controls.Panel panel5;
        private Controls.Panel panel4;
        private Controls.NumUpDown numUpDown1;
        private Controls.Label label2;
        private Controls.Label label6;
        private Controls.Label label5;
        private Controls.Label label4;
        private Controls.Label label3;
        private Controls.Separator separator2;
        private Controls.Separator separator1;
        private Controls.Panel panel2;
        private Controls.Radio radio3;
        private Controls.Radio radio2;
        private Controls.Radio radio1;
        private Controls.Label label1;
        private Controls.FormControlBox formControlBox1;
        private Controls.TextBox textBox1;
    }
}