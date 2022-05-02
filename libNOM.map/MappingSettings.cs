namespace libNOM.map;


/// <summary>
/// Holds settings how a <see cref="Mapping"/> should behave.
/// </summary>
public record class MappingSettings
{
    /// <summary>
    /// Where to download the mapping.json.
    /// </summary>
#if NET47_OR_GREATER || NETSTANDARD2_0_OR_GREATER
    public string PathDownload { get; set; } = "download";
#elif NET5_0_OR_GREATER
    public string PathDownload { get; init; } = "download";
#endif
}
