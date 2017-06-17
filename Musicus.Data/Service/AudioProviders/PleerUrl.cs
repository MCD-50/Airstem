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
using System.Threading.Tasks;


namespace Musicus.Data.Service.AudioProviders
{
    public static class PleerUrl
    {
        public static async Task<List<WebSong>> SearchPleer(string title, string artist, int limit)
        {

            return await SearchPleerAsync(title, artist, limit);
        }

        //private static async Task<List<WebSong>> SearchPleerAsync(string title, string artist, int limit)
        //{
        //    using (var response = await new PleerSearchRequest(title.Append(artist)).ToResponseAsync().DontMarshall())
        //    {
        //        var o = response.Data;
        //        if (!response.HasData || !o.Value<bool>("success")) return null;

        //        var html = o.Value<string>("html");

        //        var doc = new HtmlDocument();
        //        doc.LoadHtml(html);

        //        var songNodes =
        //            doc.DocumentNode.Descendants("li").Where(p => p.Attributes.Contains("file_id")).Take(limit);

        //        var songs = new List<WebSong>();

        //        foreach (var songNode in songNodes)
        //        {
        //            var song = new WebSong
        //            {
        //                Id = songNode.Attributes["file_id"].Value,
        //                Name = songNode.Attributes["song"].Value,
        //                Artist = songNode.Attributes["singer"].Value
        //            };


        //            try
        //            {
        //                int seconds;
        //                if (int.TryParse(songNode.Attributes["duration"].Value, out seconds))
        //                {
        //                    song.Duration = TimeSpan.FromSeconds(seconds);
        //                }
        //            }
        //            catch (Exception)
        //            {
        //                song.Duration = new TimeSpan(0, 4, 0);
        //            }

        //            var linkId = songNode.Attributes["link"].Value;
        //            song.AudioUrl = await GetPleerLinkAsync(linkId);

        //            if (string.IsNullOrEmpty(song.AudioUrl)) continue;
        //            song.ProviderNumber = 1;
        //            songs.Add(song);
        //        }

        //        return songs.Any() ? await IdentfyMatchTrack.IdentifyMatches(songs, title, artist, false) : null;
        //    }
        //}

        //private static async Task<string> GetPleerLinkAsync(string id)
        //{
        //    using (var response = await new PleerDetailsRequest(id).ToResponseAsync().DontMarshall())
        //    {
        //        return response.Data?.Value<string>("track_link");
        //    }
        //}





        private static async Task<List<WebSong>> SearchPleerAsync(string title, string artist, int limit)
        {
            using (var response = await new PleerSearchRequest(title.Append(artist)).ToResponseAsync().DontMarshall())
            {
                var o = response.Data;
                if (!response.HasData || !o.Value<bool>("success")) return null;

                var html = o.Value<string>("html");

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var songNodes =
                    doc.DocumentNode.Descendants("li").Where(p => p.Attributes.Contains("file_id")).Take(limit);

                var songs = new List<WebSong>();

                foreach (var songNode in songNodes)
                {
                    var song = new WebSong
                    {
                        Id = songNode.Attributes["file_id"].Value,
                        Name = WebUtility.HtmlDecode(songNode.Attributes["song"].Value),
                        Artist = WebUtility.HtmlDecode(songNode.Attributes["singer"].Value)
                    };


                    int bitRate;
                    if (int.TryParse(songNode.Attributes["rate"].Value.Replace(" Kb/s", ""), out bitRate))
                    {
                        song.BitRate = bitRate;
                    }
                    try
                    {
                        int seconds;
                        if (int.TryParse(songNode.Attributes["duration"].Value, out seconds))
                        {
                            song.Duration = TimeSpan.FromSeconds(seconds);
                        }
                    }
                    catch (Exception)
                    {
                        song.Duration = new TimeSpan(0, 4, 0);
                    }

                    var linkId = songNode.Attributes["link"].Value;
                    song.AudioUrl = await GetPleerLinkAsync(linkId);

                    if (string.IsNullOrEmpty(song.AudioUrl)) continue;
                    song.ProviderNumber = 1;
                    songs.Add(song);

                }

                return songs.Any() ? await IdentfyMatchTrack.IdentifyMatches(songs, title, artist, false) : null;
            }
        }

        private static async Task<string> GetPleerLinkAsync(string id)
        {
            using (var response = await new PleerDetailsRequest(id).ToResponseAsync().DontMarshall())
            {
                return response.Data?.Value<string>("track_link");
            }
        }


    }
}
