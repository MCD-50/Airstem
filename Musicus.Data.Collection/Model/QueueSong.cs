
using SQLite.Net.Attributes;

namespace Musicus.Data.Collection.Model
{
    public class QueueSong : BaseEntry
    {
        [Ignore]
        public Song Song { get; set; }

        [Indexed]
        public int SongId { get; set; }

        [Indexed]
        public int PrevId { get; set; }

        [Indexed]
        public int NextId { get; set; }

        [Indexed]
        public int ShuffleNextId { get; set; }

        [Indexed]
        public int ShufflePrevId { get; set; }


        //        [Ignore]
        //        public string SongName { get; set; }

        //        [Ignore]
        //        public string ArtistName { get; set; }

        //        [Ignore]
        //        public string AudioUrl { get; set; }

        //        [Ignore]
        //        public Uri AlbumUri { get; set; }

        //        [Ignore]
        //        public bool IsStreaming { get; set; }

        //        [Indexed]
        //        public int SongId { get; set; }
    }
}

