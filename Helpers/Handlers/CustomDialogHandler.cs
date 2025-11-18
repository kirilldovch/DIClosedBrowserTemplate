using CefSharp;

namespace DIClosedBrowserTemplate.Helpers.Handlers;

class CustomDialogHandler: IDialogHandler
{
        
    public bool OnFileDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, CefFileDialogMode mode, string title,
        string defaultFilePath, IReadOnlyCollection<string> acceptFilters, IReadOnlyCollection<string> acceptExtensions,
        IReadOnlyCollection<string> acceptDescriptions, IFileDialogCallback callback)
    {
        return false;
    }
}