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
    public static class AnyMazaUrl
    {
        public static async Task<List<WebSong>> SearchAnyMaza(string title, string artist, int limit)
        {
            return await SearchAnyMazaAync(title, artist, limit);
        }

        private static async Task<List<WebSong>> SearchAnyMazaAync(string title, string artist, int limit)
        {
            using (var resp = await new AnyMazaSearchRequest(title.Append(artist)).ToResponseAsync())
            {
                if (!resp.HasData) return null;

                var songs = new List<WebSong>();

                foreach (var googleResult in resp.Data.ResponseData.Results)
                {
                    var song = new WebSong();

                    if (!googleResult.Url.StartsWith("http://anymaza.com/music/")) continue;

                    song.SetNameAndArtistFromTitle(googleResult.TitleNoFormatting.Remove(googleResult.TitleNoFormatting.LastIndexOf(artist, StringComparison.Ordinal) + artist.Length).Replace(" By ", " - "), false);

                    using (var anymazaResp = await googleResult.Url.ToUri().GetAsync())
                    {
                        if (!anymazaResp.IsSuccessStatusCode) continue;

                        var document = await anymazaResp.ParseHtmlAsync();

                       
                        var duration = document.DocumentNode.Descendants("center").FirstOrDefault()?.InnerText;
                        //Category: Justin Bieber - Purpose ( Deluxe Edition ) Duration: 3:28 sec Singer : Justin Bieber
                        if (!string.IsNullOrEmpty(duration))
                        {
                            try
                            {
                                duration = duration.Substring(duration.IndexOf("Duration:", StringComparison.Ordinal) + 9);
                                duration = duration.Remove(duration.LastIndexOf("sec", StringComparison.Ordinal)).Trim();
                                TimeSpan dur;
                                if (TimeSpan.TryParse("00:" + duration, out dur))
                                {
                                    song.Duration = dur;
                                }
                            }
                            catch
                            {
                                song.Duration = new TimeSpan(0, 4, 0);
                            }
                        }

                        var link = document.DocumentNode.Descendants("a")
                            .FirstOrDefault(p => p.Attributes["class"]?.Value == "dowbutzip")?.Attributes["href"]?.Value;

                        if (string.IsNullOrEmpty(link)) continue;

                        if (link.StartsWith("/"))
                            song.AudioUrl = "http://anymaza.com" + link;
                        else
                            song.AudioUrl = link;
                    }
                    song.ProviderNumber = 9;
                    songs.Add(song);
                }

                return songs.Any() ? await IdentfyMatchTrack.IdentifyMatches(songs, title, artist, false) : null;
            }
        }

    }
}
