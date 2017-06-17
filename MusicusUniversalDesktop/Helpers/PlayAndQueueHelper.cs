using Musicus.Data.Collection.Model;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Musicus.Core.WinRt.Common;
using SQLite.Net;

namespace Musicus.Helpers
{
    public class PlayAndQueueHelper
    {

        private static bool _currentlyPreparing;
        private const int MaxMassPlayQueueCount = 200;
        public const double MaxPlayQueueCount = 50;
        static bool _cannotOpen = false;

        public static void Error()
        {
            _cannotOpen = true;
        } 

        public static async Task PlaySongsAsync(Song song, List<Song> songs, bool forceClear = false)
        {
            if (song == null || songs == null || songs.Count == 0) return;
            if (!song.IsMatched) return;

            if (_cannotOpen) return;

            songs = songs.Where(p => p.IsMatched).ToList();

            if (songs.Count == 0)
            {
                ToastManager.ShowError("The songs haven't been matched yet.");
                return;
            }


            var skip = songs.IndexOf(song);
            var ordered = songs.Skip(skip).ToList();
            ordered.AddRange(songs.Take(skip));

            var overflow = songs.Count - MaxMassPlayQueueCount;
            if (overflow > 0)
            {
                for (var i = 0; i < overflow; i++)
                {
                    ordered.Remove(ordered.LastOrDefault());
                }
            }

            if (_currentlyPreparing)
            {
                // cancel the previous
                _currentlyPreparing = false;
                // wait for it to stop
                await Task.Delay(50);
            }

            _currentlyPreparing = true;

            try
            {
                await App.Locator.CollectionService.ClearQueueAsync().ConfigureAwait(false);
            }
            catch (SQLiteException)
            {
                // retry
                try
                {
                    App.Locator.CollectionService.ClearQueueAsync().Wait();
                }
                catch (SQLiteException)
                {
                    // quit
                    ToastManager.ShowError("Problem clearing the queue. You seem to be running low on storage.");
                    return;
                }
            }

            var queueSong = await App.Locator.CollectionService.AddToQueueAsync(song).ConfigureAwait(false);
            App.Locator.AudioPlayerHelper.PlaySong(queueSong);

            App.Locator.SqlService.BeginTransaction();
            for (var index = 1; index < ordered.Count; index++)
            {
                if (!_currentlyPreparing) break;
                await App.Locator.CollectionService.AddToQueueAsync(ordered[index]).ConfigureAwait(false);
            }
            App.Locator.SqlService.Commit();
            _currentlyPreparing = false;
        }

        public static async Task PlaySongAsync(Song song)
        {
            if (song == null || !song.IsMatched || _cannotOpen) return;

            if (_currentlyPreparing)
            {
                // cancel the previous
                _currentlyPreparing = false;
                // wait for it to stop
                await Task.Delay(50);
            }

            _currentlyPreparing = true;

            try
            {
                await App.Locator.CollectionService.ClearQueueAsync().ConfigureAwait(false);
            }
            catch (SQLiteException)
            {
                // retry
                try
                {
                    App.Locator.CollectionService.ClearQueueAsync().Wait();
                }
                catch (SQLiteException)
                {
                    // quit
                    ToastManager.ShowError("Problem clearing the queue. You seem to be running low on storage.");
                    return;
                }
            }

            var queueSong = await App.Locator.CollectionService.AddToQueueAsync(song).ConfigureAwait(false);
            App.Locator.AudioPlayerHelper.PlaySong(queueSong);

            _currentlyPreparing = false;
        }

        public static async Task AddToQueueAsync(Song song)
        {
            if (!song.IsMatched)
            {
                ToastManager.ShowError("Can't add unmatch songs to queue.");
                return;
            }

            if (_currentlyPreparing)
            {
                ToastManager.ShowError("Something went wrong.");
                return;
            }

            if (App.Locator.CollectionService.CheckIfExist(song.Id)) return;
            await App.Locator.CollectionService.AddToQueueAsync(song).ConfigureAwait(false);

            ToastManager.ShowError("Track added to queue.");
        }
    }
}
