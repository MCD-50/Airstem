using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Musicus.Core.Utils;
using Musicus.Data.Spotify.Models;

namespace Musicus.Data.Spotify
{
    public sealed class SpotifyWebApi
    {
        public String TokenType { get; set; }
        public String AccessToken { get; set; }

        #region Search and Fetch

        public Task<List<ChartTrack>> GetViralTracks(string country = "US", string time = "weekly")
        {
            return _GetViralTracks("most_viral", country, time); ;
        }

        public Task<List<ChartTrack>> GetMostStreamedTracks(string country = "US", string time = "weekly")
        {
            return _GetViralTracks("most_streamed", country, time);
        }


        public Task<Paging<SimpleAlbum>> GetNewAlbumReleases(String country = "US", int limit = 20, int offset = 0)
        {
            limit = Math.Max(20, limit);
            StringBuilder builder = new StringBuilder("https://api.spotify.com/v1/browse/new-releases");
            builder.Append("?limit=" + limit);
            builder.Append("&offset=" + offset);
            if (country != "")
                builder.Append("&country=" + country);
            return DownloadDataAsync<Paging<SimpleAlbum>>(builder.ToString());

        }


        private async Task<List<ChartTrack>> _GetViralTracks(string type, string country = "US", string time = "weekly")
        {
            var viral = (await DownloadDataAsync<SpotifyChartsRoot>(
                string.Format("http://charts.spotify.com/api/tracks/{0}/{1}/{2}/latest", 
                type, country, time)));

            return viral != null ? viral.tracks : null;
        }




        public Task<SearchItem> SearchItems(String q, SearchType type, int limit = 20, int offset = 0, string market = "US")
        {
            limit = Math.Min(30, limit);
            var builder = new StringBuilder("https://api.spotify.com/v1/search");
            builder.Append("?q=" + WebUtility.UrlEncode(q));
            builder.Append("&type=" + type.GetSearchValue(","));
            builder.Append("&limit=" + limit);
            builder.Append("&offset=" + offset);
            builder.Append("&market=" + market);

            return DownloadDataAsync<SearchItem>(builder.ToString());
        }
        public Task<SeveralTracks> GetSeveralTracks(List<String> ids)
        {
            return DownloadDataAsync<SeveralTracks>("https://api.spotify.com/v1/tracks?ids=" + string.Join(",", ids));
        }
        public Task<SeveralAlbums> GetSeveralAlbums(List<String> ids)
        {
            return DownloadDataAsync<SeveralAlbums>("https://api.spotify.com/v1/albums?ids=" + string.Join(",", ids));
        }
        public Task<SeveralArtists> GetSeveralArtists(List<String> ids)
        {
            return DownloadDataAsync<SeveralArtists>("https://api.spotify.com/v1/artists?ids=" + string.Join(",", ids));
        }
        public Task<FullTrack> GetTrack(String id)
        {
            return DownloadDataAsync<FullTrack>("https://api.spotify.com/v1/tracks/" + id);
        }
        public Task<SeveralArtists> GetRelatedArtists(String id)
        {
            return DownloadDataAsync<SeveralArtists>("https://api.spotify.com/v1/artists/" + id + "/related-artists");
        }
        public Task<SeveralTracks> GetArtistsTopTracks(String id, String country)
        {
            return DownloadDataAsync<SeveralTracks>("https://api.spotify.com/v1/artists/" + id + "/top-tracks?country=" + country);
        }
        public Task<Paging<SimpleAlbum>> GetArtistsAlbums(String id, AlbumType type = AlbumType.ALL, String market = "", int limit = 20, int offset = 0)
        {
            limit = Math.Min(15, limit);
            var builder = new StringBuilder("https://api.spotify.com/v1/artists/" + id + "/albums");
            builder.Append("?album_type=" + type.GetAlbumValue(","));
            builder.Append("&limit=" + limit);
            builder.Append("&offset=" + offset);
            if (market != "")
                builder.Append("&market=" + market);
            return DownloadDataAsync<Paging<SimpleAlbum>>(builder.ToString());
        }
        public Task<FullArtist> GetArtist(String id)
        {
            return DownloadDataAsync<FullArtist>("https://api.spotify.com/v1/artists/" + id);
        }
        public Task<Paging<SimpleTrack>> GetAlbumTracks(String id, int limit = 20, int offset = 0)
        {
            limit = Math.Min(20, limit);
            var builder = new StringBuilder("https://api.spotify.com/v1/albums/" + id + "/tracks");
            builder.Append("?limit=" + limit);
            builder.Append("&offset=" + offset);
            return DownloadDataAsync<Paging<SimpleTrack>>(builder.ToString());
        }
        public Task<FullAlbum> GetAlbum(String id)
        {
            return DownloadDataAsync<FullAlbum>("https://api.spotify.com/v1/albums/" + id);
        }
        #endregion

        #region Util
        public async Task<T> DownloadDataAsync<T>(String url)
        {
            try
            {
                return await (await DownloadStringAsync(url)).DeserializeAsync<T>();
            }
            catch
            {
                return default (T);
            }
        }

        public async Task<string> DownloadStringAsync(String url)
        {
            if (string.IsNullOrEmpty(url)) return "";
            using (var client = new HttpClient())
            {
                using (var resp = await client.GetAsync(url))
                {
                    var text = await resp.Content.ReadAsStringAsync();
                    //Debug.WriteLine(text);
                    return text;
                 
                }
            }
        }

        #endregion
    }
}