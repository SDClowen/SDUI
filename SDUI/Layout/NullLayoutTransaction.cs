using System;

namespace SDUI.Layout;

internal struct NullLayoutTransaction : IDisposable
{
    public readonly void Dispose()
    {
    }
}