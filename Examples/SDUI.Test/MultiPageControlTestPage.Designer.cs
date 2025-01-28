﻿namespace SDUI.Test
{
    partial class MultiPageControlTestPage
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
            this.multiPageControl = new SDUI.Controls.MultiPageControl();
            this.SuspendLayout();
            // 
            // multiPageControl
            // 
            this.multiPageControl.BackColor = System.Drawing.Color.Transparent;
            this.multiPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.multiPageControl.HeaderControlSize = new System.Drawing.Size(80, 30);
            this.multiPageControl.Location = new System.Drawing.Point(0, 0);
            this.multiPageControl.Name = "multiPageControl";
            this.multiPageControl.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
            this.multiPageControl.RenderNewPageButton = true;
            this.multiPageControl.RenderPageClose = true;
            this.multiPageControl.RenderPageIcon = true;
            this.multiPageControl.SelectedIndex = -1;
            this.multiPageControl.Size = new System.Drawing.Size(1083, 552);
            this.multiPageControl.TabIndex = 6;
            this.multiPageControl.NewPageButtonClicked += new System.EventHandler(this.multiPageControl_NewPageButtonClicked);
            // 
            // MultiPageControlTestPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.multiPageControl);
            this.Name = "MultiPageControlTestPage";
            this.Size = new System.Drawing.Size(1083, 552);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.MultiPageControl multiPageControl;
    }
}
