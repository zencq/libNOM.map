using Newtonsoft.Json;

namespace libNOM.map.Json;


/// <summary>
/// Holds data of a deserialized mapping.json file and a cuntion to retrieve them.
/// </summary>
internal record class MappingJson
{
    #region Property

    [JsonProperty("libMBIN_version")]
    internal Version Version { get; set; } = null!;

    [JsonProperty("Mapping")]
    internal KeyValueData[] Data { get; set; } = null!;

    #endregion

    // //

    #region Newtonsoft

    internal static MappingJson? Deserialize(string jsonString) => JsonConvert.DeserializeObject<MappingJson>(jsonString);

    #endregion
}
