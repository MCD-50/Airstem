using Musicus.Core.Utils;
using Musicus.Data.Model.Netease;
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
    public static class NeteaseUrl
    {

        public static async Task<List<WebSong>> SearchNetease(string title, string artist, int limit ,bool checkAllLinks)
        {
            return await SearchNeteaseAsync(title, artist, limit, checkAllLinks);
        }

        private static async Task<List<WebSong>> SearchNeteaseAsync(string title, string artist, int limit,bool checkAllLinks)
        {
        
            using (
                var response =
                    await new NeteaseSearchRequest(title.Append(artist)).ToResponseAsync().ConfigureAwait(false))
            {
                var neteaseSongs = response.Data?.Result?.Songs?.Take(limit);
                if (neteaseSongs == null) return null;

                var songs = new List<WebSong>();

                foreach (var neteaseSong in neteaseSongs)
                {
                    using (
                        var detailsResponse =
                            await new NeteaseDetailsRequest(neteaseSong.Id).ToResponseAsync().ConfigureAwait(false))
                    {
                        var song = detailsResponse.Data?.Songs?.FirstOrDefault();
                        if (song != null)
                        {
                            song.ProviderNumber = 11;
                            songs.Add(new WebSong(song));
                        }
                    }
                }

                return songs.Any() ? await IdentfyMatchTrack.IdentifyMatches(songs, title, artist, false) : null;
            }
        }

    }
}
