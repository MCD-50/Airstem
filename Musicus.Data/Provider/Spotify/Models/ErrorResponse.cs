using Newtonsoft.Json;

namespace Musicus.Data.Spotify.Models
{
    public class ErrorResponse
    {
        [JsonProperty("error")]
        public Error Error { get; set; }
    }
}