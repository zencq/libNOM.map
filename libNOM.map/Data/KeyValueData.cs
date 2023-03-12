using Newtonsoft.Json;

namespace libNOM.map.Data;


/// <summary>
/// Holds data of a single deserialized key/value pair.
/// </summary>
internal record class KeyValueData
{
#if NETSTANDARD2_0_OR_GREATER
    [JsonProperty("Key")]
    internal string Key { get; set; } = null!;

    [JsonProperty("Value")]
    internal string Value { get; set; } = null!;
#elif NET6_0
    [JsonProperty("Key")]
    internal string Key { get; init; } = null!;

    [JsonProperty("Value")]
    internal string Value { get; init; } = null!;
#else
    [JsonProperty("Key")]
    internal required string Key { get; init; }

    [JsonProperty("Value")]
    internal required string Value { get; init; }
#endif
}
