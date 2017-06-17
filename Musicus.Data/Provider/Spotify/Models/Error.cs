using System;
using Newtonsoft.Json;

namespace Musicus.Data.Spotify.Models
{
    public class Error
    {
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("message")]
        public String Message { get; set; }
    }
}