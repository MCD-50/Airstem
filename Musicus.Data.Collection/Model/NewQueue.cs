using SQLite.Net.Attributes;

namespace Musicus.Data.Collection.Model
{
    public class NewQueue : BaseEntry
    {
        [Indexed]
        public int SongId { get; set; }

        [Ignore]
        public string AudioUrl { get; set; }

        [Ignore]
        public string Name { get; set; }

        [Ignore]
        public string ArtistName { get; set; }

        [Ignore]
        public int ArtistId { get; set; }

        [Ignore]
        public int AlbumId { get; set; }

        [Ignore]
        public bool IsStreaming { get; set; }
    }
}
