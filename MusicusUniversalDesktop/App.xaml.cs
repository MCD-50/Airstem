
using Musicus.Helpers;
using Musicus.Helpers.BackgroundHelpers;
using Musicus.TransitionHelpers;
using Musicus.Utilities;
using Musicus.ViewModel;
using Musicus.ViewModel.Mvvm.Dispatcher;
using SQLite;
using SQLite.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Musicus
{

    public sealed partial class App
    {
        #region Constructor

        public App()
        {
            this.InitializeComponent();
            Suspending += OnSuspending;
            Resuming += OnResuming;
            UnhandledException += OnUnhandledException;
            //(Application.Current.Resources["WidthConverter"] as DoubleWrapper).InitValue();
           
        }

        public static ApplicationViewTitleBar TitleBar { get; set; }
       
        private void SetTitleBar()
        {
            // used to make things transparent. 

            //StackPanel NewTitleBar = new StackPanel
            //{
            //    Height = TitleBarHeight,
            //    Background = new Windows.UI.Xaml.Media.SolidColorBrush(Colors.Transparent)
            //};


            //Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            //Window.Current.SetTitleBar(NewTitleBar);
            //DisplayInformation.GetForCurrentView().DpiChanged += AppViewHelper_DpiChanged;

            TitleBar = ApplicationView.GetForCurrentView().TitleBar;

            //System.Diagnostics.Debug.WriteLine(Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.Height);
            TitleBar.BackgroundColor = Color.FromArgb(0, 25, 25, 25);
            TitleBar.ButtonBackgroundColor = Color.FromArgb(0, 25, 25, 25);
            TitleBar.ForegroundColor = Colors.White;
            TitleBar.ButtonForegroundColor = Colors.White;
            TitleBar.InactiveBackgroundColor = Color.FromArgb(0, 25, 25, 25);
            TitleBar.ButtonInactiveBackgroundColor = Color.FromArgb(0, 25, 25, 25);
        }




        
        //public static double TitleBarHeight { get; set; }
        //private static void AppViewHelper_DpiChanged(DisplayInformation sender, object args)
        //{
        //    TitleBarHeight = Math.Floor(32 * (DisplayInformation.GetForCurrentView().LogicalDpi / 100)); 
        //}

      



        private void Backbutton_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Navigator.NavigationClearer();
            if (UiBlockerUtility.SupressBackEvents)
            {
                e.Handled = true;
                //if (SupressBackEvent != null)
                //    SupressBackEvent(this, e);
                SupressBackEvent?.Invoke(this, e);
            }
            else 
            if (Navigator.GoBack())
                e.Handled = true;
        }


        #endregion

        public static event EventHandler<BackRequestedEventArgs> SupressBackEvent;
        public static Navigator Navigator { get; set; }
        private static ViewModelLocator _locator;

        public static ViewModelLocator Locator
        {
            get { return _locator ?? (_locator = Current.Resources["Locator"] as ViewModelLocator); }
        }
        public static Frame RootFrame { get; private set; }




        #region Overriding

        //protected override void OnFileActivated(FileActivatedEventArgs args)
        //{
        //    base.OnFileActivated(args);
        //    StartMusicus();
        //    LocalMusicHelpers.LocalMusicHelper.GetFileFromOutside(args.Files);
        //}


        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);
            StartMusicus();
        }

        

        private async void StartMusicus()
        {
            CreateRootFrame();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
                SetTitleBar();


            if (RootFrame.Content == null)
            {
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                    ApplicationView.GetForCurrentView().VisibleBoundsChanged += OnVisibleBoundsChanged;

                RootFrame.Navigate(typeof(RootPage), null);
            }

         
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                OnVisibleBoundsChanged(null, null);

            RegisterNavigation();

            await BootAppServicesAsync();
            Window.Current.Activate();
            
        }




        #endregion

        #region Events

        public static SystemNavigationManager BackButtonNavigator { get; set; }
        private void RegisterNavigation()
        {
            BackButtonNavigator = SystemNavigationManager.GetForCurrentView();
            BackButtonNavigator.BackRequested -= Backbutton_BackRequested;
            BackButtonNavigator.BackRequested += Backbutton_BackRequested;
        }

        //private void SetWindowOnStart()
        //{
        //    var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
        //    double scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
        //    ApplicationView.PreferredLaunchViewSize = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);
        //    ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            
        //}

        private int GetScaledImageSize()
        {
            var scaledImageSize = 200;
            double scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            scaledImageSize = (int)(scaledImageSize * scaleFactor);
            return scaledImageSize;
        }

        private void OnResuming(object sender, object o)
        {
            Locator.SqlService.Initialize();
            Locator.BgSqlService.Initialize();
            Locator.AudioPlayerHelper.OnAppActive();
            Locator.Player.OnAppActive();
           
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            Locator.SqlService.Dispose();
            Locator.BgSqlService.Dispose();
            Locator.AudioPlayerHelper.OnAppActive();
            Locator.Player.OnAppActive();

            deferral.Complete();
        }

        private void OnVisibleBoundsChanged(ApplicationView sender, object args)
        {
            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            var h = Window.Current.Bounds;
            var view = ApplicationView.GetForCurrentView().Orientation;

            if (view.ToString().Trim().Equals("Portrait"))
            {
                var diff = Math.Ceiling(h.Height - bounds.Bottom);
                RootFrame.Margin = new Thickness(0, 0, 0, diff);
            }
            else if (view.ToString().Trim().Equals("Landscape"))
            {
                var diff = Math.Ceiling(h.Width - bounds.Right);
                RootFrame.Margin = new Thickness(0, 0, diff, 0);
            }

        }


        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
        }


        #endregion




        private void CreateRootFrame()
        {
            RootFrame = Window.Current.Content as Frame;
            if (RootFrame != null) return;
            DispatcherHelper.Initialize();
            RootFrame = new Frame { Style = Resources["AppFrame"] as Style };
            Window.Current.Content = RootFrame;
        }


        private bool _init;
        private bool exceptionOccured = false;
        public static EventHandler<bool> DataBaseException;

        public async Task BootAppServicesAsync()
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                await StatusBar.GetForCurrentView().HideAsync();

            if (!_init)
            {
                Locator.CollectionService.ScaledImageSize = GetScaledImageSize();
                try
                {
                    try
                    {
                       exceptionOccured = await Locator.SqlService.InitializeAsync().ConfigureAwait(false);
                    }
                    catch (SQLiteException ex)
                    {
                        if (ex.Message.Contains("IOError") || ex.Message.Contains("I/O"))
                        {
                            // issues when SQLite can't delete the wal related files, so init in delete mode
                            // and then go back to wal mode
                            Locator.SqlService.Dispose();
                            exceptionOccured = Locator.SqlService.Initialize(false);
                            Locator.SqlService.Dispose();
                            exceptionOccured = Locator.SqlService.Initialize();
                        }
                    }

                    try
                    {
                        exceptionOccured = await Locator.BgSqlService.InitializeAsync().ConfigureAwait(false);
                    }
                    catch (SQLiteException ex)
                    {
                        if (ex.Message.Contains("IOError") || ex.Message.Contains("I/O"))
                        {
                            // issues when SQLite can't delete the wal related files, so init in delete mode
                            // and then go back to wal mode
                            Locator.BgSqlService.Dispose();
                            exceptionOccured = Locator.BgSqlService.Initialize(false);
                            Locator.BgSqlService.Dispose();
                            exceptionOccured = Locator.BgSqlService.Initialize();
                        }
                    }

                    await Locator.CollectionService.LoadLibraryAsync();
                    Locator.Player.OnAppActive();
                    Locator.AudioPlayerHelper.OnAppActive();
                    await FinishLoad();
                }
                catch { await ExceptionHandler(); }
                _init = true;
            }

            if (exceptionOccured)
                DataBaseException?.Invoke(this, true);
        }

        private async Task FinishLoad()
        {
           // await Task.Delay(20);
            await DispatcherHelper.RunAsync(async() =>
            {
                CollectionHelper.MatchSongsOnStart();
                await Locator.Download.LoadDownloads();

            });


        }

        private async Task ExceptionHandler()
        {
            await MessageHelpers.RefreshMusicus();
        }


        static double padding;
        public static double GetWidth(double Width)
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                padding = 24;
            else padding = 72;
            try
            {
                double value;
                if (Width > 2000)
                {
                    value = (Width - (22 + padding));
                    value /= 8;
                }
                else if (Width > 1700 && Width <= 2000)
                {
                    value = (Width - (19 + padding));
                    value /= 7;
                }
                else if (Width > 1400 && Width <= 1700)
                {
                    value = (Width - (16 + padding));
                    value /= 6;
                }
                else if (Width > 1100 && Width <= 1400)
                {
                    value = (Width - (13 + padding));
                    value /= 5;
                }
                else if (Width > 800 && Width <= 1100)
                {
                    value = (Width - (10 + padding));
                    value /= 4;
                }
                else if (Width > 500 && Width <= 800)
                {
                    value = (Width - (7 + padding));
                    value /= 3;
                }
                else if (Width > 300 && Width <= 500)
                {
                    value = (Width - (4 + padding));
                    value /= 2;
                }
                else
                    value = (Width - (1 + padding));

                Debug.WriteLine(Math.Floor(value));
                return Math.Floor(value);
            }
            catch
            {
                return 300;
            }
        }

        public static double GetOnlineWidth(double Width)
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                padding = 24;
            else padding = 72;

            double value;
            if (Width > 2400)
            {
                value = (Width - (19 + padding));
                value /= 7;
            }
            else if (Width > 2000 && Width <= 2400)
            {
                value = (Width - (13 + padding));
                value /= 6;
            }
            else if (Width > 1600 && Width <= 2000)
            {
                value = (Width - (14 + padding));
                value /= 5;
            }
            else if (Width > 1200 && Width <= 1600)
            {
                value = (Width - (11 + padding));
                value /= 4;
            }
            else if (Width > 800 && Width <= 1200)
            {
                value = (Width - (8 + padding));
                value /= 3;
            }
            else if (Width > 400 && Width <= 800)
            {
                value = (Width - (5 + padding));
                value /= 2;
            }
            else
                value = (Width - (1 + padding));

            Debug.WriteLine(Math.Floor(value));
            return Math.Floor(value);
        }

        public static double GetVideoWidth(double Width)
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                padding = 24;
            else padding = 72;
            try
            {
                double value;
                if (Width > 2000)
                {
                    value = (Width - (31 + padding));
                    value /= 11;
                }
                else if (Width > 1800 && Width <= 2000)
                {
                    value = (Width - (28 + padding));
                    value /= 10;
                }
                else if (Width > 1600 && Width <= 1800)
                {
                    value = (Width - (25 + padding));
                    value /= 9;
                }
                else if (Width > 1400 && Width <= 1600)
                {
                    value = (Width - (22 + padding));
                    value /= 8;
                }
                else if (Width > 1200 && Width <= 1400)
                {
                    value = (Width - (19 + padding));
                    value /= 7;
                }
                else if (Width > 1000 && Width <= 1200)
                {
                    value = (Width - (16 + padding));
                    value /= 6;
                }
                else if (Width > 800 && Width <= 1000)
                {
                    value = (Width - (13 + padding));
                    value /= 5;
                }
                else if (Width > 600 && Width <= 800)
                {
                    value = (Width - (10 + padding));
                    value /= 4;
                }
                else if (Width > 400 && Width <= 600)
                {
                    value = (Width - (7 + padding));
                    value /= 3;
                }
                else if (Width > 300 && Width <= 400)
                {
                    value = (Width - (4 + padding));
                    value /= 2;
                }
                else
                    value = (Width - (1 + padding));

                Debug.WriteLine(Math.Floor(value));
                return Math.Floor(value);
            }
            catch
            {
                return 150;
            }
        }


        public static double GetOnlySearchCollectionPageAlbumWidth(double Width)
        {
            padding = 24;
            try
            {
                double value;
                if (Width > 2000)
                {
                    value = (Width - (31 + padding));
                    value /= 11;
                }
                else if (Width > 1800 && Width <= 2000)
                {
                    value = (Width - (28 + padding));
                    value /= 10;
                }
                else if (Width > 1600 && Width <= 1800)
                {
                    value = (Width - (25 + padding));
                    value /= 9;
                }
                else if (Width > 1400 && Width <= 1600)
                {
                    value = (Width - (22 + padding));
                    value /= 8;
                }
                else if (Width > 1200 && Width <= 1400)
                {
                    value = (Width - (19 + padding));
                    value /= 7;
                }
                else if (Width > 1000 && Width <= 1200)
                {
                    value = (Width - (16 + padding));
                    value /= 6;
                }
                else if (Width > 800 && Width <= 1000)
                {
                    value = (Width - (13 + padding));
                    value /= 5;
                }
                else if (Width > 600 && Width <= 800)
                {
                    value = (Width - (10 + padding));
                    value /= 4;
                }
                else if (Width > 400 && Width <= 600)
                {
                    value = (Width - (7 + padding));
                    value /= 3;
                }
                else if (Width > 300 && Width <= 400)
                {
                    value = (Width - (4 + padding));
                    value /= 2;
                }
                else
                    value = (Width - (1 + padding));

                Debug.WriteLine(Math.Floor(value));
                return Math.Floor(value);
            }
            catch
            {
                return 150;
            }
        }

    }
}

