using CefSharp;
using CefSharp.Wpf;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DIClosedBrowserTemplate.Helpers;
using DIClosedBrowserTemplate.Models.Browser;
using DIClosedBrowserTemplate.Models.Messages;
using DIClosedBrowserTemplate.Utilities;
using DIClosedBrowserTemplate.ViewModels.Pages;
using DIClosedBrowserTemplate.ViewModels.Popups;
using MvvmNavigationLib.Services;
using MvvmNavigationLib.Stores;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DIClosedBrowserTemplate.Models;

namespace DIClosedBrowserTemplate.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject, 
        IRecipient<ViewModelChangedMessage>,
        IRecipient<ModalViewModelChangedMessage>,
        IRecipient<UpdateBrowserPageMessage>
    {
        private readonly DispatcherTimer _timer = new();
        private int _sec;
        private bool _zoom;
        private bool _inverted;
        private IWebBrowser _browser;
        private readonly NavigationStore _navigationStore;
        private readonly ModalNavigationStore _modalNavigationStore;
        private readonly InactivityManager<MainPageViewModel> _inactivityManager;
        private readonly NavigationService<PasswordPopupViewModel> _passwordNavigationService;
        private readonly CloseNavigationService<NavigationStore> _closeNavigationService;
        private readonly NavigationService<MainPageViewModel> _mainNavigationService;

        public ObservableObject? CurrentViewModel => _navigationStore.CurrentViewModel;
        public ObservableObject? CurrentModalViewModel => _modalNavigationStore.CurrentViewModel;
        public bool IsModalOpen => _modalNavigationStore.CurrentViewModel is not null;

        [ObservableProperty] private InvertEffect? _effect; //инверсия цветов
        [ObservableProperty] private string? _url; // хост для браузера
        [ObservableProperty] private bool _isWebBrowserEnabled = true;
        [ObservableProperty] private double _zoomFactor; // увеличение для лупы
        [ObservableProperty] private bool _isClosedBrowser = SettingsModel.GetSettings().IsClosedBrowser; // панель управления
        [ObservableProperty] private bool _isDisableModeVisible = SettingsModel.GetSettings().DisableMode; // доступная среда
        [ObservableProperty] private bool _isDisabled; // уменьшение экрана
        [ObservableProperty] private double _browserScale; // scale

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
        private void BrowserLoaded(IWebBrowser browser)
        {
            _browser = browser;

            _browser.FrameLoadEnd += OnFrameLoadEnd;
            _browser.JavascriptMessageReceived += JavascriptMessageReceivedHandler!;
        }


        [RelayCommand]
        private void Invert()
        {
            _inverted = !_inverted;
            Effect = _inverted ? new InvertEffect() : null;
        }

        [RelayCommand]
        private void MagnifierZoom()
        {
            _zoom = !_zoom;
            if (_zoom)
            {
                IsWebBrowserEnabled = false;
                ZoomFactor = 0.5;
            }
            else
            {
                IsWebBrowserEnabled = true;
                ZoomFactor = 0;
            }
        }

        [RelayCommand]
        private void Ear()
        {
            var script = "document.getElementById(\"ear\").click()";
            _browser.ExecuteScriptAsync(script);
        }

        [RelayCommand]
        private void GeneralZoom()
        {
            var script = "document.getElementById(\"zoom\").click()";
            _browser.ExecuteScriptAsync(script);
        }

        [RelayCommand]
        private void Filter()
        {
            var script = "document.getElementById(\"filter\").click()";
            _browser.ExecuteScriptAsync(script);
        }

        [RelayCommand]
        private void Disable() => IsDisabled = !IsDisabled;

        private async void JavascriptMessageReceivedHandler(object sender, JavascriptMessageReceivedEventArgs e)
        {
            var message = e.Message;
            if (message is null) return;

            var msgString = e.Message.ToString();

            if (IsStartAppMessage(msgString))
            {
                HandleStartAppMessage(msgString);
            }
            else if (IsVideoMessage(msgString))
            {
                //ignore
            }
            else if (IsHttpMessage(msgString))
            {
                HandleHttpMessage(msgString);
            }
            else if (message is bool boolean)
            {
                HandleBooleanMessage(boolean);
            }
            else if (IsDisableMessage(msgString))
            {
                await HandleDisableMessage();
            }
            else
            {
                await HandleDefaultMessage(msgString);
            }
        }

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

                var deltaX = currentPoint.X - _dragStart.X;
                var deltaY = currentPoint.Y - _dragStart.Y;

                var newX = _transformStart.X + deltaX;
                var newY = _transformStart.Y + deltaY;

                var maxOffsetX = (grid.ActualWidth / 2) + (keyboard.ActualWidth / 2);
                var minOffsetX = -maxOffsetX;
                var maxOffsetY = (grid.ActualHeight / 2) + (keyboard.ActualHeight / 2);
                var minOffsetY = -maxOffsetY;

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
        private void OnFrameLoadEnd(object? sender, FrameLoadEndEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _browser.SetZoomLevel(SettingsModel.GetSettings().Scale);
            });
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

        private static bool IsStartAppMessage(string? message) => message!.Contains("\"command\": \"start_app\"");
        private static bool IsVideoMessage(string? message) => message!.Contains("video:");
        private static bool IsHttpMessage(string? message) => message!.Contains("http");
        private static bool IsDisableMessage(string? message) => message == "disable";

        private async void HandleStartAppMessage(string? msgString)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<StartAppModel>(msgString!);
                if (!File.Exists(model?.Path))
                {
                    await File.AppendAllTextAsync("DebugCefSharp.txt", $@"Приложение по пути {model?.Path} не найдено" + Environment.NewLine);
                    return;
                }

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Path.GetFileName(model.Path),
                        WorkingDirectory = Path.GetDirectoryName(model.Path),
                        UseShellExecute = true
                    }
                };
                process.Start();
            }
            catch (Exception exception)
            {
                await File.AppendAllTextAsync("DebugCefSharp.txt", $@"Ошибка при запуске приложения: " + exception.Message + Environment.NewLine +
                                                                   msgString + Environment.NewLine +
                                                                   exception.StackTrace);
            }
        }

        private void HandleHttpMessage(string? msgString)
        {
            Url = msgString;
        }

        private void HandleBooleanMessage(bool boolean)
        {
            Application.Current.Dispatcher.Invoke(() => IsDisableModeVisible = boolean);
        }

        private async Task HandleDisableMessage()
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                if (IsDisabled)
                {
                    IsDisabled = false;
                }
                if (_inverted)
                {
                    Invert();
                }
                if (_zoom)
                {
                    _zoom = false;
                    ZoomFactor = 0;
                }

                await Task.Delay(500);
            });
        }

        private async Task HandleDefaultMessage(string? msgString)
        {
            var id = msgString;
            if (id == null) return;
            var settings = SettingsModel.GetSettings();
            settings.Id = id;
            var url = Path.Combine(SettingsModel.GetSettings().Host, id);
            await File.WriteAllTextAsync("browserSettings.json", JsonConvert.SerializeObject(settings));
            Application.Current.Dispatcher.Invoke(() => _browser.LoadUrl(url));
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