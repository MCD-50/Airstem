using Musicus.Data.RestObjectRequests;
using Musicus.Data.Extensions;
using Musicus.Data.Model.AnyMaza;

namespace Musicus.Data.Requests
{
    internal class AnyMazaSearchRequest : RestObjectRequest<GoogleSearchRoot>
    {
        public AnyMazaSearchRequest(string query)
        {
            this.Url("https://ajax.googleapis.com/ajax/services/search/web")
                .QParam("q", query + " site:AnyMaza.com").QParam("v", "1.0");
        }
    }

}
