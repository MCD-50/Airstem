
using IF.Lastfm.Core.Objects;
using Musicus.Data.Collection.Model;
using Musicus.Data.Model.WebData;
using Musicus.Helpers;
using Musicus.Utilities;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace Musicus
{
    public sealed partial class NowPlayingPage
    {
        private bool _seeking;
        public static bool _isActive = false;
        bool videoLoaded = false;
      

        public NowPlayingPage()
        {
            this.InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                MainPivot.Title = "NOW PLAYING";
                MainPivotItem1.Header = "playing";
                MainPivotItem2.Header = "similar";
                MainPivotItem3.Header = "lyrics";
            }


            Window.Current.SizeChanged += Current_SizeChanged;
            Current_SizeChanged(null, null);
        }


        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            var bound = Window.Current.Bounds;
            CompositeTranslate.TranslateX = bound.Width;
            FromValue.Value = - bound.Width;
        }



        public override void NavigatedTo(NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);
            _isActive = true;
            MainPivot.SelectedIndex = 0;

            if (App.Locator.Player.IsPlayerActive)
                ExecuteAsync();
            else
                App.Navigator.GoBack();
        }


        private async void ExecuteAsync()
        {
            SetImage();
            App.Locator.Player.InternalTrackChanged += AudioPlayerHelperTrackChanged;
            MainImageSlideIn.Begin();
            ShowVideo();
            await Task.Delay(1);
        }



        private void SetImage(Song song = null)
        {
            try
            {
                Song currentSong;
                if (song != null)
                    currentSong = song;
                else
                    currentSong = App.Locator.Player.CurrentSong;
                           
                if (App.Locator.Player.IsPlayerActive)
                {
                    if (currentSong.Artist.HasArtwork)
                    {
                        try
                        {
                            ArtistImage.ImageSource = new BitmapImage(currentSong.Artist.Artwork.Uri);
                        }
                        catch
                        {
                            ArtistImage.ImageSource = new BitmapImage(currentSong.Album.Artwork.Uri);
                        }
                    }

                    else
                    {
                        ArtistImage.ImageSource = new BitmapImage(currentSong.Album.Artwork.Uri);                      
                    }
                }
            }
            catch
            {

            }
                      
        }


        int id = -1;
        string url;
        async void ShowVideo(Song song = null)
        {
            Song currentSong;
            if (song != null)
                currentSong = song;
            else
                currentSong = App.Locator.Player.CurrentSong;

            if (id == currentSong.Id && videoLoaded) return;
            id = currentSong.Id;

         
            Hide();
            await Task.Delay(1000);

            if (!App.Locator.Network.IsActive) return;

            await Task.Factory.StartNew(async () =>
            {
                url = await App.Locator.VideoPlayer.YouTubeVideo(currentSong.Name, currentSong.ArtistName);
            });

            if (!string.IsNullOrEmpty(url))
            {
                videoLoaded = true;
                Show();
            }
            else
            {
                videoLoaded = false;
                Hide();
            }
           
        }

        void Hide()
        {
            HideVid.Begin();
            VideoToogleButton.Visibility = Visibility.Collapsed;
        }

        void Show()
        {
            VideoToogleButton.Visibility = Visibility.Visible;
            ShowVid.Begin();
        }


        async void GetArtwork()
        {
            if (App.Locator.Setting.SpotifyArtworkSync)
                await DownloadArtworks.DownloadArtistsArtworkAsyncFromSpotify();
            else
                await DownloadArtworks.DownloadArtistsArtworkAsync();
        }

        
        public async void AudioPlayerHelperTrackChanged(object sender, Song currentSong)
        {          
            await DispatcherHelper.RunAsync(() =>
            {         
                if (App.Locator.Player.IsPlayerActive && _isActive)
                {
                   SetImage(currentSong);
                    if (MainPivot.SelectedIndex == 1)
                        PrepareLoad(currentSong);
                    else if (MainPivot.SelectedIndex == 2)
                        LoadLyrics(currentSong);
                    ChangeSong.Begin();
                    ShowVideo();
                }
            });
        }


        private int loadId = -1;
        private void PrepareLoad(Song song = null)
        {
            if (!App.Locator.Network.IsActive)
            {
                App.Locator.NowPlaying.Message = Core.StringMessage.NoInternetConnection;
                return;
            }

            Song currentSong;
            if (song != null)
                currentSong = song;
            else
                currentSong = App.Locator.Player.CurrentSong;

            var songId = currentSong.Id;
            if (loadId == songId) return;
            loadId = songId;
            App.Locator.NowPlaying.LoadTopTracks(currentSong.Name, currentSong.ArtistName, loadId);
        }


        public override void NavigatedFrom(NavigationMode mode)
        {
            base.NavigatedFrom(mode);
            _isActive = false;
            App.Locator.Player.InternalTrackChanged -= AudioPlayerHelperTrackChanged;
        }


        private async void LoadLyrics(Song currentSong = null)
        {
            try
            {
                Song song;
                if (currentSong != null)
                    song = currentSong;
                else
                    song = App.Locator.Player.CurrentSong;

               
                if (LyricsTextBlock.Tag as int? == song.Id) return;
                LyricsTextBlock.Tag = song.Id;

                string o = App.Locator.SqlService.GetLyrics(song.Name, song.ArtistName);
                if (!string.IsNullOrEmpty(o))
                {
                    LyricsTextBlock.Text = o;
                    return;
                }

                if (!App.Locator.Network.IsActive)
                {
                    LyricsTextBlock.Text = Core.StringMessage.NoInternetConnection;
                    return;
                }


                LyricsTextBlock.Text = Core.StringMessage.LoadingPleaseWait;

                string results = await App.Locator.Mp3Search.GetMetroLyrics(song.Name, song.ArtistName);
                if (!string.IsNullOrEmpty(results))
                {
                    LyricsTextBlock.Text = results;
                    if (App.Locator.Setting.LyricsOnOrOff)
                    {
                        await Task.Delay(1000);
                        await App.Locator.SqlService.InsertAsync(new Lyrics(song.Name, song.ArtistName, results));
                    }
                }
                else
                {
                    LyricsTextBlock.Text = Core.StringMessage.EmptyMessage;
                }

                if (!string.IsNullOrEmpty(results)) results = null;
            }

            catch
            {
               LyricsTextBlock.Text = Core.StringMessage.SomethinWentWrong;
            }
        }


        private void Slider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _seeking = true;
            BackgroundMediaPlayer.Current.Pause();
        }

        private void Slider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _seeking = false;
            BackgroundMediaPlayer.Current.Position = App.Locator.Player.Position;
            BackgroundMediaPlayer.Current.Play();
        }


        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            try
            {
                if (DataContext == null || App.Locator.Player.IsLoading || BackgroundMediaPlayer.Current.NaturalDuration.TotalSeconds == 0)
                {
                    return;
                }

                var diff = App.Locator.Player.Position.TotalSeconds - BackgroundMediaPlayer.Current.Position.TotalSeconds;
                if (!(diff > 10) && !(diff < -10)) return;

                if (_seeking) return;

                BackgroundMediaPlayer.Current.Position = App.Locator.Player.Position;
                BackgroundMediaPlayer.Current.Play();
            }

            catch (Exception)
            { }
        }


        private void OpenQueue(object sender, RoutedEventArgs e)
        {
            SheetUtility.OpenNowPlaying();
        }

        private async void SimilarArtistsClick(object sender, ItemClickEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                var artist = e.ClickedItem as WebArtist;
                App.Navigator.GoTo<SpotifyArtistPage, ZoomInTransition>(artist.Name);
            });
        }

        private void NextButtonClick(object sender, RoutedEventArgs e)
        {
            App.Locator.Player.NextSong();
        }

        private void PlayPasueButtonClick(object sender, RoutedEventArgs e)
        {
            App.Locator.Player.PlayPauseToggle();
        }

        private void PrevButtonClick(object sender, RoutedEventArgs e)
        {
            App.Locator.Player.PrevSong();
        }

        private void NowPlayingPageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainPivot.SelectedIndex == 1)
            {
                if(!BorderImage.Opacity.Equals(0.5))
                    PivotChanged.Begin();
                PrepareLoad();
                
            }
            else if (MainPivot.SelectedIndex == 2)
            {
                if (!BorderImage.Opacity.Equals(0.5))
                    PivotChanged.Begin();
                LoadLyrics();
            }

            else if(!BorderImage.Opacity.Equals(1.0))
                PivotChangedReverse.Begin();
        }

        private async void DiscoverFlyoutClicked(object sender, RoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<SpotifyArtistPage, ZoomInTransition>(App.Locator.Player.CurrentSong.ArtistName);
            });
        }

        private void AddToPlaylistFlyoutClick(object sender, RoutedEventArgs e)
        {
            AddToPlaylistPage.songs = new List<Song>() { App.Locator.Player.CurrentSong };
            SheetUtility.OpenAddToPlaylistPage();
        }

        void OpenVideo(object sender, RoutedEventArgs e)
        {
            if (BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing
                || BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Paused)
            {
                if (App.Locator.Network.IsActive)
                {
                    App.Locator.VideoPlayer.InvokeOnline(url);     
                }
            }

            else
            {
                MessageHelpers.ShowError("Media player state must be either Playing or Paused", "Media playback error");
            }
        }

        private void ShowFlyout(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(PlayTrackStackPanel);
        }

        private async void CancelFlyoutClicked(object sender, RoutedEventArgs e)
        {
            Song _song = App.Locator.Player.CurrentSong;
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

        private void DownloadFlyoutClicked(object sender, RoutedEventArgs e)
        {
            App.Locator.Download.StartDownloadAsync(App.Locator.Player.CurrentSong);
        }
    }
}