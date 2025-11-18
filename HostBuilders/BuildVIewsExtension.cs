using CommunityToolkit.Mvvm.Messaging;
using DIClosedBrowserTemplate.Models;
using DIClosedBrowserTemplate.Models.Browser;
using DIClosedBrowserTemplate.ViewModels;
using DIClosedBrowserTemplate.ViewModels.Pages;
using DIClosedBrowserTemplate.ViewModels.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MvvmNavigationLib.Services;
using MvvmNavigationLib.Stores;

namespace DIClosedBrowserTemplate.HostBuilders
{
    public static class BuildViewsExtension
    {
        public static IHostBuilder BuildViews(this IHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                var inactivityConfig = context.Configuration.GetValue<InactivityConfig>("inactivity");
                services.AddSingleton<IMessenger>(_ => new WeakReferenceMessenger());

                services.AddSingleton<InactivityManager<MainPageViewModel>>(s => new InactivityManager<MainPageViewModel>(
                    inactivityConfig ?? new InactivityConfig(60, 10),
                    s.GetRequiredService<NavigationStore>(),
                    s.GetRequiredService<NavigationService<MainPageViewModel>>(),
                    s.GetRequiredService<CloseNavigationService<ModalNavigationStore>>()));

                services.AddSingleton<SettingsModel>();
                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton(s => new Views.Windows.MainWindow()
                {
                    DataContext = s.GetRequiredService<MainWindowViewModel>()
                });
            });
            return builder;
        }
    }
}
