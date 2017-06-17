using Musicus.Core;
using Musicus.Core.Utils.Interfaces;
using Musicus.Core.WinRt;
using Musicus.Data.Collection;
using Musicus.Data.Collection.Model;
using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Notifications;

namespace Musicus.BackgroundPlayer
{
    internal class QueueManager
    {
        private static string DbMainPath
        {
            get
            {
                return ApplicationData.Current.LocalFolder.Path;
            }
        }

        private int GetCurrentQueueTrackIndex()
        {
            return _appSettingsHelper.Read<int>(PlayerConstants.CurrentQueueTrackIndex);
        }

        public QueueSong GetCurrentQueueSong()
        {
            int id = GetCurrentQueueTrackIndex();
            return GetQueueSongByIndexId(id);
        }

        public QueueSong GetQueueSongByIndexId(int id)
        {
            return BackgroundQueueHelper.GetQueueSong(p => p.Id == id);
        }

        public QueueSong NextId(int id)
        {
            return IsShuffle ? BackgroundQueueHelper.GetQueueSong(p => p.ShuffleNextId == id) : BackgroundQueueHelper.GetQueueSong(p => p.NextId == id);
        }

        public QueueSong PrevId(int id)
        {
            return IsShuffle ? BackgroundQueueHelper.GetQueueSong(p => p.ShufflePrevId == id) : BackgroundQueueHelper.GetQueueSong(p => p.PrevId == id);
        }


        private readonly IAppSettingsHelper _appSettingsHelper;
        private QueueSong _currentTrack;

        public int _currentTrackIndex = -1;
        public QueueManager(IAppSettingsHelper appSettingsHelper)
        {
            _appSettingsHelper = appSettingsHelper;
            BackgroundMediaPlayer.Current.MediaOpened += Current_MediaOpened;
            BackgroundMediaPlayer.Current.MediaFailed += Current_MediaFailed;
            BackgroundMediaPlayer.Current.MediaEnded += Current_MediaEnded;
        }



        private int _retryCount;
        private void Current_MediaEnded(MediaPlayer sender, object args)
        {
            BackgroundMediaPlayer.Current.Position = TimeSpan.Zero;
            if (_appSettingsHelper.Read<bool>("Repeat"))
                StartMusicInternal(GetCurrentQueueSong());
            else
                SkipToNext();
        }


        //private void Current_CurrentStateChanged(MediaPlayer sender, object args)
        //{
        //    if (sender.CurrentState == MediaPlayerState.Playing)
        //    {
        //        sender.Volume = 1.0;
        //        sender.PlaybackMediaMarkers.Clear();
        //    }
        //}

        private void Current_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            Debug.WriteLine("Failed with error code " + args.ExtendedErrorCode);
            TileUpdateManager.CreateTileUpdaterForApplication("App").Clear();
            if (_retryCount >= 2) return;
            SkipToNext();
            _retryCount++;
        }

        private void Current_MediaOpened(MediaPlayer sender, object args)
        {
            //wait for media to be ready
            //sender.Play();

            TrackChanged?.Invoke(this, CurrentTrack?.Id);
        }

        /// <summary>
        ///     Get the current track
        /// </summary>
        /// //?? (_currentTrack = GetCurrentQueueSong())
        public QueueSong CurrentTrack
        {
            get { return _currentTrack; }
        }

        public void StartMusicInternal(QueueSong track)
        {
            if (track == null)
            {
                SkipToNext();
                return;
            }

            _currentTrack = track;
            SetTrackUri(track);
            TrackChanged?.Invoke(this, _currentTrack.Id);
        }

        /// <summary>
        ///     Skip to next track
        /// </summary>
        public void SkipToNext()
        {
            var next = PrevId(GetCurrentQueueTrackIndex()) ?? PrevId(0);
            if (CurrentTrack != null && CurrentTrack == next) return;
            StartMusicInternal(next);
        }

        public void SkipToPrevious()
        {
            var prev = NextId(GetCurrentQueueTrackIndex()) ?? NextId(0);
            if (CurrentTrack != null && CurrentTrack == prev) return;
            StartMusicInternal(prev);
        }


        private bool IsShuffle
        {
            get { return _appSettingsHelper.Read<bool>("Shuffle"); }
        }

        /// <summary>
        ///     Invoked when the media player is ready to move to next track
        /// </summary>
        public event TypedEventHandler<QueueManager, object> TrackChanged;
        public event EventHandler<UnauthorizedAccessException> ErrorHandler;

        private async void SetTrackUri(QueueSong track)
        {       
            try
            {
                if (!track.Song.IsStreaming)
                {

                    var file = await StorageFile.GetFileFromPathAsync(track.Song.AudioUrl);

                    if (file != null)
                    {
                        try
                        {
                            BackgroundMediaPlayer.Current.SetFileSource(file);
                            BackgroundMediaPlayer.Current.Play();
                        }
                        catch
                        {
                            SkipToNext();
                        }
                    }
                }

                else if (track.Song.IsStreaming)
                {
                    try
                    {
                        BackgroundMediaPlayer.Current.SetUriSource(new Uri(track.Song.AudioUrl));
                        BackgroundMediaPlayer.Current.Play();
                    }
                    catch
                    {
                        SkipToNext();
                    }
                }

                else
                    return;
            }

            catch (UnauthorizedAccessException UnAuth)
            {
                ErrorHandler?.Invoke(this, UnAuth);
                return;
            }

            catch
            {
                return;
              
            }
     
            LoadAndSetTile();
        }

     
        private void LoadAndSetTile()
        {

            QueueSong track = CurrentTrack;

            if (track == null)
            {
                TileUpdateManager.CreateTileUpdaterForApplication("App").Clear();
                return;
            }

            var artworkUrl = AppConstant.MissingArtworkAppPath;
            try
            {
                artworkUrl = BackgroundQueueHelper.IsValidPathForAlbum(track.Song.AlbumId)
                 ? AppConstant.LocalStorageAppPath +
                   string.Format(AppConstant.ArtworkPath, track.Song.AlbumId)
                 : AppConstant.MissingArtworkAppPath;
            }
            catch
            {
                artworkUrl = AppConstant.MissingArtworkAppPath;
            }

            string trackName = track.Song.Name;
            string artistName = track.Song.ArtistName;

            string tileXmlString = "<tile>"
                        + "<visual branding='nameAndLogo'>"

                        + "<binding template='TileSmall' hint-textStacking='top'>"
                        + "<image src='" + artworkUrl + "'" + " placement='peek'/>"
                        + "<text hint-wrap='true' hint-maxLines='2' hint-style='body' hint-align='left'>" + artistName + "</text>"
                        + "</binding>"

                        + "<binding template='TileMedium' hint-textStacking='top'>"
                        + "<image src='" + artworkUrl + "'" + " placement='peek'/>"
                        + "<text hint-style='base' hint-align='left'>" + artistName + "</text>"
                        + "<text hint-style='captionSubtle' hint-align='left'>" + trackName + "</text>"
                        + "</binding>"

                        + "<binding template='TileWide' hint-textStacking='left'>"
                        + "<image src='" + artworkUrl + "'" + " placement='peek'/>"
                        + "<text hint-style='base' hint-align='left'>" + artistName + "</text>"
                        + "<text hint-style='captionSubtle' hint-align='left'>" + trackName + "</text>"
                        + "</binding>"


                        + "<binding template='TileLarge' hint-textStacking='left'>"
                        + "<image src='" + artworkUrl + "'" + " placement='peek'/>"
                        + "<text hint-style='base' hint-align='left'>" + artistName + "</text>"
                        + "<text hint-style='captionSubtle' hint-align='left'>" + trackName + "</text>"
                        + "</binding>"


                        + "</visual>"
                        + "</tile>";


            Windows.Data.Xml.Dom.XmlDocument toastDOM = new Windows.Data.Xml.Dom.XmlDocument();
            toastDOM.LoadXml(tileXmlString);

            try
            {
                TileNotification toast = new TileNotification(toastDOM);
                TileUpdateManager.CreateTileUpdaterForApplication("App").Update(toast);
            }
            catch (Exception)
            {
              
            }

        }

    }
}