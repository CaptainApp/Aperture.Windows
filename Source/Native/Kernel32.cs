using System.Runtime.InteropServices;

namespace Aperture {
  /// <summary>
  ///   Exported functions from the kernel32.dll Windows library.
  /// </summary>
  internal static class Kernel32 {
    /// <summary>
    ///   Retrieves the frequency of the performance counter
    /// </summary>
    /// <param name="lpFrequency">
    ///   A pointer to a variable that receives the current performance-counter frequency.
    /// </param>
    /// <returns>
    ///   If the installed hardware supports a high-resolution performance counter, the return value is nonzero.
    /// </returns>
    [DllImport(nameof(Kernel32), SetLastError = true)]
    internal static extern bool QueryPerformanceFrequency(out long lpFrequency);
  }
}