

using SQLite.Net.Attributes;

namespace Musicus.Data.Collection.Model
{
    public class PlaylistSong : BaseEntry 
    {
        public long PlaylistId { get; set; }

        [Indexed]
        public int SongId { get; set; }

        [Ignore]
        public Song Song { get; set; }

    }
}