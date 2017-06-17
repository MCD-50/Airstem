using System.Collections.Generic;
using Newtonsoft.Json;

namespace Musicus.Data.Spotify.Models
{
    public class SeveralTracks
    {
        [JsonProperty("tracks")]
        public List<FullTrack> Tracks { get; set; }
    }
}