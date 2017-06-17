using HtmlAgilityPack;
using Musicus.Data.RestObjectRequests;
using Musicus.Data.Extensions;

namespace Musicus.Data.Requests
{
    public class Mp3MinerSearchRequest : RestObjectRequest<HtmlDocument>
    {
        public Mp3MinerSearchRequest(string page, string query)
        {
            this.Url("http://mp3miner.com/{page}/{query}").
                UrlParam("page", page).UrlParam("query", query).Get();
        }
    }

}
