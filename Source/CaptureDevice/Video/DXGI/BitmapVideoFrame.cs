using System;
using SharpDX.WIC;

namespace Aperture {
  /// <inheritdoc />
  /// <summary>
  ///   Wraps a video frame on a GDI-compatible bitmap
  /// </summary>
  public class BitmapVideoFrame : VideoFrame {
    /// <summary>
    ///   Pixel format GUID
    /// </summary>
    public Guid PixelFormat { get; }

    /// <summary>
    ///   WIC bitmap object
    /// </summary>
    public BitmapSource Bitmap { get; }

    /// <summary>
    ///   WIC bitmap lock object
    /// </summary>
    public BitmapLock BitmapLock { get; }

    /// <summary>
    ///   Creates a new video frame from a locked WIC bitmap
    /// </summary>
    /// <param name="bitmap">WIC bitmap</param>
    /// <param name="bitmapLock">Bitmap lock object</param>
    /// <param name="presentTime">Time, in 100-nanosecond units, of the frame capture</param>

    internal BitmapVideoFrame(BitmapSource bitmap, BitmapLock bitmapLock, long presentTime) {
      Width = bitmap.Size.Width;
      Height = bitmap.Size.Height;

      PixelFormat = bitmap.PixelFormat;
      Bitmap = bitmap;
      BitmapLock = bitmapLock;

      PresentTime = presentTime;
    }
  }
}