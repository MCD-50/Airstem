﻿#region

using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using Musicus.Core.Common;
using Musicus.Data.Model.WebData;
using Musicus.Data.Model.WebSongs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#endregion

namespace Musicus.Data.Service.Interfaces
{
    public interface IScrobblerService
    {
       // event EventHandler<BoolEventArgs> AuthStateChanged;
       // bool HasCredentials { get; }
       // bool IsAuthenticated { get; }
       // void Logout();

     //   Task<LastFmApiError> ScrobbleNowPlayingAsync(string name, string artist, DateTime played, TimeSpan duration, string album = "",
        //    string albumArtist = "");

      //  Task<LastFmApiError> ScrobbleAsync(string name, string artist, DateTime played, TimeSpan duration, string album = "",
         //   string albumArtist = "");

      //  Task<PageResponse<LastArtist>> GetRecommendedArtistsAsync(int page = 1, int limit = 30);

        Task<LastAlbum> GetDetailAlbum(string name, string artist);
        Task<LastAlbum> GetDetailAlbumByMbid(string mbid);
        Task<LastTrack> GetDetailTrack(string name, string artist);
        Task<LastTrack> GetDetailTrackByMbid(string mbid);
        Task<LastArtist> GetDetailArtist(string name);
        Task<LastArtist> GetDetailArtistByMbid(string mbid);

        Task<PageResponse<LastTrack>> GetArtistTopTracks(string name);
        Task<PageResponse<LastAlbum>> GetArtistTopAlbums(string name);

        Task<PageResponse<LastTrack>> SearchTracksAsync(string query, int page = 1, int limit = 30);
        Task<PageResponse<LastArtist>> SearchArtistsAsync(string query, int page = 1, int limit = 30);
        Task<PageResponse<LastAlbum>> SearchAlbumsAsync(string query, int page = 1, int limit = 30);



        Task<PageResponse<LastTrack>> GetTopTracksAsync(int page = 1, int limit = 30);
        Task<WebResults> GetTopArtistsAsync(int page = 1, int limit = 30);

        Task<WebResults> GetSimilarArtistsAsync(string name, int limit = 30);
        Task<WebResults> GetSimilarTracksAsync(string name, string artistName, int limit = 30);

      //  Task<bool> AuthenticaAsync(string username, string password);
    }
}