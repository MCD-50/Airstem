using HtmlAgilityPack;
using Musicus.Data.RestObjectRequests;
using Musicus.Data.Extensions;

namespace Musicus.Data.Requests
{
    public class Mp3GluSearchRequest : RestObjectRequest<HtmlDocument>
    {
        public Mp3GluSearchRequest(string query)
        {
           this.Url("http://mp3glu.com/search.php")
                .QParam("q", query)
                .Post();
        }
    }
}