using System.IO;
using Newtonsoft.Json;
using System.Windows;

namespace DIClosedBrowserTemplate.Models.Browser;

public class SettingsModel
{
    [JsonProperty("host")] 
    public string Host;

    [JsonProperty("id")] 
    public string Id;

    [JsonProperty("available_addresses")] 
    public List<string> AvailableAddresses = [];

    [JsonProperty("scale")] 
    public double Scale;

    [JsonProperty("frame_rate")] 
    public int FrameRate;

    [JsonProperty("media_interval")] 
    public int MediaInterval;

    [JsonProperty("is_clear_cache")] 
    public bool ClearCache;

    [JsonProperty("debug_mode")]
    public bool DebugMode;

    public static SettingsModel GetSettings() =>
        JsonConvert.DeserializeObject<SettingsModel>(File.ReadAllText("browserSettings.json"))!;
}