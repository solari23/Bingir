using Bingir.ImageMutation;

namespace Bingir;

/// <summary>
/// A set of options to pass to <see cref="ImageCache"/> caching operation.
/// </summary>
public class ImageCachingOptions
{
    /// <summary>
    /// Whether to force overwrite the previously cached item.
    /// </summary>
    public bool ForceOverwrite { get; set; }

    /// <summary>
    /// Any mutations to apply to the image.
    /// </summary>
    public ImageMutationPipeline Mutations { get; set; }
}
