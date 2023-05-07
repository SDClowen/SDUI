using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace SDUI.Controls;

public class MultiPageControlItem : Panel
{
    public Rectangle Rectangle { get; set; }
    public Rectangle RectangleClose { get; set; }
    public Rectangle RectangleIcon { get; set; }

    [Localizable(true), Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
    public override string Text
    {
        get
        {
            return base.Text;
        }
        set
        {
            base.Text = value;
            Parent.Invalidate();
        }
    }

    public MultiPageControlItem()
        : base()
    {
        SetStyle(ControlStyles.CacheText, true);
        Dock = DockStyle.Fill;
        Padding = Padding.Empty;
        BackColor = Color.Transparent;
    }

    public MultiPageControlItem(string text)
        : this()
    {
        Text = text;
    }
}

[Designer(typeof(MultiPageControlDesigner))]
public class MultiPageControl : UserControl
{
    private EventHandler<int> _onSelectedIndexChanged;
    private EventHandler _onNewPageButtonClicked;
    private EventHandler _onClosePageButtonClicked;

    public event EventHandler<int> SelectedIndexChanged
    {
        add => _onSelectedIndexChanged += value;
        remove => _onSelectedIndexChanged -= value;
    }

    public event EventHandler NewPageButtonClicked
    {
        add => _onNewPageButtonClicked += value;
        remove => _onNewPageButtonClicked -= value;
    }

    public event EventHandler ClosePageButtonClicked
    {
        add => _onClosePageButtonClicked += value;
        remove => _onClosePageButtonClicked -= value;
    }

    public MultiPageControl()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint, true);

        Padding = new Padding(0, 30, 0, 0);
        ReorganizePages();
    }

    private MultiPageControlCollection _collection = new();
    [Editor(typeof(MultiPageControlCollectionEditor), typeof(UITypeEditor))]
    [MergableProperty(false)]
    public new MultiPageControlCollection Controls
    {
        get => _collection;
        set => _collection = value;
    }

    private int _selectedIndex = -1;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            var sys = Stopwatch.StartNew();

            //if (_selectedIndex == value)
              //  return;

            if (Controls.Count > 0)
            {
                if (value < 0)
                    value = Controls.Count - 1;

                if (value > Controls.Count - 1)
                    value = 0;
            }
            else
                value = -1;

            var previousSelectedIndex = _selectedIndex;
            _selectedIndex = value;
            _onSelectedIndexChanged?.Invoke(this, previousSelectedIndex);

            for (int i = 0; i < Controls.Count; i++)
                Controls[i].Visible = i == _selectedIndex;

            Debug.WriteLine($"Index: {_selectedIndex} Finished: {sys.ElapsedMilliseconds} ms {Controls.Count} & {Controls.Count}");
            Invalidate();
        }
    }

    public Size _headerControlSize;
    public Size HeaderControlSize
    {
        get => _headerControlSize;
        set
        {
            _headerControlSize = value;
            Invalidate();
        }
    }

    private Point _mouseLocation;
    private GraphicsPath _newButtonPath;
    private int _mouseState;
    private int _lastTabX;

    public bool _renderNewPageButton = true;
    public bool RenderNewPageButton
    {
        get => _renderNewPageButton;
        set
        {
            _renderNewPageButton = value;
            Invalidate();
        }
    }

    public bool _renderPageIcon = true;
    public bool RenderPageIcon
    {
        get => _renderPageIcon;
        set
        {
            _renderPageIcon = value;
            Invalidate();
        }
    }

    public bool _renderPageClose = true;
    public bool RenderPageClose
    {
        get => _renderPageClose;
        set
        {
            _renderPageClose = value;
            Invalidate();
        }
    }

    private void ReorganizePages()
    {
        _lastTabX = 6;// this.Radius / 2;

        for(int i = 0; i < Controls.Count; i++)
        {
            var control = Controls[i];

            var stringSize = TextRenderer.MeasureText(control.Text, Font);
            var width = stringSize.Width + 80;
            var rectangle = new Rectangle(_lastTabX, 6, width, _headerControlSize.Height - 6);
            control.Rectangle = rectangle;
            control.RectangleClose = new Rectangle(rectangle.X + rectangle.Width - 20, rectangle.Y + 6, 12, 12);
            control.RectangleIcon = new Rectangle(rectangle.X + 6, rectangle.Y + 5, 16, 16);
            _lastTabX += width;
        }

        _newButtonPath = new RectangleF(_lastTabX + 4, 9, 24, 16).Radius(6, 12, 12, 6);
    }

    public MultiPageControlItem Add()
    {
        return Add("New Tab " + (Controls.Count + 1));
    }

    public MultiPageControlItem Add(string text)
    {
        SuspendLayout();
        var newPage = new MultiPageControlItem { Parent = this, Text = text, Visible = false, Dock = DockStyle.Fill };
        Controls.Add(newPage);

        ReorganizePages();

        if (Controls.Count == 1)
            SelectedIndex = 0;
        else if (Controls.Count < 1)
            SelectedIndex = -1;
        else
            Invalidate();

        ResumeLayout();

        return newPage;
    }

    public void Remove()
    {
        RemoveAt(SelectedIndex);
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= _collection.Count)
            return;

        SuspendLayout();

        Controls[index].Controls.Clear();
        Controls[index].Visible = false;
        Controls.RemoveAt(index);
        ReorganizePages();

        if (Controls.Count == 1)
            SelectedIndex = 0;
        else if (Controls.Count < 1)
            SelectedIndex = -1;
        else if (SelectedIndex == index)
            SelectedIndex = index; // run set method again
        else
            Invalidate();

        ResumeLayout();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        GroupBoxRenderer.DrawParentBackground(e.Graphics, ClientRectangle, this);

        base.OnPaint(e);
        var graphics = e.Graphics;

        using var borderPen = new Pen(ColorScheme.BorderColor);

        graphics.SetHighQuality();

        //graphics.DrawLine(borderPen, 2, _headerControlSize.Height, Width - 3, _headerControlSize.Height);

        var i = 0;
        foreach (MultiPageControlItem control in Controls)
        {
            var rectangle = control.Rectangle;

            if (i == SelectedIndex)
                graphics.FillPath(borderPen.Brush, rectangle.ToRectangleF().Radius(6, 6));

            // is mouse in close button
            if (_renderPageClose)
            {
                using var closeBrush = borderPen.Color.Alpha(50).Brush();

                var isMouseHoverOnCloseBrn = control.RectangleClose.Contains(_mouseLocation);
                if (isMouseHoverOnCloseBrn)
                    closeBrush.Color = Color.DarkGray;
                
                using var closePen = new Pen(closeBrush.Color);

                graphics.DrawArc(closePen, control.RectangleClose, 0, 360);

                var inlineCloseRect = control.RectangleClose;
                inlineCloseRect.Offset(0, 0);
                inlineCloseRect.Inflate(-2, -2);

                graphics.FillPie(closeBrush, inlineCloseRect, 0, 360);
            }

            if (_renderPageIcon)
                graphics.DrawIcon(SystemIcons.Hand, control.RectangleIcon);

            i++;
            TextRenderer.DrawText(graphics, control.Text, Font, rectangle, ColorScheme.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
        }

        // new tab button
        if (_renderNewPageButton)
        {
            var bounds = _newButtonPath.GetBounds().Contains(_mouseLocation);
            using SolidBrush newPageButtonBrush = borderPen.Brush as SolidBrush;

            switch (_mouseState)
            {
                case 1:
                    if (bounds)
                        newPageButtonBrush.Color = ColorScheme.BackColor2.Alpha(30);
                    break;
                case 2:
                    if (bounds)
                        newPageButtonBrush.Color = ColorScheme.ShadowColor;
                    break;
            }

            graphics.FillPath(newPageButtonBrush, _newButtonPath);
        }

        graphics.SetDefaultQuality();
        graphics.DrawString("Index:" + _selectedIndex + " State:" + _mouseState +  " Pos:" + _mouseLocation, Font, Brushes.Red, new PointF(Width - 200, Height - 32));
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        _mouseLocation = e.Location;
        Invalidate();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        SelectedIndex += e.Delta <= -120 ? -1 : 1;
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _mouseState = 2; 
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _mouseState = 1;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _mouseState = 0;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if(_mouseState == 2)
        {
            for (int i = 0; i < Controls.Count; i++)
            {
                var item = Controls[i];
                if (item.RectangleClose.Contains(_mouseLocation))
                {
                    if(_onClosePageButtonClicked == null)
                        RemoveAt(i);
                    else
                        _onClosePageButtonClicked(this, EventArgs.Empty);
                }
                else if (item.Rectangle.Contains(_mouseLocation))
                    SelectedIndex = i;
            }

            if (_newButtonPath == null)
                return;

            if (_newButtonPath.GetBounds().Contains(_mouseLocation))
            {
                if(_onNewPageButtonClicked == null)
                    Add();
                else
                    _onNewPageButtonClicked(this, EventArgs.Empty);
            }
        }

        _mouseState = 1;
    }
}

public class MultiPageControlDesigner : ControlDesigner
{
    private DesignerActionListCollection actionList;
    public override DesignerActionListCollection ActionLists
    {
        get
        {
            if (actionList == null)
                actionList = new DesignerActionListCollection(new[] {
                    new MultiPageControlActions(this) });
            return actionList;
        }
    }
}

public class MultiPageControlActions : DesignerActionList
{
    ControlDesigner _designer;
    MultiPageControl _control;

    public MultiPageControlActions(ControlDesigner designer)
        : base(designer.Component)
    {
        this._designer = designer;
        _control = (MultiPageControl)designer.Control;
    }

    public void AddTab()
    {
        _control.Add();
    }

    public void RemoveTab()
    {
        _control.Remove();
    }

    public override DesignerActionItemCollection GetSortedActionItems()
    {
        return new()
        {
            new DesignerActionMethodItem(this, "AddTab", "Add Tab", true),
            new DesignerActionMethodItem(this, "RemoveTab", "Remove Tab", true)
        };
    }
}

public class MultiPageControlCollectionEditor : CollectionEditor
{
    public MultiPageControlCollectionEditor()
        : base(typeof(MultiPageControlCollection))
    {
    }

    protected override object SetItems(object editValue, object[] value)
    {
        var multiPageControl = this.Context.Instance as MultiPageControl;

        multiPageControl.SuspendLayout();

        object retValue = base.SetItems(editValue, value);

        multiPageControl.ResumeLayout();
        multiPageControl.Invalidate();

        return retValue;
    }

    protected override CollectionForm CreateCollectionForm()
    {
        var form = base.CreateCollectionForm();
        Type type = form.GetType();
        PropertyInfo propertyInfo = type.GetProperty("CollectionEditable", BindingFlags.Instance | BindingFlags.NonPublic);
        propertyInfo.SetValue(form, true);
        return form;
    }

    protected override Type CreateCollectionItemType()
    {
        return typeof(MultiPageControlCollection);
    }

    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
    {
        return base.EditValue(context, provider, value);
    }
}

public class MultiPageControlCollection : List<MultiPageControlItem>, IList
{
}