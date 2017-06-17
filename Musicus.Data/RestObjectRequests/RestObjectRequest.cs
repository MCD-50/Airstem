using System.Threading.Tasks;
using Musicus.Core.Extensions;
using Musicus.Data.Extensions;

namespace Musicus.Data.RestObjectRequests
{
    public abstract class RestObjectRequest<T> : RestRequest
    {
        public async Task<RestResponse<T>> ToResponseAsync()
        {
            return await this.Fetch<T>().DontMarshall();
        }
    }
}