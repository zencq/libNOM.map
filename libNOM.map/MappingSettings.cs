namespace libNOM.map;


/// <summary>
/// Holds settings how a <see cref="Mapping"/> should behave.
/// </summary>
public record class MappingSettings
{
    /// <summary>
    /// Where to download the updated mapping file.
    /// </summary>
#if NETSTANDARD2_0_OR_GREATER
    public string Download { get; set; } = "download";
#else
    public string Download { get; init; } = "download";
#endif
}
