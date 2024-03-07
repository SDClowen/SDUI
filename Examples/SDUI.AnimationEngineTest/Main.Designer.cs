namespace SDUI.AnimationEngineTest
{
    partial class Main
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
            menuCloseButton1 = new MenuCloseButton();
            SuspendLayout();
            // 
            // menuCloseButton1
            // 
            menuCloseButton1.Extended = false;
            menuCloseButton1.Location = new Point(114, 71);
            menuCloseButton1.Name = "menuCloseButton1";
            menuCloseButton1.Size = new Size(40, 40);
            menuCloseButton1.TabIndex = 0;
            menuCloseButton1.Text = "menuCloseButton1";
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(269, 200);
            Controls.Add(menuCloseButton1);
            Name = "Main";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private MenuCloseButton menuCloseButton1;
    }
}
