using libNOM.map.Data;
using libNOM.map.Extensions;
using libNOM.map.Services;

using Newtonsoft.Json.Linq;

namespace libNOM.map;


/// <summary>
/// Holds all necessary mapping data and provides obfuscation and deobfuscation.
/// </summary>
public static partial class Mapping
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

    /// <summary>
    /// Used mapping version. Either the downloaded one if exists or built-in.
    /// </summary>
    public static Version Version => _jsonDownload?.Version ?? _jsonCompiler.Version; // { get; }

    // private

    private static string CombinedPath => Path.Combine(Path.GetFullPath(Settings.DownloadDirectory), Properties.Resources.RELEASE_ASSET); // { private get; }

    private static GithubService GithubService => _githubService ??= new(); // { private get; }

    private static bool IsUpdateRunning => !_updateTask?.IsCompleted ?? false; // { private get; }

    private static MappingSettings Settings { get; set; } = new();

    #endregion

    #region Accessor

    public static MappingSettings GetSettings() => Settings;

    public static void SetSettings(MappingSettings settings)
    {
        Settings = settings;

        // Check whether there is already a file in the new path.
        if (File.Exists(CombinedPath))
            ReloadExistingFile();
    }

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

    #region Mapping

    /// <inheritdoc cref="GetMappedKeyOrInput(string, bool)"/>
    public static string GetMappedKeyOrInput(string key) => GetMappedKeyOrInput(key, false);

    /// <summary>
    /// Maps the specified key. Works for both, deobfuscated and obfuscated input.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="useAccount"></param>
    /// <returns>The mapped key or the input if no mapping found.</returns>
    public static string GetMappedKeyOrInput(string key, bool useAccount)
    {
        if (GetMapForDeobfuscation(useAccount).TryGetValue(key, out var resultFromDeobfuscation))
            return resultFromDeobfuscation;

        if (GetMapForObfuscation(useAccount).TryGetValue(key, out var resultFromObfuscation))
            return resultFromObfuscation;

        return key;
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

    #endregion

    #region Update

    /// <inheritdoc cref="UpdateAsync"/>
    public static bool Update()
    {
        var updateTask = UpdateAsync();
        updateTask.Wait();
        return updateTask.Result;
    }

    /// <summary>
    /// Downloads the latest mapping file and updates the maps.
    /// </summary>
    /// <returns>Whether a newer version of the mapping file was successfully downloaded.</returns>
    public static async Task<bool> UpdateAsync()
    {
        var result = false;
        if (!IsUpdateRunning) // no need to run if currently running
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
        await _updateTask!; // in case it was running, assume not newer
        return result;
    }

    /// <summary>
    /// Downloads the latest mapping file and persists it to a file.
    /// This method does not block the calling thread.
    /// </summary>
    /// <returns>Whether a newer version of the mapping file was successfully downloaded.</returns>
    private static async Task<bool> GetJsonDownloadAsync()
    {
        var content = await GithubService.DownloadMappingJsonAsync(Settings.IncludePrerelease);

        // Use existing download file as fallback if it has not been loaded for some reason.
        ReloadExistingFile();

        if (!string.IsNullOrEmpty(content) && MappingJson.Deserialize(content!) is MappingJson download && download.Version > Version)
        {
            // Write file only if downloaded mapping is newer than current one.
            Directory.CreateDirectory(Settings.DownloadDirectory);
            try
            {
                // File does not matter until next startup and therefore no need to wait.
#if NETSTANDARD2_0
                _ = Task.Run(() => File.WriteAllText(CombinedPath, content));
#else
                _ = File.WriteAllTextAsync(CombinedPath, content);
#endif
            }
            catch (IOException) { } // Try again next time.

            _jsonDownload = download;

            return true;
        }

        return false;
    }

    private static void ReloadExistingFile()
    {
        // Use existing download file as fallback if it has not been loaded for some reason.
        if (_jsonDownload is null && File.Exists(CombinedPath) && MappingJson.Deserialize(File.ReadAllText(CombinedPath)) is MappingJson existing && existing.Version > Version)
            _jsonDownload = existing;
    }

    #endregion
}
