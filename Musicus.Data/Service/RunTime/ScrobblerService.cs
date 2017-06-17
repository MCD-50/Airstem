#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Musicus.Core.Common;
using Musicus.Core.Utils;
using Musicus.Core.Utils.Interfaces;
using Musicus.Data.Service.Interfaces;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Model.WebData;

#endregion

namespace Musicus.Data.Service.RunTime
{
    public class ScrobblerService : IScrobblerService
    {
       // private readonly ICredentialHelper _credentialHelper;
       // private readonly AlbumApi _albumApi;
        private readonly ArtistApi _artistApi;
        private readonly ChartApi _chartApi;
        private readonly TrackApi _trackApi;
        // private readonly UserApi _userApi;
        private readonly AlbumApi _albumApi;
        private LastAuth _auth;

        public ScrobblerService()
        {
          //  _credentialHelper = credentialHelper;
            _auth = new LastAuth(ApiKeys.LastFmId, ApiKeys.LastFmSecret);
            _albumApi = new AlbumApi(_auth);
            _artistApi = new ArtistApi(_auth);
            _chartApi = new ChartApi(_auth);
            _trackApi = new TrackApi(_auth);
           // _userApi = new UserApi(_auth);
           // loadthis();
        }

        //private async void loadthis()
        //{
        //   await GetSessionTokenAsync();
        //}

       // public event EventHandler<BoolEventArgs> AuthStateChanged;

        //public bool HasCredentials
        //{
        //    get { return _credentialHelper.GetCredentials("lastfm") != null; }
        //}

        //public bool IsAuthenticated
        //{
        //    get { return _auth.Authenticated; }
        //}

        //public void Logout()
        //{
        //    _credentialHelper.DeleteCredentials("lastfm");
        //    _auth = new LastAuth(ApiKeys.LastFmId, ApiKeys.LastFmSecret);
        //    OnAuthStateChanged();
        //}

        //public async Task<LastFmApiError> ScrobbleNowPlayingAsync(string name, string artist, DateTime played,
        //    TimeSpan duration, string album = "",
        //    string albumArtist = "")
        //{
        //    if (!_auth.Authenticated)
        //        if (!await GetSessionTokenAsync())
        //            return LastFmApiError.BadAuth;

        //    var resp = await _trackApi.UpdateNowPlayingAsync(new Scrobble(artist, album, name, played)
        //    {
        //        Duration = duration,
        //        AlbumArtist = albumArtist
        //    });
        //    return resp.Error;
        //}

        //public async Task<LastFmApiError> ScrobbleAsync(string name, string artist, DateTime played, TimeSpan duration,
        //    string album = "",
        //    string albumArtist = "")
        //{
        //    if (!_auth.Authenticated)
        //        if (!await GetSessionTokenAsync())
        //            return LastFmApiError.BadAuth;

        //    var resp = await _trackApi.ScrobbleAsync(new Scrobble(artist, album, name, played)
        //    {
        //        Duration = duration,
        //        AlbumArtist = albumArtist
        //    });
        //    return resp.Error;
        //}

        //public async Task<PageResponse<LastArtist>> GetRecommendedArtistsAsync(int page = 1, int limit = 30)
        //{
        //    var resp = await _userApi.GetRecommendedArtistsAsync(page, limit);
        //    return resp;
        //}

        public async Task<LastAlbum> GetDetailAlbum(string name, string artist)
        {
            var resp = await _albumApi.GetAlbumInfoAsync(artist, name);
            return resp.Success ? resp.Content : null;
        }

        public async Task<LastAlbum> GetDetailAlbumByMbid(string mbid)
        {
            var resp = await _albumApi.GetAlbumInfoByMbidAsync(mbid);
            return resp.Success ? resp.Content : null;
        }

        public async Task<LastTrack> GetDetailTrack(string name, string artist)
        {
            var resp = await _trackApi.GetInfoAsync(name, artist);
            return resp.Success ? resp.Content : null;
        }

        public async Task<LastTrack> GetDetailTrackByMbid(string mbid)
        {
            var resp = await _trackApi.GetInfoByMbidAsync(mbid);
            return resp.Success ? resp.Content : null;
        }

        public async Task<LastArtist> GetDetailArtist(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            var resp = await _artistApi.GetArtistInfoAsync(name);
            if (!resp.Success) return null;
            return resp.Content;
        }

        public async Task<LastArtist> GetDetailArtistByMbid(string mbid)
        {
            var resp = await _artistApi.GetArtistInfoByMbidAsync(mbid);
            return resp.Success ? resp.Content : null;
        }



        public async Task<PageResponse<LastTrack>> GetArtistTopTracks(string name)
        {
            var resp = await _artistApi.GetTopTracksForArtistAsync(name);
            return resp;
        }


        public async Task<PageResponse<LastAlbum>> GetArtistTopAlbums(string name)
        {
            var resp = await _artistApi.GetTopAlbumsForArtistAsync(name);
            return resp;
        }

        public async Task<PageResponse<LastTrack>> SearchTracksAsync(string query, int page = 1, int limit = 30)
        {
            var resp = await _trackApi.SearchForTrackAsync(query, page, limit);
            return resp;
        }

        public async Task<PageResponse<LastArtist>> SearchArtistsAsync(string query, int page = 1, int limit = 30)
        {
            var resp = await _artistApi.SearchForArtistAsync(query, page, limit);
            return resp;
        }

        public async Task<PageResponse<LastAlbum>> SearchAlbumsAsync(string query, int page = 1, int limit = 30)
        {
            var resp = await _albumApi.SearchForAlbumAsync(query, page, limit);
            return resp;
        }

        public async Task<PageResponse<LastTrack>> GetTopTracksAsync(int page = 1, int limit = 30)
        {
            var resp = await _chartApi.GetTopTracksAsync(page, limit);
            return resp;
        }


        public async Task<WebResults> GetTopArtistsAsync(int page = 1, int limit = 30)
        {
            WebResults results;
            PageResponse<LastArtist> resp = new PageResponse<LastArtist>();
            try
            {
                for (int i = 1; i <= 5; i++)
                {
                    resp = await _chartApi.GetTopArtistsAsync(i, limit);
                    if (resp != null && resp.Content.Count != 0)
                        break;           
                }
            }
            catch
            {

            }

            results = CreateResults(resp);
            results.Artists = new List<WebArtist>();

            int count = 3;
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
                count = 5;

            foreach (var o in resp.Content)
            {
                if (count == 0) break;
                results.Artists.Add(CreateArtist(o, true));
                count--;
            }
            if (resp != null) resp = null;
            return results;
        }



        public async Task<WebResults> GetSimilarArtistsAsync(string name, int limit = 30)
        {
            WebResults results;
            var resp = await _artistApi.GetSimilarArtistsAsync(name, true, limit);

            results = CreateResults(resp);
            results.Artists = new List<WebArtist>();
            foreach (var o in resp.Content)
                results.Artists.Add(CreateArtist(o, false));
            if (resp != null) resp = null;
            return results;
        }

        public async Task<WebResults> GetSimilarTracksAsync(string name, string artistName, int limit = 30)
        {
            WebResults results;
            var resp = await _trackApi.GetSimilarTracksAsync(name, artistName, true, limit);
            results = CreateResults(resp);
            results.Songs = resp.Content.Select(CreateSong).ToList();
            if (resp != null) resp = null;
            return results;
        }


        private WebResults CreateResults<T>(PageResponse<T> paging) where T : new()
        {
            return new WebResults
            {
                HasMore = paging.TotalPages > paging.Page,
                PageToken = $"{paging.Page + 1}"
            };
        }

        private WebSong CreateSong(LastTrack track)
        {
            var song = new WebSong()
            {
                Name = track.Name,
                Id = track.Id,
                Artist = !string.IsNullOrEmpty(track.ArtistName) ? track.ArtistName : "Unknown Artist",
                Album = !string.IsNullOrEmpty(track.AlbumName) ? track.AlbumName : "Unknown Album"
            };

            if (track.Images != null)
            {
                var image = GetImage(track.Images);
                if (image != null)
                    song.ArtworkImage = image;
            }
      
            return song;
        }

        private WebArtist CreateArtist(LastArtist artist, bool lowest = false)
        {
            return new WebArtist()
            {
                Name = artist.Name,
                Artwork = GetImage(artist?.MainImage, lowest),
                Id = artist.Id != null ? artist.Id : artist.Mbid,
            };
        }

        private WebAlbum CreateAlbum(LastAlbum album)
        {

            var webAlbum = new WebAlbum()
            {
                Title = album.Name,
                Id = album.Id!= null ? album.Id : album.Mbid,
                ReleaseDate = album.ReleaseDateUtc.DateTime
            };

            var image = album.Images;
            if (image != null)
            {
                webAlbum.Artwork = GetImage(image);
                webAlbum.HasArtwork = true;
            }

            else
                webAlbum.HasArtwork = false;

            if (!string.IsNullOrEmpty(album.ArtistName))
                webAlbum.ArtistName = album.ArtistName;

            if (album.Tracks != null)
            {
                webAlbum.Tracks = convertToList(album.Tracks.ToList(), album.Name, album.Images);
            }

            return webAlbum;
        }

        private List<WebSong> convertToList(IEnumerable<LastTrack> list, string albumName, IF.Lastfm.Core.Objects.LastImageSet artwork)
        {
            List<WebSong> songs = new List<WebSong>();
            foreach (var deezerSong in list)
            {
                songs.Add(new WebSong()
                {
                    Name = deezerSong.Name,
                    Artist = deezerSong.ArtistName,
                    Id = deezerSong.Id,
                    Album = albumName,
                    ArtworkImage = GetImage(artwork),
                    IsDeezerTrack = true
                });
            }
            return songs;
        }

        Uri GetImage(LastImageSet artwork, bool lowest = false)
        {
            try
            {
                if (lowest)
                   return GetImageLowest(artwork);
                else
                {
                    if (string.IsNullOrEmpty(artwork?.Largest?.ToString()))
                        return artwork?.Largest;
                    else if (string.IsNullOrEmpty(artwork?.Large?.ToString()))
                        return artwork?.Large;
                    return artwork?.ExtraLarge;
                }
            }
            catch
            {
                return new Uri(string.Empty);
            }
        }

        Uri GetImageLowest(LastImageSet artwork)
        {
            try
            {
                if (string.IsNullOrEmpty(artwork?.Small?.ToString()))
                    return artwork?.Small;
                else if (string.IsNullOrEmpty(artwork?.Large?.ToString()))
                    return artwork?.Large;
                else if (string.IsNullOrEmpty(artwork?.Largest?.ToString()))
                    return artwork?.Largest;
                return artwork?.ExtraLarge;
            }
            catch
            {
                return new Uri(string.Empty);
            }
        }


        //public async Task<bool> AuthenticaAsync(string username, string password)
        //{
        //    var result = await GetSessionTokenAsync(username, password);

        //    if (result)
        //    {
        //        _credentialHelper.SaveCredentials("lastfm", username, password);
        //    }

        //    return result;
        //}

        //protected virtual void OnAuthStateChanged()
        //{
        //    var handler = AuthStateChanged;
        //    if (handler != null) handler(this, new BoolEventArgs(IsAuthenticated));
        //}

        //private async Task<bool> GetSessionTokenAsync()
        //{
        //    var creds = _credentialHelper.GetCredentials("lastfm");

        //    if (creds == null) return false;

        //    var result = await GetSessionTokenWithResultsAsync(creds.GetUsername(), creds.GetPassword());

        //    if (result == LastFmApiError.BadAuth)
        //    {
        //        Logout();
        //    }

        //    OnAuthStateChanged();

        //    return result == LastFmApiError.None;
        //}

        //private async Task<bool> GetSessionTokenAsync(string username, string password)
        //{
        //    try
        //    {
        //        var response = await GetSessionTokenWithResultsAsync(username, password);
        //        OnAuthStateChanged();
        //        return response == LastFmApiError.None;
        //    }
        //    catch(Exception)
        //    {
        //        return false;
        //    }
        //}

        //private async Task<LastFmApiError> GetSessionTokenWithResultsAsync(string username, string password)
        //{
        //    try
        //    {
        //        var response = await _auth.GetSessionTokenAsync(username, password);
        //        return response.Error;
        //    }
        //    catch(Exception)
        //    {
        //        return LastFmApiError.RequestFailed;
        //    }
        //}
    }
}