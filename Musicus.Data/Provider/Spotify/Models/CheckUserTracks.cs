using System.Collections.Generic;

namespace Musicus.Data.Spotify.Models
{
    public class CheckUserTracks : BasicModel
    {
        public List<bool> Checked { get; set; }
    }
}