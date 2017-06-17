using IF.Lastfm.Core.Objects;
using Musicus.Data.Collection.Model;
using Musicus.Helpers;
using Musicus.Utilities;
using Musicus.ViewModel.Mvvm.Dispatcher;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System;
using System.Linq;
using Musicus.Data.Model.WebData;
using Musicus.Data.Model.Advertisement;

namespace Musicus
{
    public sealed partial class HomePage 
    {
        int page = 0;
        public HomePage()
        {
            this.InitializeComponent();
        }

        public override void NavigatedTo(NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);
            FadeInAnimation.Begin();   
        }


        //public override void NavigatedFrom(NavigationMode mode)
        //{
        //    base.NavigatedFrom(mode);
        //   // _isActive = false;
        //   // timer.Stop();
        //}


        //private void AutoStart()
        //{
        //    int change = 1;
        //    timer.Interval = TimeSpan.FromSeconds(10);
        //    timer.Tick += (o, a) =>
        //    {
        //        if (_isActive && App.Locator.HomePage.TopArtists != null && App.Locator.HomePage.TopArtists.Count > 0)
        //        {
        //            if (TopArtistsFlipView.Items.Count == 0) return;
        //            int newIndex = TopArtistsFlipView.SelectedIndex + change;
        //            if (newIndex >= TopArtistsFlipView.Items.Count || newIndex < 0)
        //                change *= -1;
        //            TopArtistsFlipView.SelectedIndex += change;
        //        }
        //    };
        //    timer.Start();       
        //}

        private async void OpenSettings(object sender, RoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<SettingsPage, ZoomInTransition>(null);
            });
        }

        private async void OpenDownloads(object sender, RoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<DownloadManager, ZoomInTransition>(null);
            });
        }

        private async void OpenNowPlaying(object sender, RoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                AddDeleteShareManager.OpenNowPlaying();
            });
        }

        private async void OpenSongs(object sender, RoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<CollectionPage, ZoomInTransition>(0);
            });
        }

        private async void OpenArtists(object sender, RoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<CollectionPage, ZoomInTransition>(1);
            });
        }

        private async void OpenAlbums(object sender, RoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<CollectionPage, ZoomInTransition>(2);
            });
        }


        private async void OpenVideos(object sender, RoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<CollectionPage, ZoomInTransition>(3);
            });
        }


        private async void OpenPlaylists(object sender, RoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<CollectionPlaylistsPage, ZoomInTransition>(null);
            });
        }

        private async void OpenSearchHelper(object sender, RoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<SearchPage, ZoomInTransition>(null);
            });
        }

        private async void HistorySongClick(object sender, ItemClickEventArgs e)
        {
            Song s = e.ClickedItem as Song;
            var queueSongs = App.Locator.HomePage.History.ToList();
            int index = queueSongs.IndexOf(s);
            queueSongs = queueSongs.Skip(index).ToList();

            //var queueSongs = App.Locator.HomePage.History;
            if (queueSongs != null && queueSongs.Count > 0)
                await PlayAndQueueHelper.PlaySongsAsync(s, queueSongs, true);
        
        }

        private async void GoToArtists(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<SpotifyArtistPage, ZoomInTransition>((TopArtistsFlipView.SelectedItem as WebArtist).Name);
            });
        }

        private void PlayPauseToggle(object sender, RoutedEventArgs e)
        {
            App.Locator.AudioPlayerHelper.PlayPauseToggle();
        }

        private void NextFlyoutClicked(object sender, RoutedEventArgs e)
        {
            App.Locator.Player.NextSong();
        }

        private async void OpenNewAlbumPage(object sender, RoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<NewTrackAlbumPage, ZoomInTransition>(1);
            });
        }

        private async void OpenNewTrackPage(object sender, RoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<NewTrackAlbumPage, ZoomInTransition>(0);
            });
        }


        public void LoadMovies()
        {
              // = new ObservableCollection<Movie>(allMovies.GetRange(page * 5, 5));
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
            LoadMovies(false);
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            LoadMovies(true);
        }

        private void LoadMovies(bool nextPage)
        {
            if (nextPage)
            {
                page++;
                page = (page > 1) ? 0 : page;
            }
            else
            {
                page--;
                page = (page < 0) ? 1 : page;
            }
            LoadMovies();
        }

        private async void AlbumClicked(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Advert || e.ClickedItem is ListAdvert) return;
            var o = e.ClickedItem as WebAlbum;
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<DeezerAlbumPage, ZoomInTransition>(o);
            });
        }

        private async void ShowAllTopTracksClicked(object sender, RoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<NewTrackAlbumPage, ZoomInTransition>(0);
            });
        }

        private async void ShowAllTopAlbumsClicked(object sender, RoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<NewTrackAlbumPage, ZoomInTransition>(1);
            });
        }

    }

}



