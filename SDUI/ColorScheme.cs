using SkiaSharp;
using System;
using System.Drawing;

namespace SDUI;

/// <summary>
/// Modern 2025 design system with Material Design 3 and Fluent Design principles
/// Supports both light and dark themes with semantic color tokens
/// </summary>
public class ColorScheme
{
    private static bool _isDarkMode = false;
    
    /// <summary>
    /// Toggle between light and dark theme
    /// </summary>
    public static bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (_isDarkMode != value)
            {
                _isDarkMode = value;
                ThemeChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }

    public static event EventHandler? ThemeChanged;

    // Primary surface colors
    public static Color BackColor => IsDarkMode ? Color.FromArgb(18, 18, 18) : Color.FromArgb(255, 255, 255);
    public static Color ForeColor => IsDarkMode ? Color.FromArgb(230, 230, 230) : Color.FromArgb(28, 28, 28);
    
    // Surface variants (for cards, elevated components)
    public static Color Surface => IsDarkMode ? Color.FromArgb(28, 28, 30) : Color.FromArgb(250, 250, 250);
    public static Color SurfaceVariant => IsDarkMode ? Color.FromArgb(38, 38, 42) : Color.FromArgb(242, 242, 247);
    public static Color SurfaceContainerLowest => IsDarkMode ? Color.FromArgb(15, 15, 15) : Color.FromArgb(255, 255, 255);
    public static Color SurfaceContainerLow => IsDarkMode ? Color.FromArgb(22, 22, 24) : Color.FromArgb(247, 247, 247);
    public static Color SurfaceContainer => IsDarkMode ? Color.FromArgb(28, 28, 30) : Color.FromArgb(240, 240, 240);
    public static Color SurfaceContainerHigh => IsDarkMode ? Color.FromArgb(35, 35, 37) : Color.FromArgb(235, 235, 235);
    
    // Accent colors - modern vibrant palette
    public static Color Primary => IsDarkMode ? Color.FromArgb(120, 120, 255) : Color.FromArgb(70, 70, 230);
    public static Color PrimaryContainer => IsDarkMode ? Color.FromArgb(45, 45, 110) : Color.FromArgb(225, 225, 255);
    public static Color OnPrimary => Color.FromArgb(255, 255, 255);
    public static Color OnPrimaryContainer => IsDarkMode ? Color.FromArgb(220, 220, 255) : Color.FromArgb(20, 20, 100);
    
    public static Color Secondary => IsDarkMode ? Color.FromArgb(180, 140, 255) : Color.FromArgb(100, 60, 200);
    public static Color SecondaryContainer => IsDarkMode ? Color.FromArgb(60, 40, 100) : Color.FromArgb(235, 225, 255);
    
    public static Color Tertiary => IsDarkMode ? Color.FromArgb(255, 180, 120) : Color.FromArgb(200, 100, 50);
    public static Color TertiaryContainer => IsDarkMode ? Color.FromArgb(100, 60, 40) : Color.FromArgb(255, 235, 225);
    
    // Semantic colors
    public static Color Error => IsDarkMode ? Color.FromArgb(255, 100, 100) : Color.FromArgb(220, 50, 50);
    public static Color ErrorContainer => IsDarkMode ? Color.FromArgb(100, 40, 40) : Color.FromArgb(255, 230, 230);
    public static Color OnError => Color.FromArgb(255, 255, 255);
    
    public static Color Success => IsDarkMode ? Color.FromArgb(100, 255, 150) : Color.FromArgb(50, 200, 100);
    public static Color SuccessContainer => IsDarkMode ? Color.FromArgb(30, 80, 40) : Color.FromArgb(230, 255, 240);
    
    public static Color Warning => IsDarkMode ? Color.FromArgb(255, 200, 100) : Color.FromArgb(220, 150, 50);
    public static Color WarningContainer => IsDarkMode ? Color.FromArgb(100, 70, 30) : Color.FromArgb(255, 245, 230);
    
    // Outlines and borders
    public static Color Outline => IsDarkMode ? Color.FromArgb(70, 70, 75) : Color.FromArgb(200, 200, 205);
    public static Color OutlineVariant => IsDarkMode ? Color.FromArgb(50, 50, 55) : Color.FromArgb(220, 220, 225);
    
    // Shadows and elevation
    public static Color Shadow => Color.FromArgb(0, 0, 0);
    public static Color Scrim => Color.FromArgb(0, 0, 0);
    
    // Text colors with proper contrast
    public static Color OnBackground => ForeColor;
    public static Color OnSurface => ForeColor;
    public static Color OnSurfaceVariant => IsDarkMode ? Color.FromArgb(190, 190, 200) : Color.FromArgb(70, 70, 80);
    
    // Interactive states
    public static Color StateLayerHover => ForeColor.Alpha(IsDarkMode ? 8 : 8);
    public static Color StateLayerFocus => ForeColor.Alpha(IsDarkMode ? 12 : 12);
    public static Color StateLayerPressed => ForeColor.Alpha(IsDarkMode ? 16 : 16);
    public static Color StateLayerDragged => ForeColor.Alpha(IsDarkMode ? 16 : 16);
    
    // Legacy aliases for backward compatibility
    public static Color AccentColor => Primary;
    public static Color PrimaryColor => Primary;
    public static Color BorderColor => Outline;
    public static Color BackColor2 => SurfaceVariant;
    public static Color ShadowColor => Shadow.Alpha(IsDarkMode ? 30 : 20);
    
    /// <summary>
    /// Elevation levels (0-5) returning appropriate shadow/tint
    /// </summary>
    public static Color GetElevationTint(int level)
    {
        if (!IsDarkMode) return Color.Transparent;
        
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
