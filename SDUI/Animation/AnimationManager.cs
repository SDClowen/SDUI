using System;
using System.Windows.Forms;
using SDUI.AnimationEngine;

namespace SDUI.Animation
{
    /// <summary>
    /// Modern, optimize edilmiþ animation manager - ValueProvider tabanlý
    /// </summary>
    public class AnimationManager : IDisposable
    {
        private readonly ValueProvider<double> _valueProvider;
        private Timer _timer; // Lazy initialization için readonly kaldýrýldý
        private bool _isRunning;
        private bool _disposed;
        private System.Drawing.Point _animationSource;
        private AnimationDirection _currentDirection;
        private object[] _animationData;

        public event Action<object> OnAnimationProgress;
        public event Action<object> OnAnimationFinished;

        public bool Singular { get; set; }
        public bool InterruptAnimation { get; set; }
        public double Increment { get; set; }
        public double SecondaryIncrement { get; set; }
        public AnimationType AnimationType { get; set; }

        public AnimationManager(bool singular = true)
        {
            Singular = singular;
            InterruptAnimation = true;
            Increment = 0.15;
            SecondaryIncrement = 0.15;
            AnimationType = AnimationType.EaseInOut;

            _valueProvider = new ValueProvider<double>(0, ValueFactories.DoubleFactory, EasingMethods.DefaultEase);
            
            // Timer'ý lazy initialization ile oluþtur - handle sorununu çözer
            // _timer = new Timer { Interval = 16 }; // BU SATIR KALDIRILDI
            // _timer.Tick += OnTick; // BU SATIR KALDIRILDI
        }

        public bool Running => _isRunning;
        
        // Lazy initialization - Timer sadece gerektiðinde oluþturulur
        private void EnsureTimer()
        {
            if (_timer != null) return;
            
            try
            {
                _timer = new Timer { Interval = 16 }; // ~60 FPS
                _timer.Tick += OnTick;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AnimationManager: Timer oluþturulamadý - {ex.Message}");
                // Timer oluþturulamazsa animasyon devre dýþý býrakýlýr
                _timer = null;
            }
        }

        public void StartNewAnimation(AnimationDirection direction)
        {
            StartNewAnimation(direction, System.Drawing.Point.Empty, null);
        }

        public void StartNewAnimation(AnimationDirection direction, System.Drawing.Point source)
        {
            StartNewAnimation(direction, source, null);
        }

        public void StartNewAnimation(AnimationDirection direction, object[] data)
        {
            StartNewAnimation(direction, System.Drawing.Point.Empty, data);
        }

        public void StartNewAnimation(AnimationDirection direction, System.Drawing.Point source, object[] data)
        {
            if (_isRunning && !InterruptAnimation)
                return;

            _currentDirection = direction;
            _animationSource = source;
            _animationData = data ?? Array.Empty<object>();
            UpdateEasingMethod();

            double target = direction == AnimationDirection.In || direction == AnimationDirection.InOutIn ? 1.0 : 0.0;
            double currentIncrement = direction == AnimationDirection.InOutOut || direction == AnimationDirection.InOutRepeatingOut ? SecondaryIncrement : Increment;
            double duration = Math.Abs(target - _valueProvider.CurrentValue) / currentIncrement * 16; // milliseconds

            _valueProvider.StartTransition(_valueProvider.CurrentValue, target, TimeSpan.FromMilliseconds(Math.Max(16, duration)));
            
            _isRunning = true;
            
            // Timer'ý lazy initialization ile oluþtur
            EnsureTimer();
            
            if (_timer != null && !_timer.Enabled)
                _timer.Start();
        }

        public double GetProgress()
        {
            return GetProgress(0);
        }

        public double GetProgress(int index)
        {
            return _valueProvider.CurrentValue;
        }

        public System.Drawing.Point GetSource()
        {
            return GetSource(0);
        }

        public System.Drawing.Point GetSource(int index)
        {
            return _animationSource;
        }

        public object[] GetData()
        {
            return GetData(0);
        }

        public object[] GetData(int index)
        {
            return _animationData ?? Array.Empty<object>();
        }

        public int GetAnimationCount()
        {
            return _isRunning ? 1 : 0;
        }

        public AnimationDirection GetDirection()
        {
            return GetDirection(0);
        }

        public AnimationDirection GetDirection(int index)
        {
            return _currentDirection;
        }

        public void SetDirection(AnimationDirection direction)
        {
            _currentDirection = direction;
        }

        public void SetData(object[] data)
        {
            _animationData = data ?? Array.Empty<object>();
        }

        public void SetProgress(double progress)
        {
            progress = Math.Clamp(progress, 0, 1);
            _valueProvider.StartTransition(progress, progress, TimeSpan.Zero);
        }

        public bool IsAnimating()
        {
            return _isRunning;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (_valueProvider.Completed)
            {
                _isRunning = false;
                if (_timer != null)
                    _timer.Stop();
                OnAnimationFinished?.Invoke(this);
            }
            
            OnAnimationProgress?.Invoke(this);
        }

        private void UpdateEasingMethod()
        {
            _valueProvider.EasingMethod = AnimationType switch
            {
                AnimationType.Linear => EasingMethods.Linear,
                AnimationType.EaseIn => EasingMethods.QuadraticEaseIn,
                AnimationType.EaseOut => EasingMethods.QuadraticEaseOut,
                AnimationType.EaseInOut => EasingMethods.QuadraticEaseInOut,
                AnimationType.CubicEaseIn => EasingMethods.CubicEaseIn,
                AnimationType.CubicEaseOut => EasingMethods.CubicEaseOut,
                AnimationType.CubicEaseInOut => EasingMethods.CubicEaseInOut,
                AnimationType.QuarticEaseIn => EasingMethods.QuarticEaseIn,
                AnimationType.QuarticEaseOut => EasingMethods.QuarticEaseOut,
                AnimationType.QuarticEaseInOut => EasingMethods.QuarticEaseInOut,
                _ => EasingMethods.DefaultEase
            };
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= OnTick; // Event handler'ý kaldýr
                _timer.Dispose();
                _timer = null;
            }
            
            _disposed = true;
        }
    }
}
