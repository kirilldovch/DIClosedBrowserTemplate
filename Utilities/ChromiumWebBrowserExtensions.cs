using CefSharp;
using CefSharp.Wpf;

namespace DIClosedBrowserTemplate.Utilities;

public static class ChromiumWebBrowserExtensions
{
    public static void ScrollToTop(this ChromiumWebBrowser browser) =>
        browser.ExecuteScriptAsync("window.scrollTo({top: 0, behavior: 'smooth'});");
}