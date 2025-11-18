using Microsoft.Extensions.Hosting;

namespace DIClosedBrowserTemplate.HostBuilders;

public static class BuildClosedBrowserExtension
{
    public static IHostBuilder BuildClosedBrowser(this IHostBuilder builder)
    {
        return builder;
    }
}