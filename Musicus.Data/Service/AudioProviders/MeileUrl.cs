using Musicus.Core.Extensions;
using Musicus.Core.Utils;
using Musicus.Data.Model.Meile;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Requests;
using Musicus.Data.Service.AudioProviders.MatchStringExtentions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Musicus.Data.Service.AudioProviders
{
    public static class MeileUrl
    {
        public static async Task<List<WebSong>> SearchMeile(string title, string artist, int limit,
         bool checkAllLinks)
        {
            return await SearchMeileAsync(title, artist, limit, checkAllLinks);
        }
           
        private static async Task<List<WebSong>> SearchMeileAsync(string title, string artist, int limit , bool checkAllLinks)
        {
            using (var response = await new MeileSearchRequest(title.Append(artist))
                .Limit(limit).ToResponseAsync().DontMarshall())
            {
                if (!response.HasData) return null;
                var htmlDocument = response.Data;

                // Get the hyperlink node with the class='name'
                var songNameNodes =
                    htmlDocument.DocumentNode.Descendants("a")
                        .Where(p => p.Attributes.Contains("class") && p.Attributes["class"].Value == "name");

                // in it there is an attribute that contains the url to the song
                var songUrls = songNameNodes.Select(p => p.Attributes["href"].Value);
                var songIds = songUrls.Where(p => p.Contains("/song/")).ToList();

                var songs = new List<WebSong>();

                foreach (var songId in songIds)
                {
                    var song = await GetMeileDetailsAsync(songId.Replace("/song/", string.Empty)).DontMarshall();
                    if (song != null)
                    {
                        songs.Add(new WebSong(song));
                    }
                }

                return songs.Any() ? await IdentfyMatchTrack.IdentifyMatches(songs, title, artist, false) : null;
            }
        }

        private static async Task<MeileSong> GetMeileDetailsAsync(string id)
        {
            using (var response = await new MeileDetailsRequest(id).ToResponseAsync().DontMarshall())
            {
                return response.Data?.Values?.Songs?.FirstOrDefault();
            }
        }



    }
}
