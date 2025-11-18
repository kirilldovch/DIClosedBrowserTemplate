using CefSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MvvmNavigationLib.Services;
using Serilog;
using DIClosedBrowserTemplate.HostBuilders;
using System.IO;
using System.Windows;
using CefSharp.Wpf;
using DIClosedBrowserTemplate.Helpers;
using DIClosedBrowserTemplate.Models.Browser;
using DIClosedBrowserTemplate.ViewModels.Pages;

namespace DIClosedBrowserTemplate
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _appHost = CreateHostBuilder().Build();

        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (DebugHelper.IsRunningInDebugMode) throw e.Exception;
            var logger = _appHost.Services.GetRequiredService<ILogger>();
            logger.Error(e.Exception, "Неизвестная ошибка");
            e.Handled = true;
        }

        private static IHostBuilder CreateHostBuilder(string[]? args = null)
        {
            return Host.CreateDefaultBuilder(args)
                .BuildConfiguration()
                .BuildLogging()
                .BuildMainNavigation()
                .BuildModalNavigation()
                .BuildApi()
                .BuildViews();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            var settingsModel = SettingsModel.GetSettings();
            var logger = _appHost.Services.GetRequiredService<ILogger>();

            if (settingsModel == null)
            {
                logger.Error("GetNonStaticSettings returned null");
                throw new InvalidOperationException("SettingsModel cannot be null");
            }

            var settings = new CefSettings
            {
                CachePath = Path.GetFullPath("BrowserChache"),
                LogSeverity = LogSeverity.Verbose,
                LogFile = Path.GetFullPath("cefsharp.log")
            };
            settings.CefCommandLineArgs.Add("enable-media-stream", "1");
            settings.CefCommandLineArgs.Add("enable-image-loading", "1");
            settings.CefCommandLineArgs.Add("--disable-pinch");
            settings.CefCommandLineArgs.Add("ignore-certificate-errors");
            settings.CefCommandLineArgs.Add("disable-web-security", "1");
            settings.CefCommandLineArgs.Add("allow-running-insecure-content", "1");

            bool clearCache = settingsModel.ClearCache;
            if (clearCache && Directory.Exists("BrowserChache"))
            {
                try
                {
                    Directory.Delete("BrowserChache", true);
                    Directory.CreateDirectory("BrowserChache");
                    logger.Information("Browser cache cleared successfully");
                }
                catch (IOException ex)
                {
                    logger.Error(ex, "Failed to clear browser cache");
                }
            }
            else if (!Directory.Exists("BrowserChache"))
            {
                try
                {
                    Directory.CreateDirectory("BrowserChache");
                    logger.Information("Browser cache directory created");
                }
                catch (IOException ex)
                {
                    logger.Error(ex, "Failed to create browser cache directory");
                }
            }

            try
            {
                Cef.Initialize(settings);
                logger.Information("CEF initialized successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to initialize CEF");
                throw;
            }

            var initialNavigationService =
                _appHost.Services.GetRequiredService<NavigationService<MainPageViewModel>>();

            initialNavigationService.Navigate();
            MainWindow = _appHost.Services.GetRequiredService<Views.Windows.MainWindow>();
            MainWindow.Show();
            await _appHost.StartAsync();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _appHost.StopAsync();
            _appHost.Dispose();
            base.OnExit(e);
        }
    }
}
