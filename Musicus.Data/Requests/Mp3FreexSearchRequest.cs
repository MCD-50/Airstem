using Musicus.Data.Extensions;
using HtmlAgilityPack;
using Musicus.Data.RestObjectRequests;

namespace Musicus.Data.Requests
{
    public class Mp3FreexSearchRequest : RestObjectRequest<HtmlDocument>
    {
        public Mp3FreexSearchRequest(string query)
        {
            this.Url("http://mp3freex.com/{query}-download").UrlParam("query", query.Replace(" ", "-")).Get();
        }
    }
}