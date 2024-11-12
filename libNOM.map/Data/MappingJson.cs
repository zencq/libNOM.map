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
    internal KeyValuePair<string, string>[] Data { get; set; } = null!;
#else
    [JsonProperty("libMBIN_version")]
    internal required Version Version { get; init; }

    [JsonProperty("Mapping")]
    internal required KeyValuePair<string, string>[] Data { get; init; }
#endif

    #endregion

    // //

    #region Newtonsoft

    internal static MappingJson? Deserialize(string jsonString) => JsonConvert.DeserializeObject<MappingJson>(jsonString);

    #endregion
}
