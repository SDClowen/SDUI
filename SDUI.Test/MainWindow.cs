using SDUI.Controls;
using SDUI.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDUI.Test;

public partial class MainWindow : UIWindow
{
    public MainWindow()
    {
        InitializeComponent();
        comboBox6.Items.AddRange(Enum.GetNames<HatchStyle>());
        comboBoxHatchType.Items.AddRange(Enum.GetNames<HatchStyle>());

        toolTip1.SetToolTip(progressBar8, "Test tooltip");
    }

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
        if (Controls.Count == 0)
            return;

        listView1.Items.Clear();
        listView2.Items.Clear();

        var group1 = new ListViewGroup("Group 1");
        var group2 = new ListViewGroup("Group 2 (collapsible)");
        listView1.Groups.Add(group1);
        listView1.Groups.Add(group2);
        for (int i = 0; i <= 5; i++)
        {
            var title = "Item " + i.ToString();
            var listItem = new ListViewItem(new[] { i.ToString(), title + " Column 2", title + " Column 3", title + " Column 4" }, group1);
            if (i == 0)
            {
                listItem.BackColor = ControlPaint.Light(ColorScheme.BackColor, .15f);
                listItem.Font = new Font(Font.FontFamily, 10.25f, FontStyle.Bold);
            }
            listView1.Items.Add(listItem);

            var item2 = new ListViewItem(new[] { title });

            if (i == 5)
            {
                item2.ToolTipText = "Test tooltip";
                item2.Font = new Font("Arial", 12, FontStyle.Bold);
                item2.BackColor = Color.Aquamarine;
                item2.ForeColor = Color.White;
            }

            listView2.Items.Add(item2);
        }

        for (int i = 6; i <= 11; i++)
        {
            string sItem = "Item " + i.ToString();
            listView1.Items.Add(new ListViewItem(new[] { i.ToString(), sItem + " Column 2", sItem + " Column 3", sItem + " Column 4" }, group2));
        }

        listView1.SetGroupInfo(listView1.Handle, 1, NativeMethods.LVGS_COLLAPSIBLE);
    }

    private void MainWindow_Load(object sender, EventArgs e)
    {
        bool dark = WindowsHelper.IsDark();
        var info = new InfoControl
        {
            Name = "overlay",
            Text = "Hello this is centered overlay",
            Width = 150,
            Height = 150
        };

        info.Location = new Point(tabPage2.Width / 2 - info.Width, 0);
        
        tabPage1.Controls.Add(info);

        info.BringToFront();
    }

    private void buttonOpenInputDialog_Click(object sender, EventArgs e)
    {
        var dialog = new InputDialog("The input dialog", "This is a input dialog", "Please set the value!");
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            MessageBox.Show($"The value: {dialog.Value}");
        }
    }

    private void buttonRandomColor_Click(object sender, EventArgs e)
    {
        var random = new Random();
        var r = random.Next(0, 256);
        var g = random.Next(0, 256);
        var b = random.Next(0, 256);

        ColorScheme.BackColor = Color.FromArgb(r, g, b);
        BackColor = ColorScheme.BackColor;
    }

    private void buttonDark_Click(object sender, EventArgs e)
    {
        ColorScheme.BackColor = Color.Black;
        BackColor = ColorScheme.BackColor;
    }

    private void buttonLight_Click(object sender, EventArgs e)
    {
        ColorScheme.BackColor = Color.White;
        BackColor = ColorScheme.BackColor;
    }

    private void button3_Click(object sender, EventArgs e)
    {
        textBox1.Text = "Testing";
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
        label4.Text = textBox1.Text;
    }

    private void textBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) 
            return;

        MessageBox.Show(textBox1.Text);
    }

    private void checkBox3_Click(object sender, EventArgs e)
    {
        if (checkBox3.Checked)
        {
            FormBorderStyle = FormBorderStyle.Sizable;
        }
        else
        {
            FormBorderStyle = FormBorderStyle.None;
        }
    }

    private void listView3_ItemCheck(object sender, ItemCheckEventArgs e)
    {
        //MessageBox.Show($"Current Value: {e.CurrentValue} New Value: {e.NewValue}");
    }

    private void listView3_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
        //MessageBox.Show($"{e} \n Text: {e.Item.Text}");
    }

    private void newToolStripButton_Click(object sender, EventArgs e)
    {
        tabControl.SelectedIndex = 0;
    }

    private void openToolStripButton_Click(object sender, EventArgs e)
    {
        tabControl.SelectedIndex = 1;
    }

    private void saveToolStripButton_Click(object sender, EventArgs e)
    {
        tabControl.SelectedIndex = 2;
    }

    private void buttonAddTab_Click(object sender, EventArgs e)
    {
        multiPageControl.Add();
    }

    private void buttonRemoveTab_Click(object sender, EventArgs e)
    {
        multiPageControl.RemoveAt(multiPageControl.SelectedIndex);
    }

    private void trackBar1_Scroll(object sender, EventArgs e)
    {
        panel2.ShadowDepth = trackBar1.Value;
        progressBar6.Maximum = trackBar1.Maximum;
        progressBar6.Value = trackBar1.Value;
    }

    private void trackBar2_Scroll(object sender, EventArgs e)
    {
        panel2.Radius = trackBar2.Value;
        progressBar7.Maximum = trackBar2.Maximum;
        progressBar7.Value = trackBar2.Value;
    }

    private void checkBox6_CheckedChanged(object sender, EventArgs e)
    {
        ColorScheme.DrawDebugBorders = checkBox6.Checked;
        var backup = BackColor;
        BackColor = BackColor.Determine();
        BackColor = backup;
    }

    private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!Enum.TryParse<HatchStyle>(comboBox6.SelectedItem.ToString(), out var value))
            return;

        progressBar1.HatchType = value;
        progressBar1.Invalidate();
        progressBar2.HatchType = value;
        progressBar2.Invalidate();
        progressBar3.HatchType = value;
        progressBar3.Invalidate();
    }

    private async void button6_Click(object sender, EventArgs e)
    {
        await Task.Delay(5000);
    }

    private void checkBoxDrawFullHatch_CheckedChanged(object sender, EventArgs e)
    {
        FullDrawHatch = checkBoxDrawFullHatch.Checked;
    }

    private void checkBoxDrawTitleBarHatch_CheckedChanged(object sender, EventArgs e)
    {
        DrawHatch = checkBoxDrawTitleBarHatch.Checked;
    }

    private void numIconWidth_ValueChanged(object sender, EventArgs e)
    {
        IconWidth = (int)numIconWidth.Value;
    }

    private void numTitleHeight_ValueChanged(object sender, EventArgs e)
    {
        TitleHeight = (int)numTitleHeight.Value;
    }

    private void comboBoxHatchType_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!Enum.TryParse<HatchStyle>(comboBoxHatchType.SelectedItem.ToString(), out var @enum))
            return;

        Hatch = @enum;
        Invalidate();
    }

    private void buttonSelectColor_Click(object sender, EventArgs e)
    {
        var colorpicker = new ColorDialog();
        colorpicker.ShowDialog();
        TitleColor = colorpicker.Color;
        BackColor = colorpicker.Color;
        ForeColor = BackColor.Determine();
    }
}