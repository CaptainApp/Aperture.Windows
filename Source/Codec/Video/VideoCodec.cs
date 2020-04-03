using System.IO;

namespace Aperture {
  /// <inheritdoc />
  /// <summary>
  ///   Implements the logic behind a video encoder.
  /// </summary>
  public abstract class VideoCodec : Codec {
    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    /// <param name="width">Width, in pixels, of the frames to be fed to this encoder</param>
    /// <param name="height">Height, in pixels, of the frames to be fed to this encoder</param>
    /// <param name="destStream">Destination stream</param>
    protected VideoCodec(int width, int height, Stream destStream) : base(width, height, destStream) { }
  }
}