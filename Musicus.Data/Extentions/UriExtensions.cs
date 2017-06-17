using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Musicus.Core.Extensions;
using Musicus.Data.Model.WebSongs;
using System.Linq;

namespace Musicus.Data.Extensions
{
    public static class UriExtensions
    {
        public static async Task<HtmlDocument> ParseHtmlAsync(this HttpResponseMessage response)
        {
            var doc = new HtmlDocument();

            var html = await response.Content.ReadAsStringAsync().DontMarshall();
            doc.LoadHtml(html);

            return doc;
        }

        public static void SetNameAndArtistFromTitle(this WebSong song, string title, bool artistOnLeft,
          char seperator = '-')
        {
            var titleSplit = title.Split(seperator).Select(p => p.Trim()).ToArray();
            if (titleSplit.Length < 1) return;

            var left = titleSplit[0];
            var right = string.Join($" {seperator} ", titleSplit.Skip(1));

            if (artistOnLeft)
            {
                song.Artist = left;
                song.Name = right;
            }
            else
            {
                song.Artist = right;
                song.Name = left;
            }
        }


        public static Task<HttpResponseMessage> GetAsync(this Uri uri)
        {
            return uri.ExecuteAsync(HttpMethod.Get);
        }

        public static Task<HttpResponseMessage> HeadAsync(this Uri uri)
        {
            return uri.ExecuteAsync(HttpMethod.Head, HttpCompletionOption.ResponseHeadersRead, 10);
        }

        public static async Task<HttpResponseMessage> ExecuteAsync(this Uri uri, HttpMethod method,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, double timeout = 100.0)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(timeout);

                    return await client.SendAsync(new HttpRequestMessage(method, uri), completionOption);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}