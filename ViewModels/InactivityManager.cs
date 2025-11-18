using CommunityToolkit.Mvvm.ComponentModel;
using DIClosedBrowserTemplate.Helpers;
using DIClosedBrowserTemplate.Models;
using MvvmNavigationLib.Services;
using MvvmNavigationLib.Stores;

namespace DIClosedBrowserTemplate.ViewModels
{
    public class InactivityManager<TInitialViewModel> : IDisposable
    where TInitialViewModel : ObservableObject
    {
        private readonly NavigationStore _mainStore;
        private readonly INavigationService _initialNavigationService;
        private readonly INavigationService _closePopupNavigationService;
        private readonly BaseInactivityHelper _inactivity;
        private readonly BaseInactivityHelper _passwordInactivity;

        private InactivityConfig Config { get; }

        public InactivityManager(
            InactivityConfig config,
            NavigationStore mainStore,
            INavigationService initialNavigationService,
            INavigationService closePopupNavigationService)
        {
            _mainStore = mainStore;
            _initialNavigationService = initialNavigationService;
            _closePopupNavigationService = closePopupNavigationService;
            Config = config;
            _inactivity = new BaseInactivityHelper(Config.InactivityTime);
            _passwordInactivity = new BaseInactivityHelper(Config.PasswordInactivityTime);
        }

        public void Activate()
        {
            _inactivity.OnInactivity += _inactivity_OnInactivity;
            _passwordInactivity.OnInactivity += _passwordInactivity_OnInactivity;
        }

        public void Dispose()
        {
            _inactivity.OnInactivity -= _inactivity_OnInactivity;
            _passwordInactivity.OnInactivity -= _passwordInactivity_OnInactivity;
        }

        private void _passwordInactivity_OnInactivity(int inactivityTime) => _closePopupNavigationService.Navigate();

        private void _inactivity_OnInactivity(int inactivityTime)
        {
            if (_mainStore.CurrentViewModel is TInitialViewModel) return;
            _initialNavigationService.Navigate();
        }
    }
}