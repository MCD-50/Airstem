using Musicus.Core.Utils;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Musicus.Data.Service.AudioProviders
{
    public static class Mp3MinerUrl
    {
        public static async Task<List<WebSong>> SearchMp3Miner(string title, string artist, int limit)
        {
            return await SearchMp3MinerAsync(title, artist, limit);
        }

        private static async Task<List<WebSong>> SearchMp3MinerAsync(string title, string artist, int limit)
        {

            using (var response = await new Mp3MinerSearchRequest("mp3songs", title.Append(artist)).ToResponseAsync())
            {
                if (!response.HasData) return null;

                var songNodes =
                    response.Data.DocumentNode.Descendants("a")
                        .Where(p => p.Attributes.Contains("href") && p.Attributes["href"].Value.Contains("ref=search"))
                        .Take(limit);

                var songs = new List<WebSong>();

                foreach (var songNode in songNodes)
                {
                    using (var InnerResponse = await new Mp3MinerSearchRequest("mp3", songNode.Attributes["href"].Value.Substring(5)).ToResponseAsync())
                    {
                        if (!InnerResponse.HasData)
                        {
                            return null;
                        }

                        var songUrl =
                            InnerResponse.Data.DocumentNode.Descendants("audio")
                                .Where(p => p.Attributes.Contains("src"))
                                .FirstOrDefault();

                        if (songUrl == null)
                        {
                            break;
                        }

                        var innerSongNode =
                            InnerResponse.Data.DocumentNode.Descendants("div")
                                .Where(p => p.Attributes.Contains("class") && p.Attributes["class"].Value.Contains("mdl-card__title"))
                                .FirstOrDefault();

                        if (innerSongNode == null)
                        {
                            break;
                        }

                        var song = new WebSong
                        {
                            Id = System.Text.RegularExpressions.Regex.Match(songNode.Attributes["href"].Value, "/mp3/(.*?)-").Groups[1].ToString(),
                            AudioUrl = songUrl.Attributes["src"]?.Value
                        };

                        if (string.IsNullOrEmpty(song.AudioUrl)) continue;

                        var titleText =
                            innerSongNode.Descendants("h2")
                                .FirstOrDefault(p => p.Attributes["class"]?.Value == "mdl-card__title-text")?.InnerText;
                        if (string.IsNullOrEmpty(titleText)) continue;
                        song.Name = System.Net.WebUtility.HtmlDecode(titleText);

                        var artistText =
                            innerSongNode.Descendants("h3")
                                .FirstOrDefault().ChildNodes.FirstOrDefault(c => c.Attributes.Contains("href"))?.InnerText;
                        if (string.IsNullOrEmpty(artistText)) continue;
                        song.Artist = System.Net.WebUtility.HtmlDecode(artistText);

                        var durationText =
                            InnerResponse.Data.DocumentNode.Descendants("div")
                                .FirstOrDefault(p => p.Attributes["class"]?.Value == "info-section stretchable")?.InnerText;
                        durationText = durationText.Substring(durationText.IndexOf("/") + 1).Trim();
                        TimeSpan duration;
                        if (TimeSpan.TryParse("00:" + durationText, out duration))
                        {
                            song.Duration = duration;
                        }

                        song.ProviderNumber = 7;
                        songs.Add(song);
                    }
                }

                return songs.Any() ? await MatchStringExtentions.IdentfyMatchTrack.IdentifyMatches(songs, title, artist, false) : null;
            }
        }
    }
}
