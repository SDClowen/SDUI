using SDUI.Controls;
using SDUI.Rendering;
using System;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDUI.Demo
{
    public partial class ConfigPage : SDUI.Controls.UIElementBase
    {
        private bool _syncing;

        public ConfigPage()
        {
            InitializeComponent();
            this.Text = "Config";
            comboBoxHatchType.Items.AddRange(Enum.GetNames<HatchStyle>());
            comboBoxRenderBackend.Items.AddRange(Enum.GetNames<RenderBackend>());

            // UIElementBase WinForms Control olmadığı için HandleCreated yok.
            // Parent window/handle hazır olduğunda state'i senkronize etmek için CreateControl/VisibleChanged kullanıyoruz.
            CreateControl += (_, __) => SyncFromParent();
            VisibleChanged += (_, __) =>
            {
                if (Visible)
                    SyncFromParent();
            };


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

        private void comboBoxRenderBackend_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_syncing)
                return;

            var form = FindForm();
            if (form is not UIWindow parent)
                return;

            if (!Enum.TryParse<RenderBackend>(comboBoxRenderBackend.SelectedItem?.ToString(), out var backend))
                return;

            parent.RenderBackend = backend;
            parent.Invalidate();
        }

        private void checkBoxPerfOverlay_CheckedChanged(object sender, EventArgs e)
        {
            if (_syncing)
                return;

            var form = FindForm();
            if (form is not UIWindow parent)
                return;

            parent.ShowPerfOverlay = checkBoxPerfOverlay.Checked;
            parent.Invalidate();
        }

        private void colorPicker_SelectedColorChanged(object sender, EventArgs e)
        {
            if (_syncing)
                return;

            var form = FindForm();
            if (form is not UIWindow parent)
                return;

            parent.TitleColor = colorPicker.SelectedColor;
            parent.Invalidate();
        }

        private void SyncFromParent()
        {
            var form = FindForm();
            if (form is not UIWindow parent)
                return;

            _syncing = true;
            try
            {
                checkBoxPerfOverlay.Checked = parent.ShowPerfOverlay;
                colorPicker.SelectedColor = parent.TitleColor;

                var backendText = parent.RenderBackend.ToString();
                if (!string.Equals(comboBoxRenderBackend.SelectedItem?.ToString(), backendText, StringComparison.Ordinal))
                    comboBoxRenderBackend.SelectedItem = backendText;
            }
            finally
            {
                _syncing = false;
            }
        }
    }
}
