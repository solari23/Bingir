using System.Text.Json;
using System.Text.Json.Serialization;
using Bingir.ImageMutation;

namespace Bingir;

/// <summary>
/// Provides local caching utility to store Bing images-of-the-day.
/// </summary>
public class ImageCache
{
    private const string CacheDataFileName = "Cache.dat";

    /// <summary>
    /// Opens access to the image cache per the application config.
    /// </summary>
    /// <param name="config">The application config.</param>
    /// <returns>The cache reference.</returns>
    public static ImageCache Open(Config config)
    {
        var cache = new ImageCache(config);

        if (!Directory.Exists(config.CacheDirectory))
        {
            Directory.CreateDirectory(config.CacheDirectory);
        }

        cache.ReloadCacheData();
        return cache;
    }

    private ImageCache(Config config)
    {
        this.Config = config;
        this.CacheDataFilePath = Path.Combine(config.CacheDirectory, CacheDataFileName);
    }

    private Config Config { get; }

    private string CacheDataFilePath { get; }

    private List<ImageMetadata> CachedImages { get; } = new List<ImageMetadata>();

    /// <summary>
    /// Checks whether the cache contains the specified image.
    /// </summary>
    /// <param name="image">The image to check for.</param>
    /// <returns>True if the image is cached and false otherwise.</returns>
    public bool ContainsImage(ImageMetadata image) =>
        this.CachedImages.Any(m => m.Id == image.Id)
        && File.Exists(this.MakeImageFilePath(image));

    /// <summary>
    /// Uses the given <see cref="BingImageClient"/> to download the specified image and add it to the cache.
    /// If the image is already in the cache, this method does nothing.
    /// </summary>
    /// <param name="image">The metadata for the image to download.</param>
    /// <param name="client">The client to use to download the image.</param>
    /// <param name="mutationPipeline">Mutations to apply to the image (optional).</param>
    /// <returns>True if the image was added, false if the image was already in the cache.</returns>
    public async Task<bool> DownloadAndCacheImageAsync(
        ImageMetadata image,
        BingImageClient client,
        ImageMutationPipeline mutationPipeline = null)
    {
        if (this.ContainsImage(image))
        {
            return false;
        }

        // First, we need to make sure there is room in the cache.
        while (this.CachedImages.Count + 1 > this.Config.MaxCacheSize)
        {
            this.DeleteOldestCachedItem(persistCacheData: false);
        }

        // Now we can download and cache the image.
        var finalImageFilePath = this.MakeImageFilePath(image);
        var downloadPath = mutationPipeline is not null
            ? $"{finalImageFilePath}.tmp"
            : finalImageFilePath;
        await client.DownloadImageToAsync(image, downloadPath);

        if (mutationPipeline is not null)
        {
            await mutationPipeline.RunAsync(downloadPath, image, finalImageFilePath);
            File.Delete(downloadPath);
        }

        this.CachedImages.Add(image);
        this.PersistCacheData();

        return true;
    }

    /// <summary>
    /// Retrieves the metadata for the latest image stored in the cache.
    /// </summary>
    /// <returns>The metadata.</returns>
    public ImageMetadata GetLatestCachedImage() => this.CachedImages.MaxBy(m => m.BingStartDate);

    private void ReloadCacheData()
    {
        this.CachedImages.Clear();

        if (!File.Exists(this.CacheDataFilePath))
        {
            // Nothing to load.
            return;
        }

        using FileStream cacheDataReader = File.OpenRead(this.CacheDataFilePath);
        var cacheData = JsonSerializer.Deserialize<CacheData>(cacheDataReader);

        // Prune records where the actual cached image file isn't found on disk.
        var imagesInCache = cacheData.Images.Where(m => File.Exists(this.MakeImageFilePath(m)));

        this.CachedImages.AddRange(imagesInCache);
    }

    private void DeleteOldestCachedItem(bool persistCacheData = true)
    {
        var oldestImage = this.CachedImages.MinBy(m => m.BingStartDate);

        var imagePath = this.MakeImageFilePath(oldestImage);
        if (File.Exists(imagePath))
        {
            File.Delete(imagePath);
        }

        this.CachedImages.RemoveAll(m => m.Id == oldestImage.Id);

        if (persistCacheData)
        {
            this.PersistCacheData();
        }
    }

    private void PersistCacheData()
    {
        var cacheJson = JsonSerializer.Serialize(
            new CacheData
            {
                Images = this.CachedImages.ToArray(),
            },
            new JsonSerializerOptions
            {
                WriteIndented = true,
            });

        File.WriteAllText(this.CacheDataFilePath, cacheJson);
    }

    /// <summary>
    /// Gets the absolute file path for the cached image.
    /// </summary>
    /// <param name="image">The image whose path to get.</param>
    /// <returns>The absolute file path to the image in the cache.</returns>
    public string MakeImageFilePath(ImageMetadata image) =>
        Path.GetFullPath(Path.Combine(this.Config.CacheDirectory, image.Id));

    private class CacheData
    {
        [JsonPropertyName("$comment")]
        public string Comment { get; set; } =
            "This file was created by the Bingir app and contains metadata about the cache in the current directory. Do not edit this file unless you know what you're doing!";

        public ImageMetadata[] Images { get; set; }
    }
}
