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
    /// Gets the deobfuscated key for the specified property (name).
    /// </summary>
    /// <param name="mapForDeobfuscation"></param>
    /// <param name="jProperty"></param>
    /// <returns></returns>
    private static string? GetDeobfuscatedKey(IEnumerable<KeyValuePair<string, string>> mapForDeobfuscation, JProperty jProperty)
    {
        KeyValuePair<string, string> result = default;

        foreach (var (ObfuscatedKey, DeobfuscatedKey, PartialPath) in _mapOfCollision)
            if (jProperty.Name == ObfuscatedKey)
            {
                if (jProperty.Path.Contains(PartialPath))
                {
                    result = mapForDeobfuscation.FirstOrDefault(i => i.Key == jProperty.Name && i.Value == DeobfuscatedKey);
                }
                else
                {
                    result = mapForDeobfuscation.FirstOrDefault(i => i.Key == jProperty.Name && i.Value != DeobfuscatedKey);
                }

                // Stop if found a match.
                break;
            }

        // If no collision found, use default matching.
        if (result.Value is null)
            result = mapForDeobfuscation.FirstOrDefault(i => i.Key == jProperty.Name);

        return result.Value;
    }

    /// <summary>
    /// Iterates over all JSON properties to collect a list for deobfuscation.
    /// </summary>
    /// <param name="token">Current property that should be deobfuscated.</param>
    /// <param name="jProperties">List of properties that need to be deobfuscated.</param>
    /// <param name="unknownKeys">List of keys that cannot be deobfuscated.</param>
    private static void GetPropertiesToDeobfuscate(JToken token, List<JProperty> jProperties, IEnumerable<KeyValuePair<string, string>> mapForDeobfuscation, HashSet<string> unknownKeys)
    {
        if (token.Type == JTokenType.Property)
        {
            var property = (JProperty)(token);

            if (mapForDeobfuscation.FirstOrDefault(i => i.Key == property.Name).Key is not null)
            {
                jProperties.Add(property);
            }
            // Only add if it is not a target value as well.
            else if (mapForDeobfuscation.FirstOrDefault(i => i.Value == property.Name).Value is null)
            {
                unknownKeys.Add(property.Name);
            }
        }

        foreach (var child in token.Children().Where(i => i.HasValues))
            GetPropertiesToDeobfuscate(child, jProperties, mapForDeobfuscation, unknownKeys);
    }

    #endregion

    #region Mapping

    private static IEnumerable<KeyValuePair<string, string>> GetMapForDeobfuscation(bool useAccount) => useAccount ? _mapForCommonAccount.Concat(_mapForDeobfuscationAccount) : _mapForCommon.Concat(_mapForDeobfuscation);

    /// <inheritdoc cref="GetMappedKeyForDeobfuscationOrInput(string, bool, out string)"/>
    public static bool GetMappedKeyForDeobfuscationOrInput(string key, out string result) => GetMappedKeyForDeobfuscationOrInput(key, false, out result);

    /// <summary>
    /// Maps the specified obfuscated key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="useAccount"></param>
    /// <returns>The deobfuscated key or the input if no mapping found.</returns>
    public static bool GetMappedKeyForDeobfuscationOrInput(string key, bool useAccount, out string result)
    {
        var mapped = GetMapForDeobfuscation(useAccount).FirstOrDefault(i => i.Key == key);
        if (mapped.Value is null)
        {
            result = key;
            return false;
        }
        else
        {
            result = mapped.Value;
            return true;
        }
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
        {
            var deobfuscated = GetDeobfuscatedKey(mapForDeobfuscation, jProperty);
            if (deobfuscated is not null)
                jProperty.Rename(deobfuscated);
        }

        return unknownKeys;
    }

    #endregion
}
