
using Musicus.Data.Collection.Model;
using Musicus.Helpers;
using Musicus.LocalMusicHelpers;
using Musicus.Utilities;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System.Linq;
using System.Threading.Tasks;
using System;
using Windows.UI.Xaml.Controls.Primitives;

namespace Musicus
{
    public sealed partial class ImportRefreshPage : IModalSheetPage
    {
        public ImportRefreshPage()
        {
            this.InitializeComponent();
        }

        public int index = 1;

        public Popup Popup { get; private set; }
        public void OnOpened(Popup popup)
        {
            Bar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            Popup = popup;
            ExecuteRemaining();
        }

        async void ExecuteRemaining()
        {
            await Task.Delay(30);

            if (index == 1)
            {
                Bar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                Heading.Text = "Looking for music...";
                ImportMusic();
            }
            else if (index == 2)
            {
                Bar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                Heading.Text = "Looking for videos...";
                ImportVideos();
            }
            else
            {
                Bar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                Heading.Text = "Refreshing airstem...";
                Refresh();
            }
        }

        public void OnClosed()
        {
            Popup = null;
        }

        async void Refresh()
        {
            await Task.Factory.StartNew(async () =>
            {
                await DispatcherHelper.RunAsync(async () =>
                {
                    Message.Text = "Preparing...";
                    ScreenTimeoutHelper.PreventTimeout();
                    var importedSongs = App.Locator.CollectionService.Songs.Where(p => p.SongState == SongState.Local
                          || p.SongState == SongState.DownloadListed
                          || p.SongState == SongState.Downloaded).ToList();

                    var importedVideos = App.Locator.CollectionService.Videos.ToList();

                    App.Locator.SqlService.BeginTransaction();

                    if (importedSongs.Count > 0)
                    {
                        Message.Text = "Deleting tracks.";
                        foreach (var song in importedSongs)
                        {
                            try
                            {
                                await App.Locator.CollectionService.DeleteSongAsync(song);
                            }
                            catch
                            {
                                //ignored;
                            }
                        }

                    }

                    if (importedVideos.Count() > 0)
                    {
                        Message.Text = "Deleting videos.";
                        foreach (var video in importedVideos)
                        {
                            try
                            {
                                await App.Locator.CollectionService.DeleteVideoAsync(video);
                            }
                            catch
                            {
                                //ignored;
                            }
                        }
                    }
                    App.Locator.SqlService.Commit();
                });
            });


            Bar.Visibility = Windows.UI.Xaml.Visibility.Visible;

            await Task.Delay(50);

            await Task.Factory.StartNew(async () =>
            {
                await DispatcherHelper.RunAsync(() =>
                {
                    ImportMusic(true);
                });
            });
        }


        async void ImportMusic(bool importVideo = false)
        {
            ScreenTimeoutHelper.PreventTimeout();

            Bar.IsIndeterminate = true;

            Message.Text = "Collecting information about music.";

            LocalMusicHelper localMusicHelper = new LocalMusicHelper();
            var localMusic = await localMusicHelper.GetFilesInMusicAsync();

            var failedCount = 0;

            Bar.IsIndeterminate = false;

            App.Locator.CollectionService.Songs.SuppressEvents = true;
            App.Locator.CollectionService.Artists.SuppressEvents = true;
            App.Locator.CollectionService.Albums.SuppressEvents = true;

            await Task.Delay(10);
            Message.Text = "Working on music files.";

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

            if (importVideo)
            {
                if (App.Locator.Setting.SpotifyArtworkSync)
                    await DownloadArtworks.DownloadArtistsArtworkAsyncFromSpotify();
                else
                    await DownloadArtworks.DownloadArtistsArtworkAsync();
                await DownloadArtworks.DownloadAlbumsArtworkAsync();
                ImportVideos();
            }  
            else
            {
                if (App.Locator.Setting.SpotifyArtworkSync)
                    await DownloadArtworks.DownloadArtistsArtworkAsyncFromSpotify();
                else
                    await DownloadArtworks.DownloadArtistsArtworkAsync(); await DownloadArtworks.DownloadAlbumsArtworkAsync();
                ScreenTimeoutHelper.AllowTimeout();
                SheetUtility.CloseImportRefreshPage();
            }

            App.Locator.HomePage.SongColletionChanged();

        }


        async void ImportVideos()
        {
            ScreenTimeoutHelper.PreventTimeout();
         
            Message.Text = "Collecting information about videos.";
            Bar.IsIndeterminate = true;

            var failedCount = 0;

            LocalMusicHelper localMusicHelper = new LocalMusicHelper();
            var localVideos = await localMusicHelper.GetFilesInVideoAsync();
       
            App.Locator.CollectionService.Videos.SuppressEvents = true;

            Bar.IsIndeterminate = false;

            await Task.Delay(10);

            Message.Text = "Working on video files.";        
            Bar.Maximum = localVideos.Count;

            App.Locator.SqlService.BeginTransaction();
            for (var i = 0; i < localVideos.Count; i++)
            {
                Bar.Value = i + 1;
                try
                {
                    await localMusicHelper.SaveVideoAsync(localVideos[i]);
                }
                catch
                {
                    failedCount++;
                }
            }

            App.Locator.SqlService.Commit();
            App.Locator.CollectionService.Videos.Reset();

            ScreenTimeoutHelper.AllowTimeout();
            SheetUtility.CloseImportRefreshPage();
        }
    }
}
