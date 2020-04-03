using System;
using System.IO;

namespace Aperture {
  /// <inheritdoc />
  /// <summary>
  ///   Implements the logic behind a still image encoder.
  /// </summary>
  public abstract class StillImageCodec : Codec {
    /// <summary>
    ///   Indicates whether a frame has been fed to the encoder.
    /// </summary>
    private bool frameFed;

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    /// <param name="width">Capture width, in pixels</param>
    /// <param name="height">Capture height, in pixels</param>
    /// <param name="destStream">Destination stream</param>
    protected StillImageCodec(int width, int height, Stream destStream) : base(width, height, destStream) { }

    /// <inheritdoc />
    /// <summary>
    ///   Instructs the encoder to begin accepting frames
    /// </summary>
    /// <remarks>
    ///   This method is not relevant to the <see cref="StillImageCodec"/> class
    /// </remarks>
    public sealed override void Start() { }

    /// <inheritdoc />
    /// <summary>
    ///   Encodes a single frame and writes the output to the destination stream.
    /// </summary>
    /// <param name="frame">Video frame to be encoded</param>
    /// <remarks>
    ///   Only call the base method upon successful encoding of the frame. The class will be locked after a frame has
    ///   been encoded succesfully.
    /// </remarks>
    /// <exception cref="T:System.InvalidOperationException">A frame has already been fed to the encoder</exception>
    /// <exception cref="T:System.NotSupportedException">The supplied video frame does not have a supported format</exception>
    public override void Feed(VideoFrame frame) {
      if (this.frameFed) {
        throw new InvalidOperationException("The encoder does not accept multiple frames");
      }

      this.frameFed = true;
    }
  }
}