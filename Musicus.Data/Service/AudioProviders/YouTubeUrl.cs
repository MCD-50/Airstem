using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using HtmlAgilityPack;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Service.AudioProviders.MatchStringExtentions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;


namespace Musicus.Data.Service.AudioProviders
{
    public static class YouTubeUrl
    {
        public static async Task<List<WebSong>> SearchYoutube(string title, string artist, int limit, bool includeAudioTag)
        {
            return await SearchYoutubeAsync(title, artist, limit, includeAudioTag);
        }


        private static async Task<List<WebSong>> SearchYoutubeAsync(string title, string artist, int limit, bool includeAudioTag)
        {
            YouTubeService youtubeService;

            youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = ApiKeys.YouTubeIdMusicus,
                ApplicationName = "Musicus1" //"Musicus1"
            });


            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = IdentfyMatchTrack.CreateQuery(title, artist, false) + (includeAudioTag ? " (Audio)" : "");
            searchListRequest.MaxResults = limit;
            searchListRequest.SafeSearch = SearchResource.ListRequest.SafeSearchEnum.None;

            try
            {
                SearchListResponse searchListResponse;
                try
                {
                    // Call the search.list method to retrieve results matching the specified query term.
                    searchListResponse = await searchListRequest.ExecuteAsync();
                }

                catch
                {
                    youtubeService = new YouTubeService(new BaseClientService.Initializer()
                    {
                        ApiKey = ApiKeys.YoutubeId,
                        ApplicationName = "Airstem" //"Musicus1"
                    });

                    var o = youtubeService.Search.List("snippet");
                    o.Q = IdentfyMatchTrack.CreateQuery(title, artist, false) + (includeAudioTag ? " (Audio)" : "");
                    o.MaxResults = limit;
                    searchListResponse = await o.ExecuteAsync();
                    if (o != null) o = null;
                }

                if (searchListRequest != null) searchListRequest = null;

                List<WebSong> songs = new List<WebSong>();

                foreach (var vid in from searchResult in searchListResponse.Items
                                    where searchResult.Id.Kind == "youtube#video"
                                    select new WebSong(searchResult))
                {
                    try
                    {
                        vid.AudioUrl = GetYoutubeUrlAsync(vid.Id);
                    }
                    catch
                    {
                        // ignored
                    }

                    if (string.IsNullOrEmpty(vid.AudioUrl)) continue;

                    vid.ProviderNumber = 6;
                    songs.Add(vid);
                }

                var results = await IdentfyMatchTrack.IdentifyMatches(songs, title, artist, false);

                // try redoing the search without "(audio)"
                if (!results.Any(p => p.IsMatch) && includeAudioTag)
                    return await SearchYoutube(title, artist, limit, false);

                return results;
            }
            catch
            {
                return null;
            }
        }


        //private static string GetUrl(string id)
        //{
        //    /*
        //     * Get the available video formats.
        //     * We'll work with them in the video and audio download examples.
        //     */
        //    IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(string.Format("youtube.com/watch?v={0}", id));
        //    /*
        //     * * We want the first extractable video with the highest audio quality.
        //     */
        //    VideoInfo video = videoInfos.Where(info => info.CanExtractAudio).OrderByDescending(info => info.AudioBitrate).First();

        //    /*
        //     * * If the video has a decrypted signature, decipher it
        //     */
        //    if (video.RequiresDecryption)
        //        DownloadUrlResolver.DecryptDownloadUrl(video);

        //    return video.DownloadUrl;
        //}

        public static string GetYoutubeUrl(string id)
        {
            return GetYoutubeUrlAsync(id);
        }

        private static string GetYoutubeUrlAsync(string id)
        {

            var link = string.Format("https://www.youtube.com/watch?v={0}", id);
            return string.Format("http://youtubeinmp3.com/fetch/?video={0}", link);

            //    using (var client = new HttpClient())
            //    {
            //        var data = new Dictionary<string, string>
            //        {
            //            {"mediaurl", string.Format("youtube.com/watch?v={0}", id) }
            //        };




            //        //5GQnC6UUsZw is youtube video id
            //        //http://www.mp3ify.com/results-transcoder.php?search=5GQnC6UUsZw
            //        //http://peggo.co/dvr/5GQnC6UUsZw
            //        //http://convert2mp3.net/en/index.php?url=http://www.youtube.com/watch?v=5GQnC6UUsZw#errormsg
            //        //http://www.music-clips.net/v/5GQnC6UUsZw/mp3/
            //        //http://www.vidtomp3.com/?url=http%3A%2F%2Fwww%2Eyoutube%2Ecom%2Fwatch%3Fv%3D5GQnC6UUsZw#form
            //        //http://2conv.com/?url=http%3A%2F%2Fwww%2Eyoutube%2Ecom%2Fwatch%3Fv%3D5GQnC6UUsZw
            //        //http://www.flvto.com/?url=http%3A%2F%2Fwww%2Eyoutube%2Ecom%2Fwatch%3Fv%3D5GQnC6UUsZw#form
            //        //http://file2hd.com/Default.aspx?url=http%3A%2F%2Fwww%2Eyoutube%2Ecom%2Fwatch%3Fv%3D5GQnC6UUsZw
            //        //http://www.fullrip.net/mp3/5GQnC6UUsZw
            //        //http://www.video2mp3.net/?v=5GQnC6UUsZw&quality=1#convert


            //        //http://youtubeinmp3.com/fetch/?video={0}", youtubeUrl
            //        //http://www.youtubeinmp3.com/download/?video=http://www.youtube.com/watch?v=5GQnC6UUsZw

            //        //http://www.youtubeinmp3.com/download/?video=http://www.youtube.com/watch?v=5GQnC6UUsZw



            //        //using (var content = new FormUrlEncodedContent(data))
            //        //{
            //        //    using (var resp = await client.PostAsync("http://www.vidtomp3.com/cc/conversioncloud.php", content))
            //        //    {
            //        //        if (!resp.IsSuccessStatusCode) return null;

            //        //        var json = await resp.Content.ReadAsStringAsync();
            //        //        json = json.Substring(1, json.Length - 2);
            //        //        var o = JToken.Parse(json);
            //        //        var statusUrl = o.Value<string>("statusurl");

            //        //        if (string.IsNullOrEmpty(statusUrl)) return null;

            //        //        using (var resp2 = await client.GetAsync(statusUrl + "&json"))
            //        //        {
            //        //            if (!resp2.IsSuccessStatusCode) return null;

            //        //            json = await resp2.Content.ReadAsStringAsync();
            //        //            json = json.Substring(1, json.Length - 2);
            //        //            o = JToken.Parse(json);
            //        //            return o.Value<string>("downloadurl");
            //        //        }
            //        //    }
            //        //}
            //    }
        }
    }
}
