using Musicus.Core.Utils;
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
    public static class Mp3PmUrl
    {

        public static async Task<List<WebSong>> SearchMp3Pm(string title, string artist, int limit)
        {
            return await SearchMp3PmAsync(title, artist, limit);
        }
        private static async Task<List<WebSong>> SearchMp3PmAsync(string title, string artist, int limit )
        {
            // in the query, the artist goes first. Results don't work otherwise.
            using (var response = await new Mp3PmSearchRequest(artist.Append(title)).ToResponseAsync())
            {
                if (!response.HasData) return null;

                var songNodes =
                    response.Data.DocumentNode.Descendants("li")
                        .Where(p => p.Attributes.Contains("data-sound-url"))
                        .Take(limit);

                var songs = new List<WebSong>();

                foreach (var songNode in songNodes)
                {
                    var song = new WebSong
                    {
                        Id = songNode.Attributes["data-sound-id"]?.Value,
                        AudioUrl = songNode.Attributes["data-sound-url"]?.Value
                    };

                    if (string.IsNullOrEmpty(song.AudioUrl)) continue;

                    var titleText =
                        songNode.Descendants("b")
                            .FirstOrDefault(p => p.Attributes["class"]?.Value == "cplayer-data-sound-title")?.InnerText;
                    if (string.IsNullOrEmpty(titleText)) continue;
                    song.Name = WebUtility.HtmlDecode(titleText);

                    var artistText =
                        songNode.Descendants("i")
                            .FirstOrDefault(p => p.Attributes["class"]?.Value == "cplayer-data-sound-author")?.InnerText;
                    if (string.IsNullOrEmpty(artistText)) continue;
                    song.Artist = WebUtility.HtmlDecode(artistText);

                    try
                    {
                        var durationText =
                            songNode.Descendants("em")
                                .FirstOrDefault(p => p.Attributes["class"]?.Value == "cplayer-data-sound-time")?.InnerText;
                        TimeSpan duration;
                        if (TimeSpan.TryParse("00:" + durationText, out duration))
                        {
                            song.Duration = duration;
                        }
                    }
                    catch
                    {
                        song.Duration = new TimeSpan(0, 0, 4, 0);
                    }
                    song.ProviderNumber = 5;
                    songs.Add(song);
                }
                return songs.Any() ? await IdentfyMatchTrack.IdentifyMatches(songs, title, artist, false) : null;
            }
        }

    }
}
