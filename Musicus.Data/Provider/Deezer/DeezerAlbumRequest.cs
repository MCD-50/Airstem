using Musicus.Data.Extensions;
using Musicus.Data.Provider.Deezer.Model;
using Musicus.Data.RestObjectRequests;

namespace Musicus.Data.Provider.Deezer
{
    public class DeezerAlbumRequest: RestObjectRequest<DeezerAlbum>
    {
        public DeezerAlbumRequest(string id)
        {
            this.Url("http://api.deezer.com/album/{id}").UrlParam("id", id);
        }
    }
}