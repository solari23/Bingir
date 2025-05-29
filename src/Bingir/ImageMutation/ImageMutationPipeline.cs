using SixLabors.ImageSharp;

namespace Bingir.ImageMutation;

/// <summary>
/// A pipeline of <see cref="IMutation"/> objects to run on an image.
/// </summary>
public class ImageMutationPipeline
{
    private IReadOnlyList<IMutation> Mutations { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="ImageMutationPipeline"/> class.
    /// </summary>
    public ImageMutationPipeline(params IMutation[] mutations)
    {
        if (mutations is null || mutations.Length == 0)
        {
            throw new ArgumentNullException(nameof(mutations));
        }

        this.Mutations = [.. mutations];
    }

    /// <summary>
    /// Runs the mutations on the given image.
    /// </summary>
    /// <param name="imagePath">The path on local disk to the image.</param>
    /// <param name="metadata">Metadata about the image.</param>
    /// <param name="outputPath">Where to write the mutated image to.</param>
    public async Task RunAsync(string imagePath, ImageMetadata metadata, string outputPath)
    {
        using var img = await Image.LoadAsync(imagePath);

        foreach (var mutation in this.Mutations)
        {
            mutation.Mutate(img, metadata);
        }

        await img.SaveAsync(outputPath);
    }
}
