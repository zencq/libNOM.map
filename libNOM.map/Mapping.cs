using libNOM.map.Extensions;
using libNOM.map.Json;
using Newtonsoft.Json.Linq;

namespace libNOM.map;


/// <summary>
/// Holds all necessary mapping data and provides obfuscation and deobfuscation.
/// </summary>
public class Mapping
{
    #region Field

    private readonly HttpClient _httpClient = new();

    private readonly MappingJson _jsonCompiler; // latest MBINCompiler mapping.json when this version was created

    private MappingJson? _jsonDownload; // dynamic content from the latest MBINCompiler release on GitHub

    private readonly MappingJson _jsonLegacy; // older keys that are not present in the latest version

    private readonly MappingJson _jsonWizard; // adjust differing mapping of SaveWizard

    private readonly Dictionary<string, string> _mapForDeobfuscation = new();

    private readonly Dictionary<string, string> _mapForObfuscation = new();

    private string _path;

    private MappingSettings _settings = new();

    private Task? _updateTask;

    #endregion

    #region Property

    private bool IsRunning => !_updateTask?.IsCompleted ?? false; // { get; }

    public MappingSettings? Settings // { get; set; }
    {
        get => _settings;
        set
        {
            _settings = value ?? new();
            _path = GetCombinedPath();
        }
    }

    #endregion

    #region Singleton

    private static readonly Lazy<Mapping> _lazy = new(() => new());

    public static Mapping Instance => _lazy.Value; // { get; }

    #endregion

    #region Contructor

    private Mapping()
    {
        _jsonCompiler = MappingJson.Deserialize(Properties.Resources.MBINCompiler)!;
        _jsonLegacy = MappingJson.Deserialize(Properties.Resources.Legacy)!;
        _jsonWizard = MappingJson.Deserialize(Properties.Resources.SaveWizard)!;
        _path = GetCombinedPath();

        if (File.Exists(_path))
        {
            _jsonDownload = MappingJson.Deserialize(File.ReadAllText(_path));
        }

        CreateMap();
    }

    #endregion

    // //

    #region Create

    /// <summary>
    /// Creates maps with the mapping data of all files for obfuscation and deobfuscation.
    /// </summary>
    private void CreateMap()
    {
        _mapForDeobfuscation.Clear();
        _mapForObfuscation.Clear();

        AddToMap(_jsonCompiler, true, true);
        AddToMap(_jsonLegacy, true, true);
        AddToMap(_jsonWizard, true, false);

        // Apply additional mapping but keep those that might have become outdated by adding JsonCompiler nonetheless.
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
    private void AddToMap(MappingJson mappingJson, bool deobfuscate, bool obfuscate)
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

    #region Deobfuscate

    /// <summary>
    /// Deobfuscates a file to make it human-readable.
    /// </summary>
    /// <param name="root">Entire file as JSON object.</param>
    /// <returns>List of unknown keys.</returns>
    public HashSet<string> Deobfuscate(JObject root)
    {
        // Wait in case of currently running update.
        _updateTask?.Wait();

        var jProperties = new List<JProperty>();
        var keys = new HashSet<string>();

        // Collect all jProperties that need to be renamed.
        foreach (var child in root.Children().Where(c => c.HasValues))
        {
            CollectForDeobfuscation(child, jProperties, keys);
        }

        // Actually rename each jProperty.
        foreach (var jProperty in jProperties)
        {
            jProperty.Rename(_mapForDeobfuscation[jProperty.Name]);
        }

        return keys;
    }

    /// <summary>
    /// Iterates over all JSON properties to collect a list for deobfuscation.
    /// </summary>
    /// <param name="token">Current property that should be deobfuscated.</param>
    /// <param name="jProperties">List of properties that need to be deobfuscated.</param>
    /// <param name="keys">List of keys that cannot be deobfuscated.</param>
    private void CollectForDeobfuscation(JToken token, List<JProperty> jProperties, HashSet<string> keys)
    {
        if (token.Type == JTokenType.Property)
        {
            var property = token as JProperty;
            if (_mapForDeobfuscation.ContainsKey(property!.Name))
            {
                jProperties.Add(property);
            }
            // Only add if it is not a target value as well.
            else if (!_mapForDeobfuscation.ContainsValue(property!.Name))
            {
                keys.Add(property!.Name);
            }
        }
        foreach (var child in token.Children().Where(c => c.HasValues))
        {
            CollectForDeobfuscation(child, jProperties, keys);
        }
    }

    #endregion

    #region Getter

    /// <summary>
    /// Combines the download path from the settings with the filename.
    /// </summary>
    /// <returns></returns>
    private string GetCombinedPath()
    {
        return Path.Combine(Path.GetFullPath(_settings.PathDownload), Properties.Resources.RELEASE_ASSET);
    }

    #endregion

    #region Obfuscate

    /// <summary>
    /// Obfuscates a file to make it readable by the game.
    /// </summary>
    /// <param name="root">Entire file as JSON object.</param>
    public void Obfuscate(JObject root)
    {
        // Wait in case of currently running update.
        _updateTask?.Wait();

        var jProperties = new List<JProperty>();

        // Collect all jProperties that need to be renamed.
        foreach (var child in root.Children().Where(c => c.HasValues))
        {
            CollectForObfuscation(child, jProperties);
        }

        // Actually rename each jProperty.
        foreach (var jProperty in jProperties)
        {
            jProperty.Rename(_mapForObfuscation[jProperty.Name]);
        }
    }

    /// <summary>
    /// Iterates over all JSON properties to collect a list for obfuscation.
    /// </summary>
    /// <param name="token">Current property that should be obfuscated.</param>
    /// <param name="jProperties">List of properties that need to be obfuscated.</param>
    private void CollectForObfuscation(JToken token, List<JProperty> jProperties)
    {
        if (token.Type == JTokenType.Property)
        {
            var property = token as JProperty;
            if (_mapForObfuscation.ContainsKey(property!.Name))
            {
                jProperties.Add(property);
            }
        }
        foreach (var child in token.Children().Where(c => c.HasValues))
        {
            CollectForObfuscation(child, jProperties);
        }
    }

    #endregion

    #region Update

    /// <summary>
    /// Downloads the lastet mapping file and updates the maps.
    /// </summary>
    /// <returns>Whether a newer version of the mapping file was successfully downloaded.</returns>
    public bool Update()
    {
        var result = false;
        if (!IsRunning)
        {
            _updateTask = Task.Run(async () =>
            {
                result = await DownloadAsync();
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
    public void UpdateAsync()
    {
        // No need to run if currently running.
        if (IsRunning)
            return;

        _updateTask = Task.Run(async () =>
        {
            var result = await DownloadAsync();
            if (result)
            {
                CreateMap();
            }
        });
    }

    /// <summary>
    /// Downloads the lastet mapping file.
    /// This method does not block the calling thread.
    /// </summary>
    /// <returns>Whether a newer version of the mapping file was successfully downloaded.</returns>
    private async Task<bool> DownloadAsync()
    {
        var content = await _httpClient.DownloadTextFileContentFromGitHubReleaseAsync(Properties.Resources.REPO_OWNER, Properties.Resources.REPO_NAME, Properties.Resources.RELEASE_ASSET);
        if (string.IsNullOrEmpty(content))
            return false;

        Directory.CreateDirectory(new FileInfo(_path).DirectoryName!);
        _ = File.WriteAllTextAsync(_path, content);

        _jsonDownload = MappingJson.Deserialize(content);

        return true;
    }

    #endregion
}
