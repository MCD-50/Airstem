using Musicus.Data.Extensions;
using Musicus.Data.Provider.Deezer.Model;
using Musicus.Data.RestObjectRequests;

namespace Musicus.Data.Provider.Deezer
{
    public class DeezerArtistTopTracksRequest : RestObjectRequest<DeezerPageResponse<DeezerSong>>
    {
        public DeezerArtistTopTracksRequest(int id)
        {
            this.Url("http://api.deezer.com/artist/{id}/top").UrlParam("id", id);
        }

        public DeezerArtistTopTracksRequest Limit(int limit)
        {
            return this.Param("limit", limit);
        }

        public DeezerArtistTopTracksRequest Offset(int offset)
        {
            return this.Param("index", offset);
        }
    }
}