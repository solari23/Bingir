using SixLabors.ImageSharp;

namespace Bingir.ImageMutation;

/// <summary>
/// A <see cref="IMutation"/> which draws descriptive information text from metadata onto the image.
/// </summary>
/// <remarks>
/// The descriptive info is taken from the copyright info in metadata.
/// </remarks>
public class AddDescriptiveInfoMutation : IMutation
{
    /// <inheritdoc />
    public void Mutate(Image image, ImageMetadata metadata)
    {
        // TODO - Implement Me!
    }
}
