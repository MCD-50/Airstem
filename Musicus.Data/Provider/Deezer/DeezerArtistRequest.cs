
using Musicus.Data.Provider.Deezer.Model;
using Musicus.Data.Extensions;
using Musicus.Data.RestObjectRequests;

namespace Musicus.Data.Provider.Deezer
{
    public class DeezerArtistRequest : RestObjectRequest<DeezerArtist>
    {
        public DeezerArtistRequest(string id)
        {
            this.Url("http://api.deezer.com/artist/{id}").UrlParam("id", id);
        }
    }
}