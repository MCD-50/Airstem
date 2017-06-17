using Google.Apis.YouTube.v3.Data;
using Musicus.Core.Utils;
using Musicus.Data.Model.Advertisement;
using Musicus.Data.Model.Meile;
using Musicus.Data.Model.Mp3Clan;
using Musicus.Data.Model.Netease;
using Musicus.Data.Model.SoundCloud;
using System;
using System.Linq;
using Windows.UI.Xaml;

namespace Musicus.Data.Model.WebSongs
{
    public class WebSong : Base
    {
        public WebSong()
        {
        }

        public WebSong(SearchResult youtubeSongs)
        {
            Id = youtubeSongs.Id.VideoId;
            Name = youtubeSongs.Snippet.Title.GetSongName().Trim();
            Artist = youtubeSongs.Snippet.Title.GetArtistName().Trim();
            FileAuthor = youtubeSongs.Snippet.ChannelTitle;
            Provider = Mp3Provider.YouTube;
            ProviderNumber = 6;
            //ArtworkImage = new Uri(youtubeSongs.Snippet.Thumbnails.High.Url);
        }

        public WebSong(Mp3ClanSong mp3ClanSong)
        {
            Id = mp3ClanSong.tid;
            Name = mp3ClanSong.title;
            Artist = mp3ClanSong.artist;
            AudioUrl = mp3ClanSong.url;
            Provider = Mp3Provider.Mp3Clan;
            ProviderNumber = 11;
            if (!string.IsNullOrEmpty(mp3ClanSong.duration))
            {
                // format is x:xx, to parse correctly making it 00:x:xx
                var prefix = "0:";
                if (mp3ClanSong.duration.Length <= 3) prefix += "0";
                Duration = TimeSpan.Parse(prefix + mp3ClanSong.duration);
            }
            
        }

        public WebSong(SoundCloudSong soundCloudSong)
        {
            Id = soundCloudSong.Id.ToString();
            _Id = soundCloudSong.Id;
            Name = soundCloudSong.Title;
            AudioUrl = soundCloudSong.StreamUrl;
            Provider = Mp3Provider.SoundCloud;
            Duration = TimeSpan.FromMilliseconds(soundCloudSong.Duration);
            ByteSize = soundCloudSong.OriginalContentSize;
            Artist = FileAuthor = soundCloudSong.User.Username;

            ProviderNumber = 8;
            if (!string.IsNullOrEmpty(soundCloudSong.ArtworkUrl))
                ArtworkImage = new Uri(soundCloudSong.ArtworkUrl);
        }

        public WebSong(NeteaseDetailSong neteaseSong)
        {
            Id = neteaseSong.Id.ToString();
            Name = neteaseSong.Name;
            Artist = neteaseSong.Artists.FirstOrDefault().Name;
            AudioUrl = neteaseSong.Mp3Url;
            Provider = Mp3Provider.Netease;
            Duration = TimeSpan.FromMilliseconds(neteaseSong.Duration);
            ProviderNumber = 12;
            //BitRate = neteaseSong.BMusic.Bitrate;
            //ByteSize = neteaseSong.BMusic.Size;
        }

        public WebSong(MeileSong meileSong)
        {
            Id = meileSong.Id.ToString();
            Name = meileSong.Name;
            Artist = meileSong.ArtistName;
            AudioUrl = meileSong.Mp3;
            //Provider = Mp3Provider.Meile;
            Duration = TimeSpan.FromSeconds(meileSong.Duration);
            if (!string.IsNullOrEmpty(meileSong.NormalCover))
                ArtworkImage = new Uri(meileSong.NormalCover);
        }


        public string Album { get; set; }
        public string Artist { get; set; }
        public Uri ArtworkImage { get; set; }

        public string AudioUrl { get; set; }
        public string DirectAudioUrl { get; set; }
        public int BitRate { get; set; }
        public long ByteSize { get; set; }
        public TimeSpan Duration { get; set; }
        public string Id { get; set; }
        public int _Id { get; set; }
        public string ReleaseDate { get; set; }
        public int ProviderNumber { get; set; }
        public bool IsDeezerTrack { get; set; }
        public string FileAuthor { get; set; }
        public bool IsBestMatch { get; set; }
        public bool IsLinkDeath { get; set; }
        public bool IsMatch { get; set; }
        public string Name { get; set; }
        public Mp3Provider Provider { get; set; }
        public string Color { get; set; }
        public bool OnlyDownload { get; set; }
        public Uri GivenDownloadUrl { get; set; }

        public string FormattedBytes
        {
            get { return BytesToString(ByteSize); }
        }

        private string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }
    }
}