using System.IO;
using CefSharp;
using CefSharp.Handler;
using DIClosedBrowserTemplate.Models.Browser;

namespace DIClosedBrowserTemplate.Helpers.Handlers;

public class CustomRequestHandler : RequestHandler, IRequestHandler
{
    bool IRequestHandler.OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture,
        bool isRedirect)
    {
        var settings = SettingsModel.GetSettings();
        var address = settings.Host;
        var uri = new Uri(address);
        var origin = $"{uri.Scheme}://{uri.Authority}";

        if (!string.IsNullOrEmpty(address))
        {
            settings.AvailableAddresses.Add(origin);
            var i = settings.AvailableAddresses.Count(availableAddress => request.Url.TrimEnd().StartsWith(availableAddress));
            if (i == 0)
            {
                File.AppendAllText("DebugCefSharp.txt", $"Сообщение от CustomRequsestHandler: {request.Url} заблокирован{Environment.NewLine}");
                browser.StopLoad();
                return true;
            }
        }

        return OnBeforeBrowse(chromiumWebBrowser, browser, frame, request, userGesture, isRedirect);

    }
}