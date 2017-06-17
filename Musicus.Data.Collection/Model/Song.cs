
using Musicus.Core.Common;
using Newtonsoft.Json;
using SQLite.Net.Attributes;
using System;
using System.Collections.ObjectModel;


namespace Musicus.Data.Collection.Model
{
    public class Song : BaseEntry
    {
        private BackgroundDownload _download;
        private SongState _songState;
      

        public Song()
        {
            SongState = SongState.Matching;
            AddableTo = new ObservableCollection<AddableCollectionItem>();            
        }


        public string ProviderId { get; set; }
        [Indexed]
        public int ArtistId { get; set; }

        [Indexed]
        public int AlbumId { get; set; }

        public string Name { get; set; }
       
        // Artist prop is for the album (main), this one is specific to each song
        public string ArtistName { get; set; }
        public int TrackNumber { get; set; }
        public string AudioUrl { get; set; }
    
        [JsonIgnore]
        public SongState SongState
        {
            get { return _songState; }

            set
            {
                _songState = value;
                OnPropertyChanged();
                OnPropertyChanged("IsMatched");
            }
        }



        public int PlayCount { get; set; }
        public DateTime LastPlayed { get; set; }
        //public HeartState HeartState { get; set; }
        public TimeSpan Duration { get; set; }
       

      

        [JsonIgnore]
        public string CloudId { get; set; }

        [JsonIgnore]
        public DateTime LastUpdated { get; set; }

        [Ignore]
        [JsonIgnore]
        public bool IsStreaming
        {
            get { return SongState != SongState.Downloaded && SongState != SongState.Local; }
        }

        [Ignore]
        [JsonIgnore]
        public bool IsMatched
        {
            get { return SongState != SongState.Matching && SongState != SongState.NoMatch; }
        }


        [Ignore]
        [JsonIgnore]
        public bool IsDownload
        {
            get { return SongState == SongState.DownloadListed                
                || SongState == SongState.Downloading ||SongState == SongState.Matching 
                || SongState == SongState.NoMatch; }
        }

        [Ignore]
        [JsonIgnore]
        public bool IsOffline
        {
            get
            {
                return SongState == SongState.Local
                    || SongState == SongState.Downloaded
                    || SongState == SongState.None;
            }
        }

        [Ignore]
        [JsonIgnore]
        public Artist Artist { get; set; }

        [Ignore]
        [JsonIgnore]
        public Album Album { get; set; }



        [Ignore]
        [JsonIgnore]
        public BackgroundDownload Download
        {
            get { return _download; }

            set
            {
                _download = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public string DownloadId { get; set; }


        [Ignore]
        [JsonIgnore]
        public ObservableCollection<AddableCollectionItem> AddableTo { get; set; }

        public bool IsTemp { get; set; }
        public int RadioId { get; set; }
       
    }




    public class AddableCollectionItem
    {
        public string Name { get; set; }
        public Playlist Playlist { get; set; }
    }


    //public enum HeartState
    //{
       
    //    Like,

    //    None,

    //    Dislike
    //}

    public enum SongState
    {
        None,

        Downloading,

        Downloaded,

        Local,

        BackgroundMatching,

        Matching,

        NoMatch,

        DownloadListed,

        OnlineQueued
        // still playing with different states
    }
}