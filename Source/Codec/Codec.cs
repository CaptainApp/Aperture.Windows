using System;
using System.IO;

namespace Aperture {
  /// <inheritdoc />
  /// <summary>
  ///   Implements the logic for encoding frames
  /// </summary>
  /// <remarks>
  ///   Do not inherit from this class directly.
  ///   Use either <see cref="VideoCodec" /> or <see cref="StillImageCodec" />.
  /// </remarks>
  public abstract class Codec : IDisposable {
    /// <summary>
    ///   Size, in pixels, of the frames to be fed to this encoder
    /// </summary>
    public (int Width, int Height) FrameSize { get; }

    /// <summary>
    ///   Stream where the encoder output is to be received
    /// </summary>
    public Stream DestinationStream { get; }

    /// <summary>
    ///   Object holding the deserialized options for this codec
    /// </summary>
    public object Options { get; }

    /// <summary>
    ///   Class constructor
    /// </summary>
    /// <param name="width">Width, in pixels, of the frames to be fed to this encoder</param>
    /// <param name="height">Height, in pixels, of the frames to be fed to this encoder</param>
    /// <param name="destStream">Destination stream</param>
    /// <param name="options">Options for this codec</param>
    internal Codec(int width, int height, Stream destStream, object options = null) {
      FrameSize = (width, height);
      DestinationStream = destStream;
      Options = options;
    }

    /// <summary>
    ///   Instructs the encoder to begin accepting frames
    /// </summary>
    public abstract void Start();

    /// <summary>
    ///   Feeds a frame to the encoder
    /// </summary>
    /// <param name="frame">Video frame to be encoded</param>
    public abstract void Feed(VideoFrame frame);

    /// <inheritdoc />
    /// <summary>
    ///   Stops encoding frames and writes the final output to the destination stream, then releases all used resources.
    /// </summary>
    public abstract void Dispose();
  }
}