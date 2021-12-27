using Newtonsoft.Json;

namespace libNOM.map;


#pragma warning disable CS8618 // All fields are set after deserialization.
internal partial class MappingJson
{
    [JsonProperty("libMBIN_version")]
    internal Version Version { get; set; }

    [JsonProperty("Mapping")]
    internal KeyValueData[] Data { get; set; }
}

internal partial class KeyValueData
{
    [JsonProperty("Key")]
    internal string Key { get; set; }

    [JsonProperty("Value")]
    internal string Value { get; set; }
}
#pragma warning restore CS8618

internal partial class MappingJson
{
    internal static MappingJson? Deserialize(string value) => JsonConvert.DeserializeObject<MappingJson>(value);
}
