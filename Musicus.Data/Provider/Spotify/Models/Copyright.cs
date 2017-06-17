using System;
using Newtonsoft.Json;

namespace Musicus.Data.Spotify.Models
{
    public class Copyright
    {
        [JsonProperty("text")]
        public String Text { get; set; }
        [JsonProperty("type")]
        public String Type { get; set; }
    }
}