using Musicus.Data.Extensions;
using Musicus.Data.Provider.Deezer.Model;
using Musicus.Data.RestObjectRequests;

namespace Musicus.Data.Provider.Deezer
{
    public class DeezerTrackRequest : RestObjectRequest<DeezerSong>
    {
        public DeezerTrackRequest(string id)
        {
            this.Url("http://api.deezer.com/track/{id}").UrlParam("id", id);
        }
    }
}