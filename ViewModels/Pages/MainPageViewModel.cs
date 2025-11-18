using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DIClosedBrowserTemplate.Models.Messages;
using DIClosedBrowserTemplate.ViewModels.Windows;

namespace DIClosedBrowserTemplate.ViewModels.Pages
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly IMessenger _messenger;
        [ObservableProperty] private string? _backgroundImagePath; 

        public MainPageViewModel(MainWindowViewModel mainWindowViewModel, IMessenger messenger, string? backgroundImagePath)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _messenger = messenger;
            BackgroundImagePath = backgroundImagePath;

            messenger.RegisterAll(this);
        }

        [RelayCommand]
        private void GoToPage(string url)
        {
            _messenger.Send(new UpdateBrowserPageMessage(url));

            _mainWindowViewModel.CloseViewModel();
        }
    }
}