
using Musicus.Data.Provider.Deezer.Model;
using Musicus.Data.RestObjectRequests;
using Musicus.Data.Extensions;

namespace Musicus.Data.Provider.Deezer
{
    public class DeezerArtistAlbumsRequest : RestObjectRequest<DeezerPageResponse<DeezerAlbum>>
    {
        public DeezerArtistAlbumsRequest(int id)
        {
            this.Url("http://api.deezer.com/artist/{id}/albums").UrlParam("id", id);
        }

        public DeezerArtistAlbumsRequest Limit(int limit)
        {
            return this.Param("limit", limit);
        }

        public DeezerArtistAlbumsRequest Offset(int offset)
        {
            return this.Param("index", offset);
        }
    }
}