using Musicus.Core.Utils;
using Musicus.Core.WinRt;
using Musicus.Core.WinRt.Utilities;
using Musicus.Data.Collection;
using Musicus.Data.Collection.Model;
using System;
using System.Collections.Generic;
using Musicus.Core.WinRt.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Musicus.ViewModel.Mvvm.Dispatcher;
using Windows.Media.Playback;
using Windows.Foundation.Collections;
using Musicus.Core;

namespace Musicus.Utilities
{
    public static class CollectionHelper
    {
        public static async Task DeleteEntryAsync(BaseEntry item, bool showSuccessMessage = true)
        {
            var name = "unknown";
            var playbackQueue = App.Locator.CollectionService.PlaybackQueue;
            try
            {
                if (item is Song)
                {
                    var song = item as Song;
                    name = song.Name;

                    var queue = playbackQueue.FirstOrDefault(p => p.SongId == song.Id);
                    if (queue != null && App.Locator.Player.IsPlayerActive)
                    {
                        if (playbackQueue.Count == 1)
                        {
                            ToastManager.Show("Unable to delete ", queue.Song.Name);
                            return;
                        }
                        else if (queue.SongId == App.Locator.Player.CurrentSong.Id)
                        {
                            App.Locator.AudioPlayerHelper.NextSong();
                        }
                    }
                    await App.Locator.CollectionService.DeleteSongAsync(song);
                    if(showSuccessMessage)
                        ToastManager.Show("Track deleted.");
                    ExitIfAlbumOrArtistEmpty(song.Album, song.Artist);
                }
                else if (item is Playlist)
                {
                    var playlist = (Playlist)item;
                    name = playlist.Name;
                    await App.Locator.CollectionService.DeletePlaylistAsync(playlist);
                    if (showSuccessMessage)
                        ToastManager.Show("{0} has been deleted.", name);
                }

                else if(item is Video)
                {
                    var video = (Video)item;
                    name = video.Title;
                    await App.Locator.CollectionService.DeleteVideoAsync(video);
                    if (showSuccessMessage)
                        ToastManager.Show("{0} has been deleted.", name);
                }

                else if (item is Artist)
                {
                    await DispatcherHelper.RunAsync(async () =>
                    {
                        var artist = (Artist)item;
                        name = artist.Name;
                        bool hasDeleted = false;
                        App.Locator.CollectionService.Artists.Remove(artist);

                        var artistSongs = artist.Songs.ToList();
                        var taskList = new List<Task>();



                        //var a = playbackQueue.All(artistSongs) && playbackQueue.Count == artistSongs.Count;
                        foreach (var song in artistSongs)
                        {

                            var queue = playbackQueue.FirstOrDefault(p => p.SongId == song.Id);

                            if (queue != null && App.Locator.Player.IsPlayerActive)
                            {
                                if (playbackQueue.Count == 1)
                                {
                                    ToastManager.Show("Unable to delete ", queue.Song.Name);
                                    return;
                                }
                                else if (queue.SongId == App.Locator.Player.CurrentSong.Id)
                                {
                                    hasDeleted = true;
                                    BackgroundMediaPlayer.Current.Pause();
                                }
                            }

                            taskList.Add(App.Locator.CollectionService.DeleteSongAsync(song));
                         
                        }

                        ExitIfArtistEmpty(artist);
                        if (showSuccessMessage)
                            ToastManager.Show("{0} has been deleted.", name);

                        if (hasDeleted)
                        {
                            await Task.Delay(100);
                            if (App.Locator.CollectionService.PlaybackQueue.Count > 0)
                            {
                                App.Locator.AudioPlayerHelper.NextSong();
                            }
                            else
                            {
                                await App.Locator.AudioPlayerHelper.ShutdownPlayerAsync();
                            }
                        }
                    });
                }
                else if (item is Album)
                {
                    await DispatcherHelper.RunAsync(async () =>
                    {
                        var album = (Album)item;
                        name = album.Name;
                        bool hasDeleted = false;
                        App.Locator.CollectionService.Albums.Remove(album);

                        var albumSongs = album.Songs.ToList();
                        var taskList = new List<Task>();

                        foreach (var song in albumSongs)
                        {
                            var queue = playbackQueue.FirstOrDefault(p => p.SongId == song.Id);
                            if (queue != null && App.Locator.Player.IsPlayerActive)
                            {
                                if (playbackQueue.Count == 1)
                                {
                                    ToastManager.Show("Unable to delete ", queue.Song.Name);
                                    return;
                                }
                                else if (queue.SongId == App.Locator.Player.CurrentSong.Id)
                                {
                                    hasDeleted = true;
                                    BackgroundMediaPlayer.Current.Pause();
                                }
                            }
                            taskList.Add(App.Locator.CollectionService.DeleteSongAsync(song));
                     
                        }

                        ExitIfAlbumEmpty(album);
                        if (showSuccessMessage)
                            ToastManager.Show("{0} has been deleted.", name);
                        if (hasDeleted)
                        {
                            await Task.Delay(100);
                            if (App.Locator.CollectionService.PlaybackQueue.Count > 0)
                            {
                                App.Locator.AudioPlayerHelper.NextSong();
                            }
                            else
                            {
                                await App.Locator.AudioPlayerHelper.ShutdownPlayerAsync();
                            }
                        }
                    });
                }
            }
            catch
            {
                ToastManager.ShowError("Can't delete {0}.", name);
            }
        }



        public static async Task DeleteFromDevice(BaseEntry item)
        {
            if (item == null) return;
            try
            {
                var song = item as Song;
                string filename = song.Name.CleanForFileName("Invalid Song Name");

                if (song.ArtistName != song.Album.PrimaryArtist.Name)
                    filename = song.ArtistName.CleanForFileName("Invalid Artist Name") + "-" + filename;

                string path = string.Format(
                AppConstant.SongPath,
                song.Album.PrimaryArtist.Name.CleanForFileName("Invalid Artist Name"),
                song.Album.Name.CleanForFileName("Invalid Album Name"),
                filename);

                await WinRtStorageHelper.DeleteFileAsync(path, KnownFolders.MusicLibrary);
            }

            catch (Exception)
            {

            }

        }

        public static void MatchSong(Song song)
        {
            TaskHelper.Enqueue(MatchSongAsync(song));
        }

      
        public static void MatchSongsOnStart()
        {
            var songs = App.Locator.CollectionService.Songs.Where(p => p.SongState == SongState.Matching).ToList();
            foreach (var song in songs)
                MatchSong(song);
            if (songs != null)
                songs = null;
        }


        public static async Task MigrateAsync()
        {
            var songs = App.Locator.CollectionService.Songs.Where(p => p.SongState == SongState.Downloaded).ToList();
            var importedSongs =
                App.Locator.CollectionService.Songs.Where(
                    p => p.SongState == SongState.Local && !p.AudioUrl.Substring(1).StartsWith(":")).ToList();
            var songsFolder = await WinRtStorageHelper.GetFolderAsync("songs");

            if (songs.Count != 0 && songsFolder != null)
            {
                App.Locator.SqlService.BeginTransaction();
                UiBlockerUtility.Block("Preparing...");
                foreach (var song in songs)
                {
                    try
                    {
                        var filename = song.Name.CleanForFileName("Invalid Song Name");
                        if (song.ArtistName != song.Album.PrimaryArtist.Name)
                        {
                            filename = song.ArtistName.CleanForFileName("Invalid Artist Name") + "-" + filename;
                        }

                        var path = string.Format(
                            AppConstant.SongPath,
                            song.Album.PrimaryArtist.Name.CleanForFileName("Invalid Artist Name"),
                            song.Album.Name.CleanForFileName("Invalid Album Name"),
                            filename);

                        var file = await WinRtStorageHelper.GetFileAsync(string.Format("songs/{0}.mp3", song.Id));

                        var folder =
                            await WinRtStorageHelper.EnsureFolderExistsAsync(path, KnownFolders.MusicLibrary);
                        await file.MoveAsync(folder, filename, NameCollisionOption.ReplaceExisting);

                        song.AudioUrl = Path.Combine(folder.Path, filename);
                        await App.Locator.SqlService.UpdateItemAsync(song);
                    }
                    catch (Exception)
                    {

                    }
                }

                App.Locator.SqlService.Commit();

            }

            if (importedSongs.Count > 0)
            {

                UiBlockerUtility.Block("Almost done...");
                App.Locator.SqlService.BeginTransaction();
                foreach (var song in importedSongs)
                {
                    try
                    {
                        await App.Locator.CollectionService.DeleteSongAsync(song);
                    }
                    catch (Exception)
                    {

                    }
                }


                App.Locator.SqlService.Commit();

            }

            UiBlockerUtility.Unblock();

            if (songsFolder != null)
            {
                await songsFolder.DeleteAsync();
            }

            if (importedSongs.Count > 0)
            {
                ToastManager.Show("Few tracks do not have audiourl, make sure they exists.");
            }
        }


        private static Song Cleanup(Song song)
        {
            if(song !=null)
            {
                if (song.Artist != null) song.Artist.Name = song.Artist.Name.ToCleanQuery();
                if (song.ArtistName != null) song.ArtistName = song.ArtistName.ToCleanQuery();
                if (song.Name != null) song.Name = song.Name.ToCleanQuery();
                if (song.Album != null) song.Album.Name = song.Album.Name.ToCleanQuery(); 
            }

            return song;
        }


        private static async Task MatchSongAsync(Song song)
        {
            //if (song.RadioId > 0)
            //{
            //    var val = await App.Locator.Mp3MatchEngine.FindMp3ByProvider(song.Name, song.Artist.Name, song.RadioId).ConfigureAwait(false);
            //    if (val != null && !string.IsNullOrEmpty(val.AudioUrl))
            //    {
            //        if (song.SongState == SongState.DownloadListed) return;
            //        await App.Locator.PclDispatcherHelper.RunAsync(() =>
            //        {
            //            song.AudioUrl = val.AudioUrl;
            //            song.RadioId = val.ProviderNumber;
            //            song.CloudId = val.AudioUrl;
            //            song.SongState = SongState.DownloadListed;
            //        });

            //        await App.Locator.SqlService.UpdateItemAsync(song);
            //        return;
            //    }
            //}


            var obj = await App.Locator.Mp3MatchEngine.FindMp3For(song.Name, song.Artist.Name).ConfigureAwait(false);
            if (obj != null && string.IsNullOrEmpty(obj.AudioUrl) && string.Equals(song.Artist.Name, song.ArtistName))
                obj = await App.Locator.Mp3MatchEngine.FindMp3For(song.Name, song.ArtistName).ConfigureAwait(false);

            if (obj != null && !string.IsNullOrEmpty(obj.AudioUrl))
            {
                if (song.SongState == SongState.DownloadListed) return;
                await App.Locator.PclDispatcherHelper.RunAsync(() =>
                {
                      song.AudioUrl = obj.AudioUrl;
                      song.RadioId = obj.ProviderNumber;
                      song.CloudId = obj.AudioUrl;
                      song.SongState = SongState.DownloadListed;
                });

                await App.Locator.SqlService.UpdateItemAsync(song);
            }
        }



        private static void ExitIfArtistEmpty(Artist artist)
        {
            if (App.Navigator.CurrentPage is CollectionArtistPage && artist.Songs.Count <= 0)
            {
                App.Navigator.GoBack();
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") return;
                if (App.Navigator.StackCount > 0)
                    App.Navigator.StackCount -= 1;
            }
        }

        private static void ExitIfAlbumEmpty(Album album)
        {
            if (App.Navigator.CurrentPage is CollectionAlbumPage && album.Songs.Count <= 0)
            {
                App.Navigator.GoBack();
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") return;
                if (App.Navigator.StackCount > 0)
                    App.Navigator.StackCount -= 1;
            } 
        }

        private static void ExitIfAlbumOrArtistEmpty(Album album, Artist artist)
        {
            if (App.Navigator.CurrentPage is CollectionAlbumPage && album.Songs.Count <= 0)
            {
                App.Navigator.GoBack();
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") return;
                if (App.Navigator.StackCount > 0)
                    App.Navigator.StackCount -= 1;
            }

            if (App.Navigator.CurrentPage is CollectionArtistPage && artist.Songs.Count <= 0)
            {
                App.Navigator.GoBack();
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") return;
                if (App.Navigator.StackCount > 0)
                    App.Navigator.StackCount -= 1;
            }
        }


    }
}
