using Musicus.Data.Extensions;
using Musicus.Data.Provider.Deezer.Model;
using Musicus.Data.RestObjectRequests;
using Musicus.Data.Model.WebSongs;

namespace Musicus.Data.Provider.Deezer
{
    public class DeezerChartRequest<T> : RestObjectRequest<DeezerPageResponse<T>>
    {
        public DeezerChartRequest(WebResults.Type type)
        {
            this.Url("http://api.deezer.com/chart/0/{type}").Type(type);
        }

        public DeezerChartRequest<T> Type(WebResults.Type type)
        {
            return this.UrlParam("type", type.ToString().ToLower().Replace("song", "track") + "s");
        }

        public DeezerChartRequest<T> Limit(int limit)
        {
            return this.Param("limit", limit);
        }

        public DeezerChartRequest<T> Offset(int offset)
        {
            return this.Param("index", offset);
        }
    }
}