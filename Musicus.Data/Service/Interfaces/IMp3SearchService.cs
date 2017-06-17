#region
using Musicus.Data.Model.WebSongs;
using System.Collections.Generic;
using System.Threading.Tasks;

#endregion

namespace Musicus.Data.Service.Interfaces
{
    public interface IMp3SearchService
    {
        Task<List<WebSong>> SearchMp3Freex(string title, string artist, int limit = 4);
        Task<List<WebSong>> SearchMp3lio(string title, string artist, int limit = 4);
        Task<List<WebSong>> SearchMp3Pm(string title, string artist, int limit = 4);
        Task<List<WebSong>> SearchAnyMaza(string title, string artist, int limit = 4);
        Task<List<WebSong>> SearchMp3Miner(string title, string artist, int limit = 4);
        Task<List<WebSong>> SearchMiMp3(string title, string artist, int limit = 4);
        Task<List<WebSong>> SearchMp3Glu(string title, string artist, int limit = 4);
        Task<List<WebSong>> SearchMeile(string title, string artist, int limit = 4,
          bool checkAllLinks = false);
        Task<List<WebSong>> SearchNetease(string title, string artist, int limit = 4,
          bool checkAllLinks = false);
        Task<List<WebSong>> SearchPleer(string title, string artist, int limit = 4);
        Task<List<WebSong>> SearchYoutube(string title, string artist, int limit = 4, bool includeAudioTag = true);

        Task<List<WebSong>> SearchSoundCloud(string title, string artist, int limit = 4);

        Task<List<WebSong>> SearchMp3Clan(string title, string artist, int limit = 4,
            bool checkAllLinks = false);

        Task<List<WebSong>> SearchSongily(string title, string artist, int page = 1,
            bool checkAllLinks = false);
        Task<List<WebSong>> SearchMp3Truck(string title, string artist,
            bool checkAllLinks = false);
        Task<List<WebSong>> SearchMp3Skull(string title, string artist,
            bool checkAllLinks = false);

        string GetYoutubeUrl(string id);

        Task<string> GetMetroLyrics(string title, string artist);
    }
}