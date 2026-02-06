using System;

namespace SDUI.Controls;

public partial class MessageBox : UIWindow
{
    private readonly MessageBoxButtons _buttons;
    private readonly MessageBoxIcon _icon;

    private MessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        InitializeComponent();
        
        _buttons = buttons;
        _icon = icon;

        Text = caption;
        lblMessage.Text = text;

        ConfigureButtons();
        ConfigureIcon();

        FormBorderStyle = FormBorderStyle.FixedDialog;
        FormStartPosition = FormStartPosition.CenterScreen;
    }

    private void ConfigureButtons()
    {
        btnButton1.Visible = false;
        btnButton2.Visible = false;
        btnButton3.Visible = false;

        switch (_buttons)
        {
            case MessageBoxButtons.OK:
                btnButton2.Visible = true;
                btnButton2.Text = "OK";
                btnButton2.DialogResult = DialogResult.OK;
                btnButton2.Location = new SkiaSharp.SKPoint(ClientSize.Width / 2 - btnButton2.Width / 2, btnButton2.Location.Y);
                break;

            case MessageBoxButtons.OKCancel:
                btnButton1.Visible = true;
                btnButton1.Text = "OK";
                btnButton1.DialogResult = DialogResult.OK;
                
                btnButton2.Visible = true;
                btnButton2.Text = "Cancel";
                btnButton2.DialogResult = DialogResult.Cancel;
                break;

            case MessageBoxButtons.AbortRetryIgnore:
                btnButton1.Visible = true;
                btnButton1.Text = "Abort";
                btnButton1.DialogResult = DialogResult.Abort;
                
                btnButton2.Visible = true;
                btnButton2.Text = "Retry";
                btnButton2.DialogResult = DialogResult.Retry;
                
                btnButton3.Visible = true;
                btnButton3.Text = "Ignore";
                btnButton3.DialogResult = DialogResult.Ignore;
                break;

            case MessageBoxButtons.YesNoCancel:
                btnButton1.Visible = true;
                btnButton1.Text = "Yes";
                btnButton1.DialogResult = DialogResult.Yes;
                
                btnButton2.Visible = true;
                btnButton2.Text = "No";
                btnButton2.DialogResult = DialogResult.No;
                
                btnButton3.Visible = true;
                btnButton3.Text = "Cancel";
                btnButton3.DialogResult = DialogResult.Cancel;
                break;

            case MessageBoxButtons.YesNo:
                btnButton1.Visible = true;
                btnButton1.Text = "Yes";
                btnButton1.DialogResult = DialogResult.Yes;
                
                btnButton2.Visible = true;
                btnButton2.Text = "No";
                btnButton2.DialogResult = DialogResult.No;
                break;

            case MessageBoxButtons.RetryCancel:
                btnButton1.Visible = true;
                btnButton1.Text = "Retry";
                btnButton1.DialogResult = DialogResult.Retry;
                
                btnButton2.Visible = true;
                btnButton2.Text = "Cancel";
                btnButton2.DialogResult = DialogResult.Cancel;
                break;
        }
    }

    private void ConfigureIcon()
    {
        switch (_icon)
        {
            case MessageBoxIcon.None:
                lblIcon.Visible = false;
                lblMessage.Location = new SkiaSharp.SKPoint(20, lblMessage.Location.Y);
                break;

            case MessageBoxIcon.Error:
                lblIcon.Visible = true;
                lblIcon.Text = "✖";
                lblIcon.ForeColor = SkiaSharp.SKColors.Red;
                break;

            case MessageBoxIcon.Question:
                lblIcon.Visible = true;
                lblIcon.Text = "?";
                lblIcon.ForeColor = SkiaSharp.SKColors.DodgerBlue;
                break;

            case MessageBoxIcon.Warning:
                lblIcon.Visible = true;
                lblIcon.Text = "⚠";
                lblIcon.ForeColor = SkiaSharp.SKColors.Orange;
                break;

            case MessageBoxIcon.Information:
                lblIcon.Visible = true;
                lblIcon.Text = "ℹ";
                lblIcon.ForeColor = SkiaSharp.SKColors.DodgerBlue;
                break;
        }
    }

    private void Button_Click(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            DialogResult = btn.DialogResult;
            Close();
        }
    }

    public static DialogResult Show(string text)
    {
        return Show(text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None);
    }

    public static DialogResult Show(string text, string caption)
    {
        return Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);
    }

    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
    {
        return Show(text, caption, buttons, MessageBoxIcon.None);
    }

    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        using var messageBox = new MessageBox(text, caption, buttons, icon);
        messageBox.ShowDialog();
        
        return messageBox.DialogResult;
    }

    public static DialogResult Show(UIElementBase owner, string text)
    {
        return Show(text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None);
    }

    public static DialogResult Show(UIElementBase owner, string text, string caption)
    {
        return Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);
    }

    public static DialogResult Show(UIElementBase owner, string text, string caption, MessageBoxButtons buttons)
    {
        return Show(text, caption, buttons, MessageBoxIcon.None);
    }

    public static DialogResult Show(UIElementBase owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        return Show(text, caption, buttons, icon);
    }
}
