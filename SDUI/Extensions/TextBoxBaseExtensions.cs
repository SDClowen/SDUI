using SDUI.Controls;
using System.IO;
using System.Text;

namespace System.Windows.Forms;

public static class TextBoxBaseExtensions
{
    /// <summary>
    ///     Sync lock object
    /// </summary>
    private static readonly object _lock = new();

    /// <summary>
    ///     Append string to type in the text controls
    /// </summary>
    /// <param name="value">The TextBoxBase</param>
    /// <param name="str">The string to type in the <seealso cref="TextBoxBase" /></param>
    /// <param name="time">The time</param>
    public static void Write(this TextBox value, string str, bool time = true, bool writeToFile = false,
        string filePath = "")
    {
        var stringBuilder = new StringBuilder();
        if (time)
            stringBuilder.Append(DateTime.Now.ToString("[hh:mm:ss]\t"));

        stringBuilder.Append(str);
        stringBuilder.Append(Environment.NewLine);

        value.RunInUIThread(() =>
        {
            value.AppendText(stringBuilder.ToString());
            value.ScrollToCaret();
        });

        if (writeToFile)
            lock (_lock)
            {
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (var stream = File.AppendText(filePath))
                {
                    stream.Write(stringBuilder.ToString());
                }
            }
    }

    public static void AppendText(this TextBox value, string text)
    {
        value.Text += text;
    }

    /// <summary>
    ///     Run action a required thread on controls
    /// </summary>
    /// <param name="target">The target</param>
    /// <param name="action">The action</param>
    public static void RunInUIThread(this UIElementBase target, Action action)
    {
        if (target.InvokeRequired)
            target.Invoke(action);
        else
            action();
    }
}