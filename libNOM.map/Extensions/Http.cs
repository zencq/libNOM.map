namespace libNOM.map.Extensions;


public static class HttpExtensions
{
    /// <summary>
    /// Downloads the resource with the specified URI to a local file.
    /// This method does not block the calling thread.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="address">The URI from which to download data.</param>
    /// <param name="file">The name of the local file that is to receive the data.</param>
    /// <returns></returns>
    public static async Task<string> DownloadFileAsync(this HttpClient input, string address, string path)
    {
        var file = new FileInfo(path);
        Directory.CreateDirectory(file.DirectoryName!);

        var response = await input.GetAsync(address);
        response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = File.Create(file.FullName);

        contentStream.Seek(0, SeekOrigin.Begin);
        contentStream.CopyTo(fileStream);

        contentStream.Seek(0, SeekOrigin.Begin);
        return new StreamReader(contentStream).ReadToEnd();
    }
}
