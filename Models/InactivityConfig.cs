using Newtonsoft.Json;

namespace DIClosedBrowserTemplate.Models
{
    public record InactivityConfig(
        [property: JsonProperty("inactivityTime")] int InactivityTime,
        [property: JsonProperty("passwordInactivityTime")] int PasswordInactivityTime);
}