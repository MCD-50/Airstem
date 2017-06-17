
using Musicus.Data.Collection;
using Musicus.Data.Spotify.Models;
using Musicus.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Musicus.Core.WinRt.Common;
using Musicus.Data.Spotify;

namespace Musicus.Helpers
{
    public class SpotifySavingHelper
    {
      
        private static readonly List<string> SpotifySavingTracks = new List<string>();
        private static readonly List<string> SpotifySavingAlbums = new List<string>();
        public static async Task<SavingError> SaveSpotifyChartTrackLevel1(ChartTrack chartTrack)
        {
            //clean track only english letters
            chartTrack.Name = SongSavingHelper.ToCleanQuery(chartTrack.Name);
            chartTrack.ArtistName = SongSavingHelper.ToCleanQuery(chartTrack.ArtistName);
            chartTrack.album_name = SongSavingHelper.ToCleanQuery(chartTrack.album_name);

            //check if it is track properties are noll or not;
            if (!CheckUp(chartTrack))
            {
                ShowResults(SavingError.Unknown, chartTrack.Name);
                return SavingError.Unknown;
            }

            //checking for duplicate songs.
            if (App.Locator.CollectionService.SongAlreadyExists(chartTrack.Name, chartTrack.album_name, chartTrack.ArtistName))
            {
                ShowResults(SavingError.AlreadyExists, chartTrack.Name);
                return SavingError.AlreadyExists;
            }


            //getting track metadata from spotify and search using viez.
            var track = await App.Locator.Spotify.GetTrack(chartTrack.track_id);
            if (track != null)
            {
                var album = await App.Locator.Spotify.GetAlbum(track.Album.Id);
                var result = await SaveSpotifyTrackLevel2(track, album);
                return result;
            }

            //network fail
            ShowResults(SavingError.Network, chartTrack.Name);
            return SavingError.Network;
        }


        //saving spotify full tracks using viez.
        public static async Task<SavingError> SaveSpotifyTrackLevel1(FullTrack track, bool manualMatch = false)
        {

            //clean track only english letters
            track.Name = SongSavingHelper.ToCleanQuery(track.Name);
            track.Artist.Name = SongSavingHelper.ToCleanQuery(track.Artist.Name);
            track.Album.Name = SongSavingHelper.ToCleanQuery(track.Album.Name);

            //check if it is track properties are noll or not;
            if (!CheckUp(track))
            {
                ShowResults(SavingError.Unknown, track.Name);
                return SavingError.Unknown;
            }

            if (App.Locator.CollectionService.SongAlreadyExists(track.Name, track.Album.Name, track.Artist.Name))
            {
                ShowResults(SavingError.AlreadyExists, track.Name);
                return SavingError.AlreadyExists;
            }

            SavingError result;

            try
            {
                var album = await App.Locator.Spotify.GetAlbum(track.Album.Id);
                result = await SaveSpotifyTrackLevel2(track, album);
            }
            catch
            {
                result = SavingError.Network;
            }
            return result;
        }

       
        public static async Task SaveSpotifyAlbumLevel1(FullAlbum album)
        {

            if (album.Tracks.Items.Count == 0)
            {
                ToastManager.ShowError("Nothing found.");
                return;
            }

            //clean track only english letters
            album.Name = SongSavingHelper.ToCleanQuery(album.Name);
            album.Artist.Name = SongSavingHelper.ToCleanQuery(album.Artist.Name);
            foreach (var obj in album.Tracks.Items)
            {
                obj.Name = SongSavingHelper.ToCleanQuery(obj.Name);
            }

            //check if it is track properties are noll or not;
            if (!CheckUp(album))
            {
                ShowResults(SavingError.Unknown, album.Name);
                return;
            }
     

            var alreadySaving = SpotifySavingAlbums.FirstOrDefault(p => p == album.Id) != null;
            if (alreadySaving)
            {
                ShowResults(SavingError.AlreadySaving, album.Name);
                return;
            }

            SpotifySavingAlbums.Add(album.Id);

            //ignore
            //problem when os is not under this thread.
            //solution: use dispatcher.
            //testing under android pending.

            while (!App.Locator.CollectionService.IsLibraryLoaded) { }
            //var collAlbum = App.Locator.CollectionService.Albums.FirstOrDefault(p => p.ProviderId.Contains(album.Id));

            var collAlbum = App.Locator.CollectionService.Albums.FirstOrDefault(p => string.Equals(p.Name.ToLower(),album.Name.ToLower()));

            var alreadySaved = collAlbum != null;

            if (alreadySaved)
            {
                var missingTracks = collAlbum.Songs.Count < album.Tracks.Items.Count;
                if (!missingTracks)
                {
                    ShowResults(SavingError.AlreadyExists, album.Name);
                    SpotifySavingAlbums.Remove(album.Id);
                    return;
                }
            }
            ToastManager.ShowError("Saving " + album.Name + ".");
            var index = 0;
            if (!alreadySaved)
            {
                SavingError result;
                do
                {
                    // first save one song (to avoid duplicate album creation)
                    result = await SaveSpotifyTrackLevel3(album.Tracks.Items[index], album, false);
                    index++;
                }
                while (result != SavingError.None && index < album.Tracks.Items.Count);
            }

            bool success;
            var missing = false;
            if (album.Tracks.Items.Count > 1)
            {
                App.Locator.SqlService.BeginTransaction();
                // save the rest at the rest time
                var songs = album.Tracks.Items.Skip(index).Select(track => SaveSpotifyTrackLevel3(track, album, false));
                var results = await Task.WhenAll(songs);
                // now wait a split second before showing success message
                await Task.Delay(1000);
                var alreadySavedCount = results.Count(p => p == SavingError.AlreadyExists);
                var successCount =
                    results.Count(
                        p =>
                        p == SavingError.None || p == SavingError.AlreadyExists || p == SavingError.AlreadySaving);
                var missingCount = successCount == 0 ? -1 : album.Tracks.Items.Count - (successCount + index);
                success = missingCount == 0;
                missing = missingCount > 0;
                if (missing)
                {
                    ToastManager.ShowError("Problem while saving " + missingCount + " tracks.");
                }
                App.Locator.SqlService.Commit();
            }
            else
                success = App.Locator.CollectionService.Albums.FirstOrDefault(p => p.ProviderId.Contains(album.Id)) != null;

            if (success)
               ShowResults(SavingError.None, album.Name);

            else if (!missing)
                ShowResults(SavingError.Unknown, album.Name);


            SpotifySavingAlbums.Remove(album.Id);

            if (collAlbum == null)
            {
                collAlbum = App.Locator.CollectionService.Albums.FirstOrDefault(p => string.Equals(p.Name.ToLower(), album.Name.ToLower()));
                if(collAlbum != null && !collAlbum.HasArtwork && !collAlbum.NoArtworkFound)
                    await DownloadArtworks.SaveAlbumImageAsync(collAlbum, album.Images[0].Url);

                if (App.Locator.Setting.SpotifyArtworkSync)
                    await DownloadArtworks.DownloadArtistsArtworkAsyncFromSpotify(false, album.Artist.Name);
                else
                    await DownloadArtworks.DownloadArtistsArtworkAsync(false, album.Artist.Name);
            }

        }

      

        public static async Task<SavingError> SaveSpotifyTrackLevel2(SimpleTrack track, FullAlbum album)
        {
            var result = await SaveSpotifyTrackLevel3(track, album);
            ShowResults(result, track.Name);
            return result;
        }

        private static async Task<SavingError> SaveSpotifyTrackLevel3(SimpleTrack track, FullAlbum album, bool onFinishDownloadArtwork = true)
        {
            //album = Cleanup(album);
            // track = Cleanup(track);

            if (track == null || album == null) return SavingError.Unknown;
            var alreadySaving = SpotifySavingTracks.FirstOrDefault(p => p == track.Id) != null;
            if (alreadySaving) return SavingError.AlreadySaving;


            //adding id(alway unique) of track to list to avoid duplicate things.
            SpotifySavingTracks.Add(track.Id);

            while (!App.Locator.CollectionService.IsLibraryLoaded) { }
            var startTransaction = !App.Locator.SqlService.DbConnection.IsInTransaction;
            if (startTransaction) App.Locator.SqlService.BeginTransaction();

            var result = await SaveSpotifyTrackLevel4(track, album);
            if (startTransaction) App.Locator.SqlService.Commit();

           // ShowResults(result.Error, track.Name);
            SpotifySavingTracks.Remove(track.Id);

            if (!onFinishDownloadArtwork) return result.Error;

            if (result.Song != null)
            {
                if (!result.Song.Album.HasArtwork && !result.Song.Album.NoArtworkFound)
                    await DownloadArtworks.SaveAlbumImageAsync(result.Song.Album, album.Images[0].Url);

                if (App.Locator.Setting.SpotifyArtworkSync)
                    await DownloadArtworks.DownloadArtistsArtworkAsyncFromSpotify(false, result.Song.Artist.Name);
                else
                    await DownloadArtworks.DownloadArtistsArtworkAsync(false, result.Song.Artist.Name);
            }

            return result.Error;
        }



        private static async Task<SaveResults> SaveSpotifyTrackLevel4(SimpleTrack track, FullAlbum album)
        {
            try
            {
                if (track == null) return new SaveResults() { Error = SavingError.Unknown };
                var preparedSong = track.ToSong();
                if (App.Locator.CollectionService.SongAlreadyExists(track.Name, album.Name, album.Artist != null ? album.Artist.Name : track.Artist.Name))
                    return new SaveResults() { Error = SavingError.AlreadyExists };

                var fullTrack = track as FullTrack ?? await App.Locator.Spotify.GetTrack(track.Id);
                var artist = fullTrack != null ? fullTrack.Artist : ((track.Artist != null) ? track.Artist : new SimpleArtist { Name = "Unknown Artist"});
                preparedSong.ArtistName = fullTrack != null ? fullTrack.Artist.Name : artist.Name;
                preparedSong.Album = album.ToAlbum();
                preparedSong.Artist = fullTrack != null ? (new Data.Collection.Model.Artist { Name = preparedSong.ArtistName.Trim().Replace("  ", ""), ProviderId = "spotify." + album.Artist.Id } ) : album.Artist.ToArtist();
                preparedSong.Album.PrimaryArtist = preparedSong.Artist;
                await App.Locator.CollectionService.AddSongAsync(preparedSong).ConfigureAwait(false);
                CollectionHelper.MatchSong(preparedSong);
                return new SaveResults() { Error = SavingError.None, Song = preparedSong };
            }
            catch
            {
                return new SaveResults() { Error = SavingError.Unknown };
            }
        }

        private static bool CheckUp(ChartTrack track)
        {
            if (!string.IsNullOrEmpty(track.Name) && !string.IsNullOrEmpty(track.ArtistName)
                && !string.IsNullOrEmpty(track.album_name))
                return true;
           
            else
                return false;
        }

        private static bool CheckUp(FullTrack track)
        {

            if (!string.IsNullOrEmpty(track.Name) && !string.IsNullOrEmpty(track.Artist.Name)
                && !string.IsNullOrEmpty(track.Album.Name))
                return true;
           
            else
                return false;
        }


        private static bool CheckUp(FullAlbum album)
        {

            if (!string.IsNullOrEmpty(album.Name) && !string.IsNullOrEmpty(album.Artist.Name))
            {
                foreach (var obj in album.Tracks.Items)
                {
                    if (string.IsNullOrEmpty(obj.Name))
                        return false;
                }
                return true;
            }
            else
                return false;
        }

        private static void ShowResults(SavingError result, string trackName)
        {
            switch (result)
            {
                case SavingError.AlreadySaving:
                    ToastManager.ShowError("Already saving " + trackName + ".");
                    break;
                case SavingError.AlreadyExists:
                    ToastManager.ShowError("Already saved " + trackName + ".");
                    break;
                case SavingError.None:
                    ToastManager.ShowError("Saved " + trackName + ".");
                    break;

                case SavingError.Network:
                    ToastManager.ShowError("Weak connection, couldn't download " + trackName + ".");
                    break;
                case SavingError.Unknown:
                    ToastManager.ShowError("Something went wrong while saving " + trackName + ".");
                    break;
            }
        }
    }
}
