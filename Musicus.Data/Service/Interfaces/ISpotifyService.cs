#region

using Musicus.Data.Spotify.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

#endregion

namespace Musicus.Data.Service.Interfaces
{
    public interface ISpotifyService
    {
        Task<List<ChartTrack>> GetViralTracksAsync(string market = "us", string time = "weekly");
        Task<List<ChartTrack>> GetMostStreamedTracksAsync(string market = "us", string time = "weekly");
        Task<Paging<SimpleAlbum>> GetNewAlbumReleases(string country = "US", int limit = 20, int offset = 0);


        Task<FullArtist> GetArtistAsync(string id);
        Task<List<FullTrack>> GetArtistTracksAsync(string id);
        Task<Paging<SimpleAlbum>> GetArtistAlbumsAsync(string id, int limit = 20);

        Task<FullAlbum> GetAlbumAsync(string id);
        Task<Paging<SimpleTrack>> GetAlbumTracksAsync(string id);
        Task<Paging<FullTrack>> SearchTracksAsync(string query, int limit = 20, int offset = 0);
        Task<Paging<FullArtist>> SearchArtistsAsync(string query, int limit = 20, int offset = 0);
        Task<Paging<SimpleAlbum>> SearchAlbumsAsync(string query, int limit = 20, int offset = 0);
    }
}