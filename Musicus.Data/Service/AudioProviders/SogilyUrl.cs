using HtmlAgilityPack;
using Musicus.Core.Utils;
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
    public static class SogilyUrl
    {
        public static async Task<List<WebSong>> SearchSongily(string title, string artist, int page, bool checkAllLinks)
        {
            return await SearchSongilyAsync(title, artist, page, checkAllLinks);
        }

        private const string SongilySearchUrl = "http://songily.com/mp3/download/{1}/{0}.html";
        private static async Task<List<WebSong>> SearchSongilyAsync(string title, string artist, int page,bool checkAllLinks)
        {
            var url = string.Format(SongilySearchUrl, WebUtility.UrlEncode(IdentfyMatchTrack.CreateQuery(title, artist, false).ToCleanQuery()), page);
            using (var client = new HttpClient())
            {
                using (var resp = await client.GetAsync(url).ConfigureAwait(false))
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
                        doc.DocumentNode.Descendants("li")
                            .Where(
                                p => p.Attributes.Contains("class") && p.Attributes["class"].Value.Contains("list-group-item"));

                    var songs = new List<WebSong>();

                    foreach (var songNode in songNodes)
                    {
                        var song = new WebSong { Provider = Mp3Provider.Mp3Truck };

                        try {
                            var detailNode = songNode.Descendants("small").FirstOrDefault();
                            if (detailNode != null)
                            {
                                var duration = detailNode.InnerText;

                                var durIndex = duration.IndexOf(":");
                                if (durIndex > 0)
                                {
                                    var seconds = int.Parse(duration.Substring(durIndex + 1, 2));
                                    var minutes = int.Parse(duration.Substring(0, durIndex)); ;
                                    song.Duration = new TimeSpan(0, 0, minutes, seconds);
                                }
                            }
                        }
                        catch(Exception)
                        {
                            song.Duration = new TimeSpan(0, 4, 0);
                        }
                        var songTitle =
                            songNode.Descendants("span")
                                .FirstOrDefault();

                        if (songTitle == null) continue;

                        song.Name = WebUtility.HtmlDecode(songTitle.InnerText).Trim();

                        var linkNode =
                            songNode.Descendants("a")
                                .FirstOrDefault(
                                    p =>
                                        p.Attributes.Contains("title")
                                        && p.Attributes["title"].Value.Contains("Download"));
                        if (linkNode == null)
                        {
                            continue;
                        }

                        song.AudioUrl = "http://songily.com/" + linkNode.Attributes["href"].Value;
                        song.ProviderNumber = 2;
                        songs.Add(song);
                    }

                    return songs.Any() ? await IdentfyMatchTrack.IdentifyMatches(songs, title, artist, checkAllLinks) : null;
                }
            }
        }

    }
}
