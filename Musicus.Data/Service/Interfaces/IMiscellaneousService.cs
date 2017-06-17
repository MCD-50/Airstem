using Musicus.Data.Model.WebData;
using Musicus.Data.Model.WebSongs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Musicus.Data.Service.Interfaces
{
    public interface IMiscellaneousService
    {
        Task<WebResults> GetTopAlbumsAsync(int limit = 20, string pageToken = null);
        Task<WebResults> GetTopTracksAsync(int limit = 20, string pageToken = null);
        Task<WebAlbum> GetDeezerAlbumSongsAsync(string albumToken);
        Task<string> GetVideo(string term);
        Task<List<WebSong>> GetNewRelesedSong(int count);
        Task<List<WebSong>> GetViezResultsAsync(string term, int count);
        //Task<IEnumerable<WebVideo>> GetPopularCategoryVideos(int count, string cat);
        Task<IEnumerable<WebVideo>> GetSearchedVideos(int count, string term);
        Task<WebVideo> GetYVideoFromId(string id);
        Task<IEnumerable<WebVideo>> GetRelatedYVideoFromId(int count, string id);
    }
}