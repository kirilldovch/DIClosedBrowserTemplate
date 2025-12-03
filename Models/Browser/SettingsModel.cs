using System.IO;
using Newtonsoft.Json;
using System.Windows;

namespace DIClosedBrowserTemplate.Models.Browser;

public class SettingsModel
{
    [JsonProperty("host")] 
    public string Host { get; set; }

    [JsonProperty("id")] 
    public string Id { get; set; }

    [JsonProperty("available_addresses")]
    public List<string> AvailableAddresses { get; set; } = [];

    [JsonProperty("scale")] 
    public double Scale { get; set; }

    [JsonProperty("frame_rate")] 
    public int FrameRate { get; set; }

    [JsonProperty("media_interval")] 
    public int MediaInterval { get; set; }

    [JsonProperty("is_clear_cache")] 
    public bool ClearCache { get; set; }

    [JsonProperty("debug_mode")]
    public bool DebugMode { get; set; }

    [JsonProperty("is_closed_browser")]
    public bool IsClosedBrowser { get; set; }

    [JsonProperty("disable_mode")]
    public bool DisableMode { get; set; }

    public static SettingsModel GetSettings() =>
        JsonConvert.DeserializeObject<SettingsModel>(File.ReadAllText("browserSettings.json"))!;
}