using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Aperture {
  /// <inheritdoc />
  /// <summary>
  ///   Implements video capture device that uses native GDI calls in order to capture screen bitmaps
  /// </summary>
  [DisplayName("GDI")]
  public sealed class GdiVideoCaptureDevice : VideoCaptureDevice {
    /// <summary>
    ///   Attached window handle
    /// </summary>
    private readonly IntPtr windowHandle;

    /// <summary>
    ///   Destination bitmap handle
    /// </summary>
    private IntPtr bitmapHandle;

    /// <summary>
    ///   Destination drawing context
    /// </summary>
    private IntPtr destCtx;

    /// <summary>
    ///   Window drawing context
    /// </summary>
    private IntPtr drawCtx;

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    /// <param name="x">Horizontal coordinate, in pixels, for the virtual capture location</param>
    /// <param name="y">Vertical coordinate, in pixels, for the virtual cpature location</param>
    /// <param name="width">Width, in pixels, for the captured region</param>
    /// <param name="height">Height, in pixels, for the captured region</param>
    public GdiVideoCaptureDevice(int x, int y, int width, int height) : base(x, y, width, height) {
      this.windowHandle = User32.GetDesktopWindow();
      this.drawCtx = User32.GetWindowDC(this.windowHandle);
      this.destCtx = Gdi32.CreateCompatibleDC(this.drawCtx);
      this.bitmapHandle = Gdi32.CreateCompatibleBitmap(this.drawCtx, width, height);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Acquires a single frame from this provider
    /// </summary>
    public override void AcquireFrame() {
      Gdi32.SelectObject(this.destCtx, this.bitmapHandle);
      Gdi32.BitBlt(this.destCtx,
                   0,
                   0,
                   Size.Width,
                   Size.Height,
                   this.drawCtx,
                   VirtualLocation.X,
                   VirtualLocation.Y,
                   Gdi32.TernaryRasterOperations.SRCCOPY);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases last captured frame resources
    /// </summary>
    public override void ReleaseFrame() {
      if (this.drawCtx != IntPtr.Zero) {
        User32.ReleaseDC(this.windowHandle, this.drawCtx);
      }

      if (this.destCtx != IntPtr.Zero) {
        User32.ReleaseDC(this.windowHandle, this.destCtx);
      }

      if (this.bitmapHandle != IntPtr.Zero) {
        Gdi32.DeleteObject(this.bitmapHandle);
      }

      this.drawCtx = this.destCtx = this.bitmapHandle = IntPtr.Zero;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Creates a single bitmap from the captured frames and returns an object with its information
    /// </summary>
    /// <returns>A <see cref="T:Captain.Common.FrameData" /> containing raw bitmap information</returns>
    public override VideoFrame LockFrame() {
      Bitmap bmp = Image.FromHbitmap(this.bitmapHandle);
      BitmapData data = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size),
                                     ImageLockMode.ReadWrite,
                                     PixelFormat.Format32bppArgb);
      return new GdiBitmapVideoFrame(bmp, data);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases the bitmap created for this frame
    /// </summary>
    /// <param name="frame">Bitmap data returned by the <see cref="M:Captain.Common.VideoProvider.LockFrame" /> method</param>
    public override void UnlockFrame(VideoFrame frame) {
      if (frame is GdiBitmapVideoFrame gdiVideoFrame) {
        gdiVideoFrame.Bitmap.UnlockBits(gdiVideoFrame.BitmapData);
        gdiVideoFrame.Bitmap.Dispose();
      }
    }
  }
}