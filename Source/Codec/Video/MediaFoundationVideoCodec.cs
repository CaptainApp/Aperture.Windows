using System;
using System.IO;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;
using SharpDX.WIC;
using Device = SharpDX.DXGI.Device;

// ReSharper disable InconsistentNaming

namespace Aperture {
  /// <inheritdoc cref="VideoCodec" />
  /// <summary>
  ///   Implements common logic for Media Foundation video codecs
  /// </summary>
  public abstract class MediaFoundationVideoCodec : VideoCodec, IDxgiEnabledVideoCodec {
    /// <summary>
    ///   Do not initialize the sockets library
    /// </summary>
    private const int MFSTARTUP_NOSOCKET = 1;

    /// <summary>
    ///   No samples were processed by the sink writer
    /// </summary>
    private const int MF_E_SINK_NO_SAMPLES_PROCESSED = unchecked((int) 0xC00D4A44);

    /// <summary>
    ///   Byte stream for writing video encoder output
    /// </summary>
    private ByteStream byteStream;

    /// <summary>
    ///   DXGI device manager instance to enable accelerated texture encoding
    /// </summary>
    private DXGIDeviceManager dxgiManager;

    /// <summary>
    ///   MediaFoundation sink writer
    /// </summary>
    private SinkWriter sinkWriter;

    /// <summary>
    ///   MediaFoundation stream index
    /// </summary>
    private int streamIdx;

    /// <summary>
    ///   Timestamp of the present time for the first frame
    /// </summary>
    private long firstFramePresentTime;

    /// <summary>
    ///   Last present time
    /// </summary>
    private long lastFrameTime;

    /// <summary>
    ///   Value for the TranscodeContainerType media attribute
    /// </summary>
    private readonly Guid containerType;

    /// <summary>
    ///   Video format GUID
    /// </summary>
    private readonly Guid videoFormat;

    /// <summary>
    ///   Frame rate, in frames per second
    /// </summary>
    private readonly int frameRate;

    /// <summary>
    ///   Bit rate for the encoder, which downgrades or upgrades video output quality
    /// </summary>
    private readonly int bitRate;

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    /// <param name="width">Width, in pixels, of the frames to be fed to this encoder</param>
    /// <param name="height">Height, in pixels, of the frames to be fed to this encoder</param>
    /// <param name="destStream">Destination stream</param>
    /// <param name="containerType">Container type GUID</param>
    /// <param name="videoFormat">Video format GUID</param>
    /// <param name="frameRate">Frame rate</param>
    /// <param name="bitRate">Bit rate for the encoder</param>
    protected MediaFoundationVideoCodec(int width, int height, Stream destStream, Guid containerType, Guid videoFormat,
                                        int frameRate, int bitRate) : base(width, height, destStream) {
      this.containerType = containerType;
      this.videoFormat = videoFormat;
      this.frameRate = frameRate;
      this.bitRate = bitRate;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Binds a DXGI device to the video codec
    /// </summary>
    /// <param name="device">DXGI device object</param>
    public void BindDevice(Device device) {
      this.dxgiManager = new DXGIDeviceManager();
      this.dxgiManager.ResetDevice(device);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Starts the encoder after all properties have been initialied
    /// </summary>
    public override void Start() {
      MediaFactory.Startup(MediaFactory.Version, MFSTARTUP_NOSOCKET);

      using (var attrs = new MediaAttributes()) {
        attrs.Set(TranscodeAttributeKeys.TranscodeContainertype, this.containerType);
        attrs.Set(SinkWriterAttributeKeys.ReadwriteEnableHardwareTransforms, 1);
        attrs.Set(SinkWriterAttributeKeys.LowLatency, true);

        if (this.dxgiManager != null) {
          attrs.Set(SinkWriterAttributeKeys.D3DManager, this.dxgiManager);
        }

        // create byte stream and sink writer
        this.byteStream = new ByteStream(DestinationStream);
        this.sinkWriter = MediaFactory.CreateSinkWriterFromURL(null, this.byteStream, attrs);

        // create output media type
        using (var outMediaType = new SharpDX.MediaFoundation.MediaType()) {
          outMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
          outMediaType.Set(MediaTypeAttributeKeys.Subtype, this.videoFormat);
          outMediaType.Set(MediaTypeAttributeKeys.AvgBitrate, this.bitRate);
          outMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int) VideoInterlaceMode.Progressive);
          outMediaType.Set(MediaTypeAttributeKeys.FrameSize,
                           ((long) FrameSize.Width << 32) | (uint) FrameSize.Height);
          outMediaType.Set(MediaTypeAttributeKeys.FrameRate, ((long) this.frameRate << 32) | 1);
          outMediaType.Set(MediaTypeAttributeKeys.PixelAspectRatio, (1 << 32) | 1);

          this.sinkWriter.AddStream(outMediaType, out this.streamIdx);
        }

        // create input media type
        using (var inMediaType = new SharpDX.MediaFoundation.MediaType()) {
          inMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);

          inMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int) VideoInterlaceMode.Progressive);
          inMediaType.Set(MediaTypeAttributeKeys.FrameSize,
                          ((long) FrameSize.Width << 32) | (uint) FrameSize.Height);
          inMediaType.Set(MediaTypeAttributeKeys.FrameRate, ((long) this.frameRate << 32) | 1);
          inMediaType.Set(MediaTypeAttributeKeys.PixelAspectRatio, (1 << 32) | 1);

          try {
            // use NV12 YUV encoding
            inMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.NV12);
            this.sinkWriter.SetInputMediaType(this.streamIdx, inMediaType, null);
          } catch (SharpDXException exception)
            when (exception.ResultCode == SharpDX.MediaFoundation.ResultCode.InvalidMediaType) {
            // XXX: fall back to ARGB32
            inMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.Argb32);
            this.sinkWriter.SetInputMediaType(this.streamIdx, inMediaType, null);
          }
        }

        this.sinkWriter.BeginWriting();
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Processes and encodes a frame into the destination stream
    /// </summary>
    /// <param name="frame">
    ///   A <see cref="VideoFrame" /> instance containing information about the locked frame bitmap or texture
    /// </param>
    public override void Feed(VideoFrame frame) {
      // create sample
      Sample sample = MediaFactory.CreateSample();
      MediaBuffer buffer;

      switch (frame) {
        case D3D11VideoFrame d3D11Frame: // Direct3D 11 texture
          // create media buffer
          MediaFactory.CreateDXGISurfaceBuffer(typeof(Texture2D).GUID,
                                               d3D11Frame.Texture,
                                               0,
                                               new RawBool(false),
                                               out buffer);

          // set buffer length
          using (Buffer2D buffer2D = buffer.QueryInterface<Buffer2D>()) {
            buffer.CurrentLength = buffer2D.ContiguousLength;
          }

          break;

        case BitmapVideoFrame bmpFrame: // WIC bitmap
          // create media buffer
          MediaFactory.CreateWICBitmapBuffer(typeof(Bitmap).GUID, bmpFrame.Bitmap, out buffer);
          
          // calculate buffer length
          buffer.CurrentLength = bmpFrame.BitmapLock.Stride * bmpFrame.Height;
          
          // copy pixels
          Utilities.CopyMemory(buffer.Lock(out _, out _), bmpFrame.BitmapLock.Data.DataPointer, buffer.CurrentLength);

          // unlock bits
          buffer.Unlock();
          
          break;

        case GdiBitmapVideoFrame gdiFrame: // GDI-compatible bitmap
          // create media buffer
          // create buffer for copying the bitmap data
          MediaFactory.Create2DMediaBuffer(frame.Width,
                                           frame.Height,
                                           new FourCC((int) Format.X8R8G8B8),
                                           new RawBool(false), out buffer);

          // calculate buffer length
          buffer.CurrentLength = gdiFrame.BitmapData.Stride * frame.Height;

          // copy data
          Utilities.CopyMemory(buffer.Lock(out _, out _), gdiFrame.BitmapData.Scan0, buffer.CurrentLength);

          // unlock bits
          buffer.Unlock();
          break;

        default:
          throw new NotSupportedException("The supplied frame does not have a supported type");
      }

      // add buffer to sample
      sample.AddBuffer(buffer);

      // set up sample timing
      if (this.lastFrameTime != 0) {
        sample.SampleTime = frame.PresentTime - this.firstFramePresentTime;
        sample.SampleDuration = frame.PresentTime - this.lastFrameTime;
      } else {
        // set first frame present time so that we can set the timestamp of subsequent frames relative to the
        // beggining of the video
        this.firstFramePresentTime = frame.PresentTime;
      }

      this.lastFrameTime = frame.PresentTime;

      try {
        this.sinkWriter.WriteSample(this.streamIdx, sample);
      } finally {
        buffer.Dispose();
        sample.Dispose();
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
    /// </summary>
    public override void Dispose() {
      try {
        this.sinkWriter.Finalize();
      } catch (SharpDXException exception) when (exception.ResultCode.Code == MF_E_SINK_NO_SAMPLES_PROCESSED) {
        this.byteStream.Length = 0;
      }

      this.byteStream?.Dispose();
      this.sinkWriter?.Dispose();
      this.dxgiManager?.Dispose();

      MediaFactory.Shutdown();

      this.sinkWriter = null;
      this.byteStream = null;
      this.dxgiManager = null;
    }
  }
}