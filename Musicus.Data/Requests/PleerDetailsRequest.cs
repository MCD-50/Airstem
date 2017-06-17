using Musicus.Data.RestObjectRequests;
using Newtonsoft.Json.Linq;
using Musicus.Data.Extensions;

namespace Musicus.Data.Requests
{
    public class PleerDetailsRequest : RestObjectRequest<JToken>
    {
        public PleerDetailsRequest(string songId)
        {
            this.Url("http://pleer.com/site_api/files/get_url").Param("action", "download").Param("id", songId).Post();
        }
    }
}