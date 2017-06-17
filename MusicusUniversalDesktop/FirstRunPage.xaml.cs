
using Windows.Storage;
using Windows.UI.Xaml.Navigation;
using Musicus.LocalMusicHelpers;
using Musicus.Utilities;
using Musicus.Helpers;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Musicus.Helpers.BackgroundHelpers;

namespace Musicus
{
    public sealed partial class FirstRunPage 
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public FirstRunPage()
        {
            this.InitializeComponent();
        }

        public override void NavigatedTo(NavigationMode mode, object e)
        {
            base.NavigatedTo(mode, e);
            UiBlockerUtility.BlockNavigation();
            ScreenTimeoutHelper.PreventTimeout();
            App.Locator.PBar.IsFirstRun = true;
            isDone = false;
            GetMetadata();      
        }


        bool isDone;
        async void GetMetadata()
        {
            Bar.IsIndeterminate = true;

            LocalMusicHelper localMusicHelper = new LocalMusicHelper();
            var localMusic = await localMusicHelper.GetFilesInMusicAsync();

            var failedCount = 0;

            Bar.IsIndeterminate = false;

            App.Locator.CollectionService.Songs.SuppressEvents = true;
            App.Locator.CollectionService.Artists.SuppressEvents = true;
            App.Locator.CollectionService.Albums.SuppressEvents = true;

            Bar.Maximum = localMusic.Count;
            App.Locator.SqlService.BeginTransaction();
            
            for (var i = 0; i < localMusic.Count; i++)
            {
                Bar.Value = i + 1;
                try
                {
                    await localMusicHelper.SaveTrackAsync(localMusic[i]);
                }
                catch
                {
                    failedCount++;
                }
            }
            App.Locator.SqlService.Commit();

            App.Locator.CollectionService.Songs.Reset();
            App.Locator.CollectionService.Artists.Reset();
            App.Locator.CollectionService.Albums.Reset();


            UiBlockerUtility.Unblock();
            ScreenTimeoutHelper.AllowTimeout();
            if (App.Locator.Setting.SpotifyArtworkSync)
                await DownloadArtworks.DownloadArtistsArtworkAsyncFromSpotify();
            else
                await DownloadArtworks.DownloadArtistsArtworkAsync();

            await DownloadArtworks.DownloadAlbumsArtworkAsync();
            localSettings.Values[Core.PlayerConstants.UpdateAppString] = true;

            isDone = true;
            ButtonClick(null, null);
        }

        async void ButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isDone)
            {
                localSettings.Values[Core.PlayerConstants.UpdateAppString] = true;
                ScreenTimeoutHelper.AllowTimeout();
                UiBlockerUtility.Unblock();
                //DeleteTimer();
                await Task.Delay(1000);
            }

            App.Locator.PBar.IsFirstRun = false;
            //if(App.Locator.Setting.SafeStart)
                //Register();
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<HomePage, ZoomInTransition>(null, false);
                App.Locator.HomePage.SongColletionChanged();
            });
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
