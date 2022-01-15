namespace Bingir;

/// <summary>
/// Metadata about a Bing image-of-the-day.
/// </summary>
public class ImageMetadata
{
    /// <summary>
    /// The "startdate" for the image, from the original Bing metadata.
    /// </summary>
    public DateTime BingStartDate { get; init; }

    /// <summary>
    /// When the image was first discovered by Bingir.
    /// </summary>
    public DateTime DiscoverTime { get; init; }

    /// <summary>
    /// The image's ID, from the original Bing metadata.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// The title of the image, from the original Bing metadata.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// The copyright string for the image, from the original Bing metadata.
    /// </summary>
    /// <remarks>
    /// The copyright string typically contains additional descriptive data about the image.
    /// </remarks>
    public string Copyright { get; init; }

    /// <summary>
    /// The original URI where the image was stored on Bing servers.
    /// </summary>
    public Uri OriginalUri { get; init; }
}
