using Newtonsoft.Json;

namespace libNOM.map.Json;


internal partial class MappingJson
{
    [JsonProperty("libMBIN_version")]
    internal Version Version { get; set; } = default!;

    [JsonProperty("Mapping")]
    internal KeyValueData[] Data { get; set; } = default!;
}

internal partial class MappingJson
{
    internal static MappingJson? Deserialize(string jsonString) => JsonConvert.DeserializeObject<MappingJson>(jsonString);
}
