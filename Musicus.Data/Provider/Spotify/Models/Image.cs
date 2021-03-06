﻿using System;
using Newtonsoft.Json;

namespace Musicus.Data.Spotify.Models
{
    public class Image
    {
        [JsonProperty("url")]
        public String Url { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }
    }
}
