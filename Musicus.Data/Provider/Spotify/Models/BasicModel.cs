using System;
using Newtonsoft.Json;
using Musicus.Data.Model.Advertisement;

namespace Musicus.Data.Spotify.Models
{
    public abstract class BasicModel : Base
    {
        [JsonProperty("error")]
        public Error ErrorResponse { get; set; }

        public Boolean HasError()
        {
            return ErrorResponse != null;
        }
    }
}
