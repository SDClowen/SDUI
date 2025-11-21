using SDUI.Controls;
using System;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Demo
{
    public partial class ConfigPage : SDUI.Controls.UIElementBase
    {
        public ConfigPage()
        {
            InitializeComponent();
            this.Text = "Config";
            comboBoxHatchType.Items.AddRange(Enum.GetNames<HatchStyle>());

            // TabControl kullanımı:
            // var tc = new TabControl();
            // tc.AddPage(new TabPage() { Text = "General", Controls = { new GeneralPage() { Dock = DockStyle.Fill } } });
            // tc.AddPage(new TabPage() { Text = "ListView", Controls = { new ListViewPage() { Dock = DockStyle.Fill } } });
            // tc.Dock = DockStyle.Bottom;
            // this.Controls.Add(tc);
        }

        private void checkBoxDrawFullHatch_CheckedChanged(object sender, EventArgs e)
        {
            var form = FindForm();
            if (form == null)
                return;

            var parent = form as UIWindow;
            if (parent == null)
                return;

            parent.FullDrawHatch = checkBoxDrawFullHatch.Checked;
        }

        private void numIconWidth_ValueChanged(object sender, EventArgs e)
        {
            var form = FindForm();
            if (form == null)
                return;

            var parent = form as UIWindow;

            parent.IconWidth = (int)numIconWidth.Value;
        }

        private void numTitleHeight_ValueChanged(object sender, EventArgs e)
        {
            var form = FindForm();
            if (form == null)
                return;

            var parent = form as UIWindow;

            parent.TitleHeight = (int)numTitleHeight.Value;
        }

        private void comboBoxHatchType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Enum.TryParse<HatchStyle>(comboBoxHatchType.SelectedItem.ToString(), out var @enum))
                return;

            var form = FindForm();
            if (form == null)
                return;

            var parent = form as UIWindow;

            parent.Hatch = @enum;
            parent.Invalidate();
        }

        private void checkBoxTitleBorder_CheckedChanged(object sender, EventArgs e)
        {
            var form = FindForm();
            if (form == null)
                return;

            var parent = form as UIWindow;
            parent.DrawTitleBorder = checkBoxTitleBorder.Checked;
            parent.Invalidate();
        }

        private void buttonSelectColor_Click(object sender, EventArgs e)
        {
            var form = FindForm();
            if (form == null)
                return;

            var parent = form as UIWindow;

            var colorpicker = new ColorDialog();
            colorpicker.ShowDialog();
            parent.TitleColor = colorpicker.Color;
            parent.Invalidate();
        }

        private void buttonBorderColor_Click(object sender, EventArgs e)
        {
            var form = FindForm();
            if (form == null)
                return;

            var parent = form as UIWindow;

            var colorpicker = new ColorDialog();
            colorpicker.ShowDialog();
            parent.BorderColor = colorpicker.Color;
            parent.Invalidate();
        }

        private void buttonSelectFont_Click(object sender, EventArgs e)
        {
            var form = FindForm();
            if (form == null)
                return;

            var parent = form as UIWindow;

            var fontDialog = new FontDialog();
            fontDialog.Font = parent.Font;
            fontDialog.ShowColor = true;
            fontDialog.ShowApply = true;
            fontDialog.Apply += FontDialog_Apply;
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                parent.Font = fontDialog.Font;
                parent.Invalidate();
            }
        }

        private void FontDialog_Apply(object? sender, EventArgs e)
        {
            var form = FindForm();
            if (form == null)
                return;

            var dialog = sender as FontDialog;
            var parent = form as UIWindow;
            parent.Font = dialog.Font;
            parent.Invalidate();
        }

        private void checkBoxToggleTitle_CheckedChanged(object sender, EventArgs e)
        {
            var form = FindForm();
            if (form == null)
                return;

            var parent = form as UIWindow;
            parent.ShowTitle = !parent.ShowTitle;
        }
    }
}
