using System;

namespace Aperture {
  /// <summary>
  ///   Creates capture devices
  /// </summary>
  public static class CaptureDeviceFactory {
    /// <summary>
    ///   Picks the most suitable capture device for the current platform and environment and creates an instance of it.
    /// </summary>
    /// <param name="x">X coordinate for the region being captured</param>
    /// <param name="y">Y coordinate for the region being captured</param>
    /// <param name="width">Width of the region being captured</param>
    /// <param name="height">Height of the region being captured</param>
    /// <returns>A <see cref="VideoCaptureDevice" /> instance</returns>
    public static VideoCaptureDevice CreateVideoCaptureDevice(int x, int y, int width, int height) {
      if (Environment.OSVersion.Version >= new Version(6, 2)) {
        try {
          // desktop duplication for the win!
          return new DxgiVideoCaptureDevice(x, y, width, height);
        } catch (NotSupportedException) { }
      }

      // fall back to GDI
      return new GdiVideoCaptureDevice(x, y, width, height);
    }
  }
}