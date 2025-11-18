using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MainComponents.Popups;
using MvvmNavigationLib.Services;

namespace DIClosedBrowserTemplate.ViewModels.Popups
{
    public partial class PasswordPopupViewModel(INavigationService closeModalNavigationService, string password)
        : BasePopupViewModel(closeModalNavigationService)
    {
        [ObservableProperty] private bool _isPinPadOpen = true;

        private string _currentPassword = string.Empty;
        public string CurrentPassword
        {
            get => _currentPassword;
            set
            {
                SetProperty(ref _currentPassword, value);
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool IsValid => CurrentPassword == password;


        [RelayCommand]
        private void Exit() => Application.Current.Shutdown();

        [RelayCommand]
        private void RemoveSymbol()
        {
            if (CurrentPassword.Length > 0) CurrentPassword = CurrentPassword[..^1];
            OnPropertyChanged(nameof(IsValid));
        }

        [RelayCommand]
        private void AddSymbol(string symbol)
        {
            CurrentPassword += symbol;
            OnPropertyChanged(nameof(IsValid));
        }

        [RelayCommand]
        private void OpenPinPad() => IsPinPadOpen = true;

        [RelayCommand]
        private void ClosePinPad() => IsPinPadOpen = false;

    }
}