using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace SDUI.Controls;

public partial class ListViewItem
{
    public class ListViewSubItem
    {
        [NonSerialized] private AccessibleObject? _accessibilityObject;

        [NonSerialized] internal ListViewItem? _owner;

        public ListViewSubItem()
        {
        }

        public ListViewSubItem(ListViewItem? owner, string? text)
        {
            _owner = owner;
            this.text = text;
        }

        public ListViewSubItem(ListViewItem? owner, string? text, Color foreColor, Color backColor, Font font)
        {
            _owner = owner;
            this.text = text;
            style = new SubItemStyle
            {
                foreColor = foreColor,
                backColor = backColor,
                font = font
            };
        }

        internal AccessibleObject? AccessibilityObject
        {
            get
            {
                if (_accessibilityObject is null && _owner is not null) _accessibilityObject = null;

                return _accessibilityObject;
            }
        }

        public Color BackColor
        {
            get
            {
                if (style is not null && style.backColor != Color.Empty) return style.backColor;

                return _owner?._listView?.BackColor ?? Color.Transparent;
            }
            set
            {
                style ??= new SubItemStyle();

                if (style.backColor != value)
                {
                    style.backColor = value;
                    _owner?.InvalidateListView();
                }
            }
        }

        [Browsable(false)]
        public Rectangle Bounds
        {
            get
            {
                if (_owner?._listView is not null && _owner._listView.IsHandleCreated)
                    return _owner._listView.GetSubItemRect(_owner.Index, _owner.SubItems.IndexOf(this));

                return Rectangle.Empty;
            }
        }

        internal bool CustomBackColor => style is not null && !style.backColor.IsEmpty;

        internal bool CustomFont => style is not null && style.font is not null;

        internal bool CustomForeColor => style is not null && !style.foreColor.IsEmpty;

        internal bool CustomStyle => style is not null;

        public Font Font
        {
            get
            {
                if (style is not null && style.font is not null) return style.font;

                return _owner?._listView?.Font ?? Control.DefaultFont;
            }
            set
            {
                style ??= new SubItemStyle();

                if (style.font != value)
                {
                    style.font = value;
                    _owner?.InvalidateListView();
                }
            }
        }

        public Color ForeColor
        {
            get
            {
                if (style is not null && style.foreColor != Color.Empty) return style.foreColor;

                return _owner?._listView?.ForeColor ?? ColorScheme.ForeColor;
            }
            set
            {
                style ??= new SubItemStyle();

                if (style.foreColor != value)
                {
                    style.foreColor = value;
                    _owner?.InvalidateListView();
                }
            }
        }

        internal int Index => _owner is null ? -1 : _owner.SubItems.IndexOf(this);

        [field: OptionalField(VersionAdded = 2)]
        public object? Tag { get; set; }

        [Localizable(true)]
        [AllowNull]
        public string Text
        {
            get => text ?? string.Empty;
            set
            {
                if (text == value)
                    return;

                text = value;
                _owner?.UpdateSubItems(Index);
            }
        }

        [Localizable(true)]
        [AllowNull]
        public string Name
        {
            get => name ?? string.Empty;
            set
            {
                name = value;
                _owner?.UpdateSubItems(-1);
            }
        }

        [OnDeserializing]
        private static void OnDeserializing(StreamingContext ctx)
        {
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            name = null;
            Tag = null;
        }

        [OnSerializing]
        private static void OnSerializing(StreamingContext ctx)
        {
        }

        [OnSerialized]
        private static void OnSerialized(StreamingContext ctx)
        {
        }

        internal void ReleaseUiaProvider()
        {
            _accessibilityObject = null;
        }

        public void ResetStyle()
        {
            if (style is not null)
            {
                style = null;
                _owner?.InvalidateListView();
            }
        }

        public override string ToString()
        {
            return $"ListViewSubItem: {{{Text}}}";
        }

        private class SubItemStyle
        {
            public Color backColor = Color.Empty; // Do NOT rename (binary serialization).
            public Font? font; // Do NOT rename (binary serialization).
            public Color foreColor = Color.Empty; // Do NOT rename (binary serialization).
        }
#pragma warning disable IDE1006
        private string? text; // Do NOT rename (binary serialization).

        [OptionalField(VersionAdded = 2)] private string? name; // Do NOT rename (binary serialization).

        private SubItemStyle? style; // Do NOT rename (binary serialization).

#pragma warning restore IDE1006
    }
}