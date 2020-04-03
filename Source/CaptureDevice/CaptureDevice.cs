using System;

namespace Aperture {
  /// <inheritdoc />
  /// <summary>
  ///   Provides media output from a capture device
  /// </summary>
  /// <typeparam name="T">The type of the frame to be captured</typeparam>
  /// <remarks>
  ///   Do not inherit from this class directly. Use <see cref="T:Aperture.Source.CaptureDevice.Video.VideoCaptureDevice" /> or <see cref="!:AudioCaptureDevice" />
  ///   instead.
  /// </remarks>
  public abstract class CaptureDevice<T> : IDisposable {
    /// <summary>
    ///   Class constructor
    /// </summary>
    internal CaptureDevice() { }

    /// <summary>
    ///   Acquires a single frame
    /// </summary>
    public virtual void AcquireFrame() { }

    /// <summary>
    ///   Locks the previous frame data and returns an object with its information
    /// </summary>
    /// <returns>A <see cref="T"/> instance</returns>
    public abstract T LockFrame();

    /// <summary>
    ///   Releases the data that may have been allocated for this frame
    /// </summary>
    /// <param name="frame">Frame information returned by the <see cref="LockFrame"/> method</param>
    public virtual void UnlockFrame(T frame) { }

    /// <summary>
    ///   Releases the resources from the last captured frame
    /// </summary>
    public virtual void ReleaseFrame() { }

    /// <inheritdoc />
    /// <summary>
    ///   Releases all resources used by the capture device
    /// </summary>
    public virtual void Dispose() {
      ReleaseFrame();
    }
  }
}