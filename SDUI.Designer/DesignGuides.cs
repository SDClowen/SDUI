using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SDUI.Controls;
using SkiaSharp;

namespace SDUI.Designer;

/// <summary>
/// Overlay for showing alignment guides, snaplines, and container highlights during drag operations
/// </summary>
internal class DesignGuides : UIElementBase
{
    private readonly List<GuideLine> _horizontalGuides = new();
    private readonly List<GuideLine> _verticalGuides = new();
    private Rectangle _containerHighlight;
    private bool _showContainerHighlight;
    private Rectangle _draggedControlBounds;
    private bool _showDistanceIndicators;
    private readonly List<DistanceIndicator> _distanceIndicators = new();

    private struct GuideLine
    {
        public int Position;
        public int Start;
        public int End;
        public GuideType Type;
    }

    private struct DistanceIndicator
    {
        public Rectangle FromBounds;
        public Rectangle ToBounds;
        public bool IsHorizontal;
        public int Distance;
    }

    private enum GuideType
    {
        Left,
        Right,
        Top,
        Bottom,
        CenterHorizontal,
        CenterVertical
    }

    public DesignGuides()
    {
        BackColor = Color.Transparent;
        Visible = false;
    }

    public void ShowContainerHighlight(Rectangle bounds)
    {
        _containerHighlight = bounds;
        _showContainerHighlight = true;
        Visible = true;
        BringToFront();
        Invalidate();
    }

    public void ClearContainerHighlight()
    {
        _showContainerHighlight = false;
        CheckVisibility();
        Invalidate();
    }

    public void ShowSnapLines(Rectangle draggedBounds, IEnumerable<Rectangle> otherBounds)
    {
        _draggedControlBounds = draggedBounds;
        _horizontalGuides.Clear();
        _verticalGuides.Clear();
        _distanceIndicators.Clear();

        const int snapThreshold = 8;
        var otherBoundsList = otherBounds.ToList();

        // Calculate potential snap lines
        foreach (var other in otherBoundsList)
        {
            // Vertical alignment guides
            CheckAndAddVerticalGuide(draggedBounds.Left, other.Left, draggedBounds, other, GuideType.Left, snapThreshold);
            CheckAndAddVerticalGuide(draggedBounds.Right, other.Right, draggedBounds, other, GuideType.Right, snapThreshold);
            CheckAndAddVerticalGuide(draggedBounds.Left + draggedBounds.Width / 2, 
                other.Left + other.Width / 2, draggedBounds, other, GuideType.CenterVertical, snapThreshold);

            // Horizontal alignment guides
            CheckAndAddHorizontalGuide(draggedBounds.Top, other.Top, draggedBounds, other, GuideType.Top, snapThreshold);
            CheckAndAddHorizontalGuide(draggedBounds.Bottom, other.Bottom, draggedBounds, other, GuideType.Bottom, snapThreshold);
            CheckAndAddHorizontalGuide(draggedBounds.Top + draggedBounds.Height / 2,
                other.Top + other.Height / 2, draggedBounds, other, GuideType.CenterHorizontal, snapThreshold);

            // Distance indicators for nearby controls
            CalculateDistanceIndicators(draggedBounds, other);
        }

        _showDistanceIndicators = _distanceIndicators.Count > 0;
        Visible = _horizontalGuides.Count > 0 || _verticalGuides.Count > 0 || _showDistanceIndicators;
        
        if (Visible)
        {
            BringToFront();
            Invalidate();
        }
    }

    private void CheckAndAddVerticalGuide(int pos1, int pos2, Rectangle bounds1, Rectangle bounds2, GuideType type, int threshold)
    {
        if (Math.Abs(pos1 - pos2) <= threshold)
        {
            var minY = Math.Min(bounds1.Top, bounds2.Top);
            var maxY = Math.Max(bounds1.Bottom, bounds2.Bottom);

            _verticalGuides.Add(new GuideLine
            {
                Position = pos2,
                Start = minY,
                End = maxY,
                Type = type
            });
        }
    }

    private void CheckAndAddHorizontalGuide(int pos1, int pos2, Rectangle bounds1, Rectangle bounds2, GuideType type, int threshold)
    {
        if (Math.Abs(pos1 - pos2) <= threshold)
        {
            var minX = Math.Min(bounds1.Left, bounds2.Left);
            var maxX = Math.Max(bounds1.Right, bounds2.Right);

            _horizontalGuides.Add(new GuideLine
            {
                Position = pos2,
                Start = minX,
                End = maxX,
                Type = type
            });
        }
    }

    private void CalculateDistanceIndicators(Rectangle dragged, Rectangle other)
    {
        const int maxDistance = 50;

        // Horizontal spacing
        if (dragged.Right < other.Left && other.Left - dragged.Right < maxDistance)
        {
            // Control is to the left
            _distanceIndicators.Add(new DistanceIndicator
            {
                FromBounds = dragged,
                ToBounds = other,
                IsHorizontal = true,
                Distance = other.Left - dragged.Right
            });
        }
        else if (other.Right < dragged.Left && dragged.Left - other.Right < maxDistance)
        {
            // Control is to the right
            _distanceIndicators.Add(new DistanceIndicator
            {
                FromBounds = other,
                ToBounds = dragged,
                IsHorizontal = true,
                Distance = dragged.Left - other.Right
            });
        }

        // Vertical spacing
        if (dragged.Bottom < other.Top && other.Top - dragged.Bottom < maxDistance)
        {
            // Control is above
            _distanceIndicators.Add(new DistanceIndicator
            {
                FromBounds = dragged,
                ToBounds = other,
                IsHorizontal = false,
                Distance = other.Top - dragged.Bottom
            });
        }
        else if (other.Bottom < dragged.Top && dragged.Top - other.Bottom < maxDistance)
        {
            // Control is below
            _distanceIndicators.Add(new DistanceIndicator
            {
                FromBounds = other,
                ToBounds = dragged,
                IsHorizontal = false,
                Distance = dragged.Top - other.Bottom
            });
        }
    }

    public void ClearSnapLines()
    {
        _horizontalGuides.Clear();
        _verticalGuides.Clear();
        _distanceIndicators.Clear();
        _showDistanceIndicators = false;
        CheckVisibility();
        Invalidate();
    }

    private void CheckVisibility()
    {
        Visible = _showContainerHighlight || _horizontalGuides.Count > 0 || _verticalGuides.Count > 0 || _showDistanceIndicators;
    }

    public override void OnPaint(SKCanvas canvas)
    {
        if (!Visible) return;

        // Draw container highlight
        if (_showContainerHighlight)
        {
            using var containerPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = ColorScheme.Primary.ToSKColor().WithAlpha(100),
                StrokeWidth = 3,
                PathEffect = SKPathEffect.CreateDash(new float[] { 8, 4 }, 0)
            };

            var containerRect = new SKRect(
                _containerHighlight.X,
                _containerHighlight.Y,
                _containerHighlight.Right,
                _containerHighlight.Bottom
            );
            canvas.DrawRect(containerRect, containerPaint);

            // Fill with semi-transparent color
            using var fillPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = ColorScheme.Primary.ToSKColor().WithAlpha(20)
            };
            canvas.DrawRect(containerRect, fillPaint);
        }

        // Draw vertical snap lines
        using var snapLinePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(0, 120, 215),
            StrokeWidth = 1,
            IsAntialias = true
        };

        foreach (var guide in _verticalGuides)
        {
            canvas.DrawLine(guide.Position, guide.Start, guide.Position, guide.End, snapLinePaint);
        }

        foreach (var guide in _horizontalGuides)
        {
            canvas.DrawLine(guide.Start, guide.Position, guide.End, guide.Position, snapLinePaint);
        }

        // Draw distance indicators
        if (_showDistanceIndicators)
        {
            using var distancePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(255, 100, 100),
                StrokeWidth = 1,
                IsAntialias = true
            };

            using var font = new SKFont
            {
                Size = 10,
                Edging = SKFontEdging.Antialias
            };

            using var textPaint = new SKPaint
            {
                Color = new SKColor(255, 100, 100),
                IsAntialias = true
            };

            foreach (var indicator in _distanceIndicators)
            {
                if (indicator.IsHorizontal)
                {
                    // Horizontal distance line
                    var y = (indicator.FromBounds.Top + indicator.FromBounds.Bottom) / 2;
                    var x1 = indicator.FromBounds.Right;
                    var x2 = indicator.ToBounds.Left;

                    canvas.DrawLine(x1, y, x2, y, distancePaint);
                    canvas.DrawLine(x1, y - 3, x1, y + 3, distancePaint);
                    canvas.DrawLine(x2, y - 3, x2, y + 3, distancePaint);

                    // Draw distance text
                    var text = indicator.Distance.ToString();
                    var textWidth = font.MeasureText(text);
                    var textX = (x1 + x2) / 2 - textWidth / 2;
                    canvas.DrawText(text, textX, y - 5, font, textPaint);
                }
                else
                {
                    // Vertical distance line
                    var x = (indicator.FromBounds.Left + indicator.FromBounds.Right) / 2;
                    var y1 = indicator.FromBounds.Bottom;
                    var y2 = indicator.ToBounds.Top;

                    canvas.DrawLine(x, y1, x, y2, distancePaint);
                    canvas.DrawLine(x - 3, y1, x + 3, y1, distancePaint);
                    canvas.DrawLine(x - 3, y2, x + 3, y2, distancePaint);

                    // Draw distance text
                    var text = indicator.Distance.ToString();
                    var textY = (y1 + y2) / 2;
                    canvas.DrawText(text, x + 5, textY, font, textPaint);
                }
            }
        }
    }

    public Point SnapToGuides(Point location, Size size)
    {
        var snappedLocation = location;
        const int snapThreshold = 8;

        // Snap to vertical guides
        foreach (var guide in _verticalGuides)
        {
            switch (guide.Type)
            {
                case GuideType.Left:
                    if (Math.Abs(location.X - guide.Position) <= snapThreshold)
                        snappedLocation.X = guide.Position;
                    break;
                case GuideType.Right:
                    if (Math.Abs(location.X + size.Width - guide.Position) <= snapThreshold)
                        snappedLocation.X = guide.Position - size.Width;
                    break;
                case GuideType.CenterVertical:
                    var centerX = location.X + size.Width / 2;
                    if (Math.Abs(centerX - guide.Position) <= snapThreshold)
                        snappedLocation.X = guide.Position - size.Width / 2;
                    break;
            }
        }

        // Snap to horizontal guides
        foreach (var guide in _horizontalGuides)
        {
            switch (guide.Type)
            {
                case GuideType.Top:
                    if (Math.Abs(location.Y - guide.Position) <= snapThreshold)
                        snappedLocation.Y = guide.Position;
                    break;
                case GuideType.Bottom:
                    if (Math.Abs(location.Y + size.Height - guide.Position) <= snapThreshold)
                        snappedLocation.Y = guide.Position - size.Height;
                    break;
                case GuideType.CenterHorizontal:
                    var centerY = location.Y + size.Height / 2;
                    if (Math.Abs(centerY - guide.Position) <= snapThreshold)
                        snappedLocation.Y = guide.Position - size.Height / 2;
                    break;
            }
        }

        return snappedLocation;
    }
}
