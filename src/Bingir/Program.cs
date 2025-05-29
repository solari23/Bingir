using Bingir.ImageMutation;
using CommandLine;

using static Bingir.CommandLineVerbs;

namespace Bingir;

/// <summary>
/// Constains the entry point and top-level logic for the application.
/// </summary>
public class Program
{
    private const int SuccessCode = 0;
    private const int SystemErrorCode = 1;
    private const int UserErrorCode = 2;

    /// <summary>
    /// The entry point for the application.
    /// </summary>
    /// <param name="args">The command-line arguments to the application.</param>
    /// <returns>A numeric status code.</returns>
    public static async Task<int> Main(string[] args)
    {
        var program = new Program();
        try
        {
            await program.RunAsync(args);
        }
        catch (UserErrorException e)
        {
            Console.Error.WriteLine($"[Error] {e.Message}");
            return UserErrorCode;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"[FAILURE] Operation failed with exception: {e}");
            return SystemErrorCode;
        }

        return SuccessCode;
    }

    private Config Config { get; set; }

    private ImageCache ImageCache { get; set; }

    private async Task RunAsync(string[] args)
    {
        this.Config = await Config.LoadAsync();
        this.ImageCache = ImageCache.Open(this.Config);

        var optionsClasses = typeof(CommandLineVerbs)
            .GetNestedTypes()
            .Where(t => t.GetCustomAttributes(typeof(VerbAttribute), inherit: true).Any())
            .ToArray()
            ?? Array.Empty<Type>();
        var commandParser = new Parser(settings =>
        {
            settings.AutoHelp = true;
            settings.AutoVersion = true;
            settings.CaseSensitive = false;
            settings.CaseInsensitiveEnumValues = true;
            settings.IgnoreUnknownArguments = false;
            settings.HelpWriter = Parser.Default.Settings.HelpWriter;
        });

        var parserResult = commandParser.ParseArguments(args, optionsClasses);

        if (parserResult.Tag == ParserResultType.NotParsed)
        {
            return;
        }

        await parserResult.MapResult(
            (FetchVerbOptions o) => this.FetchAsync(o),
            (LatestVerbOptions o) => this.GetLatestAsync(o),

            _ => Task.CompletedTask);
    }

    private async Task FetchAsync(FetchVerbOptions options)
    {
        if (options.Count < 1
            || options.Count > BingImageClient.MaxImagesToFetch)
        {
            throw new UserErrorException($"The number of images to fetch should be in the range [1-{BingImageClient.MaxImagesToFetch}].");
        }

        using BingImageClient imageClient = new();
        var latestImages = await imageClient.GetLastNImagesMetadataAsync(n: options.Count);

        ImageMutationPipeline mutationPipeline = null;
        if (options.AddDescriptiveInfo)
        {
            mutationPipeline = new ImageMutationPipeline(
                new AddDescriptiveInfoMutation());
        }

        foreach (var image in latestImages)
        {
            bool cached = await this.ImageCache.DownloadAndCacheImageAsync(image, imageClient, mutationPipeline);

            if (!options.Silent)
            {
                Console.WriteLine(cached
                    ? $"Cached image {image.Id}"
                    : $"Image {image.Id} was already cached");
            }
        }
    }

    private async Task GetLatestAsync(LatestVerbOptions options)
    {
        if (options.Fetch)
        {
            try
            {
                await FetchAsync(new FetchVerbOptions
                {
                    Count = 1,
                    Silent = true,
                });
            }
            catch when (options.NoError)
            {
                // Swallow errors if no-error is set.
            }
        }

        var latestImage = this.ImageCache.GetLatestCachedImage();

        if (latestImage is null)
        {
            if (options.NoError)
            {
                return;
            }

            throw new UserErrorException(
                "There are no images in the cache. Use the 'fetch' command to get images from the Bing servers first.");
        }

        var latestImagePath = this.ImageCache.MakeImageFilePath(latestImage);
        Console.WriteLine(latestImagePath);
    }
}