using SDUI.Animation;
using SDUI.Helpers;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls;

public class TreeNode
{
    public string Text { get; set; }
    public List<TreeNode> Nodes { get; } = new();
    public TreeNode? Parent { get; internal set; }
    public bool Expanded { get; set; }
    public object? Tag { get; set; }
    /// <summary>Optional small icon color for simple icon rendering in UI</summary>
    public Color? IconColor { get; set; }

    public TreeNode(string text) => Text = text;

    public TreeNode Add(string text)
    {
        var n = new TreeNode(text) { Parent = this };
        Nodes.Add(n);
        return n;
    }
}

public class TreeNodeMouseClickEventArgs : EventArgs
{
    public TreeNode Node { get; }
    public MouseButtons Button { get; }
    public int Clicks { get; }
    public Point Location { get; }

    public TreeNodeMouseClickEventArgs(TreeNode node, MouseButtons button, int clicks, Point location)
    {
        Node = node;
        Button = button;
        Clicks = clicks;
        Location = location;
    }
}

public class TreeNodeCollection : IEnumerable<TreeNode>
{
    private readonly List<TreeNode> _inner = new();
    private readonly TreeView _owner;

    internal TreeNodeCollection(TreeView owner) => _owner = owner;

    public void Add(TreeNode node)
    {
        node.Parent = null;
        _inner.Add(node);
        _owner.Invalidate();
    }
    public void Add(string text)
    {
        var node = new TreeNode(text);
        node.Parent = null;
        _inner.Add(node);
        _owner.Invalidate();
    }

    public IEnumerator<TreeNode> GetEnumerator() => _inner.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => _inner.Count;
    public TreeNode this[int idx] => _inner[idx];
    public void Clear()
    {
        _inner.Clear();
        _owner.Invalidate();
    }
}

public class TreeView : UIElementBase
{
    public TreeNodeCollection Nodes { get; }

    private TreeNode? _selectedNode = null;
    public TreeNode? SelectedNode {
        get => _selectedNode;
        set
        {
            _selectedNode = value;
            _selectedNodes.Clear();
            if (value != null) _selectedNodes.Add(value);
            _lastSelectedNode = value;
            Focus(); // ensure this control is focused so HideSelection works as expected
            AfterSelect?.Invoke(this, EventArgs.Empty);
            SelectedNodesChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }
    public event EventHandler? AfterSelect;

    // Back-compat convenience to set selection in demo/test code
    public void SetSelectedNode(TreeNode? node)
    {
        SelectedNode = node;
    }


    private readonly Dictionary<TreeNode, Rectangle> _nodeBounds = new();
    private readonly Dictionary<TreeNode, Rectangle> _toggleBounds = new();
    private readonly Dictionary<TreeNode, Rectangle> _nodeTextBounds = new();

    // Animations & pending collapse state
    private readonly Dictionary<TreeNode, AnimationManager> _nodeAnimations = new();
    private readonly HashSet<TreeNode> _pendingCollapse = new();

    // Selection (supports multi-select)
    private readonly List<TreeNode> _selectedNodes = new();
    private TreeNode? _lastSelectedNode;

    [Category("Behavior")]
    public bool FullRowSelect { get; set; } = true;

    [Category("Behavior")]
    public bool HideSelection { get; set; } = false;

    public IReadOnlyList<TreeNode> SelectedNodes => _selectedNodes.AsReadOnly();
    public event EventHandler? SelectedNodesChanged;

    public event EventHandler<TreeNodeMouseClickEventArgs>? NodeMouseClick;
    public event EventHandler<TreeNodeMouseClickEventArgs>? NodeMouseDoubleClick;
    public event EventHandler<TreeNodeMouseClickEventArgs>? NodeMouseRightClick;

    private int _hoverIndex = -1;
    private TreeNode? _hoveredNode;
    private readonly int _indent = 18;
    private int _rowHeight => Math.Max(20, (int)Math.Ceiling(GetSkTextSize(Font) + 6));

    public TreeView()
    {
        Nodes = new TreeNodeCollection(this);
        DoubleBuffered = true;
        TabStop = true;
    }

    private float GetSkTextSize(Font font)
    {
        float dpi = DeviceDpi > 0 ? DeviceDpi : 96f;
        float pt = font.Unit == System.Drawing.GraphicsUnit.Point ? font.SizeInPoints : font.Size;
        return (float)(pt * (dpi / 72f));
    }

    private SKFont CreateFont(Font font)
    {
        var tf = SDUI.Helpers.FontManager.GetSKTypeface(font);
        return new SKFont(tf, GetSkTextSize(font)) { Hinting = SKFontHinting.Full, Edging = SKFontEdging.SubpixelAntialias, Subpixel = true };
    }

    public override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        base.OnPaint(e);
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        var font = CreateFont(Font);
        using var textPaint = new SKPaint { IsAntialias = true, Color = ColorScheme.ForeColor.ToSKColor(), Style = SKPaintStyle.Fill };

        float y = 2;
        _nodeBounds.Clear();
        _toggleBounds.Clear();

        foreach (var n in Nodes)
            y = DrawNode(canvas, n, 0, y, font, textPaint);
    }

    private float DrawNode(SKCanvas canvas, TreeNode node, int depth, float y, SKFont font, SKPaint textPaint)
    {
        float x = 4 + depth * _indent;
        float centerY = y + _rowHeight / 2f;

        // toggle area
        bool hasChildren = node.Nodes.Count > 0;
        // Size the toggle based on font metrics so it visually lines up with text across DPI/font choices
        var fm = font.Metrics;
        float capHeight = Math.Abs(fm.CapHeight) > 0 ? Math.Abs(fm.CapHeight) : font.Size;
        // Clamp toggle size to fit the row and fonts
        float toggleSize = Math.Min(_rowHeight - 4, Math.Max(8f, capHeight * 0.9f));
        if (hasChildren)
        {
            using var paint = new SKPaint { IsAntialias = true, Color = ColorScheme.OnSurface.ToSKColor(), Style = SKPaintStyle.Fill };
            // center point for the toggle
            var cx = x + toggleSize / 2f;
            var cy = centerY;

            // get animation progress for smooth rotation (0..1)
            var progress = (float)(_nodeAnimations.TryGetValue(node, out var na) ? na.GetProgress() : (node.Expanded ? 1.0 : 0.0));

            canvas.Save();
            canvas.Translate(cx, cy);
            canvas.RotateDegrees(90 * progress); // rotate right->down

            var path = new SKPath();
            // draw an isosceles triangle centered at (0,0) so rotation keeps it visually centered
            float width = toggleSize * 0.6f;
            float halfW = width / 2f;
            float halfH = toggleSize / 2f;
            path.MoveTo(-halfW, -halfH);
            path.LineTo(halfW, 0);
            path.LineTo(-halfW, halfH);
            path.Close();
            canvas.DrawPath(path, paint);
            canvas.Restore();

            // Expand hit area slightly and center vertically within the row
            var toggleRectHeight = (int)Math.Ceiling(toggleSize + 6);
            var toggleRectY = (int)Math.Round(centerY - toggleRectHeight / 2f);
            var toggleRectWidth = (int)Math.Ceiling(width + 8);
            _toggleBounds[node] = new Rectangle((int)(x), toggleRectY, toggleRectWidth, toggleRectHeight);
        }

        // text
        float textX = x + (hasChildren ? toggleSize + 6 : 0);
        var text = node.Text;

        // optional icon (simple colored circle)
        if (node.IconColor.HasValue)
        {
            float iconSize = 10f;
            float iconX = textX + iconSize / 2f;
            using var ip = new SKPaint { IsAntialias = true, Color = node.IconColor.Value.ToSKColor(), Style = SKPaintStyle.Fill };
            canvas.DrawCircle(iconX, centerY, iconSize / 2f, ip);
            textX += iconSize + 6;
        }

        float textW = font.MeasureText(text);
        var textRect = new Rectangle((int)textX, (int)y, (int)Math.Ceiling(textW), _rowHeight);

        bool isSelected = _selectedNodes.Contains(node) || ReferenceEquals(node, SelectedNode);

        // hover background (subtle)
        if (_hoveredNode == node && !isSelected)
        {
            using var hover = new SKPaint { IsAntialias = true, Color = ColorScheme.AccentColor.ToSKColor().WithAlpha(20), Style = SKPaintStyle.Fill };
            if (FullRowSelect)
                canvas.DrawRect(new SKRect(0, y, Width, y + _rowHeight), hover);
            else
                canvas.DrawRect(new SKRect(textRect.X, textRect.Y, textRect.Width, textRect.Height), hover);
        }

        // selection background (supports multi-select and HideSelection)
        if (isSelected && (!HideSelection || Focused))
        {
            using var sel = new SKPaint { IsAntialias = true, Color = ColorScheme.AccentColor.ToSKColor().WithAlpha(80), Style = SKPaintStyle.Fill };
            if (FullRowSelect)
                canvas.DrawRect(new SKRect(0, y, Width, y + _rowHeight), sel);
            else
                canvas.DrawRect(new SKRect(textRect.X, textRect.Y, textRect.Width, textRect.Height), sel);
        }

        // compute baseline so text is vertically centered in the row using font metrics
        var fm2 = font.Metrics;
        float baseline = centerY - (fm2.Ascent + fm2.Descent) / 2f;

        TextRenderingHelper.DrawText(canvas, text, textX, baseline, font, textPaint);
        _nodeBounds[node] = new Rectangle((int)textX, (int)y, (int)(Width - textX), _rowHeight);
        _nodeTextBounds[node] = textRect;

        y += _rowHeight; 

        if (node.Expanded)
        {
            foreach (var c in node.Nodes)
                y = DrawNode(canvas, c, depth + 1, y, font, textPaint);
        }

        return y;
    }

    internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var p = e.Location;
        _hoverIndex = -1;
        _hoveredNode = null;
        foreach (var kv in _nodeBounds)
        {
            if (kv.Value.Contains(p))
            {
                Cursor = Cursors.Hand;
                _hoverIndex = 1;
                _hoveredNode = kv.Key;
                Invalidate();
                return;
            }
        }
        Cursor = Cursors.Default;
        Invalidate();
    }

    internal override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        var p = e.Location;
        // toggles
        foreach (var kv in _toggleBounds)
        {
            if (kv.Value.Contains(p))
            {
                var n = kv.Key;
                // start animation when toggling; when collapsing, animate out then collapse on finish
                if (!n.Expanded)
                {
                    n.Expanded = true;
                    var am = new AnimationManager() { AnimationType = AnimationType.EaseInOut, Increment = 0.12, Singular = true, InterruptAnimation = true };
                    am.OnAnimationProgress += _ => Invalidate();
                    am.OnAnimationFinished += _ => { /* nothing special for expand */ };
                    _nodeAnimations[n] = am;
                    am.StartNewAnimation(AnimationDirection.In);
                }
                else
                {
                    // collapse -> animate out and then collapse when finished
                    _pendingCollapse.Add(n);
                    var am = new AnimationManager() { AnimationType = AnimationType.EaseInOut, Increment = 0.12, Singular = true, InterruptAnimation = true };
                    am.OnAnimationProgress += _ => Invalidate();
                    am.OnAnimationFinished += _ => { _pendingCollapse.Remove(n); n.Expanded = false; _nodeAnimations.Remove(n); Invalidate(); };
                    _nodeAnimations[n] = am;
                    am.StartNewAnimation(AnimationDirection.Out);
                }
                return;
            }
        }

        foreach (var kv in _nodeBounds)
        {
            if (kv.Value.Contains(p))
            {
                // if FullRowSelect == false, require click inside text bounds
                if (!FullRowSelect)
                {
                    if (_nodeTextBounds.TryGetValue(kv.Key, out var trect))
                    {
                        if (!trect.Contains(p))
                            continue;
                    }
                }

                var node = kv.Key;

                // Right-click: typically select (if not part of selection), then raise right-click event
                if (e.Button == MouseButtons.Right)
                {
                    if (!_selectedNodes.Contains(node))
                    {
                        _selectedNodes.Clear();
                        _selectedNodes.Add(node);
                        _lastSelectedNode = node;
                        SelectedNodesChanged?.Invoke(this, EventArgs.Empty);
                    }

                    NodeMouseRightClick?.Invoke(this, new TreeNodeMouseClickEventArgs(node, e.Button, e.Clicks, e.Location));
                    NodeMouseClick?.Invoke(this, new TreeNodeMouseClickEventArgs(node, e.Button, e.Clicks, e.Location));
                    Invalidate();
                    return;
                }

                // Double click
                if (e.Clicks >= 2)
                {
                    // ensure single selection on double click (common behavior)
                    _selectedNodes.Clear();
                    _selectedNodes.Add(node);
                    _lastSelectedNode = node;
                    SelectedNodesChanged?.Invoke(this, EventArgs.Empty);
                    NodeMouseDoubleClick?.Invoke(this, new TreeNodeMouseClickEventArgs(node, e.Button, e.Clicks, e.Location));
                    NodeMouseClick?.Invoke(this, new TreeNodeMouseClickEventArgs(node, e.Button, e.Clicks, e.Location));
                    Invalidate();
                    return;
                }

                // Modifier-based selection: Ctrl -> toggle, Shift -> range, otherwise single select
                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    if (_selectedNodes.Contains(node)) _selectedNodes.Remove(node);
                    else _selectedNodes.Add(node);
                    _lastSelectedNode = node;
                    SelectedNodesChanged?.Invoke(this, EventArgs.Empty);
                }
                else if ((ModifierKeys & Keys.Shift) == Keys.Shift && _lastSelectedNode != null)
                {
                    var list = new List<TreeNode>();
                    foreach (var n in Nodes) CollectVisible(n, list);
                    int a = list.IndexOf(_lastSelectedNode);
                    int b = list.IndexOf(node);
                    if (a >= 0 && b >= 0)
                    {
                        var start = Math.Min(a, b);
                        var end = Math.Max(a, b);
                        _selectedNodes.Clear();
                        for (int i = start; i <= end; i++) _selectedNodes.Add(list[i]);
                        SelectedNodesChanged?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        _selectedNodes.Clear(); _selectedNodes.Add(node); SelectedNodesChanged?.Invoke(this, EventArgs.Empty);
                    }
                    _lastSelectedNode = node;
                }
                else
                {
                    _selectedNodes.Clear(); _selectedNodes.Add(node); _lastSelectedNode = node; SelectedNodesChanged?.Invoke(this, EventArgs.Empty);
                }

                SelectedNode = node; // keep backward compatibility
                NodeMouseClick?.Invoke(this, new TreeNodeMouseClickEventArgs(node, e.Button, e.Clicks, e.Location));
                Invalidate();
                return;
            }
        }
    }

    internal override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        Cursor = Cursors.Default;
        _hoverIndex = -1;
        _hoveredNode = null;
        Invalidate();
    }

    internal override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.Up) { MoveSelection(-1); e.Handled = true; }
        else if (e.KeyCode == Keys.Down) { MoveSelection(1); e.Handled = true; }
        else if (e.KeyCode == Keys.Right)
        {
            if (SelectedNode != null)
            {
                if (SelectedNode.Nodes.Count > 0) SelectedNode.Expanded = true;
                else MoveSelection(1);
            }
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Left)
        {
            if (SelectedNode != null)
            {
                if (SelectedNode.Expanded) SelectedNode.Expanded = false;
                else if (SelectedNode.Parent != null) SelectedNode = SelectedNode.Parent;
            }
            e.Handled = true;
        }
        Invalidate();
    }

    private void MoveSelection(int delta)
    {
        // flatten visible nodes to list
        var list = new List<TreeNode>();
        foreach (var n in Nodes) CollectVisible(n, list);
        if (list.Count == 0) return;
        int idx = SelectedNode == null ? -1 : list.IndexOf(SelectedNode);
        idx = Math.Max(0, Math.Min(list.Count - 1, (idx == -1 ? 0 : idx) + delta));
        SelectedNode = list[idx];
    }

    private void CollectVisible(TreeNode node, List<TreeNode> list)
    {
        list.Add(node);
        if (node.Expanded)
        {
            foreach (var c in node.Nodes) CollectVisible(c, list);
        }
    }

    public void SelectNodes(IEnumerable<TreeNode> nodes)
    {
        _selectedNodes.Clear();
        _selectedNodes.AddRange(nodes);
        if (_selectedNodes.Count > 0) _lastSelectedNode = _selectedNodes[^1];
        SelectedNodesChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    public void ClearSelection()
    {
        _selectedNodes.Clear();
        _lastSelectedNode = null;
        SelectedNodesChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    internal override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    internal override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    // convenience
    public void ExpandAll()
    {
        foreach (var n in Nodes) SetExpandedRecursive(n, true);
        Invalidate();
    }

    public void CollapseAll()
    {
        foreach (var n in Nodes) SetExpandedRecursive(n, false);
        Invalidate();
    }

    private void SetExpandedRecursive(TreeNode n, bool value)
    {
        n.Expanded = value;
        foreach (var c in n.Nodes) SetExpandedRecursive(c, value);
    }
}
