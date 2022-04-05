namespace libNOM.map;


/// <summary>
/// Holds settings how a <see cref="Mapping"/> should behave.
/// </summary>
public record MappingSettings
{
    /// <summary>
    /// Where to download the mapping.json.
    /// </summary>
    public string PathDownload { get; init; } = "download";
}
