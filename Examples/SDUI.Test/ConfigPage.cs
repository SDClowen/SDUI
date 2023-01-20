using SDUI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDUI.Test
{
    public partial class ConfigPage : UserControl
    {
        public ConfigPage()
        {
            InitializeComponent();
            this.Text = "Config";
            comboBoxHatchType.Items.AddRange(Enum.GetNames<HatchStyle>());
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
            Invalidate();
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
        }
    }
}
