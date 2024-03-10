using System.Reflection;

using Octokit;

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
    /// <param name="prerelease"></param>
    /// <returns>File content as string.</returns>
    internal async Task<string?> DownloadMappingJsonAsync(bool prerelease)
    {
        try
        {
            // Get the latest release from GitHub. To include prereleases, use GetAll instead of GetLatest.
            var release = prerelease
                ? (await GitHubClient.Repository.Release.GetAll(Properties.Resources.REPO_OWNER, Properties.Resources.REPO_NAME, new() { PageCount = 1, PageSize = 1 }))[0] // only get one as we only need the latest
                : (await GitHubClient.Repository.Release.GetLatest(Properties.Resources.REPO_OWNER, Properties.Resources.REPO_NAME));

            // Get the asset to download. We assume that it exists, as it is very unlikely to change in the foreseeable future.
            var result = release.Assets.First(i => i.Name.Equals(Properties.Resources.RELEASE_ASSET));

            // Download the asset from GitHub.
            using var response = await HttpClient.GetAsync(result.BrowserDownloadUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        // Rate limit is 60 unauthenticated requests per hour.
        catch (Exception ex) when (ex is HttpRequestException or RateLimitExceededException)
        {
            return null;
        }
    }
}
