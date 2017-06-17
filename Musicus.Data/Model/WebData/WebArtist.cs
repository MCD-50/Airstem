using Musicus.Data.Model.Advertisement;
using System;

namespace Musicus.Data.Model.WebData
{
    public class WebArtist : Base
    {
        public string Name { get; set; }
        public Uri Artwork { get; set; }
        public Uri LargestUri { get; set; }
        public string Id { get; set; }
    }
}
