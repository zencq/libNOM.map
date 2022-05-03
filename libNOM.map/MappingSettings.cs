namespace libNOM.map;


/// <summary>
/// Holds settings how a <see cref="Mapping"/> should behave.
/// </summary>
public record class MappingSettings
{
    /// <summary>
    /// Where to download the updated mapping file.
    /// </summary>
#if NET47_OR_GREATER || NETSTANDARD2_0_OR_GREATER
    public string Download { get; set; } = "download";
#elif NET5_0_OR_GREATER
    public string Download { get; init; } = "download";
#endif
}
