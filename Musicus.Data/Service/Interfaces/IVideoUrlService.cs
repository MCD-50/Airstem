using System;
using System.Threading.Tasks;

namespace Musicus.Data.Service.Interfaces
{
    public interface IVideoUrlService
    {
        Task<string> GetUrlAsync(string id, bool isHd);
    }
}
