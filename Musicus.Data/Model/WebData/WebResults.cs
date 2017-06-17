using Musicus.Data.Model.WebData;
using System.Collections.Generic;

namespace Musicus.Data.Model.WebSongs
{
    public class WebResults
    {
        public enum Type
        {
            Song,
            Album,
            Artist         
        }

        public string PageToken { get; set; }
        public bool HasMore { get; set; }
        public List<WebSong> Songs { get; set; }
        public List<WebAlbum> Albums { get; set; }
        public List<WebArtist> Artists { get; set; }
    }
}