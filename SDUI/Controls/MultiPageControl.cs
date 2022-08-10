using SDUI.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace SDUI.Controls
{
    public class MultiPageControlItem : SDUI.Controls.Panel
    {
        public MultiPageControlItem()
        {
            Dock = DockStyle.Fill;
            Padding = Padding.Empty;
        }

        public override string Text { get; set; }
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
            _collection.OnItemAdded += _collection_OnItemAdded;
            _collection.OnItemRemoved += _collection_OnItemRemoved;
        }

        private void _collection_OnItemAdded(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void _collection_OnItemRemoved(object sender, EventArgs e)
        {
            Invalidate();
        }

        private MultiPageControlCollection _collection = new MultiPageControlCollection();
        [Editor(typeof(MultiPageControlCollectionEditor), typeof(UITypeEditor))]
        [MergableProperty(false)]
        public MultiPageControlCollection Collection => _collection;

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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            e.Graphics.DrawLine(new Pen(ColorScheme.BorderColor), 0, _headerControlSize.Height, Width, _headerControlSize.Height);

            var x = this.Radius / 2;
            var i = 0;
            foreach (MultiPageControlItem control in Collection)
            {
                var stringSize = TextRenderer.MeasureText(control.Text, Font);
                var width = stringSize.Width + 30;
                var rectangle = new Rectangle(x, 1, width, _headerControlSize.Height - 1);

                var brush = new SolidBrush(i == _selectedIndex ? ColorScheme.BorderColor : ColorScheme.BackColor2);
                e.Graphics.FillRectangle(brush, rectangle);
                x += width;
                i++;
                TextRenderer.DrawText(graphics, control.Text, Font, rectangle, ColorScheme.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
            }
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
        ControlDesigner designer;
        MultiPageControl control;

        public MultiPageControlActions(ControlDesigner designer) : base(designer.Component)
        {
            this.designer = designer;
            control = (MultiPageControl)designer.Control;
        }

        public void AddTab()
        {
            control.Collection.Add(new MultiPageControlItem { Text = "New Tab " + (control.Collection.Count + 1) });
        }

        public void RemoveTab()
        {
            control.Collection.RemoveAt(control.SelectedIndex);
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            return new DesignerActionItemCollection() {
                new DesignerActionMethodItem(this, "AddTab", "Add Tab",  true),
                new DesignerActionMethodItem(this, "RemoveTab", "Remove Tab",  true)
            };
        }
    }

    internal class MultiPageControlCollectionEditor : CollectionEditor
    {
        public MultiPageControlCollectionEditor() : base(typeof(MultiPageControlCollection))
        {
        }

        protected override object SetItems(object editValue, object[] value)
        {
            var multiPageControl = this.Context.Instance as MultiPageControl;
            if (multiPageControl != null)
                multiPageControl.SuspendLayout();

            object retValue = base.SetItems(editValue, value);

            if (multiPageControl != null)
                multiPageControl.ResumeLayout();


            return retValue;
        }
    }

    public class MultiPageControlCollection : ICollection<MultiPageControlItem>
    {
        public event EventHandler OnItemAdded;
        public event EventHandler OnItemRemoved;

        private readonly object _lock = new object();
        private List<MultiPageControlItem> _items = new List<MultiPageControlItem>();

        public int Count => _items.Count;

        public object SyncRoot => _lock;

        public bool IsSynchronized => true;

        public bool IsReadOnly => false;

        public void Add(MultiPageControlItem item)
        {
            lock(_lock)
            {
                _items.Add(item);
                OnItemAdded?.Invoke(item, EventArgs.Empty);
            }
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(MultiPageControlItem item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(MultiPageControlItem[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public IEnumerator GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public bool Remove(MultiPageControlItem item)
        {
            lock(_lock)
            {
                OnItemRemoved?.Invoke(item, EventArgs.Empty);
                return _items.Remove(item);
            }
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _items.Count)
                return;

            lock (_lock)
            {
                OnItemRemoved?.Invoke(_items[index], EventArgs.Empty);
                _items.RemoveAt(index);
            }
        }

        IEnumerator<MultiPageControlItem> IEnumerable<MultiPageControlItem>.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}
