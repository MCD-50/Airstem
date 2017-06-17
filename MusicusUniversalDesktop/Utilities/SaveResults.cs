using Musicus.Data.Collection;
using Musicus.Data.Collection.Model;

namespace Musicus.Utilities
{
    public class SaveResults
    {
        public SavingError Error { get; set; }
        public Song Song { get; set; }
    }
}