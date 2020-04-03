using System;

namespace Aperture {
  /// <inheritdoc />
  /// <summary>
  ///   Provides graphical output from display devices
  /// </summary>
  public abstract class VideoCaptureDevice : CaptureDevice<VideoFrame> {
    /// <summary>
    ///   Location of the capture region in the virtual desktop.
    ///   The capture device is responsible for handling monitor information and resolvin the actual location on the
    ///   selected device.
    /// </summary>
    public (int X, int Y) VirtualLocation { get; }

    /// <summary>
    ///   The size of the region to be captured.
    /// </summary>
    public (int Width, int Height) Size { get; }

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    /// <param name="x">Horizontal coordinate, in pixels, for the virtual capture location</param>
    /// <param name="y">Vertical coordinate, in pixels, for the virtual cpature location</param>
    /// <param name="width">Width, in pixels, for the captured region</param>
    /// <param name="height">Height, in pixels, for the captured region</param>
    protected VideoCaptureDevice(int x, int y, int width, int height) {
      // make sure the specified region is valid
      if (width <= 0 || height <= 0) {
        throw new ArgumentOutOfRangeException(nameof(width));
      }

      VirtualLocation = (x, y);
      Size = (width, height);
    }
  }
}