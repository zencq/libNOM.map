using Newtonsoft.Json;

namespace libNOM.map.Json;


/// <summary>
/// Holds data of a single deserialized key/value pair.
/// </summary>
internal record class KeyValueData
{
    [JsonProperty("Key")]
    internal string Key { get; set; } = null!;

    [JsonProperty("Value")]
    internal string Value { get; set; } = null!;
}
