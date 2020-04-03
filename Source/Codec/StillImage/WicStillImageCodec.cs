using System;
using System.IO;
using SharpDX;
using SharpDX.Diagnostics;
using SharpDX.Direct3D11;
using SharpDX.WIC;

namespace Aperture {
  /// <inheritdoc />
  /// <summary>
  ///   Implements generic logic for writing WIC-enabled image codecs
  /// </summary>
  public abstract class WicStillImageCodec : StillImageCodec {
    /// <summary>
    ///   WIC BitmapEncoder
    /// </summary>
    private readonly BitmapEncoder encoder;

    /// <summary>
    ///   WIC ImagingFactory
    /// </summary>
    private readonly ImagingFactory factory;

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    /// <param name="width">Capture width, in pixels</param>
    /// <param name="height">Capture height, in pixels</param>
    /// <param name="destStream">Destination stream</param>
    /// <param name="containerFormat">Container format GUID</param>
    protected WicStillImageCodec(int width, int height, Stream destStream, Guid containerFormat)
      : base(width, height, destStream) {
      this.factory = new ImagingFactory();
      this.encoder = new BitmapEncoder(this.factory, containerFormat);
      this.encoder.Initialize(destStream);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Encodes a single frame and writes the output to the destination stream.
    /// </summary>
    /// <param name="frame">Video frame to be encoded</param>
    /// <exception cref="T:System.InvalidOperationException">A frame has already been fed to the encoder</exception>
    /// <exception cref="T:System.NotSupportedException">The supplied video frame does not have a supported format</exception>
    public override void Feed(VideoFrame frame) {
      using (var frameEncode = new BitmapFrameEncode(this.encoder)) {
        frameEncode.Initialize();

        switch (frame) {
          case BitmapVideoFrame bmpVideoFrame:    // GDI-compatible bitmap (SharpDX.WIC)
            // already got a WIC bitmap
            frameEncode.WriteSource(bmpVideoFrame.Bitmap);
            break;

          case GdiBitmapVideoFrame gdiVideoFrame:  // GDI-compatible bitmap (System.Drawing)
            // construct a WIC bitmap from a GDI-compatible bitmap
            using (var bmp = new Bitmap(this.factory,
                                        frame.Width,
                                        frame.Height,
                                        PixelFormat.Format32bppBGRA,
                                        new DataRectangle(gdiVideoFrame.BitmapData.Scan0,
                                                          gdiVideoFrame.BitmapData.Stride))) {
              frameEncode.WriteSource(bmp);
            }

            break;

          case D3D11VideoFrame d3D11VideoFrame:    // ID3D11Texture2D
            try {
              // map texture to system memory so that we can access the pixel data
              DataBox box = d3D11VideoFrame.Texture.Device.ImmediateContext.MapSubresource(d3D11VideoFrame.Texture,
                                                                                           0,
                                                                                           MapMode.Read,
                                                                                           MapFlags.None);

              using (var bmp = new Bitmap(this.factory,
                                          frame.Width,
                                          frame.Height,
                                          PixelFormat.Format32bppBGRA,
                                          new DataRectangle(box.DataPointer, box.RowPitch))) {
                frameEncode.WriteSource(bmp);
              }
            } finally {
              // unmap texture from system memory and release related resources
              using (DeviceContext ctx = d3D11VideoFrame.Texture.Device.ImmediateContext) {
                ctx.UnmapSubresource(d3D11VideoFrame.Texture, 0);
                ctx.ClearState();
                ctx.Flush();
              }

#if DEBUG
              // HACK: we are accessing the `Device` property of the Texture2D in order to map the data to system memory.
              //       Internally, SharpDX gets the Device if not previously accessed. We are releasing the device on the
              //       Dispose() methods of the video providers but this weak reference stays tracked by SharpDX even
              //       after its memory has been released
              ObjectTracker.UnTrack(d3D11VideoFrame.Texture.Device);
#endif
            }

            break;
        }

        frameEncode.Commit();
        this.encoder.Commit();
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
    /// </summary>
    public override void Dispose() {
      this.encoder.Dispose();
      this.factory.Dispose();
    }
  }
}