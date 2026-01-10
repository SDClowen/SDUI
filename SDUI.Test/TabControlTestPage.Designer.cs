using System.Drawing;

namespace SDUI.Demo
{
    partial class TabControlTestPage
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
            this.multiPageControl = new SDUI.Controls.TabControl();
            this.SuspendLayout();
            // 
            // multiPageControl
            // 
            this.multiPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.multiPageControl.Location = new System.Drawing.Point(0, 0);
            this.multiPageControl.Name = "multiPageControl";
            this.multiPageControl.RenderNewPageButton = false;
            this.multiPageControl.RenderPageClose = false;
            this.multiPageControl.RenderPageIcon = false;
            this.multiPageControl.SelectedIndex = -1;
            this.multiPageControl.BorderColor = Color.Red;
            this.multiPageControl.Size = new System.Drawing.Size(1083, 552);
            this.multiPageControl.TabIndex = 6;
            this.multiPageControl.NewPageButtonClicked += new System.EventHandler(this.multiPageControl_NewPageButtonClicked);
            // 
            // MultiPageControlTestPage
            // 
            this.Controls.Add(this.multiPageControl);
            this.Name = "MultiPageControlTestPage";
            this.Size = new System.Drawing.Size(1083, 552);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.TabControl multiPageControl;
    }
}
