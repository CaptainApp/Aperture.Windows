using SharpDX.DXGI;

namespace Aperture {
  /// <summary>
  ///   Implements a DXGI-enabled accelerated video codec
  /// </summary>
  public interface IDxgiEnabledVideoCodec {
    /// <summary>
    ///   Binds a DXGI device to the video codec
    /// </summary>
    /// <param name="device">DXGI device object</param>
    void BindDevice(Device device);
  }
}