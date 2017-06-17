using System.Collections.Generic;

namespace Musicus.Data.Provider.Deezer.Model
{
    public class DataResponse<T>
    {
        public List<T> Data { get; set; }
    }
}