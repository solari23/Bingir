using System.Reflection;
using System.Text.Json;

namespace Bingir;

/// <summary>
/// Configuration for the Bingir application.
/// </summary>
public class Config
{
    /// <summary>
    /// The filename for the default Bingir configuration file.
    /// </summary>
    public const string ConfigFileName = "BingirConfig.json";

    /// <summary>
    /// Loads the default configuration file stored in the same directory as the Bingir executable
    /// with file name specified in <see cref="ConfigFileName"/>.
    /// </summary>
    /// <returns></returns>
    public static async Task<Config> LoadAsync()
    {
        var configFilePath = Path.Combine(AppContext.BaseDirectory, ConfigFileName);

        var configText = await File.ReadAllTextAsync(configFilePath);
        return JsonSerializer.Deserialize<Config>(configText);
    }

    /// <summary>
    /// The directory containing the image cache.
    /// </summary>
    public string CacheDirectory { get; set; }

    /// <summary>
    /// The maximum number of images to cache.
    /// </summary>
    public int MaxCacheSize { get; set; }
}
