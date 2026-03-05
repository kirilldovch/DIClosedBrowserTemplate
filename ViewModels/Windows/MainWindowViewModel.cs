using CefSharp;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DIClosedBrowserTemplate.Helpers;
using DIClosedBrowserTemplate.Models.Browser;
using DIClosedBrowserTemplate.Models.Messages;
using DIClosedBrowserTemplate.ViewModels.Pages;
using DIClosedBrowserTemplate.ViewModels.Popups;
using MvvmNavigationLib.Services;
using MvvmNavigationLib.Stores;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using CefSharp.Wpf;
using DIClosedBrowserTemplate.Models;
using DIClosedBrowserTemplate.Utilities;

namespace DIClosedBrowserTemplate.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject, 
        IRecipient<ViewModelChangedMessage>,
        IRecipient<ModalViewModelChangedMessage>,
        IRecipient<UpdateBrowserPageMessage>
    {
        #region Fields
        private readonly DispatcherTimer _timer = new();
        private int _sec;
        private readonly NavigationStore _navigationStore;
        private readonly ModalNavigationStore _modalNavigationStore;
        private readonly InactivityManager<MainPageViewModel> _inactivityManager;
        private readonly NavigationService<PasswordPopupViewModel> _passwordNavigationService;
        private readonly CloseNavigationService<NavigationStore> _closeNavigationService;
        private readonly NavigationService<MainPageViewModel> _mainNavigationService;
        private readonly SettingsModel _settingsModel = SettingsModel.GetSettings();
        private ChromiumWebBrowser _browser;
        #endregion

        #region Properties
        public ObservableObject? CurrentViewModel => _navigationStore.CurrentViewModel;
        public ObservableObject? CurrentModalViewModel => _modalNavigationStore.CurrentViewModel;
        public bool IsModalOpen => _modalNavigationStore.CurrentViewModel is not null;
        #endregion

        #region ObservableProperties
        [ObservableProperty] private bool _isInverted; //инверсия цветов
        [ObservableProperty] private bool _isMagnifier; //лупа
        [ObservableProperty] private string? _url; // хост для браузера
        [ObservableProperty] private bool _isClosedBrowser; // панель управления
        [ObservableProperty] private bool _isDisableModeVisible; // доступная среда
        [ObservableProperty] private bool _isScaled; // уменьшение экрана
        [ObservableProperty] private bool _isKeyBoardVisible; //видимость клавиатуры 
        #endregion

        #region Ctor
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

            Url = _settingsModel.Host;
            IsClosedBrowser = _settingsModel.IsClosedBrowser;
            IsDisableModeVisible = _settingsModel.DisableMode;
        }
        #endregion

        #region Commands
        [RelayCommand]
        private void Loaded()
        {
            ExplorerHelper.KillExplorer();
            _inactivityManager.Activate();
        }

        [RelayCommand]
        private void BrowserLoaded(ChromiumWebBrowser browser)
        {
            _browser = browser;

            _browser.FrameLoadStart += OnFrameLoadStart;
            _browser.JavascriptMessageReceived += JavascriptMessageReceivedHandler;
            _browser.VirtualKeyboardRequested += ShowKeyBoard;
        }

        [RelayCommand]
        private void GoToMainPage()
        {
            ShowKeyBoard(false);
            _mainNavigationService.Navigate();
        }

        [RelayCommand]
        private void GoBack()
        {
            if (Url != _settingsModel.Host)
                _browser.Back();
            else
                _browser.ScrollToTop();

            ShowKeyBoard(false);
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
        private void Closing()
        {
            ExplorerHelper.RunExplorer();
            _inactivityManager.Dispose();

            _browser.JavascriptMessageReceived -= JavascriptMessageReceivedHandler;
            _browser.FrameLoadStart -= OnFrameLoadStart;
            _browser.VirtualKeyboardRequested -= ShowKeyBoard;
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
        #endregion

        #region EventSubscribers
        private void OnFrameLoadStart(object? sender, FrameLoadStartEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _browser.SetZoomLevel(_settingsModel.Scale);
            });
        }

        private void ShowKeyBoard(object? sender, VirtualKeyboardRequestedEventArgs e) =>
            ShowKeyBoard(true);

        private async void JavascriptMessageReceivedHandler(object? sender, JavascriptMessageReceivedEventArgs e)
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
        #endregion

        #region Methods
        private void ShowKeyBoard(bool isVisible) =>
            IsKeyBoardVisible = isVisible;

        private void Timer(object? sender, EventArgs eventArgs)
        {
            _sec++;
            if (_sec < 7) return;
            _passwordNavigationService.Navigate();
        }

        internal void CloseViewModel() =>
            _closeNavigationService.Navigate(); 
        #endregion

        #region MessageValidators
        private static bool IsStartAppMessage(string? message) =>
            message!.Contains("\"command\": \"start_app\"");

        private static bool IsVideoMessage(string? message) =>
            message!.Contains("video:");

        private static bool IsHttpMessage(string? message) =>
            message!.Contains("http");

        private static bool IsDisableMessage(string? message) =>
            message == "disable";
        #endregion

        #region MessageHandlers
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

        private void HandleHttpMessage(string? msgString) =>
            Url = msgString;

        private void HandleBooleanMessage(bool boolean) =>
            IsDisableModeVisible = boolean;

        private async Task HandleDisableMessage()
        {
            IsScaled = false;
            IsInverted = false;
            IsMagnifier = false;

            await Task.Delay(500);
        }

        private async Task HandleDefaultMessage(string? msgString)
        {
            var id = msgString;
            if (id == null) return;
            _settingsModel.Id = id;
            var url = Path.Combine(_settingsModel.Host, id);
            await File.WriteAllTextAsync("browserSettings.json", JsonConvert.SerializeObject(_settingsModel));
            Application.Current.Dispatcher.Invoke(() => _browser.LoadUrl(url));
        }
        #endregion

        #region Recipients
        public void Receive(ViewModelChangedMessage message) =>
            OnPropertyChanged(nameof(CurrentViewModel));

        public void Receive(ModalViewModelChangedMessage message)
        {
            OnPropertyChanged(nameof(CurrentModalViewModel));
            OnPropertyChanged(nameof(IsModalOpen));
        }

        public void Receive(UpdateBrowserPageMessage message) =>
            Url = message.Value; 
        #endregion
    }
}