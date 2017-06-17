using HtmlAgilityPack;
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
using System.Net.Http;
using System.Threading.Tasks;


namespace Musicus.Data.Service.AudioProviders
{
    public static class MiMp3Url
    {

        public static async Task<List<WebSong>> SearchMiMp3(string title, string artist, int limit)
        {
            return await SearchMiMp3Aync(title, artist, limit);
        }
        private static async Task<List<WebSong>> SearchMiMp3Aync(string title, string artist, int limit)
        {
            using (var resp = await new MiMp3SSearchRequest(title.Append(artist).ToLower()).ToResponseAsync())
            {
                if (!resp.HasData) return null;

                var songNodes =
                    resp.Data.DocumentNode.Descendants("ul")
                        .FirstOrDefault(p => p.Attributes["class"]?.Value == "mp3-list").Descendants("li");

                var songs = new List<WebSong>();

                foreach (var songNode in songNodes)
                {
                    var song = new WebSong();

                    var titleNode =
                        songNode.Descendants("span").FirstOrDefault(p => p.Attributes["class"]?.Value == "mp3-title");
                    if (titleNode == null) continue;

                    titleNode.Descendants("font").FirstOrDefault().Remove();
                    var songTitle = titleNode.InnerText;
                    if (string.IsNullOrEmpty(songTitle)) continue;

                    song.Name = songTitle.Remove(songTitle.LastIndexOf(" - MP3", StringComparison.Ordinal)).Trim();

                    var meta =
                        WebUtility.HtmlDecode(
                            songNode.Descendants("span")
                                .FirstOrDefault(p => p.Attributes["class"]?.Value == "mp3-url")
                                .InnerText);
                    if (!string.IsNullOrEmpty(meta))
                    {
                        try
                        {
                            var duration = meta.Substring(10, meta.IndexOf(" ", StringComparison.Ordinal) - 10).Trim();
                            TimeSpan parsed;
                            if (TimeSpan.TryParse("00:" + duration, out parsed))
                            {
                                song.Duration = parsed;
                            }
                        }
                        catch
                        {
                            song.Duration = new TimeSpan(0, 4, 0);  //ignored
                        }
                    }

                    var linkNode =
                        songNode.Descendants("a").FirstOrDefault(p => p.Attributes["class"]?.Value == "play_button");
                    if (linkNode == null) continue;

                    song.AudioUrl = linkNode.Attributes["href"]?.Value;

                    if (string.IsNullOrEmpty(song.AudioUrl)) continue;
                    song.AudioUrl = await GetMiMp3AudioUrlAsync(song.AudioUrl);
                    if (string.IsNullOrEmpty(song.AudioUrl)) continue;

                    song.ProviderNumber = 10;
                    songs.Add(song);
                }
                songs.Take(limit).ToList();
                return songs.Any() ? await IdentfyMatchTrack.IdentifyMatches(songs, title, artist, false) : null;
            }
        }

        private static async Task<string> GetMiMp3AudioUrlAsync(string url)
        {
            using (var response = await (url).ToUri().GetAsync().DontMarshall())
            {
                if (!response.IsSuccessStatusCode) return null;

                var doc = await response.ParseHtmlAsync().DontMarshall();

                var linkNode = doc.DocumentNode.Descendants("div")
                    .FirstOrDefault(p => p.Attributes.Contains("data-url"));
                return linkNode?.Attributes["data-url"]?.Value;
            }
        }

    }
}
