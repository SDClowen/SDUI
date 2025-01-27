using SDUI.Animation;
using SDUI.Extensions;
using SDUI.SK;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class CheckBox : SKControl
    {
        private const int CHECKBOX_SIZE = 16;
        private const int CHECKBOX_SIZE_HALF = CHECKBOX_SIZE / 2;
        private const int TEXT_PADDING = 4;

        // Tik işareti koordinatları - checkbox içinde ortalı olacak şekilde ayarlandı
        private static readonly Point[] CHECKMARK_LINE = { new(4, 8), new(7, 11), new(12, 6) };

        private readonly Animation.AnimationEngine animationManager;
        private readonly Animation.AnimationEngine rippleAnimationManager;

        private int boxOffset;
        private RectangleF boxRectangle;
        private bool ripple;
        private bool _checked;
        private Point mouseLocation;
        private int _mouseState;
        private CheckState _checkState = CheckState.Unchecked;
        private bool _threeState;

        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public Point MouseLocation
        {
            get => mouseLocation;
            set
            {
                mouseLocation = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        public bool Ripple
        {
            get => ripple;
            set
            {
                ripple = value;
                AutoSize = AutoSize;
                if (value) Margin = new Padding(0);
                Invalidate();
            }
        }

        [Category("Behavior")]
        public bool ThreeState
        {
            get => _threeState;
            set
            {
                if (_threeState == value) return;
                _threeState = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        public CheckState CheckState
        {
            get => _checkState;
            set
            {
                if (_checkState == value) return;
                _checkState = value;
                _checked = value != CheckState.Unchecked;
                OnCheckStateChanged(EventArgs.Empty);
                Invalidate();
            }
        }

        [Category("Behavior")]
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;
                _checked = value;
                CheckState = value ? CheckState.Checked : CheckState.Unchecked;
                OnCheckedChanged(EventArgs.Empty);
                Invalidate();
            }
        }

        public event EventHandler CheckedChanged;
        public event EventHandler CheckStateChanged;

        protected virtual void OnCheckedChanged(EventArgs e) => CheckedChanged?.Invoke(this, e);
        protected virtual void OnCheckStateChanged(EventArgs e) => CheckStateChanged?.Invoke(this, e);

        public CheckBox()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            SetStyle(ControlStyles.FixedHeight | ControlStyles.Selectable, false);

            animationManager = new()
            {
                AnimationType = AnimationType.EaseInOut,
                Increment = 0.10
            };

            rippleAnimationManager = new(false)
            {
                AnimationType = AnimationType.Linear,
                Increment = 0.10,
                SecondaryIncrement = 0.07
            };

            animationManager.OnAnimationProgress += _ => Invalidate();
            rippleAnimationManager.OnAnimationProgress += _ => Invalidate();

            CheckedChanged += (_, _) => animationManager.StartNewAnimation(Checked ? AnimationDirection.In : AnimationDirection.Out);

            Ripple = true;
            MouseLocation = new Point(-1, -1);
        }

        private bool IsMouseInCheckArea() => boxRectangle.Contains(MouseLocation);

        public override Size GetPreferredSize(Size proposedSize)
        {
            using var paint = new SKPaint
            {
                TextSize = Font.Size.PtToPx(this),
                Typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name)
            };

            float textWidth = string.IsNullOrEmpty(Text) ? 0 : paint.MeasureText(Text);
            int width = CHECKBOX_SIZE + TEXT_PADDING + (int)Math.Ceiling(textWidth);
            int height = Ripple ? 30 : 20;

            return new Size(width + Padding.Horizontal, height + Padding.Vertical);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (DesignMode) return;

            MouseEnter += (_, _) => _mouseState = 1;
            MouseLeave += (_, _) =>
            {
                MouseLocation = new Point(-1, -1);
                _mouseState = 0;
            };
            MouseDown += (_, e) =>
            {
                _mouseState = 2;
                if (Ripple && e.Button == MouseButtons.Left && IsMouseInCheckArea())
                {
                    rippleAnimationManager.SecondaryIncrement = 0;
                    rippleAnimationManager.StartNewAnimation(AnimationDirection.InOutIn, new object[] { Checked });
                }
            };
            MouseUp += (_, _) =>
            {
                _mouseState = 1;
                rippleAnimationManager.SecondaryIncrement = 0.08;
            };
            MouseMove += (_, e) =>
            {
                MouseLocation = e.Location;
                Cursor = IsMouseInCheckArea() ? Cursors.Hand : Cursors.Default;
            };
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear();

            var animationProgress = (float)animationManager.GetProgress();
            var disabledOffColor = ColorScheme.BorderColor;
            var accentColor = Enabled ? ColorScheme.AccentColor : disabledOffColor;

            int colorAlpha = Enabled ? (int)(animationProgress * 255.0) : disabledOffColor.A;
            int backgroundAlpha = Enabled ? (int)(ColorScheme.BorderColor.A * (1.0 - animationProgress)) : disabledOffColor.A;

            // Ripple efekti
            if (Ripple && rippleAnimationManager.IsAnimating())
            {
                DrawRippleEffect(canvas, accentColor);
            }

            // Checkbox çerçeve ve arka plan
            var boxRect = new SKRect(
                boxOffset,
                boxOffset,
                boxOffset + CHECKBOX_SIZE - 1,
                boxOffset + CHECKBOX_SIZE - 1
            );

            // Arka plan
            using (var paint = new SKPaint
            {
                Color = ColorScheme.BackColor.BlendWith(Enabled ? ColorScheme.BorderColor : disabledOffColor, backgroundAlpha).ToSKColor(),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            })
            {
                canvas.DrawRoundRect(boxRect, 2, 2, paint);
            }

            // Çerçeve
            using (var paint = new SKPaint
            {
                Color = ColorScheme.BorderColor.ToSKColor(),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            })
            {
                canvas.DrawRoundRect(boxRect, 2, 2, paint);
            }

            // Checkbox dolgu ve işaret
            if (Checked || CheckState == CheckState.Indeterminate)
            {
                DrawCheckboxFill(canvas, boxRect, accentColor, colorAlpha, animationProgress);
            }

            // Text
            if (!string.IsNullOrEmpty(Text))
            {
                DrawText(canvas);
            }

            // Debug çerçevesi
            if (ColorScheme.DrawDebugBorders)
            {
                using var paint = new SKPaint
                {
                    Color = SKColors.Red,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1
                };
                canvas.DrawRect(0, 0, Width - 1, Height - 1, paint);
            }
        }

        private void DrawRippleEffect(SKCanvas canvas, Color accentColor)
        {
            var CHECKBOX_CENTER = boxOffset + CHECKBOX_SIZE_HALF - 1;

            for (int i = 0; i < rippleAnimationManager.GetAnimationCount(); i++)
            {
                var animationValue = (float)rippleAnimationManager.GetProgress(i);
                var animationSource = new SKPoint(CHECKBOX_CENTER, CHECKBOX_CENTER);

                var rippleColor = ((bool)rippleAnimationManager.GetData(i)[0]) ?
                    SKColors.Black.WithAlpha((byte)(animationValue * 40)) :
                    accentColor.ToSKColor().WithAlpha((byte)(animationValue * 40));

                var rippleHeight = (Height % 2 == 0) ? Height - 3f : Height - 2f;
                var rippleSize = (rippleAnimationManager.GetDirection(i) == AnimationDirection.InOutIn) ?
                    rippleHeight * (0.8f + (0.2f * animationValue)) : rippleHeight;

                using var paint = new SKPaint
                {
                    Color = rippleColor,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };

                canvas.DrawCircle(animationSource.X, animationSource.Y, rippleSize / 2, paint);
            }
        }

        private void DrawCheckboxFill(SKCanvas canvas, SKRect boxRect, Color accentColor, int colorAlpha, float animationProgress)
        {
            using var paint = new SKPaint
            {
                Color = accentColor.ToSKColor().WithAlpha((byte)colorAlpha),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRoundRect(boxRect, 2, 2, paint);

            using var checkPaint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            };

            if (CheckState == CheckState.Indeterminate)
            {
                canvas.DrawLine(
                    boxOffset + 4,
                    boxOffset + CHECKBOX_SIZE_HALF,
                    boxOffset + CHECKBOX_SIZE - 4,
                    boxOffset + CHECKBOX_SIZE_HALF,
                    checkPaint
                );
            }
            else
            {
                var checkMarkRect = new SKRect(
                    boxOffset,
                    boxOffset,
                    boxOffset + (CHECKBOX_SIZE * animationProgress),
                    boxOffset + CHECKBOX_SIZE
                );

                using var clipPath = new SKPath();
                clipPath.AddRect(checkMarkRect);
                canvas.Save();
                canvas.ClipPath(clipPath);

                var path = new SKPath();
                path.MoveTo(boxOffset + CHECKMARK_LINE[0].X, boxOffset + CHECKMARK_LINE[0].Y);
                path.LineTo(boxOffset + CHECKMARK_LINE[1].X, boxOffset + CHECKMARK_LINE[1].Y);
                path.LineTo(boxOffset + CHECKMARK_LINE[2].X, boxOffset + CHECKMARK_LINE[2].Y);

                canvas.DrawPath(path, checkPaint);
                canvas.Restore();
            }
        }

        private void DrawText(SKCanvas canvas)
        {
            using var textPaint = canvas.CreateTextPaint(Font, Enabled ? ColorScheme.ForeColor : Color.Gray, this, ContentAlignment.MiddleLeft);
            
            float textY = Height / 2f + (textPaint.FontMetrics.XHeight / 2f);
            float textX = boxOffset + CHECKBOX_SIZE + TEXT_PADDING;

            if (AutoEllipsis)
            {
                float maxWidth = Width - textX - Padding.Right;
                canvas.DrawTextWithEllipsis(Text, textPaint, textX, textY, maxWidth);
            }
            else if (UseMnemonic)
            {
                canvas.DrawTextWithMnemonic(Text, textPaint, textX, textY);
            }
            else
            {
                canvas.DrawText(Text, textX, textY, textPaint);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            boxOffset = (Height - CHECKBOX_SIZE) / 2;
            boxRectangle = new RectangleF(boxOffset, boxOffset, CHECKBOX_SIZE - 1, CHECKBOX_SIZE - 1);
        }

        protected override void OnClick(EventArgs e)
        {
            if (ThreeState)
            {
                CheckState = CheckState switch
                {
                    CheckState.Unchecked => CheckState.Checked,
                    CheckState.Checked => CheckState.Indeterminate,
                    CheckState.Indeterminate => CheckState.Unchecked,
                    _ => CheckState.Unchecked
                };
            }
            else
            {
                Checked = !Checked;
            }
            base.OnClick(e);
        }
    }
}