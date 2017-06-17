using HtmlAgilityPack;
using Musicus.Data.RestObjectRequests;
using Musicus.Data.Extensions;

namespace Musicus.Data.Requests
{
    public class MiMp3SSearchRequest : RestObjectRequest<HtmlDocument>
    {
        public MiMp3SSearchRequest(string query)
        {
            this.Url("http://www.mimp3s.com/{query}-mp3.html").UrlParam("query", query.Replace(" ", "-"));
        }
    }
}