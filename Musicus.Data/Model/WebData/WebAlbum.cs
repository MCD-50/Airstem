
using Musicus.Core.Common;
using Musicus.Data.Model.Advertisement;
using Musicus.Data.Model.WebSongs;
using System;
using System.Collections.Generic;

namespace Musicus.Data.Model.WebData
{
    public class WebAlbum : Base
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public string ArtistName { get; set; }
        public List<WebSong> Tracks { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public Uri Artwork { get; set; }
        public List<string> Genres { get; set; }
        public bool HasArtwork { get; set; }
        public bool IsEmpty { get; set; }
    }
}
