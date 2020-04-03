using System;
using System.Runtime.InteropServices;

namespace Aperture {
  internal static class User32 {
    /// <summary>
    ///   Retrieves a handle to the desktop window. The desktop window covers the entire screen. The desktop window is
    ///   the area on top of which other windows are painted.
    /// </summary>
    /// <returns>The return value is a handle to the desktop window.</returns>
    [DllImport(nameof(User32))]
    internal static extern IntPtr GetDesktopWindow();

    /// <summary>
    ///   The GetWindowDC function retrieves the device context (DC) for the entire window, including title bar,
    ///   menus, and scroll bars. A window device context permits painting anywhere in a window, because the origin
    ///   of the device context is the upper-left corner of the window instead of the client area. GetWindowDC
    ///   assigns default attributes to the window device context each time it retrieves the device context. Previous
    ///   attributes are lost.
    /// </summary>
    /// <param name="hWnd">
    ///   A handle to the window with a device context that is to be retrieved. If this value is
    ///   <see cref="IntPtr.Zero"/>, GetWindowDC retrieves the device context for the entire screen.
    ///   If this parameter is <see cref="IntPtr.Zero"/>, GetWindowDC retrieves the device context for the primary
    ///   display monitor.
    /// </param>
    /// <returns>
    ///   If the function succeeds, the return value is a handle to a device context for the specified window.
    ///   If the function fails, the return value is <see cref="IntPtr.Zero"/>, indicating an error or an invalid hWnd
    ///   parameter.
    /// </returns>
    [DllImport(nameof(User32), SetLastError = true)]
    internal static extern IntPtr GetWindowDC(IntPtr hWnd);

    /// <summary>
    ///   The ReleaseDC function releases a device context (DC), freeing it for use by other applications. The effect
    ///   of the ReleaseDC function depends on the type of DC. It frees only common and window DCs. It has no effect
    ///   on class or private DCs.
    /// </summary>
    /// <param name="hWnd">A handle to the window whose DC is to be released.</param>
    /// <param name="hdc">A handle to the DC to be released.</param>
    /// <returns>
    ///   The return value indicates whether the DC was released. If the DC was released, the return value is 1.
    ///   If the DC was not released, the return value is zero.
    /// </returns>
    [DllImport(nameof(User32), SetLastError = false)]
    internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);
  }
}