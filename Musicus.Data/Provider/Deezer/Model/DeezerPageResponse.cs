using System.Collections.Generic;

namespace Musicus.Data.Provider.Deezer.Model
{
    public class DeezerPageResponse<T> : DeezerBaseResponse
    {
        public List<T> Data { get; set; }
        public int Total { get; set; }
        public string Next { get; set; }
    }
}