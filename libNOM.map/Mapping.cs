using libNOM.map.Data;
using libNOM.map.Extensions;
using libNOM.map.Services;

using Newtonsoft.Json.Linq;

namespace libNOM.map;


/// <summary>
/// Holds all necessary mapping data and provides obfuscation and deobfuscation.
/// </summary>
public static class Mapping
{
    #region Field

    private static GithubService? _githubService;
    private static readonly MappingJson _jsonCompiler = MappingJson.Deserialize(Properties.Resources.MBINCompiler)!; // latest MBINCompiler mapping.json when this version was created
    private static MappingJson? _jsonDownload; // dynamic content from the latest MBINCompiler release on GitHub
    private static readonly MappingJson _jsonLegacy = MappingJson.Deserialize(Properties.Resources.Legacy)!; // older keys that are not present in the latest version
    private static readonly MappingJson _jsonWizard = MappingJson.Deserialize(Properties.Resources.SaveWizard)!; // adjust differing mapping of SaveWizard
    private static readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
    private static readonly Dictionary<string, string> _mapForCommon = [];
    private static readonly Dictionary<string, string> _mapForCommonAccount = [];
    private static readonly Dictionary<string, string> _mapForDeobfuscation = [];
    private static readonly Dictionary<string, string> _mapForDeobfuscationAccount = [];
    private static readonly Dictionary<string, string> _mapForObfuscation = [];
    private static readonly Dictionary<string, string> _mapForObfuscationAccount = [];
    private static Task? _updateTask;

    #endregion

    #region Property

    // public //

    public static MappingSettings Settings { get; set; } = new();

    /// <summary>
    /// Used mapping version. Either the downloaded one if exists or built-in.
    /// </summary>
    public static Version Version => _jsonDownload?.Version ?? _jsonCompiler.Version; // { get; }

    // private

    private static string CombinedPath => Path.Combine(Path.GetFullPath(Settings.DownloadDirectory), Properties.Resources.RELEASE_ASSET);

    private static GithubService GithubService => _githubService ??= new(); // { private get; }

    private static bool IsUpdateRunning => !_updateTask?.IsCompleted ?? false; // { private get; }

    #endregion

    // //

    #region Create

    /// <summary>
    /// Creates maps with the mapping data of all files for obfuscation and deobfuscation.
    /// </summary>
    private static void CreateMap()
    {
        _mapForCommon.Clear();
        _mapForDeobfuscation.Clear();
        _mapForObfuscation.Clear();

        _mapForCommonAccount.Clear();
        _mapForDeobfuscationAccount.Clear();
        _mapForObfuscationAccount.Clear();

        AddToMap(_jsonCompiler, true, true, true);
        AddToMap(_jsonLegacy, true, true, true);
        AddToMap(_jsonWizard, true, false, false);

        if (_jsonDownload is null && File.Exists(CombinedPath))
            _jsonDownload = MappingJson.Deserialize(File.ReadAllText(CombinedPath));

        // Apply additional mapping but keep those that might have become outdated by always adding _jsonCompiler above.
        if (_jsonDownload?.Version > _jsonCompiler.Version)
            AddToMap(_jsonDownload, true, true, true);
    }

    /// <summary>
    /// Adds mapping data of a single file to the maps for obfuscation and deobfuscation.
    /// </summary>
    /// <param name="mappingJson">Object of a deserialized file.</param>
    /// <param name="deobfuscate">Whether to add a pair to the deobfuscation map.</param>
    /// <param name="obfuscate">Whether to add a pair to the obfuscation map.</param>
    private static void AddToMap(MappingJson mappingJson, bool deobfuscate, bool obfuscate, bool includeAccount)
    {
        if (includeAccount)
            foreach (var (data, useAccount) in mappingJson.Data.SplitAtElement("UserSettingsData"))
                AddToMap(data, deobfuscate, obfuscate, useAccount);
        else
            AddToMap(mappingJson.Data, deobfuscate, obfuscate, useAccount: false);
    }

    private static void AddToMap(IEnumerable<KeyValuePair<string, string>> data, bool deobfuscate, bool obfuscate, bool useAccount)
    {
        foreach (var pair in data)
            if (deobfuscate == obfuscate)
            {
                var mapForCommon = useAccount ? _mapForCommonAccount : _mapForCommon;
                mapForCommon[pair.Key] = pair.Value;
            }
            else
            {
                if (deobfuscate)
                {
                    var mapForDeobfuscation = useAccount ? _mapForDeobfuscationAccount : _mapForDeobfuscation;
                    mapForDeobfuscation[pair.Key] = pair.Value;
                }
                if (obfuscate)
                {
                    var mapForObfuscation = useAccount ? _mapForObfuscationAccount : _mapForObfuscation;
                    mapForObfuscation[pair.Key] = pair.Value;
                }
            }
    }

    #endregion

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
        var mapForDeobfuscation = (useAccount ? _mapForCommonAccount.Concat(_mapForDeobfuscationAccount) : _mapForCommon.Concat(_mapForDeobfuscation)).ToDictionary(i => i.Key, i => i.Value);
        var unknownKeys = new HashSet<string>();

        // Collect all jProperties that need to be renamed.
        foreach (var child in node.Children().Where(i => i.HasValues))
            GetPropertiesToDeobfuscate(child, jProperties, mapForDeobfuscation, unknownKeys);

        // Actually rename each jProperty.
        foreach (var jProperty in jProperties)
            jProperty.Rename(mapForDeobfuscation[jProperty.Name]);

        return unknownKeys;
    }

    /// <summary>
    /// Ensures that the update task is complete and both maps are created.
    /// </summary>
    private static void EnsurePreconditions(JToken? node)
    {
#if NETSTANDARD2_0_OR_GREATER
        if (node is null)
            throw new ArgumentNullException(nameof(node));
#else
        ArgumentNullException.ThrowIfNull(node, nameof(node));
#endif

        // Wait in case of currently running update.
        _updateTask?.Wait();

        // Lock here to avoid entering CreateMap().
        _lock.EnterWriteLock();

        // Create map if not done yet.
        if (_mapForCommon.Count == 0 || _mapForCommonAccount.Count == 0)
            CreateMap();

        // Release lock if necessary.
        if (_lock.IsWriteLockHeld)
            _lock.ExitWriteLock();
    }

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
        var mapForObfuscation = (useAccount ? _mapForCommonAccount.Concat(_mapForObfuscationAccount) : _mapForCommon.Concat(_mapForObfuscation)).ToDictionary(i => i.Value, i => i.Key); // switch to have the origin as Key

        // Collect all jProperties that need to be renamed.
        foreach (var child in node.Children().Where(i => i.HasValues))
            GetPropertiesToObfuscate(child, jProperties, mapForObfuscation);

        // Actually rename each jProperty.
        foreach (var jProperty in jProperties)
            jProperty.Rename(mapForObfuscation[jProperty.Name]);
    }

    #endregion

    #region Update

    /// <summary>
    /// Downloads the latest mapping file and updates the maps.
    /// </summary>
    /// <returns>Whether a newer version of the mapping file was successfully downloaded.</returns>
    public static bool Update()
    {
        var result = false;
        if (!IsUpdateRunning)
        {
            _updateTask = Task.Run(async () =>
            {
                result = await GetJsonDownloadAsync();
                if (result)
                {
                    CreateMap();
                }
            });
        }
        _updateTask!.Wait(); // in case it was running, assume not newer
        return result;
    }

    /// <summary>
    /// Downloads the latest mapping file and updates the maps.
    /// This method does not block the calling thread.
    /// </summary>
    public static void UpdateAsync()
    {
        // No need to run if currently running.
        if (IsUpdateRunning)
            return;

        _updateTask = Task.Run(async () =>
        {
            var result = await GetJsonDownloadAsync();
            if (result)
            {
                CreateMap();
            }
        });
    }

    /// <summary>
    /// Downloads the latest mapping file and persists it to a file.
    /// This method does not block the calling thread.
    /// </summary>
    /// <returns>Whether a newer version of the mapping file was successfully downloaded.</returns>
    private static async Task<bool> GetJsonDownloadAsync()
    {
        var content = await GithubService.DownloadMappingJsonAsync(Settings.IncludePrerelease);
        if (string.IsNullOrEmpty(content))
            return false;

        Directory.CreateDirectory(new FileInfo(CombinedPath).DirectoryName!);
        // File does not matter until next startup and therefore no need to wait.
        try
        {
#if NETSTANDARD2_0
            _ = Task.Run(() => File.WriteAllText(CombinedPath, content));
#else
            _ = File.WriteAllTextAsync(CombinedPath, content);
#endif
        }
        catch (IOException) { } // Try again next time.

        _jsonDownload = MappingJson.Deserialize(content!);

        return true;
    }

    #endregion
}
