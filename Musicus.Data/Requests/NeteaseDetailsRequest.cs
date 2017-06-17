using Musicus.Data.Model.Netease;
using Musicus.Data.RestObjectRequests;
using Musicus.Data.Extensions;

namespace Musicus.Data.Requests
{
    public class NeteaseDetailsRequest : RestObjectRequest<NeteaseDetailRoot>
    {
        public NeteaseDetailsRequest(int songId)
        {
            this.Url("http://music.163.com/api/song/detail/").QParam("ids", $"[{songId}]").Get();
        }
    }

}
