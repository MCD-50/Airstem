
using Musicus.TransitionHelpers;
using Musicus.ViewModel.Mvvm.Dispatcher;
using Windows.Storage;
using System;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using Musicus.Helpers;
using System.Diagnostics;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Musicus.Extras;
using Musicus.Helpers.BackgroundHelpers;
using Windows.ApplicationModel.Background;

namespace Musicus
{
    public sealed partial class RootPage
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public RootPage()
        {
            this.InitializeComponent();

            //InitializeADealsAds(); 

            App.Navigator = new Navigator(this, LayoutRoot);

            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                //(Application.Current.Resources["MusicusVideoColor"] as SolidColorBrush).Color = Colors.White;

                (Application.Current.Resources["MusicusLowColor"] as SolidColorBrush).Color = Colors.Transparent;
            }
            ColorPicker _colorPicker = new ColorPicker();
            try
            {
                var indexString = App.Locator.AppSettingsHelper.Read<int>(Core.PlayerConstants.AppThemeIndex);
                if (indexString < 1)
                    _colorPicker.ChangeColor(index: 1);
                else
                    _colorPicker.ChangeColor(index: indexString);
            }
            catch
            {
                _colorPicker.ChangeColor(index: 1);
            }

            Window.Current.SizeChanged += Current_SizeChanged;
            Current_SizeChanged(null, null);



            if(!App.Locator.Setting.TurboMode)
            {
                App.Navigator.AddPage(new FirstRunPage());
                App.Navigator.AddPage(new HomePage());
                App.Navigator.AddPage(new CollectionPage());
                App.Navigator.AddPage(new CollectionAlbumPage());
                App.Navigator.AddPage(new CollectionArtistPage());
                App.Navigator.AddPage(new CollectionPlaylistPage());
                App.Navigator.AddPage(new DownloadManager());
                App.Navigator.AddPage(new NowPlayingPage());
                App.Navigator.AddPage(new SettingsPage());
                App.Navigator.AddPage(new VideoInformationPage());
                App.Navigator.AddPage(new SpotifyAlbumPage());
                App.Navigator.AddPage(new SpotifyArtistPage());
                App.Navigator.AddPage(new DeezerAlbumPage());
                App.Navigator.AddPage(new SearchPage());
                App.Navigator.AddPage(new CollectionPlaylistsPage());
                App.Navigator.AddPage(new NewTrackAlbumPage());
            }

           
        }

        //async void InitializeADealsAds()
        //{
        //    await AdDealsUniversalSDKW81.AdManager.InitSDK(this.LayoutRoot, "2611", "K7ECLHR5EH9H");
        //}

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            var bound = Window.Current.Bounds;
            if (e == null)
            {
                Debug.WriteLine(bound.Width);
                double width = App.GetWidth(bound.Width);
                double videoWidth = App.GetVideoWidth(bound.Width);
                double onlineWidth = App.GetOnlineWidth(bound.Width);
                double onlyspawidth = App.GetOnlySearchCollectionPageAlbumWidth(bound.Width);

                (Application.Current.Resources["WidthConverter"] as DoubleWrapper).OnlineWidth = onlineWidth;
                (Application.Current.Resources["WidthConverter"] as DoubleWrapper).OnlineHeight = onlineWidth - 100;
                (Application.Current.Resources["WidthConverter"] as DoubleWrapper).ExactWidth = width;
                (Application.Current.Resources["WidthConverter"] as DoubleWrapper).ExactHeight = width + 70;
                (Application.Current.Resources["WidthConverter"] as DoubleWrapper).AlbumHeight = videoWidth + 70;
                (Application.Current.Resources["WidthConverter"] as DoubleWrapper).VideoWidth = videoWidth;
                (Application.Current.Resources["WidthConverter"] as DoubleWrapper).VideoHeight = videoWidth + 100;

                (Application.Current.Resources["WidthConverter"] as DoubleWrapper).OnlySearchPageAlbumHeight = onlyspawidth + 70;
                (Application.Current.Resources["WidthConverter"] as DoubleWrapper).OnlySearchPageAlbumWidth = onlyspawidth;

            }
            else
            {
                Debug.WriteLine(e.Size.Width);
                double width = App.GetWidth(e.Size.Width);
                double videoWidth = App.GetVideoWidth(e.Size.Width);
                double onlineWidth = App.GetOnlineWidth(e.Size.Width);
                double onlyspawidth = App.GetOnlySearchCollectionPageAlbumWidth(e.Size.Width);


                (Application.Current.Resources["WidthConverter"] as DoubleWrapper).OnlineWidth = onlineWidth;
                (Application.Current.Resources["WidthConverter"] as DoubleWrapper).OnlineHeight = onlineWidth - 100;
                (Application.Current.Resources["WidthConverter"] as DoubleWrapper).ExactWidth = width;
                (Application.Current.Resources["WidthConverter"] as DoubleWrapper).ExactHeight = width + 70;
                (Application.Current.Resources["WidthConverter"] as DoubleWrapper).AlbumHeight = videoWidth + 70;
                (Application.Current.Resources["WidthConverter"] as DoubleWrapper).VideoWidth = videoWidth;
                (Application.Current.Resources["WidthConverter"] as DoubleWrapper).VideoHeight = videoWidth + 100;


                (Application.Current.Resources["WidthConverter"] as DoubleWrapper).OnlySearchPageAlbumHeight = onlyspawidth + 70;
                (Application.Current.Resources["WidthConverter"] as DoubleWrapper).OnlySearchPageAlbumWidth = onlyspawidth;

            }

        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (localSettings.Values.Keys.Contains(Core.PlayerConstants.UpdateAppString) != true)
            {
                //if (App.Locator.Setting.SafeStart)
                   // BackgroundTaskUtility.UnregisterBackgroundTasks(BackgroundTaskUtility.ServicingCompleteTaskName);
                await DispatcherHelper.RunAsync(() =>
                {
                    App.Navigator.GoTo<FirstRunPage, PageTransition>(null, false);
                });
            }
            else
            {
                //if (App.Locator.Setting.SafeStart)
                    //Register();

                await DispatcherHelper.RunAsync(() =>
                {
                    App.Navigator.GoTo<HomePage, PageTransition>(null);
                });
            }
        }


        private void Register()
        {
            var taskRegistered = false;
            var exampleTaskName = "AudioTask";

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == exampleTaskName)
                {
                    taskRegistered = true;
                    break;
                }
            }

            if (!taskRegistered)
            {
                BackgroundTaskUtility.RegisterBackgroundTask(
                    BackgroundTaskUtility.ServicingCompleteTaskEntryPoint, BackgroundTaskUtility.ServicingCompleteTaskName,
                    new SystemTrigger(SystemTriggerType.ServicingComplete, false), null);
            }
        }



    }
}
















