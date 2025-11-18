using DIClosedBrowserTemplate.ViewModels.Popups;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MvvmNavigationLib.Services;
using MvvmNavigationLib.Services.ServiceCollectionExtensions;
using MvvmNavigationLib.Stores;

namespace DIClosedBrowserTemplate.HostBuilders
{
    public static class BuildModalNavigationExtension
    {
        public static IHostBuilder BuildModalNavigation(this IHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<ModalNavigationStore>();
                services.AddUtilityNavigationServices<ModalNavigationStore>();

                services.AddNavigationService<PasswordPopupViewModel, ModalNavigationStore>(s => new PasswordPopupViewModel(
                    s.GetRequiredService<CloseNavigationService<ModalNavigationStore>>(),
                    context.Configuration.GetValue<string>("exitPassword") ?? "1234"));
            });

            return builder;
        }
    }
}
