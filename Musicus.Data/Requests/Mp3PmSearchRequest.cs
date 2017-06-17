using Musicus.Data.Extensions;
using HtmlAgilityPack;
using Musicus.Data.RestObjectRequests;

namespace Musicus.Data.Requests
{
    public class Mp3PmSearchRequest : RestObjectRequest<HtmlDocument>
    {
        public Mp3PmSearchRequest(string query)
        {
            this.Url("http://mp3pm.info/s/f/{query}/").UrlParam("query", query).Get();
        }
    }
}