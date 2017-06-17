
using Musicus.Data.Collection;
using Musicus.Data.Model.WebSongs;
using Musicus.Utilities;
using System;
using System.Threading.Tasks;
using Musicus.Core.WinRt.Common;


namespace Musicus.Helpers
{
    public class ViezTrackSavingHelper
    {
        public static async Task<SavingError> SaveViezTrackLevel1(WebSong track)
        {

            if (!string.IsNullOrEmpty(track.Id) && track.Provider != Data.Mp3Provider.YouTube)
                track.Album = await GetAlbumName(track.Id);

            else if (track.Album.ToLower().Contains("unknown") ||
                    track.Album.ToLower().Contains("various") ||
                    track.Album.ToLower().Contains("random"))
                await SaveViezTrackFindAlbum(track);

            //clean track only english letters
            track.Name = SongSavingHelper.ToCleanQuery(track.Name);
            track.Artist = SongSavingHelper.ToCleanQuery(track.Artist);
            track.Album = SongSavingHelper.ToCleanQuery(track.Album);


            //check if it is track properties are noll or not;
            if (!CheckUp(track))
            {
                ShowResults(SavingError.Unknown, track.Name);
                return SavingError.Unknown;
            }

            if (App.Locator.CollectionService.ViezSongAlreadyExists(track.Name, track.Album, track.Artist))
            {
                ShowResults(SavingError.AlreadyExists, track.Name);
                return SavingError.AlreadyExists;
            }

            if (App.Locator.Network.IsActive) 
            {
                var result = await SaveViezTrackLevel2(track);
                return result;
            }

            ShowResults(SavingError.Network, track.Name);
            return SavingError.Network;
        }



        private static async Task<SavingError> SaveViezTrackLevel2(WebSong track)
        {

            var result = await SaveViezTrackLevel3(track);
            return result;

        }


        private static async Task<SavingError> SaveViezTrackLevel3(WebSong track)
        {
            if (track == null) return SavingError.Unknown;
            while (!App.Locator.CollectionService.IsLibraryLoaded) { }

            var startTransaction = !App.Locator.SqlService.DbConnection.IsInTransaction;
            if (startTransaction) App.Locator.SqlService.BeginTransaction();

            var result = await SaveViezTrackLevel4(track);
            if (startTransaction) App.Locator.SqlService.Commit();

            ShowResults(result.Error, track.Name);

            if (result.Song != null)
            {
                if(track!= null && track.ArtworkImage != null)
                    await DownloadArtworks.SaveArtworkByUrl(track.ArtworkImage, result.Song.Album.Name);
               else
                    await DownloadArtworks.DownloadAlbumsArtworkAsync();


                if (App.Locator.Setting.SpotifyArtworkSync)
                    await DownloadArtworks.DownloadArtistsArtworkAsyncFromSpotify(false, result.Song.Artist.Name);
                else
                    await DownloadArtworks.DownloadArtistsArtworkAsync(false, result.Song.Artist.Name);
            }
            return result.Error;
        }



        private static async Task<SaveResults> SaveViezTrackLevel4(WebSong track)
        {
            try
            {
                bool isDeezerTrack = track.IsDeezerTrack;
                if (track == null) return new SaveResults() { Error = SavingError.Unknown };
                var preparedSong = track.ToViezSong();              
                preparedSong.Album = track.ToViezAlbum();
                preparedSong.Artist = track.ToViezArtist();
                preparedSong.Album.PrimaryArtist = preparedSong.Artist;

                if (App.Locator.CollectionService.ViezSongAlreadyExists(track.Name, track.Album, track.Artist))
                {
                    return new SaveResults() { Error = SavingError.AlreadyExists };
                }
                if (isDeezerTrack)
                {
                    await App.Locator.CollectionService.AddSongAsync(preparedSong, isDeezerTrack: true).ConfigureAwait(false);
                    CollectionHelper.MatchSong(preparedSong);
                }

                else
                {
                    await App.Locator.CollectionService.AddSongAsync(preparedSong).ConfigureAwait(false);
                    CollectionHelper.MatchSong(preparedSong);
                    //CollectionHelper.MatchViezTrack(preparedSong);
                }
                return new SaveResults() { Error = SavingError.None, Song = preparedSong };
            }
            catch
            {
                return new SaveResults() { Error = SavingError.Unknown };
            }
        }

        private static async Task SaveViezTrackFindAlbum(WebSong song)
        {
            try
            {
                var track = await App.Locator.ScrobblerService.GetDetailTrack(song.Name, song.Artist).ConfigureAwait(false); ;
                if (track == null)
                    song.Album = "Unknown Album";
                else
                {
                    if (track.AlbumName != null) song.Album = track.AlbumName;
                    else song.Album = "Unknown Album";
                }
            }
            catch (Exception)
            {
                song.Album = "Unknown Album";
            }
        }

        private static async Task<string> GetAlbumName(string mbid)
        {
            try
            {
                var album = await App.Locator.ScrobblerService.GetDetailAlbumByMbid(mbid).ConfigureAwait(false); ;
                if (album == null)
                    return "Unknown Album";
                else if (album.Name == null)
                    return "Unknown Album";
                else
                    return album.Name;

            }
            catch (Exception)
            {
                return "Unknown Album";
            }
        }



        private static bool CheckUp(WebSong track)
        {
            if (!string.IsNullOrEmpty(track.Name) && !string.IsNullOrEmpty(track.Artist)
                && !string.IsNullOrEmpty(track.Album))
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
