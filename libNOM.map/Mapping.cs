using libNOM.map.Extensions;
using libNOM.map.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace libNOM.map;


/// <summary>
/// Holds all necessary mapping data and provides obfuscation and deobfuscation.
/// </summary>
public class Mapping
{
    #region Constant

    private const string FILE = "mapping.json";

    #endregion

    #region Member

    private readonly HttpClient _HttpClient = new();

    private readonly MappingJson _JsonCompiler; // latest MBINCompiler mapping.json when this version was created

    private MappingJson? _JsonDownload; // dynamic content from the latest MBINCompiler release on GitHub

    private readonly MappingJson _JsonLegacy; // older keys that are not present in the latest version

    private readonly MappingJson _JsonWizard; // adjust differing mapping of SaveWizard

    private readonly Dictionary<string, string> _MapForDeobfuscation = new();

    private readonly Dictionary<string, string> _MapForObfuscation = new();

    private string _Path;

    private MappingSettings _Settings = new();

    #endregion

    #region Property

    public Task? UpdateTask { get; private set; }

    #endregion

    #region Singleton

    private static readonly Lazy<Mapping> _Lazy = new(() => new Mapping());

    public static Mapping Instance => _Lazy.Value; // { get; }

    #endregion

    #region Getter

    /// <summary>
    /// Combines the download path from the settings with the filename.
    /// </summary>
    /// <returns></returns>
    private string GetCombinedPath()
    {
        return Path.Combine(_Settings.PathDownload, FILE);
    }

    #endregion

    #region Setter

    /// <summary>
    /// Updates the instance with the new configuration.
    /// </summary>
    public void SetSettings(MappingSettings mappingSettings)
    {
        _Settings = mappingSettings ?? new();
        _Path = GetCombinedPath();
    }

    #endregion

    #region Contructor

    private Mapping()
    {
        _JsonCompiler = MappingJson.Deserialize(Properties.Resources.MBINCompiler)!;
        _JsonLegacy = MappingJson.Deserialize(Properties.Resources.Legacy)!;
        _JsonWizard = MappingJson.Deserialize(Properties.Resources.SaveWizard)!;
        _Path = GetCombinedPath();

        if (File.Exists(_Path))
        {
            _JsonDownload = MappingJson.Deserialize(File.ReadAllText(_Path));
        }

        CreateMap();
    }

    #endregion

    // //

    #region Create Map

    /// <summary>
    /// Creates maps with the mapping data of all files for obfuscation and deobfuscation.
    /// </summary>
    private void CreateMap()
    {
        _MapForDeobfuscation.Clear();
        _MapForObfuscation.Clear();

        AddToMap(_JsonCompiler, true, true);
        AddToMap(_JsonLegacy, true, true);
        AddToMap(_JsonWizard, true, false);

        // Apply additional mapping but keep those that might have become outdated by adding JsonCompiler nonetheless.
        if (_JsonDownload?.Version > _JsonCompiler.Version)
        {
            AddToMap(_JsonDownload, true, true);
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
                _MapForDeobfuscation[pair.Key] = pair.Value;
            }
            if (obfuscate)
            {
                _MapForObfuscation[pair.Value] = pair.Key;
            }
        }
    }

    #endregion

    #region Update Map

    /// <summary>
    /// Downloads the lastet mapping file and updates the maps.
    /// </summary>
    /// <returns>Whether a newer version of the mapping file was successfully downloaded.</returns>
    public bool Update()
    {
        var result = false;
        if (UpdateTask is null || UpdateTask.IsCompleted)
        {
            UpdateTask = Task.Run(async () =>
            {
                result = await DownloadAsync();
                if (result)
                {
                    CreateMap();
                }
            });
        }
        UpdateTask.Wait(); // in case of doubt not newer
        return result;
    }

    /// <summary>
    /// Downloads the lastet mapping file and updates the maps.
    /// This method does not block the calling thread.
    /// </summary>
    public void UpdateAsync()
    {
        UpdateTask = Task.Run(async () =>
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
        var name = Assembly.GetEntryAssembly()?.GetName().Name ?? Assembly.GetExecutingAssembly().GetName().Name;
        var githubClient = new Octokit.GitHubClient(new Octokit.ProductHeaderValue(name));

        // Get the latest release from GitHub.
        var release = await githubClient.Repository.Release.GetLatest(Properties.Resources.REPO_OWNER, Properties.Resources.REPO_NAME);
        if (release is null)
            return false;

#if RELEASE
        // Convert "v3.75.0-pre1" to "3.75.0.1" and check whether it is worth it to download the mapping file of the release.
        var version = new Version(release.TagName[1..].Replace("-pre", "."));
        if (version <= _JsonCompiler.Version && name != "testhost") // UnitTesting
            return false;
#endif

        // Find the mapping.json asset to download it.
        var asset = release.Assets.FirstOrDefault(a => a.Name.Equals(FILE));
        if (asset is null)
            return false;

        var json = await _HttpClient.DownloadFileAsync(asset.BrowserDownloadUrl, _Path);
        _JsonDownload = MappingJson.Deserialize(json);

        return true;
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
            jProperty.Rename(_MapForDeobfuscation[jProperty.Name]);
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
            if (_MapForDeobfuscation.ContainsKey(property!.Name))
            {
                jProperties.Add(property);
            }
            // Only add if it is not a target value as well.
            else if (!_MapForDeobfuscation.ContainsValue(property!.Name))
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

    #region Obfuscate

    /// <summary>
    /// Obfuscates a file to make it readable by the game.
    /// </summary>
    /// <param name="root">Entire file as JSON object.</param>
    public void Obfuscate(JObject root)
    {
        var jProperties = new List<JProperty>();

        // Collect all jProperties that need to be renamed.
        foreach (var child in root.Children().Where(c => c.HasValues))
        {
            CollectForObfuscation(child, jProperties);
        }

        // Actually rename each jProperty.
        foreach (var jProperty in jProperties)
        {
            jProperty.Rename(_MapForObfuscation[jProperty.Name]);
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
            if (_MapForObfuscation.ContainsKey(property!.Name))
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
}
