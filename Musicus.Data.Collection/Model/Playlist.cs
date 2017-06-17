#region
using System.Collections.ObjectModel;
using System;
using SQLite.Net.Attributes;
using System.Collections.Concurrent;

#endregion

namespace Musicus.Data.Collection.Model
{
    public class Playlist : BaseEntry
    {
        const string MissingArtworkAppPath = "ms-appx:///" + "Assets/MissingArtwork.png";

        internal readonly ConcurrentDictionary<long, PlaylistSong> LookupMap = new 
            ConcurrentDictionary<long, PlaylistSong>();


        public Playlist()
        {
            Songs = new ObservableCollection<PlaylistSong>();
        }


        public string Name { get; set; }
        public string CreatedOn { get; set; }
        

        [Ignore]
        public ObservableCollection<PlaylistSong> Songs { get; set; }

        //public Uri Artwork
        //{
        //    get  { return GetArtwork(); }
        //}

        //Uri GetArtwork()
        //{
        //    if (Songs.Count() > 0)
        //        return Songs.FirstOrDefault().Song.Album.Artwork.Uri;
        //    else
        //        return new Uri(MissingArtworkAppPath);
        //}

    }
}