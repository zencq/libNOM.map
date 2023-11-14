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
    private static readonly Dictionary<string, string> _mapForDeobfuscation = [];
    private static readonly Dictionary<string, string> _mapForObfuscation = [];
    private static MappingSettings _settings = new();
    private static Task? _updateTask;

    // Dependencies
    private static string _path = GetCombinedPath();

    #endregion

    #region Property

    private static GithubService GithubService => _githubService ??= new(); // { private get; }

    private static bool IsRunning => !_updateTask?.IsCompleted ?? false; // { private get; }

    public static MappingSettings Settings // { get; set; }
    {
        get => _settings;
        set
        {
            _settings = value;
            _path = GetCombinedPath();
        }
    }

    /// <summary>
    /// Used mapping version. Either the built-in or downloaded one.
    /// </summary>
    public static Version Version => _jsonDownload?.Version ?? _jsonCompiler.Version; // { get; }

    #endregion

    // //

    #region Create

    /// <summary>
    /// Creates maps with the mapping data of all files for obfuscation and deobfuscation.
    /// </summary>
    private static void CreateMap()
    {
        _mapForDeobfuscation.Clear();
        _mapForObfuscation.Clear();

        AddToMap(_jsonCompiler, true, true);
        AddToMap(_jsonLegacy, true, true);
        AddToMap(_jsonWizard, true, false);

        if (_jsonDownload is null && File.Exists(_path))
        {
            _jsonDownload = MappingJson.Deserialize(File.ReadAllText(_path));
        }
        // Apply additional mapping but keep those that might have become outdated by always adding _jsonCompiler.
        if (_jsonDownload?.Version > _jsonCompiler.Version)
        {
            AddToMap(_jsonDownload, true, true);
        }
    }

    /// <summary>
    /// Adds mapping data of a single file to the maps for obfuscation and deobfuscation.
    /// </summary>
    /// <param name="mappingJson">Object of a deserialized file.</param>
    /// <param name="deobfuscate">Whether to add a pair to the deobfuscation map.</param>
    /// <param name="obfuscate">Whether to add a pair to the obfuscation map.</param>
    private static void AddToMap(MappingJson mappingJson, bool deobfuscate, bool obfuscate)
    {
        foreach (var pair in mappingJson.Data)
        {
            // Set it this way to avoid conflicts if adding JsonDownload.
            if (deobfuscate)
            {
                _mapForDeobfuscation[pair.Key] = pair.Value;
            }
            if (obfuscate)
            {
                _mapForObfuscation[pair.Value] = pair.Key;
            }
        }
    }

    #endregion

    #region Getter

    /// <summary>
    /// Combines the download path from the settings with the filename.
    /// </summary>
    /// <returns></returns>
    private static string GetCombinedPath()
    {
        return Path.Combine(Path.GetFullPath(_settings.Download), Properties.Resources.RELEASE_ASSET);
    }

    /// <summary>
    /// Iterates over all JSON properties to collect a list for deobfuscation.
    /// </summary>
    /// <param name="token">Current property that should be deobfuscated.</param>
    /// <param name="jProperties">List of properties that need to be deobfuscated.</param>
    /// <param name="unknownKeys">List of keys that cannot be deobfuscated.</param>
    private static void GetPropertiesToDeobfuscate(JToken token, List<JProperty> jProperties, HashSet<string> unknownKeys)
    {
        if (token.Type == JTokenType.Property)
        {
            var property = (JProperty)(token);
            if (_mapForDeobfuscation.ContainsKey(property.Name))
            {
                jProperties.Add(property);
            }
            // Only add if it is not a target value as well.
            else if (!_mapForDeobfuscation.ContainsValue(property.Name))
            {
                unknownKeys.Add(property.Name);
            }
        }
        foreach (var child in token.Children().Where(i => i.HasValues))
        {
            GetPropertiesToDeobfuscate(child, jProperties, unknownKeys);
        }
    }

    /// <summary>
    /// Iterates over all JSON properties to collect a list for obfuscation.
    /// </summary>
    /// <param name="token">Current property that should be obfuscated.</param>
    /// <param name="jProperties">List of properties that need to be obfuscated.</param>
    private static void GetPropertiesToObfuscate(JToken token, List<JProperty> jProperties)
    {
        if (token.Type == JTokenType.Property)
        {
            var property = (JProperty)(token);
            if (_mapForObfuscation.ContainsKey(property.Name))
            {
                jProperties.Add(property);
            }
        }
        foreach (var child in token.Children().Where(i => i.HasValues))
        {
            GetPropertiesToObfuscate(child, jProperties);
        }
    }

    #endregion

    #region Mapping

    /// <summary>
    /// Deobfuscates JSON to make it human-readable.
    /// </summary>
    /// <param name="node">A node within a JSON object or the root itself.</param>
    /// <returns>List of unknown keys.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static HashSet<string> Deobfuscate(JToken? node)
    {
        EnsurePreconditions(node);

        var jProperties = new List<JProperty>();
        var unknownKeys = new HashSet<string>();

        // Collect all jProperties that need to be renamed.
        foreach (var child in node!.Children().Where(i => i.HasValues))
        {
            GetPropertiesToDeobfuscate(child, jProperties, unknownKeys);
        }

        // Actually rename each jProperty.
        foreach (var jProperty in jProperties)
        {
            jProperty.Rename(_mapForDeobfuscation[jProperty.Name]);
        }

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
        if (_mapForDeobfuscation.Count == 0 || _mapForObfuscation.Count == 0)
        {
            CreateMap();
        }

        if (_lock.IsWriteLockHeld)
            _lock.ExitWriteLock();
    }

    /// <summary>
    /// Obfuscates JSON to make it readable by the game.
    /// </summary>
    /// <param name="node">A node within a JSON object or the root itself.</param>
    public static void Obfuscate(JToken? node)
    {
        EnsurePreconditions(node);

        var jProperties = new List<JProperty>();

        // Collect all jProperties that need to be renamed.
        foreach (var child in node!.Children().Where(i => i.HasValues))
        {
            GetPropertiesToObfuscate(child, jProperties);
        }

        // Actually rename each jProperty.
        foreach (var jProperty in jProperties)
        {
            jProperty.Rename(_mapForObfuscation[jProperty.Name]);
        }
    }

    #endregion

    #region Update

    /// <summary>
    /// Downloads the lastet mapping file and updates the maps.
    /// </summary>
    /// <returns>Whether a newer version of the mapping file was successfully downloaded.</returns>
    public static bool Update()
    {
        var result = false;
        if (!IsRunning)
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
    /// Downloads the lastet mapping file and updates the maps.
    /// This method does not block the calling thread.
    /// </summary>
    public static void UpdateAsync()
    {
        // No need to run if currently running.
        if (IsRunning)
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
    /// Downloads the lastet mapping file and persists it to a file.
    /// This method does not block the calling thread.
    /// </summary>
    /// <returns>Whether a newer version of the mapping file was successfully downloaded.</returns>
    private static async Task<bool> GetJsonDownloadAsync()
    {
        var content = await GithubService.DownloadMappingJsonAsync();
        if (string.IsNullOrEmpty(content))
            return false;

        Directory.CreateDirectory(new FileInfo(_path).DirectoryName!);
        // File does not matter until next startup and therefore no need to wait.
        try
        {
#if NETSTANDARD2_0
            _ = Task.Run(() => File.WriteAllText(_path, content));
#else
            _ = File.WriteAllTextAsync(_path, content);
#endif
        }
        catch (IOException) { } // Try again next time.

        _jsonDownload = MappingJson.Deserialize(content!);

        return true;
    }

    #endregion
}
