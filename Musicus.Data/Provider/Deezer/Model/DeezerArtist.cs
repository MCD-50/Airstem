using Newtonsoft.Json;

namespace Musicus.Data.Provider.Deezer.Model
{
    public class DeezerArtist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Tracklist { get; set; }
        public string Type { get; set; }

        [JsonProperty("picture_big")]
        public string PictureBig { get; set; }
    }
}