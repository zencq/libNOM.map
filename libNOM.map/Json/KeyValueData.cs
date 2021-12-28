using Newtonsoft.Json;

namespace libNOM.map.Json;


internal partial class KeyValueData
{
    [JsonProperty("Key")]
    internal string Key { get; set; } = default!;

    [JsonProperty("Value")]
    internal string Value { get; set; } = default!;
}
