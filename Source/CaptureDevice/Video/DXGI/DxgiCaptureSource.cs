using System;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using ResultCode = SharpDX.DXGI.ResultCode;

namespace Aperture {
  /// <inheritdoc />
  /// <summary>
  ///   Source for partial video capture.
  ///   The virtual desktop may be rendered by different adapters.
  ///   A capture source contains a region of the screen associated to an output and adapter
  /// </summary>
  internal class DxgiCaptureSource : IDisposable {
    /// <summary>
    ///   Direct3D 11 device instance
    /// </summary>
    internal Device Device { get; private set; }

    /// <summary>
    ///   DXGI device instance
    /// </summary>
    internal SharpDX.DXGI.Device DxgiDevice { get; private set; }

    /// <summary>
    ///   DXGI adapter instance
    /// </summary>
    private Adapter1 Adapter1 { get; set; }

    #region DXGI outputs

    /// <summary>
    ///   DXGI output associated with this source
    /// </summary>
    private Output Output { get; set; }

    /// <summary>
    ///   DXGI 1.1 output associated with this source
    /// </summary>
    private Output1 Output1 { get; set; }

    /// <summary>
    ///   DXGI 1.6 output associated with this source
    /// </summary>
    private Output6 Output6 { get; set; }

    #endregion

    /// <summary>
    ///   Region of the virtual desktop to be captured
    /// </summary>
    internal Rectangle Region { get; }

    /// <summary>
    ///   Subregion on the device texture of the source to be captured
    /// </summary>
    internal ResourceRegion? Subregion { get; }

    /// <summary>
    ///   Staging texture for this capture source
    /// </summary>
    internal Texture2D Texture { get; private set; }

    /// <summary>
    ///   Desktop duplication for this output
    /// </summary>
    internal OutputDuplication Duplication { get; private set; }

    /// <summary>
    ///   When <c>true</c>, this source is available for capturing frames
    /// </summary>
    internal bool Alive { get; set; }

    /// <summary>
    ///   Struct constructor
    /// </summary>
    /// <param name="adapter1">DXGI adapter device instance</param>
    /// <param name="output">DXGI output associated with this source</param>
    /// <param name="region">Region of the DXGI output device</param>
    internal DxgiCaptureSource(Adapter1 adapter1, Output output, Rectangle region) {
      Device = null;
      Adapter1 = adapter1;
      Output = output;
      Output1 = null;
      Output6 = null;
      Region = region;

      if (region != output.Description.DesktopBounds) {
        Subregion = new ResourceRegion(region.Left - output.Description.DesktopBounds.Left,
                                       region.Top - output.Description.DesktopBounds.Top,
                                       0,
                                       region.Width + region.Left,
                                       region.Height + region.Top,
                                       1);
      } else {
        Subregion = null;
      }

      try {
        // create device
        Device = new Device(adapter1, DeviceCreationFlags.None, FeatureLevel.Level_11_0) {
#if DEBUG
          DebugName = output.Description.DeviceName + " // " + adapter1.Description.Description
#endif
        };

        DxgiDevice = Device.QueryInterface<SharpDX.DXGI.Device>();

        // create texture
        Texture = new Texture2D(Device, new Texture2DDescription {
          CpuAccessFlags = CpuAccessFlags.Read,
          BindFlags = BindFlags.None,
          Format = Format.B8G8R8A8_UNorm,
          Width = region.Width,
          Height = region.Height,
          OptionFlags = ResourceOptionFlags.None,
          MipLevels = 1,
          ArraySize = 1,
          SampleDescription = {Count = 1, Quality = 0},
          Usage = ResourceUsage.Staging
        });

        // duplicate desktop
        try {
          Format[] formats = {Format.B8G8R8A8_UNorm};
          Output6 = output.QueryInterface<Output6>();
          Duplication = Output6.DuplicateOutput1(Device, 0, formats.Length, formats);
        } catch (Exception exception)
          when (exception is NotSupportedException ||
                exception.HResult == ResultCode.Unsupported.Result ||
                exception.HResult == Result.NoInterface.Result) {
          Output1 = output.QueryInterface<Output1>();
          Duplication = Output1.DuplicateOutput(Device);
        }
      } catch (Exception exception)
        when (exception is NotSupportedException ||
              exception is NotImplementedException ||
              exception.HResult == ResultCode.Unsupported.Result ||
              exception.HResult == Result.NotImplemented.Result ||
              exception.HResult == Result.NoInterface.Result) {
        throw new NotSupportedException("Platform not supported", exception);
      }

      Alive = true;
    }

    /// <summary>
    ///   Creates a capture source with the same properties as this one
    /// </summary>
    /// <param name="bounds">Target screen bounds</param>
    /// <returns>A new <see cref="DxgiCaptureSource"/> instance</returns>
    internal static DxgiCaptureSource Recreate(Rectangle bounds) {
      DxgiCaptureSource source = null;

      using (var factory = new Factory1()) {
        if (factory.GetAdapterCount1() == 0) {
          throw new NotSupportedException("No suitable video adapters found");
        }

        foreach (Adapter1 adapter in factory.Adapters1) {
          foreach (Output output in adapter.Outputs) {
            var intersection = Rectangle.Intersect(bounds, output.Description.DesktopBounds);
            if (intersection.Width > 0 && intersection.Height > 0) {
              source = new DxgiCaptureSource(adapter, output, new Rectangle(intersection.X,
                                                                            intersection.Y,
                                                                            intersection.Width,
                                                                            intersection.Height));
              break;
            }

            output.Dispose();
          }

          adapter.Dispose();
          if (source != null) {
            break;
          }
        }
      }

      return source ?? throw new Exception("No suitable capture sources were found");
    }

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
    /// </summary>
    public void Dispose() {
      // release resources
      Alive = false;

      Duplication.Dispose();
      Duplication = null;

      Output.Dispose();
      Output = null;

      Output1?.Dispose();
      Output1 = null;

      Output6?.Dispose();
      Output6 = null;

      DxgiDevice.Dispose();
      DxgiDevice = null;

      Device.Dispose();
      Device = null;

      Texture.Dispose();
      Texture = null;

      Adapter1.Dispose();
      Adapter1 = null;
    }
  }
}