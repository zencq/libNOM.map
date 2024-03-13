using Newtonsoft.Json;

namespace libNOM.map.Data;


/// <summary>
/// Holds data of a deserialized mapping.json file and a function to retrieve them.
/// </summary>
internal record class MappingJson
{
    #region Property

#if NETSTANDARD2_0_OR_GREATER
    [JsonProperty("libMBIN_version")]
    internal Version Version { get; set; } = null!;

    [JsonProperty("Mapping")]
    internal KeyValueData[] Data { get; set; } = null!;
#elif NET6_0
    [JsonProperty("libMBIN_version")]
    internal Version Version { get; init; } = null!;

    [JsonProperty("Mapping")]
    internal KeyValueData[] Data { get; init; } = null!;
#else
    [JsonProperty("libMBIN_version")]
    internal required Version Version { get; init; }

    [JsonProperty("Mapping")]
    internal required KeyValueData[] Data { get; init; }
#endif

    #endregion

    // //

    #region Newtonsoft

    internal static MappingJson? Deserialize(string jsonString) => JsonConvert.DeserializeObject<MappingJson>(jsonString);

    #endregion
}
