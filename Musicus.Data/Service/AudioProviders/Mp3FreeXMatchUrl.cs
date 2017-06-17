using Musicus.Core.Extensions;
using Musicus.Core.Utils;
using Musicus.Data.Extensions;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Requests;
using Musicus.Data.Service.AudioProviders.MatchStringExtentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Musicus.Data.Service.AudioProviders
{
    public static class Mp3FreeXMatchUrl
    {

        public static async Task<List<WebSong>> SearchMp3Freex(string title, string artist, int limit)
        {
            return await SearchMp3FreexAsync(title, artist, limit);
        }



        private static async Task<List<WebSong>> SearchMp3FreexAsync(string title, string artist, int limit)
        {
            using (var response = await new Mp3FreexSearchRequest(title.Append(artist)).ToResponseAsync().DontMarshall())
            {
                if (!response.HasData) return null;

                var songNodes =
                    response.Data.DocumentNode.Descendants("div")
                        .Where(p => p.Attributes["class"]?.Value.Contains("actl") ?? false).Take(limit);

                var songs = new List<WebSong>();

                foreach (var songNode in songNodes)
                {
                    var song = new WebSong();

                    var songTitle =
                        songNode.Descendants("span")
                            .FirstOrDefault(p => p.Attributes["class"]?.Value == "res_title")?
                            .InnerText;
                    if (string.IsNullOrEmpty(songTitle)) continue;
                    song.SetNameAndArtistFromTitle(songTitle, true);

                    var meta =
                        songNode.Descendants("span")
                            .FirstOrDefault(p => p.Attributes["class"]?.Value == "label label-info")?
                            .InnerText;

                    if (!string.IsNullOrEmpty(meta))
                    {
                        try
                        {
                            var duration = meta.Substring(0, meta.IndexOf("|", StringComparison.Ordinal)).Trim();
                            var seconds = int.Parse(duration.Substring(duration.Length - 2, 2));
                            var minutes = int.Parse(duration.Remove(duration.Length - 3));
                            song.Duration = new TimeSpan(0, 0, minutes, seconds);

                        }
                        catch
                        {
                            song.Duration = new TimeSpan(0, 4, 0);
                        }
                    }

                    var linkNode =
                        songNode.Descendants("a")
                            .FirstOrDefault(p => p.Attributes["class"]?.Value.Contains("mp3download") ?? false);
                    var url = linkNode?.Attributes["data-online"]?.Value;

                    if (string.IsNullOrEmpty(url)) continue;

                    song.AudioUrl = await GetAudioUrlAsync(url).DontMarshall();

                    if (string.IsNullOrEmpty(song.AudioUrl)) continue;
                    song.ProviderNumber = 3;
                    songs.Add(song);
                }

                return songs.Any() ? await IdentfyMatchTrack.IdentifyMatches(songs, title, artist, false) : null;
            }
        }

        private static async Task<string> GetAudioUrlAsync(string downloadPart)
        {
            using (var response = await ("http://mp3freex.net/listen-online/" + downloadPart).ToUri().GetAsync().DontMarshall())
            {
                if (!response.IsSuccessStatusCode) return null;
                var doc = await response.ParseHtmlAsync().DontMarshall();
                var linkNode = doc.DocumentNode.Descendants("div").FirstOrDefault(p => p.Attributes["class"]?.Value.Contains("player") ?? false);
                return linkNode?.Attributes["data-url"]?.Value;
            }
        }


    }
}
