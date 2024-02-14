namespace libNOM.map;


/// <summary>
/// Holds settings how a <see cref="Mapping"/> should behave.
/// </summary>
public record class MappingSettings
{
#if NETSTANDARD2_0_OR_GREATER
    /// <summary>
    /// Where to download the updated mapping file.
    /// Default: ./download/
    /// </summary>
    public string Download { get; set; } = "download";

    /// <summary>
    /// Where to include prereleases when updating the mapping file.
    /// Default: true
    /// </summary>
    public bool IncludePrerelease { get; set; } = false;
#else
    /// <summary>
    /// Where to download the updated mapping file.
    /// Default: ./download/
    /// </summary>
    public string Download { get; init; } = "download";

    /// <summary>
    /// Where to include prereleases when updating the mapping file.
    /// Default: false
    /// </summary>
    public bool IncludePrerelease { get; init; } = false;
#endif
}
