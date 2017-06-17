
namespace Musicus.Data.Collection.Model
{
    public class Lyrics : BaseEntry
    {
        public Lyrics()
        {

        }

        public Lyrics(string songName, string artistName, string content)
        {
            SongName = songName;
            ArtistName = artistName;
            Content = content;
        }

        public string SongName { get; set; }
        public string ArtistName{ get; set; }
        public string Content { get; set; }
    }
}
