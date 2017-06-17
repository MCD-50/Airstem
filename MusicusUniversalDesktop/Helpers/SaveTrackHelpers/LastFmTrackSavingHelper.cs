using IF.Lastfm.Core.Objects;
using Musicus.Data.Collection;
using Musicus.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Musicus.Core.WinRt.Common;
using System;


namespace Musicus.Helpers.SaveTrackHelpers
{
    public class LastFmTrackSavingHelper
    {
        private static readonly List<string> LastfmSavingTracks = new List<string>();

        public static async Task<SavingError> SaveLastTrackLevel1(LastTrack track)
        {
            await SaveLastTrackFindAlbum(track);

            //clean track only english letters
            track.Name = SongSavingHelper.ToCleanQuery(track.Name);
            track.ArtistName = SongSavingHelper.ToCleanQuery(track.ArtistName);
            track.AlbumName = SongSavingHelper.ToCleanQuery(track.AlbumName);

            //check if it is track properties are noll or not;
            if (!CheckUp(track))
            {
                ShowResults(SavingError.Unknown, track.Name);
                return SavingError.Unknown;
            }

            var result = await SaveLastTrackLevel2(track);
            //ShowResults(result, track.Name);
            return result;
        }

        private static async Task<SavingError> SaveLastTrackLevel2(LastTrack track)
        {
            if (track == null) return SavingError.Unknown;
            var alreadySaving = LastfmSavingTracks.FirstOrDefault(p => p == track.Id) != null;
            if (alreadySaving) return SavingError.AlreadySaving;

            LastfmSavingTracks.Add(track.Id);
            while (!App.Locator.CollectionService.IsLibraryLoaded) { }
            var startTransaction = !App.Locator.SqlService.DbConnection.IsInTransaction;
            if (startTransaction) App.Locator.SqlService.BeginTransaction();
            var result = await SaveLastTrackLevel3(track);
            if (startTransaction) App.Locator.SqlService.Commit();
            ShowResults(result.Error, track.Name);
            LastfmSavingTracks.Remove(track.Id);

            if (result.Song != null)
            {
                if (!result.Song.Album.HasArtwork && !result.Song.Album.NoArtworkFound)
                {
                    if (track.Images != null && track.Images.Largest != null)
                    {
                        await DownloadArtworks.SaveAlbumImageAsync(result.Song.Album, track.Images.Largest.AbsoluteUri);
                    }
                    else
                    {
                        await DownloadArtworks.DownloadAlbumsArtworkAsync();
                    }
                }
            }

            if (App.Locator.Setting.SpotifyArtworkSync)
                await DownloadArtworks.DownloadArtistsArtworkAsyncFromSpotify(false, result.Song.Artist.Name);
            else
                await DownloadArtworks.DownloadArtistsArtworkAsync(false, result.Song.Artist.Name);
            return result.Error;
        }


        private static async Task<SaveResults> SaveLastTrackLevel3(LastTrack track)
        {
            try
            {
                //await SaveLastTrackLevel4(track);
                var preparedSong = track.ToSong();
                preparedSong.Album = track.ToAlbum();
                preparedSong.Artist = track.ToArtist();
                preparedSong.Album.PrimaryArtist = preparedSong.Artist;

                if (App.Locator.CollectionService.SongAlreadyExists(track.Name, track.AlbumName, track.ArtistName))
                {
                    return new SaveResults() { Error = SavingError.AlreadyExists };
                }

                await App.Locator.CollectionService.AddSongAsync(preparedSong,isLastTrack : true).ConfigureAwait(false);
                CollectionHelper.MatchSong(preparedSong);
                return new SaveResults { Error = SavingError.None, Song = preparedSong };
            }
            catch
            {
                return new SaveResults { Error = SavingError.Unknown };
            }
        }


        private static async Task SaveLastTrackFindAlbum(LastTrack song)
        {
            try
            {
                var track = await App.Locator.ScrobblerService.GetDetailTrack(song.Name, song.ArtistName).ConfigureAwait(false); ;
                if (track == null)
                    song.AlbumName = "Unknown Album";
                else
                {
                    if (track.AlbumName != null) song.AlbumName = track.AlbumName;
                    else song.AlbumName = "Unknown Album";
                }              
            }
            catch (Exception)
            {
                song.AlbumName = "Unknown Album";
            }
        }


        private static bool CheckUp(LastTrack track)
        {
            if (!string.IsNullOrEmpty(track.Name) && !string.IsNullOrEmpty(track.ArtistName)
                && !string.IsNullOrEmpty(track.AlbumName))
                return true;
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
