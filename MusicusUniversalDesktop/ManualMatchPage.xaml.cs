
using Musicus.Data.Collection.Model;
using Musicus.Data.Model.WebSongs;
using Musicus.Helpers;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Musicus
{
    public sealed partial class ManualMatchPage : IModalSheetPage
    {

        public ManualMatchPage()
        {
            this.InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                MainPivot.Title = "REMATCH";
                MainPivotItem1.Header = "soundcloud";
                MainPivotItem2.Header = "viez";
                MainPivotItem3.Header = "waptrack";
                MainPivotItem4.Header = "pleer";
                MainPivotItem5.Header = "mp3z";
            }
        }

        public Popup Popup { get; private set; }
        public void OnOpened(Popup popup)
        {
            Popup = popup;
            App.Navigator.ShowBack();
            LoadData();
        }

        void LoadData()
        {
             App.Locator.Manual.ReceiveSong(ModalSheetUtility.PassingObject as Song);
        }

        public void OnClosed()
        {
            Popup = null;
         
            if (App.Navigator.StackCount <= 0)
                App.Navigator.HideBack();

            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            {
                var player = App.Locator.AudioPlayerHelper.SafeMediaPlayer;
                if (player.CurrentState == MediaPlayerState.Paused && forcedPause)
                    player.Play();
                forcedPause = false;
            }

            ClearData();
        }

        async void ClearData()
        {
            await Task.Delay(1000);
            await Task.Factory.StartNew(async () =>
            {
                await DispatcherHelper.RunAsync(() =>
                {
                    App.Locator.Manual.CleanManualMatchViewModel();
                });
            });
        
            Dispose();

        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }


        bool forcedPause = false;
        //private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        //{
        //    var clickedItem = e.ClickedItem as WebSong;
        //    if (clickedItem.IsLinkDeath)
        //    {
        //        Core.WinRt.Common.ToastManager.ShowError("Link dead.");
        //        return;
        //    }
        //    var song = App.Locator.Manual.CurrentSong; 

        //    song.AudioUrl = clickedItem.AudioUrl;
        //    song.CloudId = clickedItem.AudioUrl;
        //    song.RadioId = clickedItem.ProviderNumber;
        //    song.SongState = SongState.DownloadListed;
        //    await App.Locator.SqlService.UpdateItemAsync(song);
        //    SheetUtility.CloseManualMatchPage();
        //}

        //private async void PlayClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        //{
        //    var item = (sender as Button).DataContext as WebSong;
        //    if (item == null) return;
        //    if (item.IsLinkDeath)
        //    {
        //        Core.WinRt.Common.ToastManager.ShowError("Link dead.");
        //        return;
        //    }

        //    var player = App.Locator.AudioPlayerHelper.SafeMediaPlayer;
        //    if (player.CurrentState == MediaPlayerState.Playing)
        //    {
        //        if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
        //            await App.Locator.AudioPlayerHelper.ShutdownPlayerAsync();

        //        else
        //        {
        //            forcedPause = true;
        //            player.Pause();
        //        }
                
        //    }

        //    PlaybackPlayer.Source = new Uri(item.AudioUrl);
        //    PlaybackPlayer.Play();

        //}

        private async void PlayClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var item = (sender as MenuFlyoutItem).DataContext as WebSong;
            if (item == null) return;
            if (item.IsLinkDeath)
            {
                Core.WinRt.Common.ToastManager.ShowError("Link dead.");
                return;
            }

            var player = App.Locator.AudioPlayerHelper.SafeMediaPlayer;
            if (player.CurrentState == MediaPlayerState.Playing)
            {
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                    await App.Locator.AudioPlayerHelper.ShutdownPlayerAsync();

                else
                {
                    forcedPause = true;
                    player.Pause();
                }

            }

            PlaybackPlayer.Source = new Uri(item.AudioUrl);
            PlaybackPlayer.Play();

        }

        private async void SaveClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var clickedItem = (sender as MenuFlyoutItem).DataContext as WebSong;
            if (clickedItem == null) return;
            if (clickedItem.IsLinkDeath)
            {
                Core.WinRt.Common.ToastManager.ShowError("Link dead.");
                return;
            }

            var song = App.Locator.Manual.CurrentSong;

            song.AudioUrl = clickedItem.AudioUrl;
            song.CloudId = clickedItem.AudioUrl;
            song.RadioId = clickedItem.ProviderNumber;
            song.SongState = SongState.DownloadListed;
            await App.Locator.SqlService.UpdateItemAsync(song);
            SheetUtility.CloseManualMatchPage();
        }
    }
}