using SkiaSharp;
using System;
using System.Drawing;
using System.Threading;

namespace SDUI;

public class ColorScheme
{
    /// <summary>
    /// When enabled, the UI favors a flat look (no elevation / heavy shadows).
    /// </summary>
    public static bool FlatDesign { get; set; } = true;

    private static bool _isDarkMode = false;
    private static double _themeTransitionProgress = 1.0;
    private static bool _isTransitioning;
    private static readonly object _lock = new();

    private static SynchronizationContext? _uiContext;
    private static int _themeChangedQueued;

    // Theme background transition (drives surface + derived colors)
    private static Color _themeBackgroundFrom = Color.FromArgb(250, 250, 250);
    private static Color _themeBackgroundTo = Color.FromArgb(250, 250, 250);
    private static int _themeTransitionId;

    private static double _accentTransitionProgress = 1.0;
    private static bool _isAccentTransitioning;
    private static readonly object _accentLock = new();

    // Accent palette transition (light/dark variants)
    private static Color _primaryLightFrom = Color.FromArgb(33, 150, 243);  // #2196F3
    private static Color _primaryLightTo = Color.FromArgb(33, 150, 243);
    private static Color _primaryDarkFrom = Color.FromArgb(30, 136, 229);   // #1E88E5
    private static Color _primaryDarkTo = Color.FromArgb(30, 136, 229);

    private static Color _primaryContainerLightFrom = Color.FromArgb(227, 242, 253); // #E3F2FD
    private static Color _primaryContainerLightTo = Color.FromArgb(227, 242, 253);
    private static Color _primaryContainerDarkFrom = Color.FromArgb(25, 118, 210);   // #1976D2
    private static Color _primaryContainerDarkTo = Color.FromArgb(25, 118, 210);

    private static Color _onPrimaryContainerLightFrom = Color.FromArgb(13, 71, 161);
    private static Color _onPrimaryContainerLightTo = Color.FromArgb(13, 71, 161);
    private static Color _onPrimaryContainerDarkFrom = Color.FromArgb(227, 242, 253);
    private static Color _onPrimaryContainerDarkTo = Color.FromArgb(227, 242, 253);
    
    /// <summary>
    /// Toggle between light and dark theme
    /// </summary>
    public static bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            _uiContext ??= SynchronizationContext.Current;

            // Even if the flag is already equal, user intent may be to jump back to the
            // canonical light/dark background (e.g. after random background theme).
            var targetBackground = value ? Color.FromArgb(28, 28, 30) : Color.FromArgb(250, 250, 250);
            if (_isDarkMode == value && ThemeTransitionProgress >= 0.999 && ColorsClose(CurrentThemeBackground, targetBackground))
                return;

            // Smooth transition request
            StartDarkModeTransition(value);
        }
    }

    /// <summary>
    /// Theme transition entry point: animates from current accent to target accent color.
    /// </summary>
    public static void StartThemeTransition(Color targetColor)
    {
        _uiContext ??= SynchronizationContext.Current;

        // Random/background-driven theme: adapt foreground + surfaces automatically.
        // Also choose a reasonable accent derived from the background so controls remain visible.
        var targetBackground = Color.FromArgb(targetColor.R, targetColor.G, targetColor.B);
        var targetIsDark = RelativeLuminance(targetBackground) < 0.45;
        _isDarkMode = targetIsDark;

        var accentSeed = targetBackground.Brightness(targetIsDark ? 0.35f : -0.35f);
        SetPrimarySeedColor(accentSeed);

        StartThemeBackgroundTransition(targetBackground);
    }

    public static event EventHandler? ThemeChanged;

    /// <summary>
    /// Progress of current accent (primary color) transition in range [0,1].
    /// </summary>
    public static double AccentTransitionProgress
    {
        get => _accentTransitionProgress;
        private set
        {
            _accentTransitionProgress = Math.Clamp(value, 0.0, 1.0);
            RequestThemeChanged();
        }
    }

    /// <summary>
    /// Progress of current theme transition in range [0,1].
    /// 0 = previous theme, 1 = target theme.
    /// </summary>
    public static double ThemeTransitionProgress
    {
        get => _themeTransitionProgress;
        private set
        {
            _themeTransitionProgress = Math.Clamp(value, 0.0, 1.0);
            RequestThemeChanged();
        }
    }

    /// <summary>
    /// Immediately sets theme without animation (used internally after transition).
    /// </summary>
    public static void SetThemeInstant(bool dark)
    {
        _uiContext ??= SynchronizationContext.Current;
        _isDarkMode = dark;
        var bg = dark ? Color.FromArgb(28, 28, 30) : Color.FromArgb(250, 250, 250);
        _themeBackgroundFrom = bg;
        _themeBackgroundTo = bg;
        ThemeTransitionProgress = 1.0;
    }

    private static void StartDarkModeTransition(bool targetDark)
    {
        _isDarkMode = targetDark;
        var targetBackground = targetDark ? Color.FromArgb(28, 28, 30) : Color.FromArgb(250, 250, 250);
        StartThemeBackgroundTransition(targetBackground);
    }

    private static void RequestThemeChanged()
    {
        if (Interlocked.Exchange(ref _themeChangedQueued, 1) == 1)
            return;

        void Raise()
        {
            Interlocked.Exchange(ref _themeChangedQueued, 0);
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        var ctx = _uiContext;
        if (ctx != null)
            ctx.Post(_ => Raise(), null);
        else
            Raise();
    }

    private static bool ColorsClose(Color a, Color b, int threshold = 2)
    {
        return Math.Abs(a.R - b.R) <= threshold
            && Math.Abs(a.G - b.G) <= threshold
            && Math.Abs(a.B - b.B) <= threshold
            && Math.Abs(a.A - b.A) <= threshold;
    }

    private static Color CurrentThemeBackground => Lerp(_themeBackgroundFrom, _themeBackgroundTo, ThemeTransitionProgress);
    private static bool CurrentIsDark => RelativeLuminance(CurrentThemeBackground) < 0.45;

    private static Color SurfaceAdjust(Color baseColor, double amount)
    {
        // For dark backgrounds we lift surfaces slightly, for light we shade slightly.
        return CurrentIsDark
            ? Blend(baseColor, Color.FromArgb(255, 255, 255), amount)
            : Blend(baseColor, Color.FromArgb(0, 0, 0), amount);
    }

    private static async void StartThemeBackgroundTransition(Color targetBackground)
    {
        int transitionId;

        lock (_lock)
        {
            _themeTransitionId++;
            transitionId = _themeTransitionId;

            _themeBackgroundFrom = CurrentThemeBackground;
            _themeBackgroundTo = targetBackground;
            _isTransitioning = true;
            ThemeTransitionProgress = 0.0;
        }

        const int durationMs = 220;
        const int stepMs = 16;
        int steps = Math.Max(1, durationMs / stepMs);

        for (int i = 0; i <= steps; i++)
        {
            lock (_lock)
            {
                if (transitionId != _themeTransitionId)
                    return;
            }

            double t = (double)i / steps;
            t = t * t * (3 - 2 * t);
            ThemeTransitionProgress = t;
            await System.Threading.Tasks.Task.Delay(stepMs);
        }

        ThemeTransitionProgress = 1.0;
        lock (_lock)
        {
            if (transitionId == _themeTransitionId)
                _isTransitioning = false;
        }
    }

    // Helper for smooth color lerp
    private static Color Lerp(Color from, Color to, double t)
    {
        int r = (int)Math.Round(from.R + (to.R - from.R) * t);
        int g = (int)Math.Round(from.G + (to.G - from.G) * t);
        int b = (int)Math.Round(from.B + (to.B - from.B) * t);
        int a = (int)Math.Round(from.A + (to.A - from.A) * t);
        return Color.FromArgb(a, r, g, b);
    }

    private static Color Blend(Color a, Color b, double t) => Lerp(a, b, t);

    private static double RelativeLuminance(Color c)
    {
        static double SrgbToLinear(double v)
        {
            v /= 255.0;
            return v <= 0.04045 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        }

        double r = SrgbToLinear(c.R);
        double g = SrgbToLinear(c.G);
        double b = SrgbToLinear(c.B);
        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }

    private static Color ReadableOn(Color background)
    {
        // Simple WCAG-ish threshold; good enough for our palette.
        return RelativeLuminance(background) < 0.45 ? Color.FromArgb(255, 255, 255) : Color.FromArgb(28, 28, 28);
    }

    private static void ComputePrimaryPalette(Color seed,
        out Color lightPrimary,
        out Color darkPrimary,
        out Color lightContainer,
        out Color darkContainer,
        out Color onLightContainer,
        out Color onDarkContainer)
    {
        // Light primary: seed
        lightPrimary = Color.FromArgb(seed.R, seed.G, seed.B);
        // Dark primary: slightly darkened
        darkPrimary = Blend(lightPrimary, Color.FromArgb(0, 0, 0), 0.12);

        // Containers: light gets a very light tint, dark gets a stronger tint.
        lightContainer = Blend(lightPrimary, Color.FromArgb(255, 255, 255), 0.88);
        darkContainer = Blend(lightPrimary, Color.FromArgb(0, 0, 0), 0.28);

        onLightContainer = ReadableOn(lightContainer);
        onDarkContainer = ReadableOn(darkContainer);
    }

    /// <summary>
    /// Sets a new primary (accent) seed color with smooth animation.
    /// </summary>
    public static void SetPrimarySeedColor(Color seed)
    {
        _uiContext ??= SynchronizationContext.Current;

        // Capture current interpolated palette as the "from" state so we can retarget mid-animation.
        Color curPrimaryLight = Lerp(_primaryLightFrom, _primaryLightTo, AccentTransitionProgress);
        Color curPrimaryDark = Lerp(_primaryDarkFrom, _primaryDarkTo, AccentTransitionProgress);
        Color curContainerLight = Lerp(_primaryContainerLightFrom, _primaryContainerLightTo, AccentTransitionProgress);
        Color curContainerDark = Lerp(_primaryContainerDarkFrom, _primaryContainerDarkTo, AccentTransitionProgress);
        Color curOnContainerLight = Lerp(_onPrimaryContainerLightFrom, _onPrimaryContainerLightTo, AccentTransitionProgress);
        Color curOnContainerDark = Lerp(_onPrimaryContainerDarkFrom, _onPrimaryContainerDarkTo, AccentTransitionProgress);

        ComputePrimaryPalette(seed,
            out var nextPrimaryLight,
            out var nextPrimaryDark,
            out var nextContainerLight,
            out var nextContainerDark,
            out var nextOnContainerLight,
            out var nextOnContainerDark);

        lock (_accentLock)
        {
            _primaryLightFrom = curPrimaryLight;
            _primaryDarkFrom = curPrimaryDark;
            _primaryContainerLightFrom = curContainerLight;
            _primaryContainerDarkFrom = curContainerDark;
            _onPrimaryContainerLightFrom = curOnContainerLight;
            _onPrimaryContainerDarkFrom = curOnContainerDark;

            _primaryLightTo = nextPrimaryLight;
            _primaryDarkTo = nextPrimaryDark;
            _primaryContainerLightTo = nextContainerLight;
            _primaryContainerDarkTo = nextContainerDark;
            _onPrimaryContainerLightTo = nextOnContainerLight;
            _onPrimaryContainerDarkTo = nextOnContainerDark;

            _isAccentTransitioning = true;
        }

        StartAccentTransition();
    }

    private static async void StartAccentTransition()
    {
        lock (_accentLock)
        {
            if (_isAccentTransitioning == false)
                _isAccentTransitioning = true;
        }

        const int durationMs = 220;
        const int stepMs = 16;
        int steps = Math.Max(1, durationMs / stepMs);

        for (int i = 0; i <= steps; i++)
        {
            double t = (double)i / steps;
            t = t * t * (3 - 2 * t);
            AccentTransitionProgress = t;
            await System.Threading.Tasks.Task.Delay(stepMs);
        }

        AccentTransitionProgress = 1.0;
        lock (_accentLock)
        {
            _isAccentTransitioning = false;
        }
    }

    // Surface colors (derived from current transitioning background)
    public static Color Surface => CurrentThemeBackground;
    public static Color BackColor => CurrentThemeBackground;
    public static Color ForeColor => ReadableOn(CurrentThemeBackground);

    public static Color SurfaceVariant => SurfaceAdjust(Surface, 0.10);
    public static Color SurfaceContainerLowest => SurfaceAdjust(Surface, 0.03);
    public static Color SurfaceContainerLow => SurfaceAdjust(Surface, 0.06);
    public static Color SurfaceContainer => SurfaceAdjust(Surface, 0.08);
    public static Color SurfaceContainerHigh => SurfaceAdjust(Surface, 0.12);
    
    // Accent colors - DodgerBlue-ish modern palette
    private static Color PrimaryLightCurrent => Lerp(_primaryLightFrom, _primaryLightTo, AccentTransitionProgress);
    private static Color PrimaryDarkCurrent => Lerp(_primaryDarkFrom, _primaryDarkTo, AccentTransitionProgress);
    private static Color PrimaryContainerLightCurrent => Lerp(_primaryContainerLightFrom, _primaryContainerLightTo, AccentTransitionProgress);
    private static Color PrimaryContainerDarkCurrent => Lerp(_primaryContainerDarkFrom, _primaryContainerDarkTo, AccentTransitionProgress);
    private static Color OnPrimaryContainerLightCurrent => Lerp(_onPrimaryContainerLightFrom, _onPrimaryContainerLightTo, AccentTransitionProgress);
    private static Color OnPrimaryContainerDarkCurrent => Lerp(_onPrimaryContainerDarkFrom, _onPrimaryContainerDarkTo, AccentTransitionProgress);

    public static Color Primary => CurrentIsDark ? PrimaryDarkCurrent : PrimaryLightCurrent;
    public static Color PrimaryContainer => CurrentIsDark ? PrimaryContainerDarkCurrent : PrimaryContainerLightCurrent;
    public static Color OnPrimary => Color.FromArgb(255, 255, 255);
    public static Color OnPrimaryContainer => CurrentIsDark ? OnPrimaryContainerDarkCurrent : OnPrimaryContainerLightCurrent;
    
    public static Color Secondary => CurrentIsDark ? Color.FromArgb(120, 144, 156) : Color.FromArgb(96, 125, 139);
    public static Color SecondaryContainer => CurrentIsDark ? Color.FromArgb(55, 71, 79) : Color.FromArgb(236, 239, 241);
    
    public static Color Tertiary => CurrentIsDark ? Color.FromArgb(255, 183, 77) : Color.FromArgb(255, 152, 0);
    public static Color TertiaryContainer => CurrentIsDark ? Color.FromArgb(239, 108, 0) : Color.FromArgb(255, 243, 224);
    
    // Semantic colors
    public static Color Error => CurrentIsDark ? Color.FromArgb(255, 100, 100) : Color.FromArgb(220, 50, 50);
    public static Color ErrorContainer => CurrentIsDark ? Color.FromArgb(100, 40, 40) : Color.FromArgb(255, 230, 230);
    public static Color OnError => Color.FromArgb(255, 255, 255);
    
    public static Color Success => CurrentIsDark ? Color.FromArgb(100, 255, 150) : Color.FromArgb(50, 200, 100);
    public static Color SuccessContainer => CurrentIsDark ? Color.FromArgb(30, 80, 40) : Color.FromArgb(230, 255, 240);
    
    public static Color Warning => CurrentIsDark ? Color.FromArgb(255, 200, 100) : Color.FromArgb(220, 150, 50);
    public static Color WarningContainer => CurrentIsDark ? Color.FromArgb(100, 70, 30) : Color.FromArgb(255, 245, 230);
    
    // Outlines and borders
    public static Color Outline => SurfaceAdjust(Surface, 0.22);
    public static Color OutlineVariant => SurfaceAdjust(Surface, 0.16);
    
    // Shadows and elevation
    public static Color Shadow => Color.FromArgb(0, 0, 0);
    public static Color Scrim => Color.FromArgb(0, 0, 0);
    
    // Text colors with proper contrast
    public static Color OnBackground => ForeColor;
    public static Color OnSurface => ForeColor;
    public static Color OnSurfaceVariant => Blend(ForeColor, Surface, 0.35);
    
    // Interactive states
    public static Color StateLayerHover => ForeColor.Alpha(CurrentIsDark ? 8 : 8);
    public static Color StateLayerFocus => ForeColor.Alpha(CurrentIsDark ? 12 : 12);
    public static Color StateLayerPressed => ForeColor.Alpha(CurrentIsDark ? 16 : 16);
    public static Color StateLayerDragged => ForeColor.Alpha(CurrentIsDark ? 16 : 16);
    
    // Legacy aliases for backward compatibility
    public static Color AccentColor => Primary;
    public static Color PrimaryColor => Primary;
    public static Color BorderColor => Outline;
    public static Color BackColor2 => SurfaceVariant;
    public static Color ShadowColor => FlatDesign ? Color.Transparent : Shadow.Alpha(CurrentIsDark ? 30 : 20);
    
    /// <summary>
    /// Elevation levels (0-5) returning appropriate shadow/tint
    /// </summary>
    public static Color GetElevationTint(int level)
    {
        if (!CurrentIsDark) return Color.Transparent;
        
        return level switch
        {
            0 => Color.Transparent,
            1 => Color.FromArgb(5, 255, 255, 255),
            2 => Color.FromArgb(8, 255, 255, 255),
            3 => Color.FromArgb(11, 255, 255, 255),
            4 => Color.FromArgb(14, 255, 255, 255),
            5 => Color.FromArgb(17, 255, 255, 255),
            _ => Color.FromArgb(20, 255, 255, 255)
        };
    }
    
    /// <summary>
    /// Returns shadow blur radius for elevation level
    /// </summary>
    public static float GetElevationBlur(int level)
    {
        if (FlatDesign)
            return 0f;

        return level switch
        {
            0 => 0f,
            1 => 2f,
            2 => 4f,
            3 => 8f,
            4 => 12f,
            5 => 16f,
            _ => 20f
        };
    }
    
    /// <summary>
    /// Returns shadow offset for elevation level
    /// </summary>
    public static float GetElevationOffset(int level)
    {
        if (FlatDesign)
            return 0f;

        return level switch
        {
            0 => 0f,
            1 => 1f,
            2 => 2f,
            3 => 4f,
            4 => 6f,
            5 => 8f,
            _ => 10f
        };
    }

    /// <summary>
    /// Gets or sets debug borders for layout inspection
    /// </summary>
    public static bool DrawDebugBorders;
}
