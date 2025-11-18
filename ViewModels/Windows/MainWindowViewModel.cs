using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MvvmNavigationLib.Services;
using MvvmNavigationLib.Stores;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DIClosedBrowserTemplate.Helpers;
using DIClosedBrowserTemplate.Models.Messages;
using DIClosedBrowserTemplate.ViewModels.Pages;
using DIClosedBrowserTemplate.ViewModels.Popups;

namespace DIClosedBrowserTemplate.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject, 
        IRecipient<ViewModelChangedMessage>,
        IRecipient<ModalViewModelChangedMessage>,
        IRecipient<UpdateBrowserPageMessage>
    {
        private readonly DispatcherTimer _timer = new();
        private int _sec;
        private readonly NavigationStore _navigationStore;
        private readonly ModalNavigationStore _modalNavigationStore;
        private readonly InactivityManager<MainPageViewModel> _inactivityManager;
        private readonly NavigationService<PasswordPopupViewModel> _passwordNavigationService;
        private readonly CloseNavigationService<NavigationStore> _closeNavigationService;
        private readonly NavigationService<MainPageViewModel> _mainNavigationService;

        public ObservableObject? CurrentViewModel => _navigationStore.CurrentViewModel;
        public ObservableObject? CurrentModalViewModel => _modalNavigationStore.CurrentViewModel;
        public bool IsModalOpen => _modalNavigationStore.CurrentViewModel is not null;

        [ObservableProperty] private string? _url;

        public MainWindowViewModel(
            IMessenger messenger,
            NavigationStore navigationStore,
            ModalNavigationStore modalNavigationStore,
            InactivityManager<MainPageViewModel> inactivityManager,
            NavigationService<PasswordPopupViewModel> passwordNavigationService,
            CloseNavigationService<NavigationStore> closeNavigationService,
            NavigationService<MainPageViewModel> mainNavigationService)
        {
            _navigationStore = navigationStore;
            _modalNavigationStore = modalNavigationStore;
            _inactivityManager = inactivityManager;
            _passwordNavigationService = passwordNavigationService;
            _closeNavigationService = closeNavigationService;
            _mainNavigationService = mainNavigationService;
            messenger.RegisterAll(this);
        }

        internal void CloseViewModel() => _closeNavigationService.Navigate();

        [RelayCommand]
        private void GoToMainPage() => _mainNavigationService.Navigate();

        private Point _dragStart;
        private Point _transformStart;

        [RelayCommand]
        private void StartMoveKeyboard(MouseButtonEventArgs e)
        {
            var border = (FrameworkElement)e.OriginalSource;
            var keyboard = (FrameworkElement)((FrameworkElement)border.Parent).Parent;
            var grid = (FrameworkElement)keyboard.Parent;

            border.CaptureMouse();

            if (keyboard.RenderTransform is not TranslateTransform)
                keyboard.RenderTransform = new TranslateTransform();

            var transform = (TranslateTransform)keyboard.RenderTransform;

            keyboard.Opacity = 0.6;

            _dragStart = e.GetPosition(grid);
            _transformStart = new Point(transform.X, transform.Y);
        }

        [RelayCommand]
        private void DragKeyboard(MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            var border = (FrameworkElement)e.OriginalSource;
            var keyboard = (FrameworkElement)((FrameworkElement)border.Parent).Parent;
            var grid = (FrameworkElement)keyboard.Parent;

            if (keyboard.RenderTransform is TranslateTransform transform)
            {
                var currentPoint = e.GetPosition(grid);

                double deltaX = currentPoint.X - _dragStart.X;
                double deltaY = currentPoint.Y - _dragStart.Y;

                double newX = _transformStart.X + deltaX;
                double newY = _transformStart.Y + deltaY;

                double maxOffsetX = (grid.ActualWidth / 2) + (keyboard.ActualWidth / 2);
                double minOffsetX = -maxOffsetX;
                double maxOffsetY = (grid.ActualHeight / 2) + (keyboard.ActualHeight / 2);
                double minOffsetY = -maxOffsetY;

                newX = Math.Max(minOffsetX, Math.Min(newX, maxOffsetX));
                newY = Math.Max(minOffsetY, Math.Min(newY, maxOffsetY));

                transform.X = newX;
                transform.Y = newY;
            }
        }

        [RelayCommand]
        private void EndMoveKeyboard(MouseButtonEventArgs e)
        {
            var border = (FrameworkElement)e.OriginalSource;
            border.ReleaseMouseCapture();

            var keyboard = (FrameworkElement)((FrameworkElement)border.Parent).Parent;
            keyboard.Opacity = 1;
        }

        private void Timer(object? sender, EventArgs eventArgs)
        {
            _sec++;
            if (_sec < 7) return;
            _passwordNavigationService.Navigate();
        }

        [RelayCommand]
        private void Loaded()
        {
            ExplorerHelper.KillExplorer();
            _inactivityManager.Activate();
        }

        [RelayCommand]
        private void Closing()
        {
            ExplorerHelper.RunExplorer();
            _inactivityManager.Dispose();
        }

        [RelayCommand]
        private void StopTimer()
        {
            _timer.Tick -= Timer;
            _timer.Stop();
            _sec = 0;
        }

        [RelayCommand]
        private void StartTimer()
        {
            _timer.Stop();
            _sec = 0;
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer;
            _timer.Start();
        }

        public void Receive(ViewModelChangedMessage message) => OnPropertyChanged(nameof(CurrentViewModel));

        public void Receive(ModalViewModelChangedMessage message)
        {
            OnPropertyChanged(nameof(CurrentModalViewModel));
            OnPropertyChanged(nameof(IsModalOpen));
        }

        public void Receive(UpdateBrowserPageMessage message)
        {
            Url = message.Value;
        }
    }
}