#region

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Musicus.Core;
using Musicus.Data.Collection.Model;
using Musicus.Helpers;
using System.Linq;
using Musicus.Core.WinRt.Common;
using Windows.Foundation;


#endregion

namespace Musicus
{
    public sealed partial class NowPlayingSheet : IModalSheetPage
    {

        public NowPlayingSheet()
        {
            this.InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                MainPivot.Title = "NOW PLAYING";
                MainPivotItem1.Header = "queue";
            }
        }
   
        private void CurrentQueueViewOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            LoadData();
        }

        private void LoadData(Song currentSong = null)
        {
            QueueSong queueSong;

            if (currentSong != null)
                queueSong = ConvertTrackToQueueTrack(currentSong);
            else
                queueSong = ConvertTrackToQueueTrack(App.Locator.Player.CurrentSong);

            CurrentQueueView.SelectedItem = queueSong;
            CurrentQueueView.ScrollIntoView(CurrentQueueView.SelectedItem);
        }

        public Popup Popup { get; private set; }
        public void OnOpened(Popup popup)
        {
            Popup = popup;
            CurrentQueueView.Loaded += CurrentQueueViewOnLoaded;
            App.Locator.Player.InternalTrackChanged += AudioPlayerHelperOnTrackChanged;
            App.Navigator.ShowBack();
        }

        private void AudioPlayerHelperOnTrackChanged(object sender, Song currentSong)
        {
            LoadData(currentSong);
        }

        public void OnClosed()
        {
            Popup = null;
            Clear();
            if (App.Navigator.StackCount > 0)return;
            App.Navigator.HideBack();
        }

        public void Clear()
        {
            CurrentQueueView.Loaded -= CurrentQueueViewOnLoaded;
            App.Locator.Player.InternalTrackChanged += AudioPlayerHelperOnTrackChanged;
            CurrentQueueView.SelectedItem = null;
        }

        QueueSong ConvertTrackToQueueTrack(Song o)
        {
            return App.Locator.CollectionService.CurrentPlaybackQueue.FirstOrDefault(tr => tr.SongId == o.Id);
        }


        private void CurrentQueueView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var song = CurrentQueueView.SelectedItem as QueueSong;
            if (song == null) return;
            var currentPlayingId = App.Locator.AppSettingsHelper.Read<int>(PlayerConstants.CurrentTrack);
            if (e.RemovedItems.Count != 0 && song.SongId != currentPlayingId)
                App.Locator.AudioPlayerHelper.PlaySong(song);

            // App.Locator.AudioPlayerHelper.PlaySong(song.SongId , startPlayback : false);
        }

        private void RemoveFlyoutClicked(object sender, RoutedEventArgs e)
        {
            var queueList = CurrentQueueView.Items.Cast<QueueSong>().ToList();
            if (queueList.Count == 1)
            {
                ToastManager.ShowError("Something went wrong.");
                return;
            }
            QueueSong song = (sender as MenuFlyoutItem).DataContext as QueueSong;
            if(App.Locator.Player.CurrentSong.Id == song.SongId)
            {
                ToastManager.ShowError("Cannot remove.");
                return;
            }
            App.Locator.CollectionService.DeleteFromQueueAsync(song);
        }

        
    }
}
