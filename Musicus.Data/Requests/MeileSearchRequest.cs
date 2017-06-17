using HtmlAgilityPack;
using Musicus.Data.RestObjectRequests;
using Musicus.Data.Extensions;
namespace Musicus.Data.Requests
{
    public class MeileSearchRequest : RestObjectRequest<HtmlDocument>
    {
        public MeileSearchRequest(string query)
        {
            this.Url("http://www.meile.com/search").QParam("q", query).Get();
        }

        public MeileSearchRequest Limit(int limit)
        {
            return this.Param("count", limit);
        }
    }
}