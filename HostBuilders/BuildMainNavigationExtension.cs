using CommunityToolkit.Mvvm.Messaging;
using DIClosedBrowserTemplate.Models.Browser;
using DIClosedBrowserTemplate.ViewModels.Pages;
using DIClosedBrowserTemplate.ViewModels.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MvvmNavigationLib.Services.ServiceCollectionExtensions;
using MvvmNavigationLib.Stores;

namespace DIClosedBrowserTemplate.HostBuilders
{
    public static class BuildMainNavigationExtension
    {
        public static IHostBuilder BuildMainNavigation(this IHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                var imagePath = context.Configuration.GetValue<string?>("imagePath");

                services.AddSingleton<NavigationStore>();
                services.AddUtilityNavigationServices<NavigationStore>();
                services.AddNavigationService<MainPageViewModel, NavigationStore>(s => new MainPageViewModel(
                    s.GetRequiredService<MainWindowViewModel>(),
                    s.GetRequiredService<IMessenger>(),
                    imagePath));
            });

            return builder;
        }
    }
}