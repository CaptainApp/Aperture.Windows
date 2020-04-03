using System;

namespace Aperture {
  /// <inheritdoc />
  /// <summary>
  ///   Specifies media type information for a codec
  /// </summary>
  [AttributeUsage(AttributeTargets.Class)]
  public sealed class MediaType : Attribute {
    /// <summary>
    ///   MIME type
    /// </summary>
    public string Type { get; }

    /// <summary>
    ///   File extension
    /// </summary>
    public string Extension { get; }

    /// <inheritdoc />
    /// <summary>
    ///   Sets the localized display name for this object
    /// </summary>
    /// <param name="mimetype">MIME type for this container format</param>
    /// <param name="extension">File extension for this container format</param>
    public MediaType(string mimetype = "", string extension = "") => (Type, Extension) = (mimetype.Trim().ToLower(), extension.Trim(' ', '.').ToLower());
  }
}