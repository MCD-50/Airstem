using Musicus.Core.Common;
using Newtonsoft.Json;
using SQLite.Net.Attributes;
using System;

namespace Musicus.Data.Collection.Model
{

    public class Video : BaseEntry
    {
        private IBitmapImage _artwork;
        private VideoState _videoState;
        //private BackgroundDownload _download;

        public Video()
        {
            //VideoState = SongState.Matching;
        }

        public string Title { get; set; }
        public string Author { get; set; }
        public TimeSpan Duration { get; set; }
        public string AddedOn { get; set; }
        public int ViewCount { get; set; }
        public string VideoUrl { get; set; }

        [JsonIgnore]
        public string DownloadId { get; set; }

        [JsonIgnore]
        public VideoState VideoState
        {
            get { return _videoState; }
            set
            {
                _videoState = value;
                OnPropertyChanged();
            }
        }

        [Ignore]
        [JsonIgnore]
        public bool IsOffline
        {
            get { return VideoState == VideoState.Local; }
        }

        [Ignore]
        [JsonIgnore]
        public bool IsDownloading
        {
            get { return VideoState == VideoState.Downloading; }
        }

        [Ignore]
        [JsonIgnore]
        public bool IsOnline
        {
            get { return VideoState == VideoState.Web; }
        }

        [JsonIgnore]
        public bool NoArtworkFound { get; set; }

        [JsonIgnore]
        public string YoutubeId { get; set; }

        [JsonIgnore]
        public bool HasArtwork { get; set; }

        [Ignore]
        [JsonIgnore]
        public IBitmapImage Artwork
        {
            get { return _artwork; }
            set
            {
                _artwork = value;
                OnPropertyChanged();
            }
        }

        //[Ignore]
        //[JsonIgnore]
        //public BackgroundDownload Download
        //{
        //    get { return _download; }

        //    set
        //    {
        //        _download = value;
        //        OnPropertyChanged();
        //    }
        //}


     

    }

    public enum VideoState
    {
        Local,
        Web,
        Downloading
    }

}
