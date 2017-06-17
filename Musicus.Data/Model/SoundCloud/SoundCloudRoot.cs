using Newtonsoft.Json;
using System.Collections.Generic;

namespace Musicus.Data.Model.SoundCloud
{
    public class SoundCloudRoot
    {
        public List<SoundCloudSong> Collection { get; set; }

        [JsonProperty("next_href")]
        public string NextHref { get; set; }
    }
}