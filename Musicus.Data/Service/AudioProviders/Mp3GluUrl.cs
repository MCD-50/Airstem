using Musicus.Core.Extensions;
using Musicus.Core.Utils;
using Musicus.Data.Extensions;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Requests;
using Musicus.Data.Service.AudioProviders.MatchStringExtentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Musicus.Data.Service.AudioProviders
{
    public static class Mp3GluUrl
    {

        public static async Task<List<WebSong>> SearchMp3Glu(string title, string artist, int limit)
        {
            return await SearchMp3GluAsync(title, artist, limit);
        }

        private static async Task<List<WebSong>> SearchMp3GluAsync(string title, string artist, int limit)
        {
            using (var response = await new Mp3GluSearchRequest(title.Append(artist)).ToResponseAsync())
            {
                if (!response.HasData) return null;

                var songNodes1 =
                    response.Data.DocumentNode.Descendants("div");

                var songNodes = 
                        songNodes1.Where(p => p.Attributes["class"]?.Value.Contains("result_list") ?? false).Take(limit);

                var songs = new List<WebSong>();

                foreach (var songNode in songNodes)
                {
                    var song = new WebSong();

                    var detailsNode = songNode.Descendants("em").FirstOrDefault();

                    try
                    {
                        var duration = detailsNode.InnerText.Substring(0, detailsNode.InnerText.IndexOf("min ", StringComparison.Ordinal));
                        if (!string.IsNullOrEmpty(duration))
                        {
                            duration = duration.Replace("Duration : ", "").Trim();
                            var seconds = int.Parse(duration.Substring(duration.Length - 2, 2));
                            var minutes = int.Parse(duration.Remove(duration.Length - 3));
                            song.Duration = new TimeSpan(0, 0, minutes, seconds);
                        }
                    }

                    catch(Exception)
                    {
                        song.Duration = new TimeSpan(0, 4, 0);
                    }

                    var songTitle = songNode.Descendants("strong").FirstOrDefault()?.InnerText;
                    if (string.IsNullOrEmpty(songTitle)) continue;
                    song.SetNameAndArtistFromTitle(WebUtility.HtmlDecode(songTitle.Substring(0, songTitle.Length - 4)).Trim(), true);

                    var linkNode = songNode.Descendants("a")
                        .FirstOrDefault();
                    song.AudioUrl = linkNode?.Attributes["href"]?.Value;

                    if (string.IsNullOrEmpty(song.AudioUrl)) continue;
                    song.ProviderNumber = 14;
                    songs.Add(song);
                }

                return songs.Any() ? await IdentfyMatchTrack.IdentifyMatches(songs, title, artist, false) : null;
            }
        }
    }
}
