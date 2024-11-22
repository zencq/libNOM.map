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
    /// Iterates over all JSON properties to collect a list for deobfuscation.
    /// </summary>
    /// <param name="token">Current property that should be deobfuscated.</param>
    /// <param name="jProperties">List of properties that need to be deobfuscated.</param>
    /// <param name="unknownKeys">List of keys that cannot be deobfuscated.</param>
    private static void GetPropertiesToDeobfuscate(JToken token, List<JProperty> jProperties, Dictionary<string, string> mapForDeobfuscation, HashSet<string> unknownKeys)
    {
        if (token.Type == JTokenType.Property)
        {
            var property = (JProperty)(token);

            if (mapForDeobfuscation.ContainsKey(property.Name))
            {
                jProperties.Add(property);
            }
            // Only add if it is not a target value as well.
            else if (!mapForDeobfuscation.ContainsValue(property.Name))
            {
                unknownKeys.Add(property.Name);
            }
        }

        foreach (var child in token.Children().Where(i => i.HasValues))
            GetPropertiesToDeobfuscate(child, jProperties, mapForDeobfuscation, unknownKeys);
    }

    #endregion

    #region Mapping

    private static Dictionary<string, string> GetMapForDeobfuscation(bool useAccount) => (useAccount ? _mapForCommonAccount.Concat(_mapForDeobfuscationAccount) : _mapForCommon.Concat(_mapForDeobfuscation)).ToDictionary(i => i.Key, i => i.Value);

    /// <inheritdoc cref="GetMappedKeyForDeobfuscationOrInput(string, bool)"/>
    public static string GetMappedKeyForDeobfuscationOrInput(string key) => GetMappedKeyForDeobfuscationOrInput(key, false);

    /// <summary>
    /// Maps the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="useAccount"></param>
    /// <returns>The deobfuscated key or the input if no mapping found.</returns>
    public static string GetMappedKeyForDeobfuscationOrInput(string key, bool useAccount)
    {
        if (GetMapForDeobfuscation(useAccount).TryGetValue(key, out var resultFromDeobfuscation))
            return resultFromDeobfuscation;

        return key;
    }

    // //

    /// <inheritdoc cref="Deobfuscate(JToken, bool)"/>
    public static HashSet<string> Deobfuscate(JToken node) => Deobfuscate(node, false);

    /// <summary>
    /// Deobfuscates JSON to make it human-readable.
    /// </summary>
    /// <param name="node">A node within a JSON object or the root itself.</param>
    /// <param name="useAccount"></param>
    /// <returns>List of unknown keys.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static HashSet<string> Deobfuscate(JToken node, bool useAccount)
    {
        EnsurePreconditions(node);

        var jProperties = new List<JProperty>();
        var mapForDeobfuscation = GetMapForDeobfuscation(useAccount);
        var unknownKeys = new HashSet<string>();

        // Collect all jProperties that need to be renamed.
        foreach (var child in node.Children().Where(i => i.HasValues))
            GetPropertiesToDeobfuscate(child, jProperties, mapForDeobfuscation, unknownKeys);

        // Actually rename each jProperty.
        foreach (var jProperty in jProperties)
            jProperty.Rename(mapForDeobfuscation[jProperty.Name]);

        return unknownKeys;
    }

    #endregion
}
