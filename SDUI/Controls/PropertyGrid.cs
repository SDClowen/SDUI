#nullable enable
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using SkiaSharp;

namespace SDUI.Controls;

/// <summary>
/// Professional PropertyGrid control for editing object properties
/// </summary>
public class PropertyGrid : UIElementBase
{
    private readonly FlowLayoutPanel _categoryPanel;
    private object? _selectedObject;
    private PropertySort _propertySortOrder = PropertySort.Categorized;
    private readonly Dictionary<string, GroupBox> _categories = new();
    private int _scrollOffset;

    public PropertyGrid()
    {
        BackColor = ColorScheme.BackColor;
        Size = new Size(300, 400);
        AutoScroll = true;
        
        _categoryPanel = new FlowLayoutPanel
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            FlowDirection = System.Windows.Forms.FlowDirection.TopDown,
            BackColor = Color.Transparent,
            ShadowDepth = 0,
            Radius = 0,
            Border = new System.Windows.Forms.Padding(0),
            AutoScroll = true,
            WrapContents = false,
        };
        
        Controls.Add(_categoryPanel);
    }

    public object? SelectedObject
    {
        get => _selectedObject;
        set
        {
            if (_selectedObject == value) return;
            _selectedObject = value;
            RefreshProperties();
        }
    }

    public PropertySort PropertySort
    {
        get => _propertySortOrder;
        set
        {
            if (_propertySortOrder == value) return;
            _propertySortOrder = value;
            RefreshProperties();
        }
    }

    public event PropertyValueChangedEventHandler? PropertyValueChanged;

    private void RefreshProperties()
    {
        // Performans için layout suspend/resume kaldır
        _categoryPanel.Controls.Clear();
        _categories.Clear();

        if (_selectedObject == null)
        {
            return;
        }

        if (_selectedObject is TabControl tabControl)
        {
            AddTabPagesEditor(tabControl);
        }

        var properties = _selectedObject.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => p.GetCustomAttribute<BrowsableAttribute>()?.Browsable != false)
            .ToArray();

        if (_propertySortOrder == PropertySort.Categorized)
        {
            // Group by category
            var grouped = properties.GroupBy(p =>
            {
                var categoryAttr = p.GetCustomAttribute<CategoryAttribute>();
                return categoryAttr?.Category ?? "Misc";
            });

            foreach (var group in grouped.OrderBy(g => g.Key))
            {
                var categoryGroup = new GroupBox
                {
                    Width = 280,
                    Text = group.Key,
                    Collapsible = true,
                    Collapsed = false,
                    AutoScroll = true,
                };
                _categories[group.Key] = categoryGroup;

                var yPos = 0;
                foreach (var prop in group.OrderBy(p => p.Name))
                {
                    var item = CreatePropertyItem(prop);
                    item.Location = new Point(0, yPos);
                    categoryGroup.Controls.Add(item);
                    yPos += item.Height;
                }

                categoryGroup.Height = 30 + yPos + categoryGroup.Padding.Vertical;
                _categoryPanel.Controls.Add(categoryGroup);
            }
        }
        else if (_propertySortOrder == PropertySort.Alphabetical)
        {
            var categoryGroup = new GroupBox
            {
                Width = 280,
                Text = "Properties",
                Collapsible = true,
                Collapsed = false,
                AutoScroll = true,
            };
            _categories["All"] = categoryGroup;

            var yPos = 0;
            foreach (var prop in properties.OrderBy(p => p.Name))
            {
                var item = CreatePropertyItem(prop);
                item.Location = new Point(0, yPos);
                categoryGroup.Controls.Add(item);
                yPos += item.Height;
            }

            categoryGroup.Height = 30 + yPos + categoryGroup.Padding.Vertical;
            _categoryPanel.Controls.Add(categoryGroup);
        }

        PerformLayout();
    }

    private void AddTabPagesEditor(TabControl tabControl)
    {
        var tabsGroup = new GroupBox
        {
            Width = 280,
            Text = "Tabs",
            Collapsible = true,
            Collapsed = false,
            AutoScroll = true
        };

        var listPanel = new FlowLayoutPanel
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            FlowDirection = System.Windows.Forms.FlowDirection.TopDown,
            BackColor = Color.Transparent,
            ShadowDepth = 0,
            Radius = 0,
            Border = new System.Windows.Forms.Padding(0),
            AutoScroll = true,
            WrapContents = false
        };

        tabsGroup.Controls.Add(listPanel);

        var lineHeight = Math.Max(24, (int)Math.Ceiling(24 * ScaleFactor));
        var rowHeight = (lineHeight * 2) + 6;
        var contentWidth = 260;

        var headerRow = new Panel
        {
            Width = contentWidth,
            Height = lineHeight,
            BackColor = Color.Transparent,
            ShadowDepth = 0,
            Radius = 0,
            Border = new System.Windows.Forms.Padding(0)
        };

        var addButton = new Button
        {
            Text = "+",
            Size = new Size(lineHeight - 4, lineHeight - 4),
            Location = new Point(0, 2),
            BackColor = ColorScheme.OnSurface,
            ForeColor = ColorScheme.ForeColor,
            Radius = 4,
            ShadowDepth = 0
        };
        addButton.Click += (s, e) =>
        {
            AddTabPage(tabControl);
            RefreshProperties();
        };

        var addLabel = new Label
        {
            Text = "Add Tab",
            Location = new Point(lineHeight, 4),
            Size = new Size(contentWidth - lineHeight, lineHeight - 4),
            ForeColor = ColorScheme.ForeColor.Alpha(220)
        };

        headerRow.Controls.Add(addButton);
        headerRow.Controls.Add(addLabel);
        listPanel.Controls.Add(headerRow);

        var pages = GetTabPages(tabControl);
        for (var i = 0; i < pages.Count; i++)
            listPanel.Controls.Add(CreateTabPageRow(tabControl, pages[i], i, pages.Count, lineHeight, contentWidth));

        var contentHeight = headerRow.Height + (pages.Count * rowHeight);
        tabsGroup.Height = Math.Max(80, 30 + Math.Min(220, contentHeight) + tabsGroup.Padding.Vertical);

        _categoryPanel.Controls.Add(tabsGroup);
    }

    private UIElementBase CreateTabPageRow(TabControl tabControl, TabPage page, int index, int totalCount, int lineHeight, int contentWidth)
    {
        var rowHeight = (lineHeight * 2) + 6;
        var row = new Panel
        {
            Width = contentWidth,
            Height = rowHeight,
            BackColor = Color.Transparent,
            ShadowDepth = 0,
            Radius = 0,
            Border = new System.Windows.Forms.Padding(0)
        };

        var buttonSpacing = 4;
        var buttonSize = lineHeight - 4;
        var selectWidth = buttonSize + 6;
        var buttonsWidth = selectWidth + (buttonSize * 3) + (buttonSpacing * 3);
        var textBoxWidth = Math.Max(80, contentWidth - buttonsWidth - 4);

        var titleBox = new TextBox
        {
            Text = page.Text,
            Location = new Point(0, 2),
            Size = new Size(textBoxWidth, lineHeight - 4)
        };
        titleBox.TextChanged += (s, e) =>
        {
            page.Text = titleBox.Text;
            tabControl.Invalidate();
        };

        var selectButton = new Button
        {
            Text = "Sel",
            Size = new Size(selectWidth, buttonSize),
            Location = new Point(textBoxWidth + buttonSpacing, 2),
            BackColor = ColorScheme.OnSurface,
            ForeColor = ColorScheme.ForeColor,
            Radius = 4,
            ShadowDepth = 0
        };
        selectButton.Click += (s, e) =>
        {
            var pages = GetTabPages(tabControl);
            var pageIndex = pages.IndexOf(page);
            if (pageIndex >= 0)
                tabControl.SelectedIndex = pageIndex;
        };

        var upButton = new Button
        {
            Text = "↑",
            Size = new Size(buttonSize, buttonSize),
            Location = new Point(textBoxWidth + buttonSpacing + selectWidth + buttonSpacing, 2),
            BackColor = ColorScheme.OnSurface,
            ForeColor = ColorScheme.ForeColor,
            Radius = 4,
            ShadowDepth = 0,
            Enabled = index > 0
        };
        upButton.Click += (s, e) =>
        {
            var pages = GetTabPages(tabControl);
            var pageIndex = pages.IndexOf(page);
            if (pageIndex > 0)
            {
                tabControl.MovePage(page, pageIndex - 1);
                RefreshProperties();
            }
        };

        var downButton = new Button
        {
            Text = "↓",
            Size = new Size(buttonSize, buttonSize),
            Location = new Point(textBoxWidth + buttonSpacing + selectWidth + buttonSpacing + buttonSize + buttonSpacing, 2),
            BackColor = ColorScheme.OnSurface,
            ForeColor = ColorScheme.ForeColor,
            Radius = 4,
            ShadowDepth = 0,
            Enabled = index < totalCount - 1
        };
        downButton.Click += (s, e) =>
        {
            var pages = GetTabPages(tabControl);
            var pageIndex = pages.IndexOf(page);
            if (pageIndex >= 0 && pageIndex < pages.Count - 1)
            {
                tabControl.MovePage(page, pageIndex + 1);
                RefreshProperties();
            }
        };

        var removeButton = new Button
        {
            Text = "×",
            Size = new Size(buttonSize, buttonSize),
            Location = new Point(textBoxWidth + buttonSpacing + selectWidth + buttonSpacing + buttonSize + buttonSpacing + buttonSize + buttonSpacing, 2),
            BackColor = ColorScheme.SurfaceVariant,
            ForeColor = ColorScheme.ForeColor,
            Radius = 4,
            ShadowDepth = 0
        };
        removeButton.Click += (s, e) =>
        {
            tabControl.RemovePage(page);
            RefreshProperties();
        };

        var iconBox = new TextBox
        {
            Text = page.IconPath,
            PlaceholderText = "Icon path",
            Location = new Point(0, lineHeight + 2),
            Size = new Size(contentWidth - buttonSize - 4, lineHeight - 4)
        };

        void ApplyIconPath()
        {
            page.IconPath = iconBox.Text;
            tabControl.Invalidate();
        }

        iconBox.LostFocus += (s, e) => ApplyIconPath();
        iconBox.KeyDown += (s, e) =>
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                ApplyIconPath();
                e.Handled = true;
            }
        };

        var clearButton = new Button
        {
            Text = "Clr",
            Size = new Size(buttonSize, buttonSize),
            Location = new Point(contentWidth - buttonSize, lineHeight + 2),
            BackColor = ColorScheme.SurfaceVariant,
            ForeColor = ColorScheme.ForeColor,
            Radius = 4,
            ShadowDepth = 0
        };
        clearButton.Click += (s, e) =>
        {
            iconBox.Text = string.Empty;
            ApplyIconPath();
        };

        row.Controls.Add(titleBox);
        row.Controls.Add(selectButton);
        row.Controls.Add(upButton);
        row.Controls.Add(downButton);
        row.Controls.Add(removeButton);
        row.Controls.Add(iconBox);
        row.Controls.Add(clearButton);

        return row;
    }

    private void AddTabPage(TabControl tabControl)
    {
        var index = GetTabPages(tabControl).Count + 1;
        var page = new TabPage
        {
            Text = $"Tab {index}",
            Name = $"TabPage{index}"
        };
        tabControl.AddPage(page);
    }

    private List<TabPage> GetTabPages(TabControl tabControl)
    {
        return tabControl.Pages.ToList();
    }

    private PropertyItem CreatePropertyItem(PropertyInfo property)
    {
        var item = new PropertyItem(_selectedObject!, property);
        item.ValueChanged += (s, e) =>
        {
            PropertyValueChanged?.Invoke(this, new PropertyValueChangedEventArgs(
                e.PropertyName, e.OldValue, e.NewValue));
        };
        return item;
    }
}

/// <summary>
/// Individual property editor item
/// </summary>
internal class PropertyItem : UIElementBase
{
    private readonly object _target;
    private readonly PropertyInfo _property;
    private readonly Label _nameLabel;
    private readonly UIElementBase _valueEditor;

    public event EventHandler<PropertyItemValueChangedEventArgs>? ValueChanged;

    public PropertyItem(object target, PropertyInfo property)
    {
        _target = target;
        _property = property;
        
        Width = 280;
        Height = 45;
        BackColor = Color.Transparent;
        Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);

        _nameLabel = new Label
        {
            Text = property.Name,
            Location = new Point(10, 3),
            Size = new Size(130, 18),
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = ColorScheme.ForeColor.Alpha(200)
        };

        _valueEditor = CreateEditor(property);
        _valueEditor.Location = new Point(145, 2);
        _valueEditor.Size = new Size(125, 40);

        Controls.Add(_nameLabel);
        Controls.Add(_valueEditor);
    }

    private UIElementBase CreateEditor(PropertyInfo property)
    {
        var currentValue = property.GetValue(_target);
        var propertyType = property.PropertyType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        // Boolean properties
        if (underlyingType == typeof(bool))
        {
            var checkBox = new CheckBox
            {
                Checked = (bool)(currentValue ?? false),
                Text = ""
            };
            checkBox.CheckedChanged += (s, e) => OnValueChangedInternal(checkBox.Checked);
            return checkBox;
        }

        // Enum properties
        if (underlyingType.IsEnum)
        {
            var comboBox = new ComboBox
            {
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            };
            
            foreach (var enumValue in Enum.GetValues(underlyingType))
            {
                comboBox.Items.Add(enumValue.ToString());
            }
            
            comboBox.SelectedItem = currentValue?.ToString();
            comboBox.SelectedIndexChanged += (s, e) =>
            {
                if (comboBox.SelectedItem != null)
                {
                    var newValue = Enum.Parse(underlyingType, comboBox.SelectedItem.ToString()!);
                    OnValueChangedInternal(newValue);
                }
            };
            return comboBox;
        }

        // Color properties
        if (underlyingType == typeof(Color))
        {
            var textBox = new TextBox
            {
                Text = currentValue?.ToString() ?? "",
                BackColor = Color.Transparent
            };
            textBox.TextChanged += (s, e) =>
            {
                if (TryParseColor(textBox.Text, out var parsedColor))
                {
                    textBox.BackColor = parsedColor;
                    OnValueChangedInternal(parsedColor);
                }
            };
            return textBox;
        }

        // Numeric properties
        if (underlyingType == typeof(int) || underlyingType == typeof(float) || 
            underlyingType == typeof(double) || underlyingType == typeof(decimal) ||
            underlyingType == typeof(long) || underlyingType == typeof(short))
        {
            var textBox = new TextBox
            {
                Text = currentValue?.ToString() ?? "0"
            };
            textBox.TextChanged += (s, e) =>
            {
                if (TryParseNullable(textBox.Text, propertyType, out var parsed))
                {
                    OnValueChangedInternal(parsed);
                }
            };
            return textBox;
        }

        // Point, Size, Padding properties
        if (underlyingType == typeof(Point) || underlyingType == typeof(Size) || 
            underlyingType == typeof(System.Windows.Forms.Padding) ||
            underlyingType == typeof(Rectangle))
        {
            var textBox = new TextBox
            {
                Text = currentValue?.ToString() ?? "",
                ReadOnly = false
            };
            textBox.TextChanged += (s, e) =>
            {
                // Parse format like "10, 20" or "{X=10,Y=20}"
                if (TryParseNullable(textBox.Text, propertyType, out var parsed))
                {
                    OnValueChangedInternal(parsed);
                }
            };
            return textBox;
        }

        // String and other properties
        var defaultTextBox = new TextBox
        {
            Text = currentValue?.ToString() ?? ""
        };
        defaultTextBox.TextChanged += (s, e) =>
        {
            OnValueChangedInternal(defaultTextBox.Text);
        };
        return defaultTextBox;
    }

    private bool TryParseNumeric(string text, Type targetType, out object? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        try
        {
            if (TryParseNumberWithCulture(text, out var value))
            {
                if (targetType == typeof(int) && TryConvertToInt(value, out var intValue))
                    result = intValue;
                else if (targetType == typeof(long) && TryConvertToLong(value, out var longValue))
                    result = longValue;
                else if (targetType == typeof(short) && TryConvertToShort(value, out var shortValue))
                    result = shortValue;
                else if (targetType == typeof(float))
                    result = (float)value;
                else if (targetType == typeof(double))
                    result = value;
                else if (targetType == typeof(decimal))
                    result = (decimal)value;
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool TryParseStructure(string text, Type targetType, out object? result)
    {
        result = null;
        try
        {
            if (!TryExtractNumbers(text, out var numbers))
                return false;

            if (targetType == typeof(Point) && numbers.Count >= 2)
            {
                if (!TryConvertToInt(numbers[0], out var x) || !TryConvertToInt(numbers[1], out var y))
                    return false;
                result = new Point(x, y);
                return true;
            }
            if (targetType == typeof(Size) && numbers.Count >= 2)
            {
                if (!TryConvertToInt(numbers[0], out var w) || !TryConvertToInt(numbers[1], out var h))
                    return false;
                result = new Size(w, h);
                return true;
            }
            if (targetType == typeof(Rectangle) && numbers.Count >= 4)
            {
                if (!TryConvertToInt(numbers[0], out var x) ||
                    !TryConvertToInt(numbers[1], out var y) ||
                    !TryConvertToInt(numbers[2], out var w) ||
                    !TryConvertToInt(numbers[3], out var h))
                    return false;
                result = new Rectangle(x, y, w, h);
                return true;
            }
            if (targetType == typeof(System.Windows.Forms.Padding) && numbers.Count >= 1)
            {
                if (!TryConvertToInt(numbers[0], out var a))
                    return false;

                if (numbers.Count == 1)
                {
                    result = new System.Windows.Forms.Padding(a);
                    return true;
                }

                if (numbers.Count == 2)
                {
                    if (!TryConvertToInt(numbers[1], out var b))
                        return false;
                    result = new System.Windows.Forms.Padding(a, b, a, b);
                    return true;
                }

                if (numbers.Count >= 4)
                {
                    if (!TryConvertToInt(numbers[1], out var b) ||
                        !TryConvertToInt(numbers[2], out var c) ||
                        !TryConvertToInt(numbers[3], out var d))
                        return false;
                    result = new System.Windows.Forms.Padding(a, b, c, d);
                    return true;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private bool TryParseNullable(string text, Type targetType, out object? result)
    {
        result = null;
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (string.IsNullOrWhiteSpace(text))
        {
            if (underlyingType != null)
                return true;
            return false;
        }

        var effectiveType = underlyingType ?? targetType;

        if (effectiveType == typeof(Color))
        {
            if (TryParseColor(text, out var parsedColor))
            {
                result = parsedColor;
                return true;
            }
            return false;
        }

        if (effectiveType == typeof(Point) || effectiveType == typeof(Size) ||
            effectiveType == typeof(System.Windows.Forms.Padding) || effectiveType == typeof(Rectangle))
            return TryParseStructure(text, effectiveType, out result);

        if (effectiveType == typeof(int) || effectiveType == typeof(float) ||
            effectiveType == typeof(double) || effectiveType == typeof(decimal) ||
            effectiveType == typeof(long) || effectiveType == typeof(short))
            return TryParseNumeric(text, effectiveType, out result);

        return false;
    }

    private bool TryParseColor(string text, out Color result)
    {
        result = Color.Empty;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var trimmed = text.Trim();

        if (trimmed.StartsWith("#", StringComparison.Ordinal))
        {
            if (TryParseHexColor(trimmed, out var hexColor))
            {
                result = hexColor;
                return true;
            }
        }

        if (TryExtractNumbers(trimmed, out var numbers))
        {
            if (numbers.Count == 3)
            {
                if (TryConvertToByte(numbers[0], out var r) &&
                    TryConvertToByte(numbers[1], out var g) &&
                    TryConvertToByte(numbers[2], out var b))
                {
                    result = Color.FromArgb(r, g, b);
                    return true;
                }
            }
            if (numbers.Count >= 4)
            {
                if (TryConvertToByte(numbers[0], out var a) &&
                    TryConvertToByte(numbers[1], out var r) &&
                    TryConvertToByte(numbers[2], out var g) &&
                    TryConvertToByte(numbers[3], out var b))
                {
                    result = Color.FromArgb(a, r, g, b);
                    return true;
                }
            }
        }

        var named = Color.FromName(trimmed);
        if (named.IsKnownColor || named.IsNamedColor)
        {
            result = named;
            return true;
        }

        return false;
    }

    private bool TryParseHexColor(string text, out Color color)
    {
        color = Color.Empty;
        var hex = text.TrimStart('#');
        if (hex.Length == 6 || hex.Length == 8)
        {
            if (uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
            {
                if (hex.Length == 6)
                {
                    var r = (int)((value >> 16) & 0xFF);
                    var g = (int)((value >> 8) & 0xFF);
                    var b = (int)(value & 0xFF);
                    color = Color.FromArgb(r, g, b);
                    return true;
                }
                else
                {
                    var a = (int)((value >> 24) & 0xFF);
                    var r = (int)((value >> 16) & 0xFF);
                    var g = (int)((value >> 8) & 0xFF);
                    var b = (int)(value & 0xFF);
                    color = Color.FromArgb(a, r, g, b);
                    return true;
                }
            }
        }
        return false;
    }

    private bool TryExtractNumbers(string text, out List<double> numbers)
    {
        var parsedNumbers = new List<double>();
        if (string.IsNullOrWhiteSpace(text))
        {
            numbers = parsedNumbers;
            return false;
        }

        var token = string.Empty;
        void FlushToken()
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                token = string.Empty;
                return;
            }

            if (TryParseNumberWithCulture(token, out var value))
                parsedNumbers.Add(value);

            token = string.Empty;
        }

        for (int i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (char.IsDigit(c) || c == '-' || c == '+' || c == '.')
            {
                token += c;
            }
            else if (c == ',' || c == ';' || char.IsWhiteSpace(c) || c == '|')
            {
                FlushToken();
            }
            else
            {
                FlushToken();
            }
        }

        FlushToken();
        numbers = parsedNumbers;
        return numbers.Count > 0;
    }

    private bool TryParseNumberWithCulture(string text, out double value)
    {
        if (double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture, out value))
            return true;

        return double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands,
            CultureInfo.CurrentCulture, out value);
    }

    private bool TryConvertToInt(double value, out int result)
    {
        var rounded = Math.Round(value);
        if (Math.Abs(value - rounded) > 0.0001)
        {
            result = 0;
            return false;
        }
        result = (int)rounded;
        return true;
    }

    private bool TryConvertToLong(double value, out long result)
    {
        var rounded = Math.Round(value);
        if (Math.Abs(value - rounded) > 0.0001)
        {
            result = 0;
            return false;
        }
        result = (long)rounded;
        return true;
    }

    private bool TryConvertToShort(double value, out short result)
    {
        if (!TryConvertToInt(value, out var intValue))
        {
            result = 0;
            return false;
        }

        if (intValue < short.MinValue || intValue > short.MaxValue)
        {
            result = 0;
            return false;
        }

        result = (short)intValue;
        return true;
    }

    private bool TryConvertToByte(double value, out byte result)
    {
        if (!TryConvertToInt(value, out var intValue))
        {
            result = 0;
            return false;
        }

        if (intValue < byte.MinValue || intValue > byte.MaxValue)
        {
            result = 0;
            return false;
        }

        result = (byte)intValue;
        return true;
    }

    private void OnValueChangedInternal(object? newValue)
    {
        var oldValue = _property.GetValue(_target);
        
        try
        {
            _property.SetValue(_target, newValue);
            ValueChanged?.Invoke(this, new PropertyItemValueChangedEventArgs(
                _property.Name, oldValue, newValue));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to set property '{_property.Name}' on '{_target.GetType().Name}'.", ex);
        }
    }
}

/// <summary>
/// Property item value changed event args
/// </summary>
internal class PropertyItemValueChangedEventArgs : EventArgs
{
    public string PropertyName { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }

    public PropertyItemValueChangedEventArgs(string propertyName, object? oldValue, object? newValue)
    {
        PropertyName = propertyName;
        OldValue = oldValue;
        NewValue = newValue;
    }
}

/// <summary>
/// Property value changed event handler
/// </summary>
public delegate void PropertyValueChangedEventHandler(object sender, PropertyValueChangedEventArgs e);

/// <summary>
/// Property value changed event args
/// </summary>
public class PropertyValueChangedEventArgs : EventArgs
{
    public string PropertyName { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }

    public PropertyValueChangedEventArgs(string propertyName, object? oldValue, object? newValue)
    {
        PropertyName = propertyName;
        OldValue = oldValue;
        NewValue = newValue;
    }
}

/// <summary>
/// Property sort order
/// </summary>
public enum PropertySort
{
    Alphabetical,
    Categorized,
    CategorizedAlphabetical,
}
