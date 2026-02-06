using SDUI.Controls;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
 // SKPoint, Size

namespace SDUI.Demo
{
    // Basit tasarýmcý pencere: yüzeye kontrol ekleme ve Derle ile Designer.cs üretme
    public class DesignerWindow : UIWindow
    {
        private readonly SDUI.Controls.Panel _designSurface;
        private readonly SDUI.Controls.Panel _toolBar;
        private readonly SDUI.Controls.Button _btnAddButton;
        private readonly SDUI.Controls.Button _btnAddCombo;
        private readonly SDUI.Controls.Button _btnAddPanel;
        private readonly SDUI.Controls.Button _btnCompile;
        private int _nameIndex = 1;
        private SKPoint _dragOffset;
        private UIElementBase _dragging;

        public DesignerWindow()
        {
            Text = "SDUI Designer";
            Width = 1000;
            Height = 700;
            StartPosition = FormStartPosition.CenterScreen;

            _toolBar = new SDUI.Controls.Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                Border = new Padding(0,0,0,1),
                Name = "toolPanel"
            };
            Controls.Add(_toolBar);

            _btnAddButton = MakeToolButton("+ Button", (s,e)=> AddControl(new SDUI.Controls.Button(){ Text = "Button", Width=120, Height=36 }));
            _btnAddCombo = MakeToolButton("+ Combo", (s,e)=> AddControl(new SDUI.Controls.ComboBox(){ Width=180, Height=32, Name = NewName("combo") }));
            _btnAddPanel = MakeToolButton("+ Panel", (s,e)=> AddControl(new SDUI.Controls.Panel(){ Width=250, Height=150, Border=new Padding(1), Name = NewName("panel") }));
            _btnCompile = MakeToolButton("Derle", (s,e)=> GenerateDesignerCode());

            _toolBar.Controls.Add(_btnAddButton);
            _toolBar.Controls.Add(_btnAddCombo);
            _toolBar.Controls.Add(_btnAddPanel);
            _toolBar.Controls.Add(_btnCompile);

            LayoutToolbar();

            _designSurface = new SDUI.Controls.Panel
            {
                Dock = DockStyle.Fill,
                Name = "designSurface",
                Border = new Padding(1)
            };
            Controls.Add(_designSurface);

            // Mouse ile sürükleme
            _designSurface.ControlAdded += (s,e)=>
            {
                if(e is UIElementEventArgs args && args.Element is UIElementBase el)
                {
                    el.Cursor = Cursors.SizeAll;
                    el.MouseDown += Element_MouseDown;
                    el.MouseMove += Element_MouseMove;
                    el.MouseUp += Element_MouseUp;
                }
            };
        }

        private void LayoutToolbar()
        {
            int x = 8;
            foreach(var btn in _toolBar.Controls.OfType<SDUI.Controls.Button>())
            {
                btn.Location = new SKPoint(x, 6);
                x += btn.Width + 8;
            }
        }

        private SDUI.Controls.Button MakeToolButton(string text, EventHandler handler)
        {
            var b = new SDUI.Controls.Button
            {
                Text = text,
                Height = 36,
                Width = 100,
                Name = NewName("toolBtn")
            };
            b.Click += handler;
            return b;
        }

        private string NewName(string prefix) => $"{prefix}{_nameIndex++}";

        private void AddControl(UIElementBase element)
        {
            element.Name = string.IsNullOrEmpty(element.Name) ? NewName(element.GetType().Name.ToLower()) : element.Name;
            element.Location = new SKPoint(30 * (_designSurface.Controls.Count+1), 30);
            _designSurface.Controls.Add(element);
        }

        private void Element_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is UIElementBase el && e.Button == MouseButtons.Left)
            {
                _dragging = el;
                _dragOffset = e.Location;
            }
        }

        private void Element_MouseMove(object sender, MouseEventArgs e)
        {
            if(_dragging != null && e.Button == MouseButtons.Left)
            {
                var newLoc = _dragging.Location;
                newLoc.Offset(e.X - _dragOffset.X, e.Y - _dragOffset.Y);
                _dragging.Location = newLoc;
                _designSurface.Invalidate();
            }
        }

        private void Element_MouseUp(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                _dragging = null;
            }
        }

        private void GenerateDesignerCode()
        {
            var sb = new StringBuilder();
            sb.AppendLine("using SDUI.Controls;");
            sb.AppendLine("");
            sb.AppendLine();
            sb.AppendLine("namespace SDUI.DesignerOutput");
            sb.AppendLine("{");
            sb.AppendLine("    public static class Designer");
            sb.AppendLine("    {");
            sb.AppendLine("        // Otomatik üretilen kod. DesignerWindow tarafýndan oluþturuldu.");
            sb.AppendLine("        public static void Build(UIWindow window)");
            sb.AppendLine("        {");
            foreach (var el in _designSurface.Controls.OfType<UIElementBase>())
            {
                var typeName = el.GetType().FullName; // SDUI.Controls.Button vb.
                sb.AppendLine($"            var {el.Name} = new {typeName}()");
                sb.AppendLine("            {");
                sb.AppendLine($"                Name = \"{el.Name}\",");
                sb.AppendLine($"                Text = \"{(el is SDUI.Controls.Button btn ? btn.Text : el.Text)}\",");
                sb.AppendLine($"                Width = {el.Width}, Height = {el.Height},");
                sb.AppendLine($"                Location = new SKPoint({el.Location.X}, {el.Location.Y})");
                sb.AppendLine("            };\n");
                sb.AppendLine($"            window.Controls.Add({el.Name});");
            }
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            try
            {
                var targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Designer.cs");
                File.WriteAllText(targetPath, sb.ToString(), Encoding.UTF8);
                MessageBox.Show($"Designer.cs üretildi: {targetPath}", "Derle", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Yazma hatasý: " + ex.Message, "Derle", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
