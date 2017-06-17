using System.Collections.Generic;
using Newtonsoft.Json;

namespace Musicus.Data.Spotify.Models
{
    public class SeveralAlbums
    {
        [JsonProperty("albums")]
        public List<FullAlbum> Albums { get; set; }
    }
}