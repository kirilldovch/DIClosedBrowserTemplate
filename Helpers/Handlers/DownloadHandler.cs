using CefSharp;

namespace DIClosedBrowserTemplate.Helpers.Handlers;

internal class DownloadHandler: IDownloadHandler
{
    public bool CanDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, string url, string requestMethod)
    {
        return false;
    }

    public bool OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem,
        IBeforeDownloadCallback callback)
    {
        return false;
    }

    public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem,
        IDownloadItemCallback callback)
    {
    }
}