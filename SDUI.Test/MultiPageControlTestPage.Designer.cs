namespace SDUI.Test
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
            this.buttonRemoveTab = new SDUI.Controls.Button();
            this.buttonAddTab = new SDUI.Controls.Button();
            this.buttonPrev = new SDUI.Controls.Button();
            this.buttonNext = new SDUI.Controls.Button();
            this.formControlBox1 = new SDUI.Controls.FormControlBox();
            this.formControlBox2 = new SDUI.Controls.FormControlBox();
            this.SuspendLayout();
            // 
            // multiPageControl
            // 
            this.multiPageControl.BackColor = System.Drawing.Color.Transparent;
            this.multiPageControl.HeaderControlSize = new System.Drawing.Size(80, 30);
            this.multiPageControl.Location = new System.Drawing.Point(51, 68);
            this.multiPageControl.Name = "multiPageControl";
            this.multiPageControl.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
            this.multiPageControl.SelectedIndex = 0;
            this.multiPageControl.Size = new System.Drawing.Size(974, 420);
            this.multiPageControl.TabIndex = 6;
            // 
            // buttonRemoveTab
            // 
            this.buttonRemoveTab.Color = System.Drawing.Color.Transparent;
            this.buttonRemoveTab.Location = new System.Drawing.Point(142, 39);
            this.buttonRemoveTab.Name = "buttonRemoveTab";
            this.buttonRemoveTab.Radius = 6;
            this.buttonRemoveTab.ShadowDepth = 4F;
            this.buttonRemoveTab.Size = new System.Drawing.Size(75, 23);
            this.buttonRemoveTab.TabIndex = 7;
            this.buttonRemoveTab.Text = "Remove Tab";
            this.buttonRemoveTab.UseVisualStyleBackColor = true;
            this.buttonRemoveTab.Click += new System.EventHandler(this.buttonRemoveTab_Click);
            // 
            // buttonAddTab
            // 
            this.buttonAddTab.Color = System.Drawing.Color.Transparent;
            this.buttonAddTab.Location = new System.Drawing.Point(51, 39);
            this.buttonAddTab.Name = "buttonAddTab";
            this.buttonAddTab.Radius = 6;
            this.buttonAddTab.ShadowDepth = 4F;
            this.buttonAddTab.Size = new System.Drawing.Size(75, 23);
            this.buttonAddTab.TabIndex = 8;
            this.buttonAddTab.Text = "Add tab";
            this.buttonAddTab.UseVisualStyleBackColor = true;
            this.buttonAddTab.Click += new System.EventHandler(this.buttonAddTab_Click);
            // 
            // buttonPrev
            // 
            this.buttonPrev.Color = System.Drawing.Color.Transparent;
            this.buttonPrev.Font = new System.Drawing.Font("Webdings", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.buttonPrev.Location = new System.Drawing.Point(223, 39);
            this.buttonPrev.Name = "buttonPrev";
            this.buttonPrev.Radius = 6;
            this.buttonPrev.ShadowDepth = 4F;
            this.buttonPrev.Size = new System.Drawing.Size(20, 23);
            this.buttonPrev.TabIndex = 7;
            this.buttonPrev.Text = "3";
            this.buttonPrev.UseVisualStyleBackColor = true;
            this.buttonPrev.Click += new System.EventHandler(this.buttonPrev_Click);
            // 
            // buttonNext
            // 
            this.buttonNext.Color = System.Drawing.Color.Transparent;
            this.buttonNext.Font = new System.Drawing.Font("Webdings", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.buttonNext.Location = new System.Drawing.Point(249, 39);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Radius = 6;
            this.buttonNext.ShadowDepth = 4F;
            this.buttonNext.Size = new System.Drawing.Size(20, 23);
            this.buttonNext.TabIndex = 7;
            this.buttonNext.Text = "4";
            this.buttonNext.UseVisualStyleBackColor = true;
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // formControlBox1
            // 
            this.formControlBox1.BackColor = System.Drawing.Color.Transparent;
            this.formControlBox1.EnableMaximize = true;
            this.formControlBox1.Font = new System.Drawing.Font("Webdings", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.formControlBox1.IsVertical = true;
            this.formControlBox1.Location = new System.Drawing.Point(22, 39);
            this.formControlBox1.Name = "formControlBox1";
            this.formControlBox1.Size = new System.Drawing.Size(23, 65);
            this.formControlBox1.TabIndex = 9;
            this.formControlBox1.Text = "formControlBox1";
            // 
            // formControlBox2
            // 
            this.formControlBox2.BackColor = System.Drawing.Color.Transparent;
            this.formControlBox2.EnableMaximize = true;
            this.formControlBox2.Font = new System.Drawing.Font("Webdings", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.formControlBox2.IsVertical = false;
            this.formControlBox2.Location = new System.Drawing.Point(22, 10);
            this.formControlBox2.Name = "formControlBox2";
            this.formControlBox2.Size = new System.Drawing.Size(65, 23);
            this.formControlBox2.TabIndex = 9;
            this.formControlBox2.Text = "formControlBox1";
            // 
            // MultiPageControlTestPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.formControlBox2);
            this.Controls.Add(this.formControlBox1);
            this.Controls.Add(this.buttonNext);
            this.Controls.Add(this.buttonPrev);
            this.Controls.Add(this.buttonRemoveTab);
            this.Controls.Add(this.buttonAddTab);
            this.Controls.Add(this.multiPageControl);
            this.Name = "MultiPageControlTestPage";
            this.Size = new System.Drawing.Size(1083, 552);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.MultiPageControl multiPageControl;
        private Controls.Button buttonRemoveTab;
        private Controls.Button buttonAddTab;
        private Controls.Button buttonPrev;
        private Controls.Button buttonNext;
        private Controls.FormControlBox formControlBox1;
        private Controls.FormControlBox formControlBox2;
    }
}
