using System;

// ReSharper disable InconsistentNaming

namespace Aperture {
  /// <summary>
  ///   Exported functions from the gdi32.dll Windows library.
  /// </summary>
  internal partial class Gdi32 {
    /// <summary>
    ///   Ternary raster operations used by GDI bit block transfer functions
    /// </summary>
    [Flags]
    public enum TernaryRasterOperations {
      /// <summary>
      ///   dest = source
      /// </summary>
      SRCCOPY = 0x00CC0020
    }
  }
}