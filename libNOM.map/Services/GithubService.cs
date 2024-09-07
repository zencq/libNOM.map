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

    private GitHubClient GithubClient => _githubClient ??= new(new ProductHeaderValue(Assembly.GetExecutingAssembly().GetName().Name));

    private HttpClient HttpClient => _httpClient ??= new();

    #endregion

    #region Constructor

    internal GithubService() { }

    #endregion

    // //

    /// <summary>
    /// Downloads the latest mapping file.
    /// This method does not block the calling thread.
    /// </summary>
    /// <param name="prerelease"></param>
    /// <returns>File content as string.</returns>
    internal async Task<string?> DownloadMappingJsonAsync(bool prerelease)
    {
        // Rate limit is 60 unauthenticated requests per hour.
        if ((await GetRateLimit())?.Remaining > 0)
        {
            try
            {
                // Get the latest release from GitHub. To include pre-releases, use GetAll instead of GetLatest.
                var release = prerelease
                    ? (await GithubClient.Repository.Release.GetAll(Properties.Resources.REPO_OWNER, Properties.Resources.REPO_NAME, new() { PageCount = 1, PageSize = 1 }))[0] // only get one as we only need the latest
                    : (await GithubClient.Repository.Release.GetLatest(Properties.Resources.REPO_OWNER, Properties.Resources.REPO_NAME));

                // Get the asset to download. We assume that it exists, as it is very unlikely to change in the foreseeable future.
                var result = release.Assets.First(i => i.Name.Equals(Properties.Resources.RELEASE_ASSET));

                // Download the asset from GitHub.
                return await HttpClient.GetStringAsync(result.BrowserDownloadUrl);
            }
            catch (Exception ex) when (ex is HttpRequestException) { }
        }

        return null;
    }

    private async Task<RateLimit?> GetRateLimit()
    {
        try
        {
            return GithubClient.GetLastApiInfo()?.RateLimit ?? (await GithubClient.RateLimit.GetRateLimits()).Rate;
        }
        catch (Exception ex) when (ex is HttpRequestException)
        {
            return null;
        }
    }
}
