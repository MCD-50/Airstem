using Musicus.Data.Model.Meile;
using Musicus.Data.RestObjectRequests;
using Musicus.Data.Extensions;

namespace Musicus.Data.Requests
{
    public class MeileDetailsRequest : RestObjectRequest<MeileDetailRoot>
    {
        public MeileDetailsRequest(string songId)
        {
            this.Url("http://www.meile.com/song/mult").QParam("songId", songId).Get();
        }
    }
}
