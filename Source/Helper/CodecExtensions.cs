using System;
using System.Linq;

namespace Aperture {
  /// <summary>
  ///   Extension methods for the <see cref="Codec" /> class
  /// </summary>
  public static class CodecExtensions {
    /// <summary>
    ///   Gets the media type attribute for a <see cref="Codec" /> instance
    /// </summary>
    /// <param name="codec">Codec instance</param>
    /// <returns>A <see cref="MediaType" /> instance or <c>null</c> if none is present</returns>
    public static MediaType GetMediaType(this Codec codec) {
      try {
        return codec.GetType().GetCustomAttributes(typeof(MediaType), false).First() as MediaType;
      } catch (InvalidOperationException) {
        return null;
      }
    }
  }
}