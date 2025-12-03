using Newtonsoft.Json;

namespace DIClosedBrowserTemplate.Models;

public class StartAppModel
{
    [JsonProperty("command")]
    public string Command { get; set; }

    [JsonProperty("path")]
    public string Path { get; set; }
}