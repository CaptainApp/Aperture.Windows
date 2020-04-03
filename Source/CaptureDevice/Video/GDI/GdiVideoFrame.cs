using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Aperture {
  /// <inheritdoc />
  /// <summary>
  ///   Wraps a video frame on a GDI-compatible bitmap
  /// </summary>
  public class GdiBitmapVideoFrame : VideoFrame {
    /// <summary>
    ///   GDI bitmap instance
    /// </summary>
    public Bitmap Bitmap { get; }

    /// <summary>
    ///   GDI bitmap lock object
    /// </summary>
    public BitmapData BitmapData { get; }

    /// <summary>
    ///   Creates a new video frame from a locked GDI bitmap
    /// </summary>
    /// <param name="bitmap">GDI bitmap</param>
    /// <param name="bitmapLock">Bitmap lock object</param>

    internal GdiBitmapVideoFrame(Bitmap bitmap, BitmapData bitmapLock) {
      Width = bitmap.Size.Width;
      Height = bitmap.Size.Height;

      Bitmap = bitmap;
      BitmapData = bitmapLock;

      // TODO: maybe use system high resolution timer?
      PresentTime = DateTime.Now.Ticks;
    }
  }
}