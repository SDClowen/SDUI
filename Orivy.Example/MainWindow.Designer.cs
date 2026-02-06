using SDUI.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orivy.Example;

internal partial class MainWindow
{
    internal void InitializeComponent()
    {
        this.SuspendLayout();

        //
        // panel
        this.panel = new()
        {
            Name = "panel",
            Size = new(400, 300),
            Location = new(200, 75),
            Radius = new(4),
            Border = new(0, 1, 0, 1)
        };

        // 
        // MainWindow
        // 
        this.Name = "MainWindow";
        this.Text = "Orivy Example";
        this.Width = 800;
        this.Height = 450;
        this.FormStartPosition = SDUI.FormStartPosition.CenterScreen;
        this.FormBorderStyle = SDUI.FormBorderStyle.None;
        this.ResumeLayout(false);
    }

    private Element panel;
}
