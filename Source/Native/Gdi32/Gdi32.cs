using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Aperture {
  /// <summary>
  ///   Exported functions from the gdi32.dll Windows library.
  /// </summary>
  internal static partial class Gdi32 {
    /// <summary>
    ///   The DeleteObject function deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system
    ///   resources associated with the object. After the object is deleted, the specified handle is no longer valid.
    /// </summary>
    /// <param name="hObject">A handle to a logical pen, brush, font, bitmap, region, or palette.</param>
    /// <returns>
    ///   If the function succeeds, the return value is nonzero.
    ///   If the specified handle is not valid or is currently selected into a DC, the return value is zero.
    /// </returns>
    [DllImport(nameof(Gdi32))]
    public static extern bool DeleteObject([In] IntPtr hObject);

    /// <summary>
    ///   The SelectObject function selects an object into the specified device context (DC). The new object replaces
    ///   the previous object of the same type.
    /// </summary>
    /// <param name="hDC">A handle to the DC.</param>
    /// <param name="hObject">
    ///   A handle to the object to be selected. The specified object must have been created by using
    ///   one of the following functions.
    /// </param>
    /// <returns>
    ///   If the selected object is not a region and the function succeeds, the return value is a handle to the  object
    ///   being replaced.
    /// </returns>
    [DllImport(nameof(Gdi32))]
    public static extern IntPtr SelectObject([In] IntPtr hDC, [In] IntPtr hObject);

    /// <summary>
    ///   The BitBlt function performs a bit-block transfer of the color data corresponding to a rectangle of pixels
    ///   from the specified source device context into a destination device context.
    /// </summary>
    /// <param name="hObject">A handle to the destination device context.</param>
    /// <param name="nXDest">
    ///   The x-coordinate, in logical units, of the upper-left corner of the destination rectangle.
    /// </param>
    /// <param name="nYDest">
    ///   The y-coordinate, in logical units, of the upper-left corner of the destination rectangle.
    /// </param>
    /// <param name="nWidth">The width, in logical units, of the source and destination rectangles.</param>
    /// <param name="nHeight">The height, in logical units, of the source and the destination rectangles.</param>
    /// <param name="hObjectSource">A handle to the source device context.</param>
    /// <param name="nXSrc">
    ///   The x-coordinate, in logical units, of the upper-left corner of the source rectangle.
    /// </param>
    /// <param name="nYSrc">
    ///   The y-coordinate, in logical units, of the upper-left corner of the source rectangle.
    /// </param>
    /// <param name="dwRop">
    ///   A raster-operation code. These codes define how the color data for the source rectangle is to be combined
    ///   with the color data for the destination rectangle to achieve the final color.
    /// </param>
    /// <returns></returns>
    [DllImport(nameof(Gdi32))]
    public static extern bool BitBlt(
      [In] IntPtr hObject,
      [In] int nXDest,
      [In] int nYDest,
      [In] int nWidth,
      [In] int nHeight,
      [In] IntPtr hObjectSource,
      [In] int nXSrc,
      [In] int nYSrc,
      [In] TernaryRasterOperations dwRop);

    /// <summary>
    ///   The CreateCompatibleBitmap function creates a bitmap compatible with the device that is associated with the
    ///   specified device context.
    /// </summary>
    /// <param name="hDC">A handle to the device context.</param>
    /// <param name="nWidth">The bitmap width, in pixels.</param>
    /// <param name="nHeight">The bitmap height, in pixels.</param>
    /// <returns>
    ///   If the function succeeds, the return value is a handle to the compatible bitmap (DDB).
    ///   If the function fails, the return value is NULL.
    /// </returns>
    [DllImport(nameof(Gdi32))]
    public static extern IntPtr CreateCompatibleBitmap([In] IntPtr hDC, [In] int nWidth, [In] int nHeight);

    /// <summary>
    ///   The CreateCompatibleDC function creates a memory device context (DC) compatible with the specified device.
    /// </summary>
    /// <param name="hDC">
    ///   A handle to an existing DC. If this handle is NULL, the function creates a memory DC compatible with the
    ///   application's current screen.
    /// </param>
    /// <returns>
    ///   If the function succeeds, the return value is the handle to a memory DC.
    ///   If the function fails, the return value is NULL.
    /// </returns>
    [DllImport(nameof(Gdi32))]
    public static extern IntPtr CreateCompatibleDC([In] IntPtr hDC);
  }
}