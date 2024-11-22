using libNOM.map.Extensions;

using Newtonsoft.Json.Linq;

namespace libNOM.map;


/// <summary>
/// Holds all necessary mapping data and provides obfuscation and deobfuscation.
/// </summary>
public static partial class Mapping
{
    // //

    #region Getter

    /// <summary>
    /// Iterates over all JSON properties to collect a list for obfuscation.
    /// </summary>
    /// <param name="token">Current property that should be obfuscated.</param>
    /// <param name="jProperties">List of properties that need to be obfuscated.</param>
    private static void GetPropertiesToObfuscate(JToken token, List<JProperty> jProperties, Dictionary<string, string> mapForObfuscation)
    {
        if (token.Type == JTokenType.Property)
        {
            var property = (JProperty)(token);

            if (mapForObfuscation.ContainsKey(property.Name))
            {
                jProperties.Add(property);
            }
        }

        foreach (var child in token.Children().Where(i => i.HasValues))
            GetPropertiesToObfuscate(child, jProperties, mapForObfuscation);
    }

    #endregion

    #region Mapping

    private static Dictionary<string, string> GetMapForObfuscation(bool useAccount) => (useAccount ? _mapForCommonAccount.Concat(_mapForObfuscationAccount) : _mapForCommon.Concat(_mapForObfuscation)).ToDictionary(i => i.Value, i => i.Key); // switch to have the origin as Key

    /// <inheritdoc cref="GetMappedKeyForObfuscationOrInput(string, bool)"/>
    public static string GetMappedKeyForObfuscationOrInput(string key) => GetMappedKeyForObfuscationOrInput(key, false);

    /// <summary>
    /// Maps the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="useAccount"></param>
    /// <returns>The obfuscated key or the input if no mapping found.</returns>
    public static string GetMappedKeyForObfuscationOrInput(string key, bool useAccount)
    {
        if (GetMapForObfuscation(useAccount).TryGetValue(key, out var resultFromObfuscation))
            return resultFromObfuscation;

        return key;
    }

    // //

    /// <inheritdoc cref="Obfuscate(JToken, bool)"/>
    public static void Obfuscate(JToken node) => Obfuscate(node, false);

    /// <summary>
    /// Obfuscates JSON to make it readable by the game.
    /// </summary>
    /// <param name="node">A node within a JSON object or the root itself.</param>
    /// <param name="useAccount"></param>
    public static void Obfuscate(JToken node, bool useAccount)
    {
        EnsurePreconditions(node);

        var jProperties = new List<JProperty>();
        var mapForObfuscation = GetMapForObfuscation(useAccount);

        // Collect all jProperties that need to be renamed.
        foreach (var child in node.Children().Where(i => i.HasValues))
            GetPropertiesToObfuscate(child, jProperties, mapForObfuscation);

        // Actually rename each jProperty.
        foreach (var jProperty in jProperties)
            jProperty.Rename(mapForObfuscation[jProperty.Name]);
    }

    #endregion
}
