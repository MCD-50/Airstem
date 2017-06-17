using Musicus.Data.Model.Advertisement;
using System;
using System.Collections.Generic;

namespace Musicus.Data.Model.WebData
{
    public class WebVideo : Base
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string VideoId { get; set; }
        public bool IsOffline { get { return false; } }
        public string Views { get; set; }
        public ulong? ViewCount { get; set; }
        public ulong? LikeCount { get; set; }
        public ulong? DislikeCount { get; set; }
        public string Length { get; set; }
        public string Date { get; set; }
        public string Info { get; set; }
        public IEnumerable<string> Comments { get; set; }
        public Uri Artwork { get; set; }
    }
}
