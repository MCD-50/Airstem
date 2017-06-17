
using Musicus.Core.Utils;
using Musicus.Data.Model.SoundCloud;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Service.AudioProviders.MatchStringExtentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


namespace Musicus.Data.Service.AudioProviders
{
    public static class SoundCloudUrl
    {
        //public static async Task<List<WebSong>> SearchSoundCloud(string title, string artist, int limit)
        //{
        //    return await SearchSoundCloudAsync(title, artist, limit);
        //}

        //private static async Task<List<WebSong>> SearchSoundCloudAsync(string title, string artist, int limit)
        //{
        //    using (var response = await new Requests.SoundCloudSearchRequest(ApiKeys.SoundCloudId, title.Append(artist))
        //        .Limit(limit).ToResponseAsync().DontMarshall())
        //    {
        //        return await IdentfyMatchTrack.IdentifyMatches(response.Data?.Collection?.Select(p => new WebSong(p))?.ToList(), title, artist, false); 
        //    }
        //}

        public static async Task<List<WebSong>> SearchSoundCloud(string title, string artist, int limit)
        {
            return await SearchSoundCloudAsync(title, artist, limit);
        }

        private const string SoundCloudSearchUrl =
           "https://api.soundcloud.com/search/sounds?client_id={0}&limit={1}&q={2}";

        private static async Task<List<WebSong>> SearchSoundCloudAsync(string title, string artist, int limit)
        {
            var url = string.Format(
                SoundCloudSearchUrl,
                ApiKeys.SoundCloudId,
                limit,
                IdentfyMatchTrack.CreateQuery(title, artist));

            using (var client = new HttpClient())
            {
                using (var resp = await client.GetAsync(url))
                {
                    var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var parseResp = await json.DeserializeAsync<SoundCloudRoot>().ConfigureAwait(false);
                    if (parseResp == null || parseResp.Collection == null || !resp.IsSuccessStatusCode)
                    {
                        return null;
                    }


                    parseResp.Collection.RemoveAll(p => p.StreamUrl == null);

                    foreach (var song in parseResp.Collection)
                    {
                        if (song.StreamUrl.Contains("soundcloud") && !song.StreamUrl.Contains("client_id"))
                        {
                            song.StreamUrl += "?client_id=" + ApiKeys.SoundCloudId;

                        }

                        if (string.IsNullOrEmpty(song.ArtworkUrl))
                        {
                            song.ArtworkUrl = song.User.AvatarUrl;
                        }

                        if (song.ArtworkUrl.IndexOf('?') > -1)
                        {
                            song.ArtworkUrl = song.ArtworkUrl.Remove(song.ArtworkUrl.LastIndexOf('?'));
                        }
                        song.ProviderNumber = 8;
                        song.ArtworkUrl = song.ArtworkUrl.Replace("large", "t500x500");
                    }

                    return await
                            IdentfyMatchTrack.IdentifyMatches(parseResp.Collection.Select(p => new WebSong(p)).ToList(), title, artist, false);
                }
            }
        }
    }
}
