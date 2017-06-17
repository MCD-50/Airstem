using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Musicus.Data.Collection.Model;

namespace Musicus.Data.Collection
{
    public interface ISongDownloadService
    {
        ObservableCollection<Song> ActiveDownloads { get; }

        /// <summary>
        ///     Loads all downloads and attaches to them
        /// </summary>
        Task LoadDownloads();

        /// <summary>
        ///     Pause all downloads
        /// </summary>
        /// <summary>
        /// /
        /// </summary>
        /// <param name="backgroundDownload"></param>
        void Cancel(Song song);

        /// <summary>
        ///     Starts a BackgroundDownload.
        /// </summary>
        Task StartDownloadAsync(Song song);
    }
}
