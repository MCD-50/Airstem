using Musicus.Data.Model.Netease;
using Musicus.Data.RestObjectRequests;
using Musicus.Data.Extensions;

namespace Musicus.Data.Requests
{
    public class NeteaseSearchRequest : RestObjectRequest<NeteaseRoot>
    {
        public NeteaseSearchRequest(string query)
        {
            this.Url("http://music.163.com/api/search/suggest/web")
                .Referer("http://music.163.com").Param("s", query).Post();
        }
    }
}
