using HtmlAgilityPack;
using Musicus.Core.Extensions;
using Musicus.Core.Utils;
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
    public static class Mp3LioUrl
    {
        public static async Task<List<WebSong>> SearchMp3lio(string title, string artist, int limit)
        {
            return await SearchMp3lioAync(title, artist, limit);
        }

        private static async Task<List<WebSong>> SearchMp3lioAync(string title, string artist, int limit)
        {
            using (var response = await new Mp3lioSearchRequest(title.Append(artist)).ToResponseAsync().DontMarshall())
            {
                if (!response.HasData) return null;

                var songNodes =
                    response.Data.DocumentNode.Descendants("div")
                        .Where(p => p.Attributes["class"]?.Value.Contains("item") ?? false).Take(limit);

                var songs = new List<WebSong>();

                foreach (var songNode in songNodes)
                {
                    var song = new WebSong();

                    var duration = songNode.Descendants("em").FirstOrDefault();
                    if (duration != null)
                    {
                        try
                        {
                            var text = duration.InnerText.Replace("Duration: ", "").Replace(" min", "");
                            var seconds = int.Parse(text.Substring(text.Length - 2, 2));
                            var minutes = int.Parse(text.Remove(text.Length - 3));
                            song.Duration = new TimeSpan(0, 0, minutes, seconds);
                        }

                        catch (Exception)
                        {
                            song.Duration = new TimeSpan(0, 4, 0);
                        }

                    }

                    //http://c.mp3fly.in/get.php?id=z-diRlyLGzo&name=Channa+Mereya+-++Ae+Dil+Hai+Mushkil++Karan+Johar++Ranbir++Anushka++Aishwarya++Pritam++Arijit&hash=73429b2cf5cf826f4fc8d4071112f60a1361cc6b&expire=1479907845

                    var songTitle = songNode.Descendants("strong").FirstOrDefault()?.InnerText;
                    if (string.IsNullOrEmpty(songTitle)) continue;
                    song.Name = songTitle;
                    

                    var linkNode = songNode.Descendants("a").FirstOrDefault();

                    if (linkNode?.Attributes["href"]?.Value != null)
                    {
                        var webConnect = new HtmlWeb();
                        HtmlDocument _htmlDoc = await webConnect.LoadFromWebAsync(linkNode?.Attributes["href"]?.Value);
                        var htmlNode = _htmlDoc.DocumentNode.Descendants("a");
                        foreach(HtmlNode node in htmlNode)
                        {
                            if (node.Attributes["href"]?.Value.Contains("http://c.mp3fly.in/") == true)
                            {
                                song.AudioUrl = node.Attributes["href"]?.Value;
                                break;
                            }
                        }
                       // song.AudioUrl = linkNode?.Attributes["href"]?.Value;
                    }
                   
                    if (string.IsNullOrEmpty(song.AudioUrl))
                        continue;

                    song.ProviderNumber = 4;
                    songs.Add(song);
                }

                return songs.Any() ? await IdentfyMatchTrack.IdentifyMatches(songs, title, artist, false) : null;
            }


        }


    }
}
