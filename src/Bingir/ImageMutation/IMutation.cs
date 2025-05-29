using SixLabors.ImageSharp;

namespace Bingir.ImageMutation;

/// <summary>
/// Represents a mutation that can be applied to am inage.
/// </summary>
public interface IMutation
{
    /// <summary>
    /// Performs a mutation on the given image.
    /// </summary>
    /// <param name="image">The image to mutate.</param>
    /// <param name="metadata">Metadata about the image.</param>
    void Mutate(Image image, ImageMetadata metadata);
}
