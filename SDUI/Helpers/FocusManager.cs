using SDUI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SDUI.Helpers;

/// <summary>
/// Modern focus management system with proper keyboard navigation
/// </summary>
public class FocusManager
{
    private UIElementBase? _currentFocus;
    private readonly UIWindowBase _window;
    private readonly List<UIElementBase> _focusableElements = new();
    private bool _isNavigating;

    public FocusManager(UIWindowBase window)
    {
        _window = window;
    }

    /// <summary>
    /// Current focused element
    /// </summary>
    public UIElementBase? FocusedElement
    {
        get => _currentFocus;
        private set
        {
            if (_currentFocus == value) return;

            var oldFocus = _currentFocus;
            _currentFocus = value;

            // Update visual states
            if (oldFocus != null)
            {
                oldFocus.Focused = false;
                oldFocus.Invalidate();
            }

            if (_currentFocus != null)
            {
                _currentFocus.Focused = true;
                _currentFocus.Invalidate();
                EnsureVisible(_currentFocus);
            }

            FocusChanged?.Invoke(this, new FocusChangedEventArgs(oldFocus, _currentFocus));
        }
    }

    public event EventHandler<FocusChangedEventArgs>? FocusChanged;

    /// <summary>
    /// Refreshes the list of focusable elements
    /// </summary>
    public void RefreshFocusableElements()
    {
        _focusableElements.Clear();
        
        // Collect from window's controls
        foreach (var control in _window.Controls.OfType<UIElementBase>())
        {
            CollectFromElement(control);
        }
        
        _focusableElements.Sort((a, b) => a.TabIndex.CompareTo(b.TabIndex));
    }

    private void CollectFromElement(UIElementBase element)
    {
        if (element.Visible && element.Enabled && element.TabStop && element.CanSelect)
        {
            _focusableElements.Add(element);
        }

        // Recursive for nested containers
        if (element is IUIElement uiElement)
        {
            foreach (var child in uiElement.Controls.OfType<UIElementBase>())
            {
                CollectFromElement(child);
            }
        }
    }

    /// <summary>
    /// Sets focus to specific element
    /// </summary>
    public bool SetFocus(UIElementBase? element)
    {
        if (element == null)
        {
            FocusedElement = null;
            return true;
        }

        if (!element.Visible || !element.Enabled || !element.CanSelect)
            return false;

        FocusedElement = element;
        return true;
    }

    /// <summary>
    /// Navigate to next focusable element
    /// </summary>
    public bool FocusNext(bool reverse = false)
    {
        if (_isNavigating) return false;
        _isNavigating = true;

        try
        {
            RefreshFocusableElements();
            if (_focusableElements.Count == 0) return false;

            int currentIndex = _currentFocus != null
                ? _focusableElements.IndexOf(_currentFocus)
                : -1;

            int nextIndex;
            if (reverse)
            {
                nextIndex = currentIndex > 0
                    ? currentIndex - 1
                    : _focusableElements.Count - 1;
            }
            else
            {
                nextIndex = currentIndex < _focusableElements.Count - 1
                    ? currentIndex + 1
                    : 0;
            }

            return SetFocus(_focusableElements[nextIndex]);
        }
        finally
        {
            _isNavigating = false;
        }
    }

    /// <summary>
    /// Focus first element
    /// </summary>
    public bool FocusFirst()
    {
        RefreshFocusableElements();
        return _focusableElements.Count > 0 && SetFocus(_focusableElements[0]);
    }

    /// <summary>
    /// Focus last element
    /// </summary>
    public bool FocusLast()
    {
        RefreshFocusableElements();
        return _focusableElements.Count > 0 && SetFocus(_focusableElements[^1]);
    }

    /// <summary>
    /// Handle keyboard navigation
    /// </summary>
    public bool ProcessKeyNavigation(KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Tab:
                bool handled = FocusNext(e.Shift);
                e.Handled = handled;
                return handled;

            case Keys.Home when e.Control:
                e.Handled = FocusFirst();
                return e.Handled;

            case Keys.End when e.Control:
                e.Handled = FocusLast();
                return e.Handled;

            case Keys.Escape:
                if (_currentFocus != null)
                {
                    SetFocus(null);
                    e.Handled = true;
                    return true;
                }
                break;
        }

        return false;
    }

    /// <summary>
    /// Ensures focused element is visible by scrolling containers
    /// </summary>
    private void EnsureVisible(UIElementBase element)
    {
        // Find scrollable parent and scroll to make element visible
        var parent = element.Parent;
        while (parent != null)
        {
            if (parent is UIElementBase parentElement && parentElement.AutoScroll)
            {
                // Calculate if element is outside visible bounds
                var elementBounds = element.Bounds;
                var parentBounds = parentElement.ClientRectangle;

                // Scroll logic would go here
                // This is a placeholder for future scroll implementation
            }

            parent = parent is UIElementBase pe ? pe.Parent : null;
        }
    }

    /// <summary>
    /// Draw focus indicator for the current element
    /// </summary>
    public void DrawFocusIndicator(SkiaSharp.SKCanvas canvas, Rectangle bounds, float cornerRadius)
    {
        using var paint = new SkiaSharp.SKPaint
        {
            IsAntialias = true,
            Color = ColorScheme.Primary.ToSKColor(),
            IsStroke = true,
            StrokeWidth = 2f,
            PathEffect = SkiaSharp.SKPathEffect.CreateDash(new[] { 4f, 2f }, 0)
        };

        var rect = new SkiaSharp.SKRect(
            bounds.Left - 2,
            bounds.Top - 2,
            bounds.Right + 2,
            bounds.Bottom + 2
        );

        canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, paint);
    }
}

public class FocusChangedEventArgs : EventArgs
{
    public UIElementBase? OldFocus { get; }
    public UIElementBase? NewFocus { get; }

    public FocusChangedEventArgs(UIElementBase? oldFocus, UIElementBase? newFocus)
    {
        OldFocus = oldFocus;
        NewFocus = newFocus;
    }
}
