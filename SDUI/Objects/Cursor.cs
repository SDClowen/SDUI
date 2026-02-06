using SkiaSharp;
using System;

namespace SDUI;

/// <summary>
/// Lightweight wrapper around a platform cursor handle. We avoid WinForms types so SDUI can be used
/// outside of System.Windows.Forms contexts. System cursors are not disposed by SDUI (we don't own them).
/// </summary>
public sealed class Cursor : IDisposable
{
    public IntPtr Handle { get; }
    public string Name { get; }
    public bool IsSystem { get; }
    public SKPoint Position { get; set; }

    internal Cursor(IntPtr handle, string name, bool isSystem = true)
    {
        Handle = handle;
        Name = name ?? "Cursor";
        IsSystem = isSystem;
    }

    /// <summary>
    /// Create a cursor for a custom handle (SDUI does not own the handle by default).
    /// </summary>
    public static Cursor FromHandle(IntPtr handle, string name = "Handle") => new Cursor(handle, name, isSystem: false);

    internal static Cursor CreateSystem(IntPtr handle, string name) => new Cursor(handle, name, isSystem: true);

    public void Dispose()
    {
        // We do not dispose system cursors. If in future we add ownership semantics for custom cursors, do it here.
    }

    public override string ToString() => Name;
}
