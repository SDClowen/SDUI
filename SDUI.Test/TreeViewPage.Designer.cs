namespace SDUI.Demo
{
    partial class TreeViewPage
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.treeView1 = new SDUI.Controls.TreeView();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.BackColor = SkiaSharp.SKColor.Transparent;
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.SKPoint(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new SkiaSharp.SKSize(1075, 646);
            this.treeView1.TabIndex = 0;
            // 
            // TreeViewPage
            // 
            this.Controls.Add(this.treeView1);
            this.Name = "TreeViewPage";
            this.Size = new SkiaSharp.SKSize(1075, 646);
            this.ResumeLayout(false);
        }

        #endregion

        private SDUI.Controls.TreeView treeView1;
    }
}