
using Musicus.Core.Utils;
using Musicus.Core.WinRt;
using Musicus.Core.WinRt.Utilities;
using Musicus.Data.Collection;
using Musicus.Data.Collection.Model;
using Musicus.Helpers;
using Musicus.Utilities;
using Musicus.Core.WinRt.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TagLib;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Notifications;
using Windows.Web;
using CreationCollisionOption = Windows.Storage.CreationCollisionOption;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System.Threading;

namespace Musicus
{
    public class SongDownloadService : ISongDownloadService
    {
        public SongDownloadService(ICollectionService service, ISqlService sqlService)
        {
            this.service = service;
            this.sqlService = sqlService;
            ActiveDownloads = new ObservableCollection<Song>();
        }

        private readonly ICollectionService service;
        private readonly ISqlService sqlService;
        public ObservableCollection<Song> ActiveDownloads { get; private set; }
        List<Task> onGoing = new List<Task>();
        List<string> id = new List<string>();




        public async Task LoadDownloads()
        {
            await DiscoverActiveDownloadsAsync();
            Debug.WriteLine("Loaded downloads.");
        }


        private async Task DiscoverActiveDownloadsAsync()
        {
            // list containing all the operations (except grouped ones)
            IReadOnlyList<DownloadOperation> downloads;
            try
            {
                downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            }
            catch (Exception ex)
            {
                if (!IsExceptionHandled("Discovery error", ex))
                {
                    throw;
                }
                return;
            }


            // no downloads? exit!
            if (downloads.Count > 0)
            {
                foreach (var download in downloads)
                {
                    // With the uri get the song
                    var songEntry = service.Songs.FirstOrDefault(p => p.DownloadId == download.Guid.ToString());
                    id.Add(songEntry.DownloadId);
                    if (songEntry != null)
                    {
                        onGoing.Add(HandleDownload(songEntry, download, false));
                    }
                }

                await Task.WhenAll(onGoing);
            }


        }

        public async Task StartDownloadAsync(Song song)
        {

            if (!App.Locator.Network.IsActive)
            {
                ToastManager.ShowError("No internet connection.");
                return;
            }

            Uri source;
            if (!Uri.TryCreate(song.AudioUrl.Trim(), UriKind.Absolute, out source))
            {
                ToastManager.ShowError("Uri seems to be broken.");
                return;
            }

            song.SongState = SongState.Downloading;
            StorageFile destinationFile;
            try
            {
                //chnaged local folder to temp folder
                var path = string.Format("songs/{0}.mp3", song.Id);
                destinationFile = await WinRtStorageHelper.CreateFileAsync(path, ApplicationData.Current.TemporaryFolder,
                             CreationCollisionOption.ReplaceExisting).ConfigureAwait(false);
            }
            catch (Exception)
            {
                ToastManager.ShowError("Error while creating file.");
                return;
            }


            try
            {
                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation download = downloader.CreateDownload(new Uri(song.AudioUrl), destinationFile);
                download.Priority = BackgroundTransferPriority.Default;
                song.DownloadId = download.Guid.ToString();
                id.Add(song.DownloadId);
                await sqlService.UpdateItemAsync(song).ConfigureAwait(false);
                //try
                //{
                //    List<DownloadOperation> requestOperations = new List<DownloadOperation>();
                //    requestOperations.Add(download);
                //    await BackgroundDownloader.RequestUnconstrainedDownloadsAsync(requestOperations);
                //}
                //catch
                //{
                //    //ignored...
                //}

                //run even if battery saver in on               
                await DispatcherHelper.RunAsync(async () => await HandleDownload(song, download, true));
            }
            catch (Exception e)
            {
                if (e.Message.Contains("there is not enough space on the disk"))
                {
                    ToastManager.ShowError("No space.");
                }
                ExceptionHelper(song);
            }
        }


        void DownloadProgress(DownloadOperation download)
        {

            // Get the associated song BackgroundDownload
            var songDownload = ActiveDownloads.FirstOrDefault(
                    p => p.DownloadId == download.Guid.ToString());

            if (songDownload == null)
            {
                Debug.WriteLine("download null");
                return;
            }

            switch (download.Progress.Status)
            {
                case BackgroundTransferStatus.Running:
                    Debug.WriteLine("Updating song BackgroundDownload progress for {0}", songDownload.Name);
                    songDownload.Download.Status = "Downloading";
                    if (id.Contains(download.Guid.ToString()))
                    {
                        PauseOnce(download);
                        id.Remove(download.Guid.ToString());
                    }
                    Debug.WriteLine("...Running");
                    break;

                case BackgroundTransferStatus.Canceled:
                    Debug.WriteLine("...Canceled");
                    break;

                case BackgroundTransferStatus.Completed:
                    Debug.WriteLine("...Completed");
                    break;

                case BackgroundTransferStatus.Error:
                    Debug.WriteLine("...Canceled");
                    break;

                case BackgroundTransferStatus.Idle:
                    Debug.WriteLine("...Idle");
                    break;

                case BackgroundTransferStatus.PausedByApplication:
                    Debug.WriteLine("...PausedByApplication");
                    break;

                case BackgroundTransferStatus.PausedCostedNetwork:
                    Debug.WriteLine("...PausedCostedNetwork");
                    break;

                case BackgroundTransferStatus.PausedNoNetwork:
                    Debug.WriteLine("...PausedNoNetwork");
                    break;

                case BackgroundTransferStatus.PausedSystemPolicy:
                    Debug.WriteLine("...PausedSystemPolicy");
                    break;
            }

            songDownload.Download.Status = download.Progress.Status.ToString();



            if (download.Progress.TotalBytesToReceive > 0)
            {
                songDownload.Download.BytesToReceive = BytesToString(download.Progress.TotalBytesToReceive);
                songDownload.Download.BytesReceived = BytesToString(download.Progress.BytesReceived);
            }
            else
            {
                songDownload.Download.Status = "Pending";
            }

        }

        async void PauseOnce(DownloadOperation download)
        {
            download.Pause();
            await Task.Delay(100);
            download.Resume();
        }

        private double BytesToString(double byteCount)
        {

            if (byteCount == 0)
                return 0.0;
            double bytes = Math.Abs(byteCount);
            double num = Math.Round(bytes / Math.Pow(1024, 2), 1);
            return (Math.Sign(byteCount) * num);
        }

        private async Task HandleDownload(Song song, DownloadOperation download, bool start)
        {
            if (song == null || download == null) return;
            try
            {

                song.Download = new BackgroundDownload(download);
                ActiveDownloads.Add(song);
                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                if (start)
                {
                    // Start the download and attach a progress handler.
                    await download.StartAsync().AsTask(song.Download.CancellationTokenSrc.Token, progressCallback);
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler.
                    await download.AttachAsync().AsTask(song.Download.CancellationTokenSrc.Token, progressCallback);
                    try
                    {
                        song.SongState = SongState.Downloading;
                        sqlService.UpdateItem(song);
                    }
                    catch
                    {

                    }
                }

                // song.Download = new BackgroundDownload(download);
                //  ActiveDownloads.Add(song);


                ResponseInformation response = download.GetResponseInformation();
                if (response.StatusCode >= 200)
                {
                    var file = download.ResultFile as StorageFile;

                    if (await IsFileValid(file))
                        await DownloadFinishedForAsync(song, download);

                    else
                    {
                        song.SongState = SongState.NoMatch;
                        sqlService.UpdateItem(song);
                        ToastManager.ShowError("Try re matching.");
                        deleteFileSafe(download);
                    }
                }
                else
                {
                    song.SongState = SongState.NoMatch;
                    sqlService.UpdateItem(song);
                    ToastManager.ShowError("No http response.");
                    deleteFileSafe(download);
                }

            }

            catch (Exception e)
            {

                //   var status = BackgroundTransferError.GetStatus(-2147012879);
                //  Debug.WriteLine(status);

                if (e.HResult == -2147012879 || e.Message.Equals("Exception from HRESULT: 0x80072EF1"))
                    MessageHelpers.ShowError("Error while downloading file, operation cancelled by system.",
                        "Restart device!!!");

                if (e.HResult == -2145844844 || e.Message.Equals("Not found (404). (Exception from HRESULT: 0x80190194)"))
                    MessageHelpers.ShowError("Error while downloading file, link broken.", "Rematch track!!!", song: song, IsRematchError: true);

                try
                {
                    song.Download.CancellationTokenSrc?.Cancel();
                }
                catch
                {
                }

                song.SongState = SongState.DownloadListed;
                sqlService.UpdateItem(song);
                deleteFileSafe(download);
            }

            finally
            {
                ActiveDownloads.Remove(song);
            }
        }




        async void deleteFileSafe(DownloadOperation download)
        {
            try
            {
                await download.ResultFile.DeleteAsync();
            }
            catch
            {

            }
        }

        private async Task<bool> IsFileValid(IStorageFile file)
        {
            if (file == null)
                return false;

            BasicProperties properties = await file.GetBasicPropertiesAsync();

            if (properties.Size != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }




        private async Task DownloadFinishedForAsync(Song song, DownloadOperation download)
        {

            var downloadOperation = download;
            bool _isSuccessfull = await UpdateId3TagsAsync(song, downloadOperation.ResultFile);
            if (_isSuccessfull)
            {
                var filename = song.Name.CleanForFileName("Invalid Song Name");
                if (song.ArtistName != song.Album.PrimaryArtist.Name)
                    filename = song.ArtistName.CleanForFileName("Invalid Artist Name") + "-" + filename;

                var path = string.Format(
                    AppConstant.SongPath,
                    song.Album.PrimaryArtist.Name.CleanForFileName("Invalid Artist Name"),
                    song.Album.Name.CleanForFileName("Invalid Album Name"),
                    filename);

                var newDestination = await WinRtStorageHelper.CreateFileAsync(path, KnownFolders.MusicLibrary, CreationCollisionOption.ReplaceExisting);

                //MessageHelpers.ShowError("Saving file... if progress bar doesn't collapse after some time change the default music download location to your device instead of your sd card.",
                //       "Important");

                //C:\Data\Users\Public\Music\Airstem\The Chainsmokers\Unknown Album\All We Know.mp3


                try
                {
                    await downloadOperation.ResultFile.MoveAndReplaceAsync(newDestination);
                }
                catch
                {

                }
                song.AudioUrl = newDestination.Path;
                song.SongState = SongState.Downloaded;

                song.DownloadId = null;

                await sqlService.UpdateItemAsync(song);
                if (App.Locator.Setting.Notification)
                    ToastNotifications(song.ArtistName, song.Name);
            }
            else
            {
                song.SongState = SongState.NoMatch;
                sqlService.UpdateItem(song);
                ToastManager.ShowError("Cannot update tags.");
                await downloadOperation.ResultFile.DeleteAsync();
            }
        }


        public static void ToastNotifications(string artists, string tracks)
        {
            artists = artists.Replace("&", "and");
            tracks = tracks.Replace("&", "and");
            string toastXmlString = "<toast>"
                            + "<visual version='2'>"
                            + "<binding template='ToastImageAndText04'>"
                            + "<text id='1'>" + "Downloaded: " + tracks + "</text>"
                            + "<text id='2'>" + "By: " + artists + "</text>"
                            + "</binding>"
                            + "</visual>"
                            + "</toast>";

            Windows.Data.Xml.Dom.XmlDocument toastDOM = new Windows.Data.Xml.Dom.XmlDocument();
            toastDOM.LoadXml(toastXmlString);

            // Create a toast, then create a ToastNotifier object to show
            // the toast
            try
            {
                ToastNotification toast = new ToastNotification(toastDOM);
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            catch (Exception)
            {
                toastXmlString = "<toast>"
                           + "<visual version='2'>"
                           + "<binding template='ToastImageAndText04'>"
                           + "<text id='1'>" + "Downloaded: " + tracks + "</text>"
                           + "<text id='2'>" + "By: " + artists + "</text>"
                           + "</binding>"
                           + "</visual>"
                           + "</toast>";
                ToastNotification toast = new ToastNotification(toastDOM);
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }

        }






        /// <summary>
        ///     Updates the id3 tags. WARNING, there needs to be more testing with lower end devices.
        /// </summary>
        /// <param name="song">The song.</param>
        /// <param name="file">The file.</param>
        private async Task<bool> UpdateId3TagsAsync(Song song, IStorageFile file)
        {
            using (var fileStream = await file.OpenStreamForWriteAsync().ConfigureAwait(false))
            {
                TagLib.File tagFile;

                try
                {
                    tagFile = TagLib.File.Create(new SimpleFileAbstraction(file.Name, fileStream, fileStream));
                }
                catch (CorruptFileException)
                {
                    await CollectionHelper.DeleteEntryAsync(song);
                    await file.DeleteAsync();
                    return false;
                }

                catch
                {
                    return false;
                }

                var newTags = tagFile.GetTag(TagTypes.Id3v2, true);

                newTags.Title = song.Name;

                if (song.Artist.ProviderId != "autc.unknown")
                {
                    newTags.Performers = song.ArtistName.Split(',').Select(p => p.Trim()).ToArray();
                }

                newTags.Album = song.Album.Name;
                newTags.AlbumArtists = new[] { song.Album.PrimaryArtist.Name };

                if (!string.IsNullOrEmpty(song.Album.Genre))
                {
                    newTags.Genres = song.Album.Genre.Split(',').Select(p => p.Trim()).ToArray();
                }

                newTags.Track = (uint)song.TrackNumber;
                newTags.MusicIpId = song.RadioId.ToString();
                newTags.MusicBrainzTrackId = song.CloudId;
                newTags.Comment = "Downloaded by airstem for Windows 10.";

                try
                {
                    if (song.Album.HasArtwork)
                    {
                        var albumFilePath = string.Format(AppConstant.ArtworkPath, song.Album.Id);
                        var artworkFile = await StorageHelper.GetFileAsync(albumFilePath).ConfigureAwait(false);

                        using (var artworkStream = await artworkFile.OpenAsync(PCLStorage.FileAccess.Read))
                        {
                            using (var memStream = new MemoryStream())
                            {
                                await artworkStream.CopyToAsync(memStream);
                                newTags.Pictures = new IPicture[]
                                {
                                    new Picture(
                                        new ByteVector(
                                            memStream.ToArray(),
                                            (int) memStream.Length))
                                };
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    ToastManager.ShowError("UnauthorizedAccess Exception.");
                    // Should never happen, since we are opening the files in read mode and nothing is locking it.
                }

                await Task.Run(() => tagFile.Save());
                return true;
            }
        }


        public void Cancel(Song _song)
        {
            try
            {
                _song.Download.CancellationTokenSrc?.Cancel();
                if (_song.Download.CancellationTokenSrc != null)
                    _song.Download.CancellationTokenSrc = null;

            }
            catch
            {

            }
        }

        async void updateAsync(Song song)
        {
            try
            {
                await sqlService.UpdateItemAsync(song).ConfigureAwait(false);
            }
            catch
            {

            }
        }

        private void ExceptionHelper(Song song)
        {
            song.SongState = SongState.DownloadListed;
            updateAsync(song);
        }

        private bool IsExceptionHandled(string title, Exception ex, DownloadOperation download = null)
        {
            WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);

            if (error == WebErrorStatus.Unknown)
                return false;

            if (download == null)
                ToastManager.ShowError("Something went wrong.");

            else
                ToastManager.ShowError("Something went wrong.");

            return true;
        }
    }
}

