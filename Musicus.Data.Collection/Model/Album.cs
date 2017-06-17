#region

using Musicus.Core.Common;
using Newtonsoft.Json;
using SQLite.Net.Attributes;
using System;
using System.Collections.ObjectModel;

#endregion

namespace Musicus.Data.Collection.Model
{
    public class Album : BaseEntry
    {
        private IBitmapImage _artwork;
        private IBitmapImage _smallArtwork;
      

        public Album()
        {
            Songs = new OptimizedObservableCollection<Song>();
            AddableTo = new ObservableCollection<AddableCollectionItem>();
        }


        public string ProviderId { get; set; }

        [Indexed]
        public int PrimaryArtistId { get; set; }

        public string Name { get; set; }
        public string Genre { get; set; }
        public DateTime ReleaseDate { get; set; }

        [JsonIgnore]
        public DateTime LastUpdated { get; set; }

        [JsonIgnore]
        public string CloudId { get; set; }

        [Ignore]
        [JsonIgnore]
        public OptimizedObservableCollection<Song> Songs { get; set; }

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

        [JsonIgnore]
        public bool NoArtworkFound { get; set; }

        [Ignore]
        [JsonIgnore]
        public IBitmapImage SmallArtwork
        {
            get { return _smallArtwork; }
            set
            {
                _smallArtwork = value;
                OnPropertyChanged();
            }
        }

        [Ignore]
        [JsonIgnore]
        public Artist PrimaryArtist { get; set; }

        [JsonIgnore]
        public bool HasArtwork { get; set; }

        [Ignore]
        [JsonIgnore]
        public ObservableCollection<AddableCollectionItem> AddableTo { get; set; }
    }
}