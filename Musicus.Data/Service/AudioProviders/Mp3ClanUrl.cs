using Musicus.Core.Utils;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Mp3Clan.Model;
using Musicus.Data.Service.AudioProviders.MatchStringExtentions;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


namespace Musicus.Data.Service.AudioProviders
{
    public static class Mp3ClanUrl
    {

        public static async Task<List<WebSong>> SearchMp3Clan(string title, string artist, int limit ,bool checkAllLinks)
        {
            return await SearchMp3ClanAsync(title, artist, limit,checkAllLinks);
        }

        private const string Mp3ClanSearchUrl = "http://mp3clan.com/app/mp3Search.php?q={0}&count={1}";
        private static async Task<List<WebSong>> SearchMp3ClanAsync(string title, string artist, int limit , bool checkAllLinks )
        {
            // mp3clan search doesn't work that well with the pound key (even encoded)
            var url = string.Format(
                Mp3ClanSearchUrl,
                IdentfyMatchTrack.CreateQuery(title.Contains("#") ? title.Replace("#", string.Empty) : title, artist),
                limit);

            using (var client = new HttpClient())
            {
                using (var resp = await client.GetAsync(url).ConfigureAwait(false))
                {
                    var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var parseResp = await json.DeserializeAsync<Mp3ClanRoot>().ConfigureAwait(false);

                    if (parseResp == null || parseResp.response == null || !resp.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    return
                        await
                            IdentfyMatchTrack.IdentifyMatches(parseResp.response.Select(p => new WebSong(p)).ToList(), title, artist,
                                checkAllLinks);
                }
            }
        }
    }
}
