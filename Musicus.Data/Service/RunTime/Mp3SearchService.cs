using HtmlAgilityPack;
using Musicus.Core.Utils.Interfaces;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Service.AudioProviders;
using Musicus.Data.Service.AudioProviders.MatchStringExtentions;
using Musicus.Data.Service.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Musicus.Data.Service.RunTime
{
    public class Mp3SearchService : IMp3SearchService
    {

        private const string Mp3FreeXMatch = "http://mp3freex.com/{query}-download";//song name - artist ; its best in spped.
        private const string Mp3Lio = "http://mp3lio.com/{query}";// song name - artist ; moderate in speed.
        private const string Mp3Pm = "http://mp3pm.com/s/f/{query}/";//only song name; slow in speed.
        private const string Mp3SkullSearchUrl = "http://mp3skull.com/search_db.php?q={0}&fckh={1}";//with space it can find if song; very slow in speed.
        private const string SoundCloudSearchUrl =
           "https://api.soundcloud.com/search/sounds?client_id={0}&limit={1}&q={2}";//with space it can find if song; slow in speed and quality is bad.
        private const string Mp3ClanSearchUrl = "http://mp3clan.com/app/mp3Search.php?q={0}&count={1}";
        private const string PleerSearchUrl =
               "http://pleer.com/search?page=1&q={0}&sort_mode=0&sort_by=0&quality=best&onlydata=true";
        private const string SongilySearchUrl = "http://songily.com/mp3/download/{1}/{0}.html";
        private const string PleerFileUrl = "http://pleer.com/site_api/files/get_url";
        private const string Mp3TruckSearchUrl = "https://mp3truck.net/ajaxRequest.php";
        private const string MeileSearchUrl = "http://www.meile.com/search?type=&q={0}";
        private const string MeileDetailUrl = "http://www.meile.com/song/mult?songId={0}";
        private const string NeteaseSuggestApi = "http://music.163.com/api/search/suggest/web?csrf_token=";
        private const string NeteaseDetailApi = "http://music.163.com/api/song/detail/?ids=%5B{0}%5D";
        private readonly IAppSettingsHelper _appSettingsHelper;
        private string _mp3SkullFckh;

        public Mp3SearchService(IAppSettingsHelper appSettingsHelper)
        {
            this._appSettingsHelper = appSettingsHelper;
            HtmlNode.ElementsFlags.Remove("form");
        }


        public string Mp3SkullFckh
        {
            get { return _mp3SkullFckh ?? (_mp3SkullFckh = _appSettingsHelper.Read<string>("Mp3SkullFckh")); }
            set
            {
                _mp3SkullFckh = value;
                _appSettingsHelper.Write("Mp3SkullFckh", value);
            }
        }


        //get Lyrics
        private const string MetroLyricsUrl =
           "http://api.metrolyrics.com/v1/get/fullbody/?title={0}&artist={1}&X-API-KEY=b84a4db3a6f9fb34523c25e43b387f1f56f987a5&format=json";
        public async Task<string> GetMetroLyrics(string title, string artist)
        {
            var url = string.Format(MetroLyricsUrl, WebUtility.UrlEncode(title), WebUtility.UrlEncode(artist));
            using (var client = new HttpClient())
            {
                using (var resp = await client.GetAsync(new Uri(url, UriKind.RelativeOrAbsolute)))
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    try
                    {
                        var o = JToken.Parse(json);
                        return o.Value<string>("song");
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }


        //public async Task<string> GetLyricAsync(string song, string artist)
        //{
        //    using (var response = await new MusixMatchLyricsRequest(song, artist).ToResponseAsync().DontMarshall())
        //    {
        //        if (response.HasData)
        //        {
        //            return response.Data["message"]["body"]["macro_calls"]["track.lyrics.get"]["message"]["body"]["lyrics"].Value<string>("lyrics_body");
        //        }
        //        return string.Empty;
        //    }
        //}


        //get Mp3FreeX.

        public async Task<List<WebSong>> SearchMp3Freex(string title, string artist, int limit = 4)
        {
            return await Mp3FreeXMatchUrl.SearchMp3Freex(title, artist, limit);
        }

        //get Mp3Miner
        public async Task<List<WebSong>> SearchMp3Miner(string title, string artist, int limit = 4)
        {
            return await Mp3MinerUrl.SearchMp3Miner(title, artist, limit);
        }


        //get Mp3lio.
        public async Task<List<WebSong>> SearchMp3lio(string title, string artist, int limit = 4)
        {
            return await Mp3LioUrl.SearchMp3lio(title, artist, limit);
        }

        //Get Mp3Pm
        public async Task<List<WebSong>> SearchMp3Pm(string title, string artist, int limit = 4)
        {

            return await Mp3PmUrl.SearchMp3Pm(title, artist, limit);
        }

        //get AnyMaza
        public async Task<List<WebSong>> SearchAnyMaza(string title, string artist, int limit = 4)
        {
            return await AnyMazaUrl.SearchAnyMaza(title, artist, limit);
        }

        //get MiMp3
        public async Task<List<WebSong>> SearchMiMp3(string title, string artist, int limit = 4)
        {
            return await MiMp3Url.SearchMiMp3(title, artist, limit);
        }

        //get Mp3Glu
        public async Task<List<WebSong>> SearchMp3Glu(string title, string artist, int limit = 4)
        {
            return await Mp3GluUrl.SearchMp3Glu(title, artist, limit);
        }

        //get Meile
        public async Task<List<WebSong>> SearchMeile(string title, string artist, int limit = 4,
          bool checkAllLinks = false)
        {
            return await MeileUrl.SearchMeile(title, artist, limit, checkAllLinks);
        }


        //get Netease
        public async Task<List<WebSong>> SearchNetease(string title, string artist, int limit = 4,
          bool checkAllLinks = false)
        {
            return await NeteaseUrl.SearchNetease(title, artist, limit, checkAllLinks);
        }



        //get Pleer
        public async Task<List<WebSong>> SearchPleer(string title, string artist, int limit = 4)
        {
            return await PleerUrl.SearchPleer(title, artist, limit);
        }

       
        //get Youtube
        public async Task<List<WebSong>> SearchYoutube(string title, string artist, int limit = 4, bool includeAudioTag = true)
        {
           return await YouTubeUrl.SearchYoutube(title, artist, limit, includeAudioTag);
        }

        //get YouTube audio links
        public string GetYoutubeUrl(string id)
        {
            return YouTubeUrl.GetYoutubeUrl(id);
        }


        //get soundcloud
        public async Task<List<WebSong>> SearchSoundCloud(string title, string artist, int limit = 4)
        {
            return await SoundCloudUrl.SearchSoundCloud(title, artist, limit);
        }

        //get mp3clan
        public async Task<List<WebSong>> SearchMp3Clan(string title, string artist, int limit = 4,
            bool checkAllLinks = false)
        {
            return await Mp3ClanUrl.SearchMp3Clan(title, artist, limit, checkAllLinks);
        }

        //get sogily
        public async Task<List<WebSong>> SearchSongily(string title, string artist, int page = 1,
            bool checkAllLinks = false)
        {
            return await SogilyUrl.SearchSongily(title, artist, page, checkAllLinks);
        }

        //get mp3truck
        public async Task<List<WebSong>> SearchMp3Truck(string title, string artist,
            bool checkAllLinks = false)
        {
            return await Mp3TruckUrl.SearchMp3Truck(title, artist, checkAllLinks);
        }


        //get mp3skull
        public async Task<List<WebSong>> SearchMp3Skull(string title, string artist,
            bool checkAllLinks = false)
        {
            using (var client = new HttpClient())
            {
                var url = string.Format(Mp3SkullSearchUrl, IdentfyMatchTrack.CreateQuery(title, artist), Mp3SkullFckh);
                using (var resp = await client.GetAsync(url).ConfigureAwait(false))
                {
                    if (!resp.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    var html = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    if (html.Contains("You have made too many request"))
                    {
                        return null;
                    }

                    if (html.Contains("Your search session has expired"))
                    {
                        var fckhNode =
                            doc.DocumentNode.Descendants("input")
                                .FirstOrDefault(
                                    p => p.Attributes.Contains("name") && p.Attributes["name"].Value == "fckh");
                        if (fckhNode == null)
                        {
                            return null;
                        }

                        Mp3SkullFckh = fckhNode.Attributes["value"].Value;

                        return await SearchMp3Skull(title, artist);
                    }

                    // Get the div node
                    var songNodes = doc.DocumentNode.Descendants("div").Where(p => p.Id == "song_html");

                    var songs = new List<WebSong>();

                    foreach (var songNode in songNodes)
                    {
                        var song = new WebSong(); //{ Provider = Mp3Provider.Mp3Skull };

                        var songUrlNode = songNode.Descendants("a").FirstOrDefault(p => p.InnerText == "Download");

                        if (songUrlNode == null)
                        {
                            continue;
                        }

                        song.AudioUrl = songUrlNode.Attributes["href"].Value;
                        var songInfo =
                            songNode.Descendants("div")
                                .FirstOrDefault(p => p.Attributes["class"].Value == "left")
                                .InnerText.Replace("<!-- info mp3 here -->", string.Empty)
                                .Trim();


                        var bitRateIndex = songInfo.IndexOf("kbps", StringComparison.Ordinal);
                        if (bitRateIndex > -1)
                        {
                            var bitrateTxt = songInfo.Substring(0, bitRateIndex);
                            int bitrate;
                            if (int.TryParse(bitrateTxt, out bitrate))
                            {
                                song.BitRate = bitrate;
                            }
                        }

                        #region Duration

                        if (bitRateIndex > -1)
                        {
                            songInfo = songInfo.Remove(0, bitRateIndex + 4);
                        }

                        var durationIndex = songInfo.IndexOf(":", StringComparison.Ordinal);
                        if (durationIndex > -1)
                        {
                            var durationText = songInfo.Substring(0, durationIndex + 3);
                            var seconds = int.Parse(durationText.Substring(durationText.Length - 2, 2));
                            var minutes = int.Parse(durationText.Remove(durationText.Length - 3));

                            song.Duration = new TimeSpan(0, 0, minutes, seconds);
                        }

                        #endregion

                        #region Size

                        if (durationIndex > -1)
                        {
                            songInfo = songInfo.Remove(0, durationIndex + 3);
                        }

                        var sizeIndex = songInfo.IndexOf("mb", StringComparison.Ordinal);
                        if (sizeIndex > -1)
                        {
                            var sizeText = songInfo.Substring(0, sizeIndex);
                            long size;
                            if (long.TryParse(sizeText, out size))
                            {
                                song.ByteSize = (long)(size * (1024 * 1024.0));
                            }
                        }

                        #endregion

                        var songTitle = songNode.Descendants("b").FirstOrDefault().InnerText;
                        songTitle = songTitle.Substring(0, songTitle.Length - 4).Trim();

                        song.Name = songTitle;
                        //song.ProviderNumber = 9;
                        songs.Add(song);
                    }

                    return songs.Any() ? await IdentfyMatchTrack.IdentifyMatches(songs, title, artist, checkAllLinks) : null;
                }
            }
        }

    }
}