using HtmlAgilityPack;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Service.AudioProviders.MatchStringExtentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Musicus.Data.Service.AudioProviders
{
    public static class Mp3TruckUrl
    {

        public static async Task<List<WebSong>> SearchMp3Truck(string title, string artist,
            bool checkAllLinks)
        {
            return await SearchMp3TruckAsync(title,artist,checkAllLinks);
        }

        private const string Mp3TruckSearchUrl = "https://mp3truck.net/ajaxRequest.php";

        private static async Task<List<WebSong>> SearchMp3TruckAsync(string title, string artist,
            bool checkAllLinks)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Referrer = new Uri("https://mp3truck.net/");
                var data = new Dictionary<string, string>
                {
                    {"sort", "relevance"},
                    {"p", "1"},
                    {"q", IdentfyMatchTrack.CreateQuery(title, artist)}
                };

                using (var content = new FormUrlEncodedContent(data))
                {
                    using (var resp = await client.PostAsync(Mp3TruckSearchUrl, content).ConfigureAwait(false))
                    {
                        if (!resp.IsSuccessStatusCode)
                        {
                            return null;
                        }

                        var html = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

                        var doc = new HtmlDocument();
                        doc.LoadHtml(html);

                        // Get the div node with the class='actl'
                        var songNodes =
                            doc.DocumentNode.Descendants("div")
                                .Where(
                                    p => p.Attributes.Contains("class") && p.Attributes["class"].Value.Contains("actl"));

                        var songs = new List<WebSong>();

                        foreach (var songNode in songNodes)
                        {
                            var song = new WebSong { Provider = Mp3Provider.Mp3Truck };

                            if (songNode.Attributes.Contains("data-id"))
                            {
                                song.Id = songNode.Attributes["data-id"].Value;
                            }

                            if (songNode.Attributes.Contains("data-bitrate"))
                            {
                                song.BitRate = int.Parse(songNode.Attributes["data-bitrate"].Value);
                            }

                            if (songNode.Attributes.Contains("data-filesize"))
                            {
                                song.ByteSize = (int)double.Parse(songNode.Attributes["data-filesize"].Value);
                            }

                            if (songNode.Attributes.Contains("data-duration"))
                            {
                                var duration = songNode.Attributes["data-duration"].Value;

                                if (duration.Contains(":"))
                                {
                                    var seconds = int.Parse(duration.Substring(duration.Length - 2, 2));
                                    var minutes = int.Parse(duration.Remove(duration.Length - 3));
                                    song.Duration = new TimeSpan(0, 0, minutes, seconds);
                                }
                                else
                                {
                                    song.Duration = new TimeSpan(0, 0, 0, int.Parse(duration));
                                }
                            }

                            var songTitle =
                                songNode.Descendants("div")
                                    .FirstOrDefault(
                                        p => p.Attributes.Contains("id") && p.Attributes["id"].Value == "title")
                                    .InnerText;
                            songTitle = WebUtility.HtmlDecode(songTitle.Substring(0, songTitle.Length - 4)).Trim();

                            // artist - title
                            var dashIndex = songTitle.IndexOf('-');
                            if (dashIndex != -1)
                            {
                                var titlePart = songTitle.Substring(dashIndex, songTitle.Length - dashIndex);
                                song.Artist = songTitle.Replace(titlePart, string.Empty).Trim();

                                songTitle = titlePart.Remove(0, 1).Trim();
                            }

                            song.Name = songTitle;

                            var linkNode =
                                songNode.Descendants("a")
                                    .FirstOrDefault(
                                        p =>
                                            p.Attributes.Contains("class")
                                            && p.Attributes["class"].Value.Contains("mp3download"));
                            if (linkNode == null)
                            {
                                continue;
                            }

                            song.AudioUrl = linkNode.Attributes["href"].Value.Replace("/idl.php?u=", string.Empty);
                            song.ProviderNumber = 13;
                            songs.Add(song);
                        }

                        return songs.Any() ? await IdentfyMatchTrack.IdentifyMatches(songs, title, artist, checkAllLinks) : null;
                    }
                }
            }
        }
    }
}
