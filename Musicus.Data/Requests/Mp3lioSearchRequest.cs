using Musicus.Data.Extensions;
using HtmlAgilityPack;
using Musicus.Data.RestObjectRequests;

namespace Musicus.Data.Requests
{
    public class Mp3lioSearchRequest : RestObjectRequest<HtmlDocument>
    {
        public Mp3lioSearchRequest(string query)
        {
            this.Url("http://mp3lio.com/{query}").UrlParam("query", query.Replace(" ", "-")).Get();
        }
    }
}