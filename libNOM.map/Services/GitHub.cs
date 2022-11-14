using Octokit;
using System.Reflection;

namespace libNOM.map.Services;


/// <summary>
/// Specialised client for GitHub download the mapping file.
/// </summary>
internal class GitHubService
{
    #region Field

    private GitHubClient? _gitHubClient;
    private HttpClient? _httpClient;

    #endregion

    #region Property

    private GitHubClient GitHubClient => _gitHubClient ??= new(new ProductHeaderValue(Assembly.GetExecutingAssembly().GetName().Name));

    private HttpClient HttpClient => _httpClient ??= new();

    #endregion

    // //

    internal GitHubService() { }

    // //

    /// <summary>
    /// Downloads the lastet mapping file.
    /// This method does not block the calling thread.
    /// </summary>
    /// <returns>File content as string.</returns>
    internal async Task<string?> DownloadMappingJsonAsync()
    {
        // Get the latest release from GitHub.
        Release? release = null;
        try
        {
            release = await GitHubClient.Repository.Release.GetLatest(Properties.Resources.REPO_OWNER, Properties.Resources.REPO_NAME);
        }
        catch (RateLimitExceededException) { } // Too many unauthenticated requests (60 per hour). Try again next time.
        if (release is null)
            return null;

        // Find the asset to download it.
        var result = release.Assets.FirstOrDefault(a => a.Name.Equals(Properties.Resources.RELEASE_ASSET));
        if (result is null)
            return null;

        var response = await HttpClient.GetAsync(result.BrowserDownloadUrl);
        response.EnsureSuccessStatusCode();

        using var contentStream = await response.Content.ReadAsStreamAsync();
        return new StreamReader(contentStream).ReadToEnd();
    }
}
