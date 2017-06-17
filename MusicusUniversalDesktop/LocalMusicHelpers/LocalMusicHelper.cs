#region

using Musicus.Core.WinRt.Common;
using Musicus.Data.Collection.Model;
using Musicus.Data.Model;
using Musicus.Helpers;
using Musicus.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
#endregion

namespace Musicus.LocalMusicHelpers
{
    public class LocalMusicHelper
    {
        private bool _currupt = false;
        // first - a method to retrieve files from folder recursively 
        private async Task RetriveFilesInFolder(List<StorageFile> list, StorageFolder parent)
        {
            try
            {
                list.AddRange((await parent.GetFilesAsync()).Where(p =>
                      p.FileType == ".wma"
                      || p.FileType == ".m4a"
                      || p.FileType == ".mp3"));

                foreach (var folder in (await parent.GetFoldersAsync()).Where(folder =>
                    folder.Name != "Xbox Music" && folder.Name != "Subscription Cache" && folder.Name != "Podcasts"))
                {
                    try
                    {
                        await RetriveFilesInFolder(list, folder);
                    }
                    catch
                    {
                        ToastManager.Show("Add a foder first.");
                        return;
                    }
                }
            }

            catch
            {
                ToastManager.Show(Core.StringMessage.SomethinWentWrong);
                return;
            }
        }


        // first - a method to retrieve files from folder recursively 
        private async Task RetriveVideoFilesInFolder(List<StorageFile> list, StorageFolder parent)
        {
            try
            {
                list.AddRange((await parent.GetFilesAsync()).Where(p => 
                p.FileType == ".flv"
                || p.FileType == ".mp4"
                || p.FileType == ".3gp"));

                foreach (var folder in (await parent.GetFoldersAsync()))
                {
                    try
                    {
                        await RetriveVideoFilesInFolder(list, folder);
                    }
                    catch
                    {
                        ToastManager.Show("Add a foder first.");
                        return;
                    }
                }
            }
            catch
            {
                ToastManager.Show(Core.StringMessage.SomethinWentWrong);
                return;
            }
        }


        public async Task<List<StorageFile>> GetFilesInMusicAsync()
        {
            var audioFiles = new List<StorageFile>();
            //scan music folder
            await RetriveFilesInFolder(audioFiles, KnownFolders.MusicLibrary);

            await Task.Run(async() => 
            {
                if (App.Locator.CollectionService.Folders.Count() > 0)
                    foreach (Folder obj in App.Locator.CollectionService.Folders)
                        if (!string.IsNullOrEmpty(obj.Url) && Directory.Exists(obj.Url))
                            await RetriveFilesInFolder(audioFiles, await StorageFolder.GetFolderFromPathAsync(obj.Url));
            });
          
            return audioFiles;
        }



        public async Task<List<StorageFile>> GetFilesInVideoAsync()
        {
            var videoFiles = new List<StorageFile>();
            //scan music folder
            await RetriveVideoFilesInFolder(videoFiles, KnownFolders.VideosLibrary);

            await Task.Run(async () =>
            {
                if (App.Locator.CollectionService.Folders.Count > 0)
                    foreach (Folder obj in App.Locator.CollectionService.Folders)
                        if (!string.IsNullOrEmpty(obj.Url) && Directory.Exists(obj.Url))
                            await RetriveVideoFilesInFolder(videoFiles, await StorageFolder.GetFolderFromPathAsync(obj.Url));
            });

            return videoFiles;
        }

        public async Task<SavingError> SaveTrackAsync(StorageFile file)
        {
            var audioPath = file.Path;

            if (App.Locator.CollectionService.SongAlreadyExists(audioPath))
            {
                return SavingError.AlreadyExists;
            }

            #region Getting metadata

            #region id3 tags

            TagLib.Tag tags = null;
            TimeSpan duration;
            // var tryAsM4A = false;
            var fileStream = await file.OpenStreamForReadAsync();
            TagLib.File tagFile = null;
            try
            {
                tagFile = TagLib.File.Create(new TagLib.StreamFileAbstraction(file.Name, fileStream, fileStream));
                _currupt = false;
            }
            catch (Exception e)
            {
                // tryAsM4A = e is CorruptFileException;
                if (e.InnerException != null)
                {
                    Debug.WriteLine(e.InnerException.Message);
                }
                _currupt = true;

            }

            //if (tryAsM4A)
            //{
            //    //need to reopen (when it fails to open it disposes of the stream
            //    fileStream = await file.OpenStreamForReadAsync();
            //    try
            //    {
            //        tagFile = TagLib.File.Create(new StreamFileAbstraction(
            //            file.Name.Replace(".mp3", ""), fileStream, fileStream));
            //    }
            //    catch
            //    {
            //    }
            //}

            if (!_currupt)
            {
                tags = tagFile == null ? null : tagFile.Tag;
                if (tags != null)
                {
                    duration = tagFile.Properties.Duration;
                }
                fileStream.Dispose();
                if (tagFile != null) tagFile.Dispose();

                #endregion

                LocalSong track;
                if (tags == null)
                {
                    //if there aren't any id3tags, try using the properties from the file
                    var prop = await file.Properties.GetMusicPropertiesAsync().AsTask().ConfigureAwait(false);
                    track = new LocalSong(prop)
                    {
                        FilePath = audioPath
                    };
                }
                else
                {
                    track = new LocalSong(tags.Title, tags.JoinedPerformers.Replace(";", ","), tags.Album, tags.FirstAlbumArtist, tags.MusicIpId, tags.MusicBrainzTrackId)
                    {
                        FilePath = audioPath,
                        Genre = tags.FirstGenre,
                        TrackNumber = (int)tags.Track,
                        Duration = duration
                    };
                }

                var song = track.ToSong();

                #endregion

                //if no metadata was obtain, then result to using the filename
                if (string.IsNullOrEmpty(song.Name))
                {
                    song.Name = file.DisplayName;
                    song.ProviderId += Convert.ToBase64String(Encoding.UTF8.GetBytes(song.Name.ToLower()));
                }


                if (App.Locator.CollectionService.SongAlreadyExists(track.Title, track.AlbumName, track.ArtistName))
                {
                    return SavingError.AlreadyExists;
                }

                try
                {
                    await App.Locator.CollectionService.AddSongAsync(song, tags, isLocalTrack: true)
                             .ConfigureAwait(false);
                    return SavingError.None;
                }
               
                catch
                {
                    return SavingError.Unknown;
                }
            }

            else
            {
                return SavingError.Unknown;
            }
        }


        public async Task<SavingError> SaveVideoAsync(StorageFile file)
        {
            var audioPath = file.Path;

            if (App.Locator.CollectionService.VideoAlreadyExists(audioPath))
            {
                return SavingError.AlreadyExists;
            }

            #region Getting metadata

            #region id3 tags

            TagLib.Tag tags = null;
            TimeSpan duration;
            // var tryAsM4A = false;
            var fileStream = await file.OpenStreamForReadAsync();
            TagLib.File tagFile = null;
            try
            {
                tagFile = TagLib.File.Create(new TagLib.StreamFileAbstraction(file.Name, fileStream, fileStream));
                _currupt = false;
            }
            catch (Exception e)
            {
                // tryAsM4A = e is CorruptFileException;
                if (e.InnerException != null)
                {
                    Debug.WriteLine(e.InnerException.Message);
                }
                _currupt = true;

            }


            if (!_currupt)
            {
                tags = tagFile == null ? null : tagFile.Tag;
                if (tags != null)
                {
                    duration = tagFile.Properties.Duration;

                }
                fileStream.Dispose();
                if (tagFile != null) tagFile.Dispose();

                #endregion

                LocalVideo video;
                //if there aren't any id3tags, try using the properties from the file
                var prop = await file.Properties.GetVideoPropertiesAsync().AsTask().ConfigureAwait(false);
                video = new LocalVideo(prop)
                {
                    FilePath = audioPath,
                };

                var val = video.ToVideo();

                #endregion

                //if no metadata was obtain, then result to using the filename
                if (string.IsNullOrEmpty(val.Title))
                {
                    val.Title = file.DisplayName;
                    val.Author = file.DisplayName;
                }

                if (App.Locator.CollectionService.VideoAlreadyExists(val.Title, val.Author))
                {
                    return SavingError.AlreadyExists;
                }

                StorageItemThumbnail thumb;
                try
                {
                    try
                    {
                        thumb = await file.GetThumbnailAsync(ThumbnailMode.VideosView);
                    }
                    catch
                    {
                        thumb = null;
                    }
                    await App.Locator.CollectionService.AddVideoAsync(val, tags, thumb).ConfigureAwait(false);
                    return SavingError.None;
                }

                catch
                {
                    return SavingError.Unknown;
                }
            }

            else
            {
                return SavingError.Unknown;
            }
        }

        //static async Task<Uri> FetchLocalThumbnail(StorageFile videofile)
        //{
        //    try
        //    {
        //        //Fetching the thumbnail in form of StorageItemThumbnail
        //        var thumbnail = await videofile.GetThumbnailAsync(ThumbnailMode.VideosView);

        //        var inputBuffer = new Windows.Storage.Streams.Buffer(2048);

        //        IBuffer buf;
        //        IRandomAccessStream stream = new InMemoryRandomAccessStream();

        //        while ((buf = (await thumbnail.ReadAsync(inputBuffer, inputBuffer.Capacity, InputStreamOptions.None))).Length > 0)
        //            await stream.WriteAsync(buf);

        //        var image = new BitmapImage();
        //        image.SetSource(stream);

        //        return image.UriSource;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}


    }
}