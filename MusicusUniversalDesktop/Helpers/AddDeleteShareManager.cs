using Musicus.Data.Collection.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Musicus.ViewModel.Mvvm.Dispatcher;
using Musicus.Utilities;
using Musicus.Core.WinRt.Common;

namespace Musicus.Helpers
{
    public static class AddDeleteShareManager
    {
        public static async void Delete(List<Song> songs)
        {
            await MessageHelpers.DeleteConfirm(songs);
        }

        public static async Task DeletePhaseConfirmed(List<Song> songs)
        {
            await DispatcherHelper.RunAsync(async () =>
            {

                var tasks = songs.Select(m => CollectionHelper.DeleteEntryAsync(m as Song, false)).ToList();
                if (tasks.Count == 0) return;
                string message = songs.Count == 1 ? "Track deleted." : songs.Count + " Tracks deleted.";
                ToastManager.Show(message);
                await Task.WhenAll(tasks);

                var newTask = songs.Where(m => m.IsDownload).ToList().Select(x => CollectionHelper.DeleteFromDevice(x as Song)).ToList();
                if (newTask.Count == 0) return;
                await Task.WhenAll(newTask);
               
                //foreach (var song in songs)
                   //if (!song.IsDownload)
                      //await CollectionHelper.DeleteFromDevice(song);
            });
        }

        public static void AddToPlaylist(List<Song> songs)
        {
            //var song = songs.Select(m => m as Song).ToList();
            //if (song.Count == 0) return;
            AddToPlaylistPage.songs = songs;
            SheetUtility.OpenAddToPlaylistPage();
        }

        public static void Share(List<Song> songs)
        {
            if (songs.Count == 0)
            {
                ToastManager.ShowError("Unable to share file.");
                return;
            }
            App.Locator.PBar.IsEnable = true;
            ShareSongHelper.ShareSong(songs);
            App.Locator.PBar.IsEnable = false;
        }

        public static void OpenNowPlaying()
        {
            if (App.Locator.Player.IsPlayerActive)
                App.Navigator.GoTo<NowPlayingPage, ZoomInTransition>(null);
        }

    }
}
