#region
using Musicus.Data.Collection.Model;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using System.Collections.Generic;
using Musicus.Utilities;
using Musicus.Helpers;
using System.Linq;
using Musicus.ViewModel.Mvvm.Dispatcher;
using Musicus.Core.WinRt.Common;


#endregion

namespace Musicus
{
    public sealed partial class SongViewer : UserControl
    {
        private bool _playlistMode;
        private bool _queueMode;
        private Song _song;


        public static bool _showFlyout = true;

        public SongViewer()
        {
            this.InitializeComponent();
        }


        public void ClearData()
        {
            _queueMode = true;
            _playlistMode = true;
            _song = null;
            SongNameTextBlock.ClearValue(TextBlock.TextProperty);
        
            ArtistNameTextBlock.ClearValue(TextBlock.TextProperty);
            AlbumNameTextBlock.ClearValue(TextBlock.TextProperty);
            DataContext = null;
        }

        public void ShowDownload()
        {
            if (_queueMode) return;
            DownloadProgressGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// This method visualizes the placeholder state of the data item. When
        /// showing a placehlder, we set the opacity of other elements to zero
        /// so that stale data is not visible to the end user.
        /// </summary>
        /// <param name="song">The song.</param>
        /// <param name="queueMode">if set to <c>true</c> [queue mode].</param>
        /// <param name="playlistMode">if set to <c>true</c> [playlist mode].</param>
        public void ShowPlaceholder(Song song, bool queueMode = false, bool playlistMode = false)
        {
            _playlistMode = playlistMode;
            _queueMode = queueMode;

            //DownloadOptionGrid.Visibility = Visibility.Collapsed;
            DownloadProgressGrid.Visibility = Visibility.Collapsed;

            DataContext = song;
            _song = song;
            SongNameTextBlock.Opacity = 0;
            ArtistNameTextBlock.Opacity = 0;
            AlbumNameTextBlock.Opacity = 0;

            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                //ArtistNameTextBlock.Opacity = 0;
                ArtistBorbderAlbum.Opacity = 0;
                //AlbumNameTextBlock.Opacity = 0;
            }
            //else
            //    ArtistAlbumNameTextBlock.Opacity = 0;
        }

        //Holding="Song_OnHolding"
        /// <summary>
        /// Visualize artist information by updating the correct TextBlock and
        /// setting Opacity to 1.
        /// </summary>
        /// <param name="withAlbumName">if set to <c>true</c> [with album name].</param>
        public void ShowTitle()
        {
            SongNameTextBlock.Text = _song.Name;
            SongNameTextBlock.Opacity = 1;
        }


        public void ShowRest(bool withAlbumName = true)
        {

            try
            {
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                {

                    if (!string.IsNullOrEmpty(_song.Artist.Name) && !string.IsNullOrEmpty(_song.Album.Name))
                    {

                        ArtistNameTextBlock.Text = _song.Artist.Name;
                        AlbumNameTextBlock.Text = _song.Album.Name;

                    }

                    else if (string.IsNullOrEmpty(_song.ArtistName) && !string.IsNullOrEmpty(_song.Album.Name))
                    {

                        ArtistNameTextBlock.Text = "Unknown Artist";
                        AlbumNameTextBlock.Text = _song.Album.Name;

                    }

                    else if (!string.IsNullOrEmpty(_song.ArtistName) && string.IsNullOrEmpty(_song.Album.Name))
                    {

                        ArtistNameTextBlock.Text = _song.Artist.Name;
                        AlbumNameTextBlock.Text = "Unknown Album";
                    }

                    else
                    {
                        ArtistNameTextBlock.Text = "Unknown Artist";
                        AlbumNameTextBlock.Text = "Unknown Album";
                    }

                    ArtistNameTextBlock.Opacity = 1;
                    ArtistBorbderAlbum.Opacity = 1;
                    AlbumNameTextBlock.Opacity = 1;
                }
                else
                {
                    if (!string.IsNullOrEmpty(_song.Artist.Name) && !string.IsNullOrEmpty(_song.Album.Name))
                    {

                        ArtistNameTextBlock.Text = _song.Artist.Name;
                        AlbumNameTextBlock.Text = _song.Album.Name;

                    }

                    else if (string.IsNullOrEmpty(_song.ArtistName) && !string.IsNullOrEmpty(_song.Album.Name))
                    {

                        ArtistNameTextBlock.Text = "Unknown Artist";
                        AlbumNameTextBlock.Text = _song.Album.Name;

                    }

                    else if (!string.IsNullOrEmpty(_song.ArtistName) && string.IsNullOrEmpty(_song.Album.Name))
                    {

                        ArtistNameTextBlock.Text = _song.Artist.Name;
                        AlbumNameTextBlock.Text = "Unknown Album";
                    }

                    else
                    {
                        ArtistNameTextBlock.Text = "Unknown Artist";
                        AlbumNameTextBlock.Text = "Unknown Album";
                    }

                    ArtistNameTextBlock.Opacity = 1;
                    AlbumNameTextBlock.Opacity = 1;
                    //if (!string.IsNullOrEmpty(_song.ArtistName) && !string.IsNullOrEmpty(_song.Album.Name))
                    //{
                    //    ArtistAlbumNameTextBlock.Text = _song.Artist.Name;
                    //    ArtistAlbumNameTextBlock.Opacity = 1;
                    //}


                    //else if (string.IsNullOrEmpty(_song.ArtistName))
                    //{
                    //    ArtistAlbumNameTextBlock.Text = "Unknown Artist";
                    //    ArtistAlbumNameTextBlock.Opacity = 1;
                    //}

                }
            }

            catch (Exception)
            { }

            ShowDownload();
        }



        #region Song user events

        private void Song_OnHolding(object sender, HoldingRoutedEventArgs e)
        {
            if (!_showFlyout) return;
            FlyoutBase.ShowAttachedFlyout(RootGrid);
            FlyoutToggle();
        }

        private void Right_Tapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (!_showFlyout) return;
            FlyoutBase.ShowAttachedFlyout(RootGrid);
            FlyoutToggle();
        }
   
        private void FlyoutToggle()
        {
            if (_playlistMode)
            {
                RemovePlaylistTrack.Visibility = Visibility.Visible;
                PlayTrack.Visibility = Visibility.Visible;
                DeleteOption.Visibility = Visibility.Collapsed;
                DiscoverArtistOption.Visibility = Visibility.Collapsed;
                ManualMatch.Visibility = Visibility.Collapsed;
                FlyoutSubMenu.Visibility = Visibility.Collapsed;
                Share.Visibility = Visibility.Collapsed;
            }

            else
            {
                RemovePlaylistTrack.Visibility = Visibility.Collapsed;
                PlayTrack.Visibility = Visibility.Visible;
                DeleteOption.Visibility = Visibility.Visible;
                DiscoverArtistOption.Visibility = Visibility.Visible;
                ManualMatch.Visibility = Visibility.Visible;
                FlyoutSubMenu.Visibility = Visibility.Visible;
                Share.Visibility = Visibility.Visible;
            }
        }


        private void MenuFlyout_Opened(object sender, object e)
        {
            if (_song.IsDownload)
            {
                if (_song.SongState != SongState.NoMatch 
                    && _song.SongState != SongState.Matching 
                    && App.Locator.Network.IsActive)
                    PlayTrack.IsEnabled = true;
                else
                    PlayTrack.IsEnabled = false;
            }
            else
                PlayTrack.IsEnabled = true;

            //PlayTrack.IsEnabled = !_song.IsMatched;
            Share.IsEnabled = !_song.IsDownload;
            DiscoverArtistOption.IsEnabled = _song.IsMatched;
            ManualMatch.IsEnabled = !(_song.SongState == SongState.Downloading);
            FlyoutSubMenu.IsEnabled = _song.IsMatched;
            //AddToPlaylistOption.IsEnabled = _song.IsMatched;
            //AddToQueueOption.IsEnabled = _song.IsMatched;
            
        }

        private void AddToMenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            AddToPlaylistPage.songs = new List<Song>() { _song };
            SheetUtility.OpenAddToPlaylistPage();
        }

        private void DeleteMenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            App.Locator.Collection.Commands.DeleteClickCommand.Execute(_song);
        }

        private void DownloadButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            App.Locator.Download.StartDownloadAsync(_song);
        }

        private async void RemovePlaylistClick(object sender, RoutedEventArgs e)
        {
            var song = App.Locator.CollectionPlaylist.Playlist.Songs.Where(p => p.Song.Id == _song.Id).FirstOrDefault();
            await App.Locator.CollectionService.DeleteFromPlaylistAsync(App.Locator.CollectionPlaylist.Playlist, song);
            if (song != null) song = null;
        }


        private async void CancelButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_song.Download != null)
                    App.Locator.Download.Cancel(_song);
                else
                {
                    _song.SongState = SongState.DownloadListed;
                    await App.Locator.SqlService.UpdateItemAsync(_song).ConfigureAwait(false);
                }
            }
            catch
            {
                _song.SongState = SongState.DownloadListed;
                await App.Locator.SqlService.UpdateItemAsync(_song).ConfigureAwait(false);
            }
        }

        #endregion

        //rematch
        private async void ManualMatch_Click(object sender, RoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                SheetUtility.OpenManualMatchPage(_song);
            });
        }

        //share  
        private void ShareClick(object sender, RoutedEventArgs e)
        {
            ShareSongHelper.ShareSong(new List<Song>() { _song });
        }

        //discover artist
        private async void DiscoverMenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<SpotifyArtistPage, ZoomInTransition>(_song.Artist.Name);
            });
        }



        //add to queue
        private async void AddToQueueFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (App.Locator.Player.IsPlayerActive)
            {
                if (!_song.IsMatched)
                {
                    ToastManager.ShowError("Can not add.");
                    return;
                }

                await PlayAndQueueHelper.AddToQueueAsync(_song);
            }
            else ToastManager.ShowError("Play any song before.");
                

            //var currentTrack = App.Locator.Player.CurrentSong;
            //var queue = App.Locator.CollectionService.CurrentPlaybackQueue;
            //if (currentTrack.SongId == _song.Id) return;
            //if (queue.FirstOrDefault(p => p.SongId == _song.Id) != null) return;
            //if (App.Locator.Player.IsPlayerActive)
            //{
            //    App.Locator.AudioPlayerHelper.AddToQueueAsync(_song);
            //    QueueSong newQueueSong = new QueueSong
            //    {
            //        SongName = _song.Name,
            //        ArtistName = _song.ArtistName,
            //        AlbumUri = _song.Album.Artwork.Uri,
            //        IsStreaming = _song.IsStreaming,
            //        AudioUrl = _song.AudioUrl,
            //        SongId = _song.Id
            //    };

            //    MessageService.SendMessageToBackground(new AddToPlaylistMessage(new List<QueueSong> { newQueueSong }));
            //}
        }

        async void PlayClick(object sender, RoutedEventArgs e)
        {
            await PlayAndQueueHelper.PlaySongAsync(_song);
        }

        private void AddANewPlaylist_OnClick(object sender, RoutedEventArgs e)
        {
            SheetUtility.OpenAddAPlaylistPage();
        }

        //private void SelectClick(object sender, RoutedEventArgs e)
        //{
        //    App.Locator.PBar.Invoke();
        //}
    }
}