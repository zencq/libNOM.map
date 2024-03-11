namespace libNOM.map;


/// <summary>
/// Holds settings how a <see cref="Mapping"/> should behave.
/// </summary>
public record class MappingSettings
{
    /// <summary>
    /// Where to download the updated mapping file.
    /// Default: ./download/
    /// </summary>
    public string DownloadDirectory { get; set; } = "download";

    /// <summary>
    /// Where to include prereleases when updating the mapping file.
    /// Default: true
    /// </summary>
    public bool IncludePrerelease { get; set; } = false;
}
