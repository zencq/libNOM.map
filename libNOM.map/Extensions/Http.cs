using Octokit;
using System.Reflection;

namespace libNOM.map.Extensions;


public static class HttpExtensions
{
    /// <summary>
    /// Downloads the specified asset from the latest GitHub release of the specified repository.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="owner"></param>
    /// <param name="name"></param>
    /// <param name="asset"></param>
    /// <returns>File content as string.</returns>
    public static async Task<string> DownloadTextFileContentFromGitHubReleaseAsync(this HttpClient input, string owner, string name, string asset)
    {
        var userAgent = Assembly.GetExecutingAssembly().GetName().Name;
        var githubClient = new GitHubClient(new ProductHeaderValue(userAgent));

        // Get the latest release from GitHub.
        var release = await githubClient.Repository.Release.GetLatest(owner, name);
        if (release is null)
            return string.Empty;

        // Find the asset to download it.
        var result = release.Assets.FirstOrDefault(a => a.Name.Equals(asset));
        if (result is null)
            return string.Empty;

        var response = await input.GetAsync(result.BrowserDownloadUrl);
        response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync();
        return new StreamReader(contentStream).ReadToEnd();
    }
}
