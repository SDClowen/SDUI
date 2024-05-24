namespace SDUI.Test
{
    partial class FluentForm
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
            button2 = new Controls.Button();
            button3 = new Controls.Button();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Color = System.Drawing.Color.Transparent;
            button1.Location = new System.Drawing.Point(365, 282);
            button1.Name = "button1";
            button1.Radius = 6;
            button1.ShadowDepth = 4F;
            button1.Size = new System.Drawing.Size(94, 29);
            button1.TabIndex = 0;
            button1.Text = "Dark";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Color = System.Drawing.Color.Transparent;
            button2.Location = new System.Drawing.Point(465, 282);
            button2.Name = "button2";
            button2.Radius = 6;
            button2.ShadowDepth = 4F;
            button2.Size = new System.Drawing.Size(94, 29);
            button2.TabIndex = 1;
            button2.Text = "Light";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Color = System.Drawing.Color.Transparent;
            button3.Location = new System.Drawing.Point(565, 282);
            button3.Name = "button3";
            button3.Radius = 6;
            button3.ShadowDepth = 4F;
            button3.Size = new System.Drawing.Size(94, 29);
            button3.TabIndex = 2;
            button3.Text = "Transparent";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // FluentForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(994, 572);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            DwmMargin = -1;
            Location = new System.Drawing.Point(0, 0);
            Name = "FluentForm";
            Text = "FluentForm";
            ResumeLayout(false);
        }

        #endregion

        private Controls.Button button1;
        private Controls.Button button2;
        private Controls.Button button3;
    }
}