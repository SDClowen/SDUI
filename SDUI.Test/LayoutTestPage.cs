using SDUI.Controls;
using SkiaSharp;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Demo
{
    /// <summary>
    /// Comprehensive test page for new Measure/Arrange layout API
    /// Tests Dock, Anchor, AutoSize and mixed scenarios
    /// </summary>
    public class LayoutTestPage : UIElementBase
    {
        public LayoutTestPage()
        {
            InitializeComponent();
            Text = "Layout Test";
        }

        private void InitializeComponent()
        {
            // Top status bar
            var statusLabel = new SDUI.Controls.Label
            {
                Text = "Layout Test - Measure/Arrange API, Dock, Anchor, AutoSize",
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = SKColors.LightBlue.ToDrawingColor(),
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            Controls.Add(statusLabel);

            // Main container with tests
            var mainPanel = new SDUI.Controls.Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };
            Controls.Add(mainPanel);

            // === DOCK TEST ===
            var dockTestGroup = new SDUI.Controls.GroupBox
            {
                Text = "Dock Test",
                Location = new Point(10, 10),
                Size = new Size(350, 250)
            };
            mainPanel.Controls.Add(dockTestGroup);

            var topPanel = new SDUI.Controls.Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = SKColors.LightCoral.ToDrawingColor()
            };
            topPanel.Controls.Add(new SDUI.Controls.Label
            {
                Text = "Dock.Top",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });
            dockTestGroup.Controls.Add(topPanel);

            var bottomPanel = new SDUI.Controls.Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = SKColors.LightGreen.ToDrawingColor()
            };
            bottomPanel.Controls.Add(new SDUI.Controls.Label
            {
                Text = "Dock.Bottom",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });
            dockTestGroup.Controls.Add(bottomPanel);

            var leftPanel = new SDUI.Controls.Panel
            {
                Dock = DockStyle.Left,
                Width = 80,
                BackColor = SKColors.LightSkyBlue.ToDrawingColor()
            };
            leftPanel.Controls.Add(new SDUI.Controls.Label
            {
                Text = "Left",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });
            dockTestGroup.Controls.Add(leftPanel);

            var rightPanel = new SDUI.Controls.Panel
            {
                Dock = DockStyle.Right,
                Width = 80,
                BackColor = SKColors.LightYellow.ToDrawingColor()
            };
            rightPanel.Controls.Add(new SDUI.Controls.Label
            {
                Text = "Right",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });
            dockTestGroup.Controls.Add(rightPanel);

            var fillPanel = new SDUI.Controls.Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SKColors.LightGray.ToDrawingColor()
            };
            fillPanel.Controls.Add(new SDUI.Controls.Label
            {
                Text = "Dock.Fill",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });
            dockTestGroup.Controls.Add(fillPanel);

            // === ANCHOR TEST ===
            var anchorTestGroup = new SDUI.Controls.GroupBox
            {
                Text = "Anchor Test",
                Location = new Point(370, 10),
                Size = new Size(350, 250)
            };
            mainPanel.Controls.Add(anchorTestGroup);

            var topLeftBtn = new SDUI.Controls.Button
            {
                Text = "Top-Left",
                Location = new Point(10, 20),
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            anchorTestGroup.Controls.Add(topLeftBtn);

            var topRightBtn = new SDUI.Controls.Button
            {
                Text = "Top-Right",
                Location = new Point(260, 20),
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            anchorTestGroup.Controls.Add(topRightBtn);

            var bottomLeftBtn = new SDUI.Controls.Button
            {
                Text = "Bottom-L",
                Location = new Point(10, 210),
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            anchorTestGroup.Controls.Add(bottomLeftBtn);

            var bottomRightBtn = new SDUI.Controls.Button
            {
                Text = "Bottom-R",
                Location = new Point(260, 210),
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            anchorTestGroup.Controls.Add(bottomRightBtn);

            var stretchBtn = new SDUI.Controls.Button
            {
                Text = "Stretch Horizontal",
                Location = new Point(10, 120),
                Size = new Size(330, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            anchorTestGroup.Controls.Add(stretchBtn);

            // === AUTOSIZE TEST ===
            var autoSizeTestGroup = new SDUI.Controls.GroupBox
            {
                Text = "AutoSize Test",
                Location = new Point(10, 270),
                Size = new Size(710, 200)
            };
            mainPanel.Controls.Add(autoSizeTestGroup);

            var testLabel = new SDUI.Controls.Label
            {
                Text = "Short text",
                AutoSize = true,
                Location = new Point(10, 30),
                BackColor = SKColors.LightYellow.ToDrawingColor(),
                Padding = new Padding(5)
            };
            autoSizeTestGroup.Controls.Add(testLabel);

            var changeTextBtn = new SDUI.Controls.Button
            {
                Text = "Change Text",
                Location = new Point(10, 70),
                Size = new Size(120, 30)
            };
            changeTextBtn.Click += (s, e) =>
            {
                testLabel.Text = testLabel.Text == "Short text"
                    ? "This is a much longer text that should trigger AutoSize"
                    : "Short text";
                statusLabel.Text = $"Text changed. Label size: {testLabel.Size}";
            };
            autoSizeTestGroup.Controls.Add(changeTextBtn);

            var testLabel2 = new SDUI.Controls.Label
            {
                Text = "Font test",
                AutoSize = true,
                Location = new Point(200, 30),
                BackColor = SKColors.LightCyan.ToDrawingColor(),
                Padding = new Padding(5)
            };
            autoSizeTestGroup.Controls.Add(testLabel2);

            var changeFontBtn = new SDUI.Controls.Button
            {
                Text = "Change Font",
                Location = new Point(200, 70),
                Size = new Size(120, 30)
            };
            changeFontBtn.Click += (s, e) =>
            {
                testLabel2.Font = testLabel2.Font.Size == 9
                    ? new Font("Segoe UI", 16, FontStyle.Bold)
                    : new Font("Segoe UI", 9);
                statusLabel.Text = $"Font changed. Label size: {testLabel2.Size}";
            };
            autoSizeTestGroup.Controls.Add(changeFontBtn);

            var testLabel3 = new SDUI.Controls.Label
            {
                Text = "Padding",
                AutoSize = true,
                Location = new Point(390, 30),
                BackColor = SKColors.LightGreen.ToDrawingColor(),
                Padding = new Padding(5)
            };
            autoSizeTestGroup.Controls.Add(testLabel3);

            var changePaddingBtn = new SDUI.Controls.Button
            {
                Text = "Change Padding",
                Location = new Point(390, 70),
                Size = new Size(120, 30)
            };
            changePaddingBtn.Click += (s, e) =>
            {
                testLabel3.Padding = testLabel3.Padding.All == 5
                    ? new Padding(20)
                    : new Padding(5);
                statusLabel.Text = $"Padding changed. Label size: {testLabel3.Size}";
            };
            autoSizeTestGroup.Controls.Add(changePaddingBtn);

            // Measure API test
            var measureBtn = new SDUI.Controls.Button
            {
                Text = "Test Measure API",
                Location = new Point(10, 120),
                Size = new Size(150, 35)
            };
            measureBtn.Click += (s, e) =>
            {
                var testControl = new SDUI.Controls.Label
                {
                    Text = "Measure me!",
                    AutoSize = true,
                    Padding = new Padding(10)
                };
                
                var availableSize = new Size(200, 100);
                var measuredSize = testControl.Measure(availableSize);

                statusLabel.Text = $"Measured: {measuredSize.Width}x{measuredSize.Height} | " +
                    $"Available: {availableSize.Width}x{availableSize.Height}";
                
                testControl.Arrange(new Rectangle(50, 50, measuredSize.Width, measuredSize.Height));
            };
            autoSizeTestGroup.Controls.Add(measureBtn);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return new Size(750, 500);
        }
    }
}
