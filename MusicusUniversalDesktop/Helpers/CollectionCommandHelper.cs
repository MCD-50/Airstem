
using Musicus.Data.Collection;
using Musicus.Data.Collection.Model;
using Musicus.Utilities;
using Musicus.ViewModel.Mvvm.Commands;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Musicus.Helpers
{
    public class CollectionCommandHelper
    {
        private readonly ISongDownloadService _downloadService;
        private readonly ICollectionService _service;
        private readonly ISqlService _sqlService;

        public CollectionCommandHelper(ICollectionService service, ISqlService sqlService, ISongDownloadService downloadService)
        {
            _service = service;
            _sqlService = sqlService;
            _downloadService = downloadService;
            CreateCommand();
        }

        public Command<ItemClickEventArgs> PlaylistClickCommand { get; set; }
        public Command<ItemClickEventArgs> AlbumClickCommand { get; set; }
        public Command<ItemClickEventArgs> ArtistClickCommand { get; set; }
        
        public Command<Song> CancelClickCommand { get; set; }
        public Command<Song> DownloadClickCommand { get; set; }
        public Command<BaseEntry> PlayTrackCommand { get; set; }
        public Command<BaseEntry> DeleteClickCommand { get; set; }
        public Command<BaseEntry> AddToPlaylistCommand { get; set; }
        public Command<BaseEntry> EntryPlayClickCommand { get; set; }
        public Command<BaseEntry> ShareClickCommand { get; set; }
        public Command<BaseEntry> RemoveClickCommand { get; set; }
        public Command<object> EditMetadataClickCommand { get; set; }
      
        public Command<Song> ViewInCollectionCommand { get; set; }
        private void CreateCommand()
        {
            ArtistClickCommand = new Command<ItemClickEventArgs>(ArtistClickExecute);
            AlbumClickCommand = new Command<ItemClickEventArgs>(AlbumClickExecute);
            PlaylistClickCommand = new Command<ItemClickEventArgs>(PlaylistClickExecute);
            PlayTrackCommand = new Command<BaseEntry>(PlayTrackExecute);
            DeleteClickCommand = new Command<BaseEntry>(DeleteClickExecute);
            DownloadClickCommand = new Command<Song>(DownloadClickExecute);
            CancelClickCommand = new Command<Song>(CancelClickExecute);
            EntryPlayClickCommand = new Command<BaseEntry>(EntryPlayClickExecute);
            ShareClickCommand = new Command<BaseEntry>(ShareClickExecute);
            RemoveClickCommand = new Command<BaseEntry>(RemoveClickExecute);
            AddToPlaylistCommand = new Command<BaseEntry>(AddToPlaylistExecute);
            EditMetadataClickCommand = new Command<object>(EditMetadataExecute);
            ViewInCollectionCommand = new Command<Song>(ViewInCollectionCommandExecute);
         }

      
        //to be performed
        private void EditMetadataExecute(object obj)
        {
            if(obj is Data.Spotify.Models.SimpleTrack)
               SheetUtility.OpenEditTrackMetadataPage(obj as Data.Spotify.Models.SimpleTrack);
            else if(obj is Data.Model.WebSongs.WebSong)
                SheetUtility.OpenEditTrackMetadataPage(obj as Data.Model.WebSongs.WebSong);
            else if(obj is IF.Lastfm.Core.Objects.LastTrack)
                SheetUtility.OpenEditTrackMetadataPage(obj as IF.Lastfm.Core.Objects.LastTrack);
        }

        private void LastEditMetadataExecute(IF.Lastfm.Core.Objects.LastTrack obj)
        {
            SheetUtility.OpenEditTrackMetadataPage(obj as IF.Lastfm.Core.Objects.LastTrack);
        }

        private async void ViewInCollectionCommandExecute(Song obj)
        {
            App.Locator.CollectionSearch.ClearCollectionSeachData();
            await System.Threading.Tasks.Task.Delay(10);
            SheetUtility.CloseSearchCollectionPage();
            int id = App.Locator.CollectionService.Artists.Where(p => p.Id == obj.Artist.Id).FirstOrDefault().Id;
            App.Navigator.GoTo<CollectionArtistPage, ZoomInTransition>(id);
        }

        async void PlayTrackExecute(BaseEntry obj)
        {
            await PlayAndQueueHelper.PlaySongAsync(obj as Song);
        }

        public async void DeleteClickExecute(BaseEntry item)
        {
            if (item is Playlist)
                await MessageHelpers.DeleteConfirm(item as Playlist);

            else if (item is Album || item is Artist || item is Video)
                await MessageHelpers.DeleteConfirm(item);
            else
                await MessageHelpers.DeleteConfirm(item as Song);
        }

        private async void EntryPlayClickExecute(BaseEntry item)
        {
            try
            {
                List<Song> queueSongs = null;
                if (item is Artist)
                {
                    var artist = item as Artist;
                    queueSongs = artist.Songs.ToList();
                }
                else if (item is Album)
                {
                    var album = item as Album;
                    queueSongs = album.Songs.ToList();
                }
                else if (item is Playlist)
                {
                    var playlist = item as Playlist;
                    queueSongs = playlist.Songs.Select(p => p.Song).ToList();
                }
                if (queueSongs != null && queueSongs.Count > 0)
                {
                    await PlayAndQueueHelper.PlaySongsAsync(queueSongs[0], queueSongs, true);
                }
            }
            catch (Exception)
            {

            }
        }

        private void ShareClickExecute(BaseEntry item)
        {
            try
            {
                List<Song> shareSongs = null;
                if (item is Song)
                {
                    var song = item as Song;
                    if (song.IsDownload)
                    {
                        Core.WinRt.Common.ToastManager.ShowError("Unable to share file.");
                        return;
                    }
                    shareSongs = new List<Song>() { song };
                }
                else if(item is Video)
                {
                    ShareSongHelper.ShareVideo(item as Video);
                    return;
                }
                else if (item is Artist)
                {
                    var artist = item as Artist;
                    shareSongs = artist.Songs.Where(p => (!p.IsDownload && !p.IsTemp)).ToList();
                }
                else if (item is Album)
                {
                    var album = item as Album;
                    shareSongs = album.Songs.Where(p => (!p.IsDownload && !p.IsTemp)).ToList();
                }
                else if (item is Playlist)
                {
                    var playlist = item as Playlist;
                    shareSongs = playlist.Songs.Select(p => p.Song).Where(q => (!q.IsDownload && !q.IsTemp)).ToList();
                }

                if (shareSongs != null && shareSongs.Count > 0)
                {
                    ShareSongHelper.ShareSong(shareSongs);
                }
            }
            catch (Exception)
            {

            }
        }


        private async void RemoveClickExecute(BaseEntry obj)
        {
            await _service.DeleteFromPlaylistAsync(App.Locator.CollectionPlaylist.Playlist, obj as PlaylistSong);
        }

        private void AddToPlaylistExecute(BaseEntry item)
        {
            try
            {
                List<Song> addToPlaylistSongs = null;
                if (item is Artist)
                {
                    var artist = item as Artist;
                    if(artist.Songs.Count > 0)
                        addToPlaylistSongs = artist.Songs.Where(p => (!p.IsTemp)).ToList();
                }
                else if (item is Album)
                {
                    var album = item as Album;
                    if (album.Songs.Count > 0)
                        addToPlaylistSongs = album.Songs.Where(p => (!p.IsTemp)).ToList();
                }
                if (addToPlaylistSongs != null && addToPlaylistSongs.Count > 0)
                {
                    AddToPlaylistPage.songs = addToPlaylistSongs;
                    SheetUtility.OpenAddToPlaylistPage();
                }
            }

            catch(Exception)
            {

            }
        }

   

        private async void AlbumClickExecute(ItemClickEventArgs obj)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<CollectionAlbumPage, ZoomInTransition>(((Album)obj.ClickedItem).Id);
            });
        }

        private async void ArtistClickExecute(ItemClickEventArgs obj)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<CollectionArtistPage, ZoomInTransition>(((Artist)obj.ClickedItem).Id);
            });
        }

        private async void PlaylistClickExecute(ItemClickEventArgs obj)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<CollectionPlaylistPage, ZoomInTransition>(((Playlist)obj.ClickedItem).Id);
            });
        }

        private async void CancelClickExecute(Song _song)
        {
            try
            {
                if (_song.Download != null)
                    _downloadService.Cancel(_song);
                else
                {
                    _song.SongState = SongState.DownloadListed;
                    await _sqlService.UpdateItemAsync(_song).ConfigureAwait(false);
                }
            }
            catch
            {
                _song.SongState = SongState.DownloadListed;
                await _sqlService.UpdateItemAsync(_song).ConfigureAwait(false);
            }
        }

        private void DownloadClickExecute(Song song)
        {
            _downloadService.StartDownloadAsync(song);
        }

    }
}