using System.Globalization;
using System.Text.Json;

namespace Bingir;

/// <summary>
/// Client that implements logic to fetch images from Bing servers.
/// </summary>
public class BingImageClient : IDisposable
{
    /// <summary>
    /// The maximum number of images' metadata that will be fetched when
    /// calling <see cref="GetLastNImagesMetadataAsync(int)"/>.
    ///
    /// Bing will only return the last 14 days' worth of images, even
    /// with indexed batching.
    /// </summary>
    public const int MaxImagesToFetch = 14;

    /// <summary>
    /// The Bing API will only return at most 7 images' metadata in one call to
    /// the HPImageArchive endpoint. It is possible to go further back into the
    /// archive by setting an index value on the query string.
    ///
    /// This constant represents that limit and is used for batching.
    /// </summary>
    private const int ImageMetadataFetchBatchSize = 7;

    private HttpClient HttpClient { get; } = new HttpClient();

    /// <summary>
    /// Retreives the metadata for the latest Bing image-of-the-day from Bing servers.
    /// </summary>
    /// <returns>The metadata for the latest Bing image.</returns>
    public async Task<ImageMetadata> GetCurrentImageMetadataAsync() =>
        (await this.GetLastNImagesMetadataAsync(1)).FirstOrDefault();

    /// <summary>
    /// Retreives the metadata for the latest N Bing images-of-the-day from Bing servers.
    /// The metadata is returned in order from the oldest image to the newest.
    /// </summary>
    /// <param name="n">
    /// The number of images to retrieve metadata for.
    /// This value must be between 1 and <see cref="MaxImagesToFetch"/>.
    /// </param>
    /// <returns>The metadata for the latest N Bing images.</returns>
    public async Task<IEnumerable<ImageMetadata>> GetLastNImagesMetadataAsync(int n)
    {
        if (n < 1 || n > MaxImagesToFetch)
        {
            throw new ArgumentOutOfRangeException(nameof(n), $"The value of 'n' must be in the range [1-{MaxImagesToFetch}]");
        }

        var bingResponseImageMetadata = new List<Image>();
        var index = 0;

        while (n > 0)
        {
            var batchSize = Math.Min(ImageMetadataFetchBatchSize, n);

            // URI to the Bing image-of-the-day metadata.
            // The parameters specify to return the response as JSON, and that we want the latest n images for en-US market.
            string bingMetadataUri = $"https://www.bing.com/HPImageArchive.aspx?format=js&idx={index}&n={batchSize}&mkt=en-US";

            var response = await this.HttpClient.GetAsync(bingMetadataUri);
            response.EnsureSuccessStatusCode();

            var responseContentStream = await response.Content.ReadAsStreamAsync();
            var responseMetadata = JsonSerializer.Deserialize<BingImageMetadataResponse>(responseContentStream);

            bingResponseImageMetadata.AddRange(responseMetadata.images);

            n -= batchSize;
            index += batchSize;
        }

        return bingResponseImageMetadata
            .Select(m => m.ConvertToImageMetadata())
            .OrderBy(m => m.BingStartDate);
    }

    /// <summary>
    /// Downloads the image file from Bing servers and stores it at the given path.
    /// </summary>
    /// <param name="image">The metadata for the image to download.</param>
    /// <param name="path">The path to download the image file to.</param>
    /// <returns>A task that indicates completion of the action.</returns>
    public async Task DownloadImageToAsync(ImageMetadata image, string path)
    {
        using var fileStream = File.OpenWrite(path);

        using var imageStream = await this.HttpClient.GetStreamAsync(image.DefaultHDUri);
        await imageStream.CopyToAsync(fileStream);

        imageStream.Close();
        fileStream.Close();
    }

    /// <summary>
    /// Helper to extract the image's ID from the 'id' query string parameter in the given URI.
    /// </summary>
    /// <param name="uri">The URI to extract the ID from.</param>
    /// <returns>The image's ID.</returns>
    private static string ExtractImageIdFromUri(Uri uri) =>
        uri.Query
            .TrimStart('?')
            .Split('&')
            .FirstOrDefault(s => s.StartsWith("id=", StringComparison.OrdinalIgnoreCase))?
            .Substring(3)
        ?? String.Empty;

    #region Bing Metadata DTOs

    private class BingImageMetadataResponse
    {
        public Image[] images { get; set; }
    }

    private class Image
    {
        // Contains a date in the form yyyyMMdd when the image was posted.
        public string startdate { get; set; }

        // Despite the name, this only contains the path and query string
        // to retrieve the image from host https://bing.com .
        // This url requests a default 1920x1080 image.
        public string url { get; set; }

        // Like 'url', this is only the path and query string.
        // This version won't resolve, it needs to have a resolution selection
        // appended to it.
        // See doc comments on ImageMetadata.BaseUri for details.
        public string urlbase { get; set; }

        // Copyright info, also including a brief description of the image.
        public string copyright { get; set; }

        // Image title given by the artist.
        public string title { get; set; }

        public ImageMetadata ConvertToImageMetadata()
        {
            var defaultHDImageUri = new Uri($"https://bing.com{this.url}");

            return new ImageMetadata
            {
                BingStartDate = DateTime.SpecifyKind(
                    DateTime.ParseExact(
                        this.startdate,
                        "yyyyMMdd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None),
                    DateTimeKind.Utc),
                DiscoverTime = DateTime.UtcNow,
                Id = ExtractImageIdFromUri(defaultHDImageUri),
                Title = this.title,
                Copyright = this.copyright,
                DefaultHDUri = defaultHDImageUri,
                BaseUri = new Uri($"https://bing.com{this.urlbase}"),
            };
        }
    }

    #endregion

    #region IDisposable Support

    private bool isDisposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            if (disposing)
            {
                this.HttpClient?.Dispose();
            }

            this.isDisposed = true;
        }
    }

    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
