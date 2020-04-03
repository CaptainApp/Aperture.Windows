namespace Aperture {
  /// <summary>
  ///   Represents a generic video frame
  /// </summary>
  public abstract class VideoFrame {
    /// <summary>
    ///   Number of pixels in a scanline
    /// </summary>
    public int Width { get; protected set; }

    /// <summary>
    ///   Number of scanlines
    /// </summary>
    public int Height { get; protected set; }

    /// <summary>
    ///   Frame presentation time, in 100-nanosecond time units
    /// </summary>
    public long PresentTime { get; protected set; }

    /// <summary>
    ///   User data
    /// </summary>
    public object Tag { get; set; }
  }
}