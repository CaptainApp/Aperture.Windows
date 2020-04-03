using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WIC;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Resource = SharpDX.DXGI.Resource;
using ResultCode = SharpDX.DXGI.ResultCode;

namespace Aperture {
  /// <inheritdoc />
  /// <summary>
  ///   Provides DXGI Desktop Duplication support for capturing the screen
  /// </summary>
  [DisplayName("Desktop Duplication")]
  public sealed class DxgiVideoCaptureDevice : VideoCaptureDevice {
    /// <summary>
    ///   Timeout, in milliseconds, to consider a desktop duplication frame lost
    /// </summary>
    private const int DuplicationFrameTimeout = -1;

    /// <summary>
    ///   Frequency of the system performance counter
    /// </summary>
    private readonly long perfFreq;

    /// <summary>
    ///   Array of capture sources
    /// </summary>
    private readonly DxgiCaptureSource[] sources;

    /// <summary>
    ///   Last time, in system performance counter units, a frame was received from a duplicated output
    /// </summary>
    private long lastPresentTime;

    /// <summary>
    ///   Used-specified rectangle to be captured.
    /// </summary>
    /// <remarks>
    ///   Note that the coordinates for this rectangle represent a virtual desktop point and may be outside of the
    ///   primary monitor.
    /// </remarks>
    private readonly Rectangle virtualRect;

    /// <summary>
    ///   Enumerates the devices being used by the current capture sources
    /// </summary>
    public IEnumerable<SharpDX.DXGI.Device> Devices => this.sources.Select(s => s.DxgiDevice);

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    /// <param name="x">Horizontal coordinate, in pixels, for the virtual capture location</param>
    /// <param name="y">Vertical coordinate, in pixels, for the virtual cpature location</param>
    /// <param name="width">Width, in pixels, for the captured region</param>
    /// <param name="height">Height, in pixels, for the captured region</param>
    public DxgiVideoCaptureDevice(int x, int y, int width, int height) : base(x, y, width, height) {
      // we'll need the system timer frequency in order to convert the output duplication present time to ticks
      Kernel32.QueryPerformanceFrequency(out this.perfFreq);
      this.virtualRect = new Rectangle(x, y, width, height);

      // obtain a list of capture sources
      // obtain a list of capture sources
      using (var factory = new Factory1()) {
        if (factory.GetAdapterCount1() == 0) {
          throw new NotSupportedException("No suitable video adapters found");
        }

        var captureSources = new List<DxgiCaptureSource>();

        foreach (Adapter1 adapter in factory.Adapters1) {
          Debug.WriteLine($"+ {adapter.Description1.Description}");

          foreach (Output output in adapter.Outputs) {
            Debug.Write($"+   {output.Description.DeviceName} ");
            var intersection = Rectangle.Intersect(this.virtualRect, output.Description.DesktopBounds);
            if (intersection.Width > 0 && intersection.Height > 0) {
              try {
                Debug.WriteLine($"[XSECT: {intersection}]");
                captureSources.Add(new DxgiCaptureSource(adapter,
                                                         output,
                                                         new Rectangle(intersection.X,
                                                                       intersection.Y,
                                                                       intersection.Width,
                                                                       intersection.Height)));
              } catch (NotSupportedException exception)
                when (exception.InnerException?.HResult == ResultCode.Unsupported.Result) {
                // HACK: when Captain itself is running on the dGPU, DDA calls fail with DXGI_ERROR_UNSPUPORTED
                //       if the main desktop is not bound to it (see https://support.microsoft.com/en-us/kb/3019314).
                //       We fix this by trying the next adapter
              }
            } else {
              Debug.WriteLine("[MISMATCH]");
            }
          }
        }

        this.sources = captureSources.ToArray();
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Acquires a single frame
    /// </summary>
    /// <inheritdoc />
    /// <summary>
    ///   Acquires a single frame from this provider
    /// </summary>
    public override void AcquireFrame() {
      for (int i = 0; i < this.sources.Length; i++) {
        DxgiCaptureSource source = this.sources[i];

        try {
          OutputDuplicateFrameInformation info;
          Resource desktopResource = null;

          do {
            // release previous frame if last capture attempt failed
            if (desktopResource != null) {
              desktopResource.Dispose();
              source.Duplication.ReleaseFrame();
            }

            // try to capture a frame
            source.Duplication.AcquireNextFrame(DuplicationFrameTimeout,
                                                out info,
                                                out desktopResource);
          } while (info.TotalMetadataBufferSize == 0);

          this.lastPresentTime = info.LastPresentTime;

          using (var srcResource = desktopResource.QueryInterface<SharpDX.Direct3D11.Resource>())
          using (var destResource = source.Texture.QueryInterface<SharpDX.Direct3D11.Resource>()) {
            // copy the entire screen region to the target texture
            source.Device.ImmediateContext.CopySubresourceRegion(
              srcResource,
              0,
              source.Subregion,
              destResource,
              0);
          }

          // release resources
          desktopResource.Dispose();
          source.Duplication.ReleaseFrame();
        } catch (SharpDXException exception) when (exception.ResultCode == ResultCode.AccessLost ||
                                                   exception.ResultCode == ResultCode.DeviceHung ||
                                                   exception.ResultCode == ResultCode.DeviceRemoved) {
          // device has been lost - we can't ignore this and should try to reinitialize the D3D11 device until it's
          // available again (...)
          // we'll be receiving black/unsynced frames beyond this point - it is OK until we restore the device
          this.sources[i].Alive = false;
          while (!this.sources[i].Alive) {
            try {
              this.sources[i] = DxgiCaptureSource.Recreate(this.virtualRect);
              this.sources[i].Alive = true;
            } catch (SharpDXException) {
              /* could not restore the capture source - keep trying */
            }
          }
        }
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Creates a single bitmap from the captured frames and returns an object with its information
    /// </summary>
    /// <returns>
    ///   A <see cref="VideoFrame" /> instance that can be either a <see cref="D3D11VideoFrame"/> or
    ///   a <see cref="BitmapVideoFrame" />
    /// </returns>
    public override VideoFrame LockFrame() {
      long presentTimeTicks = (long) (this.lastPresentTime * 10e6 / this.perfFreq);

      if (this.sources.Length == 1 && this.sources.First().Alive) {
        // TODO: if multiple textures are owned by a single adapter, merge them using CopySubresourceRegion
        return new D3D11VideoFrame(this.sources[0].Texture, presentTimeTicks);
      }

      using (var factory = new ImagingFactory2()) {
        // create Bitmap but DON'T dispose it here
        var bmp = new Bitmap(factory,
                             this.virtualRect.Width,
                             this.virtualRect.Height,
                             PixelFormat.Format32bppBGRA,
                             BitmapCreateCacheOption.CacheOnDemand);

        // caller is responsible for disposing BitmapLock
        BitmapLock data = bmp.Lock(BitmapLockFlags.Write);
        int minX = this.sources.Select(s => s.Region.Left).Min();
        int minY = this.sources.Select(s => s.Region.Top).Min();

        // map textures
        foreach (DxgiCaptureSource source in this.sources) {
          using (var res = source.Texture.QueryInterface<SharpDX.Direct3D11.Resource>()) {
            DataBox map = source.Device.ImmediateContext.MapSubresource(res, 0, MapMode.Read, MapFlags.None);

            // merge partial captures into one big bitmap
            IntPtr dstScan0 = data.Data.DataPointer,
                   srcScan0 = map.DataPointer;
            int dstStride = data.Stride,
                srcStride = map.RowPitch;
            int srcWidth = source.Region.Right - source.Region.Left,
                srcHeight = source.Region.Bottom - source.Region.Top;
            int dstPixelSize = dstStride / data.Size.Width,
                srcPixelSize = srcStride / srcWidth;
            int dstX = source.Region.Left - minX,
                dstY = source.Region.Top - minY;

            for (int y = 0; y < srcHeight; y++) {
              Utilities.CopyMemory(IntPtr.Add(dstScan0, dstPixelSize * dstX + (y + dstY) * dstStride),
                                   IntPtr.Add(srcScan0, y * srcStride), srcPixelSize * srcWidth);
            }

            // release system memory
            source.Device.ImmediateContext.UnmapSubresource(res, 0);
          }
        }

        // return locked bitmap frame
        return new BitmapVideoFrame(bmp, data, presentTimeTicks);
        
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases the bitmap that may have been created for this frame
    /// </summary>
    /// <param name="frame">
    ///   Frame data structure returned by the <see cref="LockFrame" /> method
    /// </param>
    public override void UnlockFrame(VideoFrame frame) {
      switch (frame) {
        case BitmapVideoFrame bitmapFrame:
          bitmapFrame.BitmapLock.Dispose();
          bitmapFrame.Bitmap.Dispose();
          break;
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases resources used by the video provider
    /// </summary>
    public override void Dispose() {
      foreach (DxgiCaptureSource source in this.sources) {
        source.Dispose();
      }

      base.Dispose();
    }
  }
}