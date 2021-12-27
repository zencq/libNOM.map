using System.Reflection;
using libNOM.map.Extensions;
using Newtonsoft.Json.Linq;
using Serilog;

namespace libNOM.map;


/// <summary>
/// Holds all necessary mapping data and provides obfuscation and deobfuscation functions.
/// </summary>
public class Mapping
{
    #region Constant

    private const string DIRECTORY = "download";
    private const string FILE = "mapping.json";
    private const string PATH = $"{DIRECTORY}/{FILE}";

    #endregion

    #region Member

    private readonly Dictionary<string, string> MapForDeobfuscation = new();

    private readonly Dictionary<string, string> MapForObfuscation = new();

    private readonly MappingJson JsonCompiler; // lastet MBINCompiler mapping.json when this version created

    private MappingJson? JsonDownload; // dynamic content from the latest MBINCompiler release on GitHub

    private readonly MappingJson JsonLegacy; // older keys that are not present in the latest version

    private readonly MappingJson JsonWizard; // adjust differing mapping of SaveWizard

    #endregion

    #region Singleton

    private static readonly Lazy<Mapping> _Lazy = new(() => new Mapping());

    /// <summary>
    /// Instance that has all necessary data to obfuscate and deobfuscate a file.
    /// </summary>
    public static Mapping Instance => _Lazy.Value; // { get; }

    #endregion

    #region Contructor

#pragma warning disable CS8601, CS8618 // As we are loading resources those will not be null.
    private Mapping()
    {
        JsonCompiler = MappingJson.Deserialize(Properties.Resources.MBINCompiler);
        JsonLegacy = MappingJson.Deserialize(Properties.Resources.Legacy);
        JsonWizard = MappingJson.Deserialize(Properties.Resources.SaveWizard);
#pragma warning restore CS8601, CS8618
        if (File.Exists(PATH))
        {
            JsonDownload = MappingJson.Deserialize(File.ReadAllText(PATH));
        }

        CreateMap();
    }

    #endregion

    #region Create Map

    /// <summary>
    /// Create maps with the mapping data of all files for obfuscation and deobfuscation.
    /// </summary>
    private void CreateMap()
    {
        MapForDeobfuscation.Clear();
        MapForObfuscation.Clear();

        AddToMap(JsonCompiler, true, true);
        AddToMap(JsonLegacy, true, true);
        AddToMap(JsonWizard, true, false);

        // Apply additional mapping but keep those that might have become outdated by adding JsonCompiler nonetheless.
        if (JsonDownload?.Version > JsonCompiler.Version)
        {
            AddToMap(JsonDownload, true, true);
        }
    }

    /// <summary>
    /// Add mapping data of a single file to the maps for obfuscation and deobfuscation.
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
                MapForDeobfuscation[pair.Key] = pair.Value;
            }
            if (obfuscate)
            {
                MapForObfuscation[pair.Value] = pair.Key;
            }
        }
    }

    #endregion

    #region Update Map

    /// <summary>
    /// Download the mapping file from the lastet MBINCompiler release on GitHub and create new maps.
    /// </summary>
    public void Update()
    {
        var task = DownloadAsync();
        task.Wait();
        if (task.Result)
        {
            CreateMap();
        }
    }

    /// <summary>
    /// Check the lastet MBINCompiler release on GitHub and download the mapping file if it is newer.
    /// </summary>
    /// <returns>Whether a newer version of the mapping file was successfully downloaded.</returns>
    private async Task<bool> DownloadAsync()
    {
        var name = Assembly.GetEntryAssembly()?.GetName().Name ?? Assembly.GetExecutingAssembly().GetName().Name;
        var githubClient = new Octokit.GitHubClient(new Octokit.ProductHeaderValue(name));

        // Get the latest release from GitHub.
        var release = await githubClient.Repository.Release.GetLatest("monkeyman192", "MBINCompiler");
        if (release is null)
            return false;

        // Convert "v3.75.0-pre1" to "3.75.0.1" and check whether it is worth it to download the mapping file of the release. 
        var version = new Version(release.TagName[1..].Replace("-pre", "."));
        if (version <= JsonCompiler.Version)
            return false;

        // Find the mapping.json asset to download it.
        var asset = release.Assets.FirstOrDefault(a => a.Name.Equals(FILE));
        if (asset is null)
            return false;

        Directory.CreateDirectory(DIRECTORY);

        // Download the mapping.json and save it to disk.
        using var httpClient = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, asset.BrowserDownloadUrl);
        using Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync();
        using Stream fileStream = new FileStream(PATH, FileMode.Create, FileAccess.Write, FileShare.None);
        await contentStream.CopyToAsync(fileStream);

        Log.Information($"Downloaded {version} of {FILE}");

        // Deserialize the downloaded file. 
        contentStream.Position = 0;
        JsonDownload = MappingJson.Deserialize(new StreamReader(contentStream).ReadToEnd());

        return true;
    }

    #endregion

    #region Deobfuscate

    /// <summary>
    /// Deobfuscate a file to make it human-readable.
    /// </summary>
    /// <param name="root">Entire file as JObject.</param>
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
            jProperty.Rename(MapForDeobfuscation[jProperty.Name]);
        }

        return keys;
    }

    /// <summary>
    /// Iterate over all JProperty to collect a list for deobfuscation.
    /// </summary>
    /// <param name="token">Current JToken that should be deobfuscated.</param>
    /// <param name="jProperties">List of JProperty that need to be deobfuscated.</param>
    /// <param name="keys">List of keys that cannot be deobfuscated.</param>
    private void CollectForDeobfuscation(JToken token, List<JProperty> jProperties, HashSet<string> keys)
    {
        if (token.Type == JTokenType.Property)
        {
#pragma warning disable CS8602 // As Type is JTokenType.Property we can be sure that "token as JProperty" is not null.
            var property = token as JProperty;
            if (MapForDeobfuscation.ContainsKey(property.Name))
            {
                jProperties.Add(property);
            }
            else if (!MapForDeobfuscation.ContainsValue(property.Name))
            {
                keys.Add(property.Name);
            }
#pragma warning restore CS8602
        }
        foreach (var child in token.Children().Where(c => c.HasValues))
        {
            CollectForDeobfuscation(child, jProperties, keys);
        }
    }

    #endregion

    #region Obfuscate

    /// <summary>
    /// Obfuscate a file to make it readable by the game again.
    /// </summary>
    /// <param name="root">Entire file as JObject.</param>
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
            jProperty.Rename(MapForObfuscation[jProperty.Name]);
        }
    }

    /// <summary>
    /// Iterate over all JProperty to collect a list for obfuscation.
    /// </summary>
    /// <param name="token">Current JToken that should be obfuscated.</param>
    /// <param name="jProperties">List of JProperty that need to be obfuscated.</param>
    private void CollectForObfuscation(JToken token, List<JProperty> jProperties)
    {
        if (token.Type == JTokenType.Property)
        {
#pragma warning disable CS8602 // As Type is JTokenType.Property we can be sure that "token as JProperty" is not null.
            var property = token as JProperty;
            if (MapForObfuscation.ContainsKey(property.Name))
            {
                jProperties.Add(property);
            }
#pragma warning restore CS8602
        }
        foreach (var child in token.Children().Where(c => c.HasValues))
        {
            CollectForObfuscation(child, jProperties);
        }
    }

    #endregion
}
