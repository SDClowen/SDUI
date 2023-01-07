using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace SDUI.Controls;

public class MultiPageControlItem : Panel
{
    [
    Localizable(true),
        Browsable(true),
        EditorBrowsable(EditorBrowsableState.Always)
        ]
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
public class MultiPageControl : Panel
{
    public MultiPageControl()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);

        Padding = new Padding(0, 30, 0, 0);
    }

    private MultiPageControlCollection _collection = new();
    [Editor(typeof(MultiPageControlCollectionEditor), typeof(UITypeEditor))]
    [MergableProperty(false)]
    public MultiPageControlCollection Collection
    {
        get => _collection;
        set => _collection = value;
    }

    private int _selectedIndex;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            _selectedIndex = value;
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

    public MultiPageControlItem Add()
    {
        return Add("New Tab " + (Collection.Count + 1));
    }

    public MultiPageControlItem Add(string text)
    {
        var newPage = new MultiPageControlItem { Parent = this, Text = text };
        Collection.Add(newPage);

        SuspendLayout();
        Invalidate();
        ResumeLayout();

        return newPage;
    }

    public void Remove()
    {
        RemoveAt(SelectedIndex);
    }

    public void Remove(MultiPageControlItem item)
    {
        Collection.Remove(item);

        SuspendLayout();
        Invalidate();
        ResumeLayout();
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= _collection.Count)
            return;

        Collection.RemoveAt(index);

        SuspendLayout();
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

        e.Graphics.DrawLine(borderPen, 2, _headerControlSize.Height, Width - 3, _headerControlSize.Height);

        var x = this.Radius / 2;
        var i = 0;
        foreach (MultiPageControlItem control in Collection)
        {
            var stringSize = TextRenderer.MeasureText(control.Text, Font);
            var width = stringSize.Width + 80;
            var rectangle = new Rectangle(x, 6, width, _headerControlSize.Height - 6);

            if (i == SelectedIndex)
                e.Graphics.FillPath(borderPen.Brush, rectangle.ToRectangleF().Radius(6, 6, 0, 0));

            // is mouse in close button
            if (true)
            {
                const int closeWH = 12;
                using var closeBrush = borderPen.Color.Alpha(50).Brush();
                e.Graphics.DrawArc(borderPen, new Rectangle(rectangle.X + rectangle.Width - 20, rectangle.Y + 6, closeWH, closeWH), 0, 360);
                e.Graphics.FillPie(closeBrush, new Rectangle(rectangle.X + rectangle.Width - 18, rectangle.Y + 8, closeWH - 4, closeWH - 4), 0, 360);
            }

            if (true)
                e.Graphics.DrawIcon(SystemIcons.Hand, new Rectangle(rectangle.X + 6, rectangle.Y + 5, 16, 16));

            x += width;
            i++;
            TextRenderer.DrawText(graphics, control.Text, Font, rectangle, ColorScheme.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
        }

        // new tab button
        if (true)
        {
            e.Graphics.FillPath(borderPen.Brush, new RectangleF(x + 4, 9, 24, 16).Radius(6, 12, 12, 6));
        }

        graphics.SetDefaultQuality();
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