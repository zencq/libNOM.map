using Octokit;
using System.Reflection;

namespace libNOM.map.Services;


/// <summary>
/// Specialized client for GitHub to download the mapping file.
/// </summary>
internal class GithubService
{
    #region Field

    private GitHubClient? _githubClient;
    private HttpClient? _httpClient;

    #endregion

    #region Property

    private GitHubClient GitHubClient => _githubClient ??= new(new ProductHeaderValue(Assembly.GetExecutingAssembly().GetName().Name));

    private HttpClient HttpClient => _httpClient ??= new();

    #endregion

    // //

    internal GithubService() { }

    // //

    /// <summary>
    /// Downloads the lastet mapping file.
    /// This method does not block the calling thread.
    /// </summary>
    /// <returns>File content as string.</returns>
    internal async Task<string?> DownloadMappingJsonAsync()
    {
        // Get the latest release from GitHub.
        Release release;
        try
        {
            release = await GitHubClient.Repository.Release.GetLatest(Properties.Resources.REPO_OWNER, Properties.Resources.REPO_NAME);
        }
        catch (Exception ex) when (ex is HttpRequestException or RateLimitExceededException or TaskCanceledException) // Rate limit is 60 unauthenticated requests per hour.
        {
            return null;
        }

        // Find the asset to download.
        ReleaseAsset? result = release.Assets.FirstOrDefault(i => i.Name.Equals(Properties.Resources.RELEASE_ASSET));
        if (result is null)
            return null;

        // Download the asset from GitHub.
        try
        {
            using HttpResponseMessage response = await HttpClient.GetAsync(result.BrowserDownloadUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return null;
        }
    }
}
