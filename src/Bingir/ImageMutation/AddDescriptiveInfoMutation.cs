using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace Bingir.ImageMutation;

/// <summary>
/// A <see cref="IMutation"/> which draws descriptive information text from metadata onto the image.
/// </summary>
/// <remarks>
/// The descriptive info is taken from the copyright info in metadata.
/// </remarks>
public class AddDescriptiveInfoMutation : IMutation
{
    private static Font ArialFont = SystemFonts.CreateFont("Arial", 28);

    /// <inheritdoc />
    public void Mutate(Image image, ImageMetadata metadata)
    {
        var descriptiveText = metadata.Copyright.Substring(0, metadata.Copyright.LastIndexOf('('));
        image.Mutate(x => ApplyWaterMark(x, ArialFont, descriptiveText, 10));
    }

    private static IImageProcessingContext ApplyWaterMark(
        IImageProcessingContext processingContext,
        Font font,
        string text,
        float padding)
    {
        Size imageSize = processingContext.GetCurrentSize();
        var textSize = TextMeasurer.MeasureSize(text, new RichTextOptions(font));
        var textBoxBounds = new FontRectangle(
            x: padding,
            y: imageSize.Height - textSize.Height,
            width: textSize.Width,
            height: textSize.Height);

        // Draw semi-transparent rectangle as the base of the textbox.
        var rectangle = new RectangleF(
            textBoxBounds.X - (padding/2),
            textBoxBounds.Y - 2 * padding,
            textBoxBounds.Width + padding,
            textBoxBounds.Height + padding);
        var drawingOptions = new DrawingOptions
        {
            GraphicsOptions = new()
            {
                BlendPercentage = 0.4f,
            },
        };
        processingContext.Fill(drawingOptions, Color.Black, rectangle);

        // Draw the text on the image.
        var textOptions = new RichTextOptions(font)
        {
            Origin = textBoxBounds.Location,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
        processingContext.DrawText(textOptions, text, Color.White);

        return processingContext;
    }
}
