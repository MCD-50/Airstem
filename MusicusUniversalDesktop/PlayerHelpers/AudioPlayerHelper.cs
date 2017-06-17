
using Musicus.Core;
using Musicus.Core.Utils.Interfaces;
using Musicus.Data.Collection;
using Musicus.Data.Collection.Model;
using Musicus.Helpers;
using Musicus.Utilities;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;

namespace Musicus
{
    public class AudioPlayerHelper
    {
        private readonly IAppSettingsHelper _appSettings;
        private bool _isShutdown = true;
        private int _retryCount;

        //private AutoResetEvent backgroundAudioTaskStarted;
        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA
        //private bool isMyBackgroundTaskRunning = false;

      

        public AudioPlayerHelper(IAppSettingsHelper appSettings)
        {
            _appSettings = appSettings;
            _appSettings.Write(PlayerConstants.AppState, PlayerConstants.ForegroundAppActive);
            App.DataBaseException += DatabaseExceptionHelper;       
        }

        private void DatabaseExceptionHelper(object sender, bool e)
        {
            PlayAndQueueHelper.Error();
            MessageHelpers.ShowError("Unable to open databse file. We suggest reinstall Airstem.", "Database Error!!!");
        }

        /// <summary>
        /// Gets the information about background task is running or not by reading the setting saved by background task.
        /// This is used to determine when to start the task and also when to avoid sending messages.
        /// </summary>
        //private bool IsMyBackgroundTaskRunning
        //{
        //    get
        //    {
        //        if (isMyBackgroundTaskRunning)
        //            return true;

        //        string value = _appSettings.Read<string>(PlayerConstants.BackgroundTaskState);
        //        if (value == null)
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            try
        //            {
        //                isMyBackgroundTaskRunning = value.Equals(PlayerConstants.BackgroundTaskRunning) ? true : false;
        //            }
        //            catch (ArgumentException)
        //            {
        //                isMyBackgroundTaskRunning = false;
        //            }
        //            return isMyBackgroundTaskRunning;
        //        }
        //    }
        //}


        /// <summary>
        /// You should never cache the MediaPlayer and always call Current. It is possible
        /// for the background task to go away for several different reasons. When it does
        /// an RPC_S_SERVER_UNAVAILABLE error is thrown. We need to reset the foreground state
        /// and restart the background task.
        /// </summary>
        public MediaPlayer SafeMediaPlayer
        {
            get
            {
                MediaPlayer mp = null;
                try
                {
                    mp = BackgroundMediaPlayer.Current;
                }
                catch (Exception ex)
                {
                    if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                    {
                        ShowError("Error restart app!!!");
                        // The foreground app uses RPC to communicate with the background process.
                        // If the background process crashes or is killed for any reason RPC_S_SERVER_UNAVAILABLE
                        // is returned when calling Current.
                        //ResetAfterLostBackground();
                        //StartBackgroundAudioTask();
                    }
                }

                if (mp == null)
                {
                    ShowError("Error restart app!!!");
                    //throw new Exception("Failed to get a MediaPlayer instance.");
                }
                return mp;
            }
        }

        //private void StartBackgroundAudioTask()
        //{
        //    throw new NotImplementedException();
        //}

        //private void ResetAfterLostBackground()
        //{
        //    BackgroundMediaPlayer.Shutdown();
        //    isMyBackgroundTaskRunning = false;
        //    backgroundAudioTaskStarted.Reset();
        //    _appSettings.Write(PlayerConstants.BackgroundTaskState, PlayerConstants.BackgroundTaskUnknown);

        //    try
        //    {
        //        BackgroundMediaPlayer.MessageReceivedFromBackground +=
        //        BackgroundMediaPlayer_MessageReceivedFromBackground;
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
        //        {
        //            throw new Exception("Failed to get a MediaPlayer instance.");
        //        }
        //    }
        //}

        private void ShowError(string title)
        {
            MessageHelpers.ShowError("The foreground app uses RPC to communicate with the background process, If the background process crashes or is killed for any reason RPC_S_SERVER_UNAVAILABLE is returned when calling MediaPlayer.Current.", title);
        }

        public MediaPlayerState SafePlayerState
        {
            get
            {
                var player = SafeMediaPlayer;

                try
                {
                    return player == null ? MediaPlayerState.Closed : player.CurrentState;
                }
                catch
                {
                    if (_retryCount >= 2)
                    {
                        _retryCount = 0;
                        return MediaPlayerState.Closed;
                    }
                    _retryCount++;
                    return SafePlayerState;
                }
            }
        }


        public event EventHandler Shutdown;
        public event EventHandler TrackChanged;
        public event EventHandler<PlaybackStateEventArgs> PlaybackStateChanged;


        public void NextSong()
        {
            //BackgroundMediaPlayer.Current.Pause();
            var value = new ValueSet { { PlayerConstants.SkipNext, string.Empty } };
            BackgroundMediaPlayer.SendMessageToBackground(value);
        }

        public void PrevSong()
        {
            //BackgroundMediaPlayer.Current.Pause();
            var value = new ValueSet { { PlayerConstants.SkipPrevious, string.Empty } };
            BackgroundMediaPlayer.SendMessageToBackground(value);
        }

        public async void OnAppActive()
        {
            App.Locator.AppSettingsHelper.Write(PlayerConstants.AppState, PlayerConstants.ForegroundAppActive);
            await AddMediaPlayerEventHandlers();
            RaiseEvent(TrackChanged);
            OnPlaybackStateChanged(SafePlayerState);
        }

        public void OnAppSuspending()
        {
            App.Locator.AppSettingsHelper.Write(PlayerConstants.AppState, PlayerConstants.ForegroundAppSuspended);
            RemoveMediaPlayerEventHandlers(SafeMediaPlayer);    
        }

        public void OnShuffleChanged()
        {
            RaiseEvent(TrackChanged);
        }

        public void PlayPauseToggle()
        {
            var player = SafeMediaPlayer;

            if (player == null)
            {
                return;
            }

            switch (SafePlayerState)
            {
                case MediaPlayerState.Playing:
                    player.Pause();
                    break;
                case MediaPlayerState.Paused:
                    player.Play();
                    break;
            }
        }


        public async void PlaySong(QueueSong queueSong)
        {
            try
            {
                var song = App.Locator.CollectionService.GetSongById(queueSong.SongId);
                await Task.Factory.StartNew(() =>
                {
                    if (song.IsDownload && !App.Locator.Network.IsActive)
                    {
                        MessageHelpers.ShowError("Problem while streaming.", "No internet connection");
                        NextSong();
                        return;
                    }
                });
                await CompletePlayAsync(queueSong);
            }
            catch
            {
                //ignore
            }
   
            //await Task.Run(() =>
            //{              
            //    if (song == null && !song.IsDownload)
            //    {
            //        if (!File.Exists(song.AudioUrl) || string.IsNullOrEmpty(song.AudioUrl))
            //        {
            //            MessageHelpers.DeleteRequestAsync("File doesn't exists at given url.", "Delete", song);
            //            if (App.Locator.CollectionService.CurrentPlaybackQueue.Count > 0)
            //                NextSong();
            //            return;
            //        }
            //    }

            //    else if (song.IsDownload &&  !App.Locator.Network.IsActive)
            //    {
            //        MessageHelpers.ShowError("Problem while streaming.", "No internet connection");
            //        NextSong();
            //        return;
            //    }


            //    //if (song.IsDownload)
            //    //{
            //    //    if (!await IsValid(song.AudioUrl))
            //    //    {
            //    //        MessageHelpers.ShowError("Audio url is not responding we suggest rematch.", "Re match", song: song, IsRematchError: true);
            //    //        NextSong();
            //    //        return;
            //    //    }
            //    //}
            //});

        }


        private async Task CompletePlayAsync(QueueSong song)
        {
            if (_isShutdown)
                await AddMediaPlayerEventHandlers();
            _appSettings.Write(PlayerConstants.CurrentTrack, song.SongId);
            _appSettings.Write(PlayerConstants.CurrentQueueTrackIndex, song.Id);

            Debug.WriteLine(_appSettings.Read<int>(PlayerConstants.CurrentTrack));
            try
            {
                var message = new ValueSet { { PlayerConstants.StartPlayback, null } };
                BackgroundMediaPlayer.SendMessageToBackground(message);
            }
            catch
            {
                ShowError("Error restart device!!!");
            }
        }

        

        public async Task ShutdownPlayerAsync()
        {
            try
            {
                var player = SafeMediaPlayer;

                await Task.Delay(5);

                if (player == null) return;
                RemoveMediaPlayerEventHandlers(player);
                _appSettings.Write(PlayerConstants.CurrentTrack, null);
                _appSettings.Write(PlayerConstants.CurrentQueueTrackIndex, null);
                BackgroundMediaPlayer.Shutdown();
                _isShutdown = true;
                RaiseEvent(Shutdown);
            }
            catch
            {
                //ognore
            }
        }

        protected virtual async void OnPlaybackStateChanged(MediaPlayerState state)
        {
            await DispatcherHelper.RunAsync(
                 () =>
                 {
                     PlaybackStateChanged?.Invoke(this, new PlaybackStateEventArgs(state));
                 });
        }

        private async Task AddMediaPlayerEventHandlers()
        {
            var player = SafeMediaPlayer;

            if (player == null)
            {

                return;
            }

            try
            {
                // avoid duplicate events
                RemoveMediaPlayerEventHandlers(player);
                player.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
                BackgroundMediaPlayer.MessageReceivedFromBackground +=
                BackgroundMediaPlayer_MessageReceivedFromBackground;
                _isShutdown = false;
                await Task.Delay(250);
                return;
            }
            catch
            {
                // ignored
            }

            if (_retryCount >= 2)
            {
                _isShutdown = true;
                _retryCount = 0;
                return;
            }

            _retryCount++;
            await AddMediaPlayerEventHandlers();
        }


        private void BackgroundMediaPlayer_MessageReceivedFromBackground(
            object sender, MediaPlayerDataReceivedEventArgs e)
        {
            if (e.Data.Keys.Contains(PlayerConstants.Trackchanged))
            {
                Debug.WriteLine(PlayerConstants.Trackchanged);
                RaiseEvent(TrackChanged);
            }

            else if(e.Data.Keys.Contains(PlayerConstants.Error))
            {
                Debug.WriteLine(PlayerConstants.Error);
                ShowUnAuthError(e.Data);
            }

        }

        private void ShowUnAuthError(ValueSet data)
        {
             MessageHelpers.ShowError("Cannot access the specified file. The item is not in a location that the application has access to (including application data folders, folders that are accessible via capabilities, and persisted items in the StorageApplicationPermissions lists). Verify that the file is not marked with system or hidden file attributes. We suggest remove added folders and add them again.", "Access denied!!!");
        }

        

        private void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            OnPlaybackStateChanged(SafePlayerState);
        }

        
        private async void RaiseEvent(EventHandler handler)
        {
            await DispatcherHelper.RunAsync(
                  () =>
                  {
                      handler?.Invoke(this, EventArgs.Empty);
                  });
        }

        private void RemoveMediaPlayerEventHandlers(MediaPlayer player)
        {
            player.CurrentStateChanged -= MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        private async Task<bool> IsValid(string url)
        {
            WebRequest webRequest = WebRequest.Create(url);
            WebResponse webResponse;
            try
            {
                webResponse = await webRequest.GetResponseAsync();
            }
            catch (Exception) //If exception thrown then couldn't get response from address
            {
                return false;
            }
            return true;
        }

    }

    public class PlaybackStateEventArgs : EventArgs
    {
        public PlaybackStateEventArgs(MediaPlayerState state)
        {
            State = state;
        }
        public MediaPlayerState State { get; set; }
    }
}




















//using Musicus.Core;
//using Musicus.Core.Utils.Interfaces;
//using Musicus.Core.WinRt;
//using Musicus.Core.WinRt.Common;
//using Musicus.Data.Collection;
//using Musicus.Data.Collection.Messages;
//using Musicus.Data.Collection.Messages.Helpers;
//using Musicus.Data.Collection.Model;
//using Musicus.Helpers;
//using Musicus.ViewModel.Mvvm.Dispatcher;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Net;
//using System.Threading;
//using System.Threading.Tasks;
//using Windows.Media.Playback;

//namespace Musicus.PlayerHelpers
//{
//    public class AudioPlayerHelper
//    {
//        private readonly IAppSettingsHelper _appSettings;
//        private readonly ICollectionService _service;
//        private readonly ISqlService _sql;
//        private static bool _currentlyPreparing;

//        private AutoResetEvent backgroundAudioTaskStarted;
//        private bool isMyBackgroundTaskRunning = false;
//        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA
//        private bool _isShutdown;


//        private bool _updatingPlaylist = false;
//        private List<QueueSong> queueSongs;

//        public AudioPlayerHelper(IAppSettingsHelper appSettings, ICollectionService service , ISqlService sql)
//        {
//            _appSettings = appSettings;
//            _service = service;
//            _sql = sql;
//            _appSettings.Write(PlayerConstants.AppState, PlayerConstants.ForegroundAppActive);
//            // Setup the initialization lock
//            backgroundAudioTaskStarted = new AutoResetEvent(false);
//        }


//        /// <summary>
//        /// You should never cache the MediaPlayer and always call Current. It is possible
//        /// for the background task to go away for several different reasons. When it does
//        /// an RPC_S_SERVER_UNAVAILABLE error is thrown. We need to reset the foreground state
//        /// and restart the background task.
//        /// </summary>
//        public MediaPlayer SafeMediaPlayer
//        {
//            get
//            {
//                MediaPlayer mp = null;
//                try
//                {
//                    mp = BackgroundMediaPlayer.Current;
//                }
//                catch (Exception ex)
//                {
//                    if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
//                    {
//                        // The foreground app uses RPC to communicate with the background process.
//                        // If the background process crashes or is killed for any reason RPC_S_SERVER_UNAVAILABLE
//                        // is returned when calling Current.
//                        ResetAfterLostBackground();
//                        StartBackgroundAudioTask();
//                    }
//                }

//                if (mp == null)
//                {
//                    throw new Exception("Failed to get a MediaPlayer instance.");
//                }
//                return mp;
//            }
//        }

//        /// <summary>
//        /// Gets the information about background task is running or not by reading the setting saved by background task.
//        /// This is used to determine when to start the task and also when to avoid sending messages.
//        /// </summary>
//        private bool IsMyBackgroundTaskRunning
//        {
//            get
//            {
//                if (isMyBackgroundTaskRunning)
//                    return true;

//                string value = _appSettings.Read(PlayerConstants.BackgroundTaskState);
//                if (value == null)
//                    return false;
//                else
//                {
//                    try
//                    {
//                        isMyBackgroundTaskRunning = value.Equals(PlayerConstants.BackgroundTaskRunning) ? true : false;
//                    }
//                    catch (ArgumentException)
//                    {
//                        isMyBackgroundTaskRunning = false;
//                    }
//                    return isMyBackgroundTaskRunning;
//                }
//            }
//        }


//        public MediaPlayerState SafePlayerState
//        {
//            get
//            {
//                var player = SafeMediaPlayer;
//                try
//                {
//                    return player == null ? MediaPlayerState.Closed : player.CurrentState;
//                }
//                catch
//                {
//                   return MediaPlayerState.Closed;
//                }
//            }
//        }

//        /// <summary>
//        /// The background task did exist, but it has disappeared. Put the foreground back into an initial state.
//        /// </summary>
//        private void ResetAfterLostBackground()
//        {
//            BackgroundMediaPlayer.Shutdown();
//            isMyBackgroundTaskRunning = false;
//            backgroundAudioTaskStarted.Reset();
//            _appSettings.Write(PlayerConstants.BackgroundTaskState, PlayerConstants.BackgroundTaskUnknown);

//            try
//            {
//                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
//            }
//            catch (Exception ex)
//            {
//                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
//                {
//                    ShowError();
//                }
//            }
//        }


//        /// <summary>
//        /// Initialize Background Media Player Handlers and starts playback
//        /// </summary>
//        private async void StartBackgroundAudioTask()
//        {
//           await AddMediaPlayerEventHandlers();

//            await DispatcherHelper.RunAsync(async () =>
//            {
//                bool result = backgroundAudioTaskStarted.WaitOne(9000);
//                await _service.ClearQueueAsync();
//                //Send message to initiate playback
//                if (result == true)
//                {
//                    int id = _appSettings.Read<int>(PlayerConstants.CurrentTrack);
//                    if (queueSongs.Count > 0 && id != -1)
//                    {
//                        Debug.WriteLine("Foreground sending message");
//                        ActionHelper.Try(() => 
//                        {
//                            MessageService.SendMessageToBackground(new UpdatePlaylistMessage(queueSongs, id, startPlayback: true));
//                            ToastManager.Show("Something went wrong...");
//                        });

//                    }
//                }
//                else
//                {

//                    throw new Exception("Background Audio Task didn't start in expected time.");
//                }
//            });
//        }


//        void ShowError()
//        {
//            MessageHelpers.ShowError("Restart device...","Failed to get MediaPlayer instance.");
//        }



//        public void NextSong()
//        {
//            MessageService.SendMessageToBackground(new SkipNextMessage());
//        }



//        public void PrevSong()
//        {
//            MessageService.SendMessageToBackground(new SkipPreviousMessage()); 
//        }



//        public async void OnAppActive()
//        {
//            if (IsMyBackgroundTaskRunning)
//            {
//                // If yes, it's safe to reconnect to media play handlers
//                await AddMediaPlayerEventHandlers();
//                RaiseEvent(TrackChanged);
//                OnPlaybackStateChanged(SafePlayerState);

//                // Send message to background task that app is resumed so it can start sending notifications again
//                MessageService.SendMessageToBackground(new AppResumedMessage());
//            }
//        }



//        public void OnAppSuspending()
//        {
//            _appSettings.Write(PlayerConstants.AppState, PlayerConstants.ForegroundAppSuspended);
//            // Only if the background task is already running would we do these, otherwise
//            // it would trigger starting up the background task when trying to suspend.
//            if (IsMyBackgroundTaskRunning)
//            {
//                // Stop handling player events immediately
//                RemoveMediaPlayerEventHandlers(SafeMediaPlayer);
//                // Tell the background task the foreground is suspended
//                MessageService.SendMessageToBackground(new AppSuspendedMessage());
//            }
//        }

//        public void OnShuffleChanged()
//        {
//            RaiseEvent(TrackChanged);
//            MessageService.SendMessageToBackground(new ShuffleMessage());
//        }



//        public void PlayPauseToggle()
//        {
//            var player = SafeMediaPlayer;

//            if (player == null)
//            {
//                return;
//            }

//            switch (SafePlayerState)
//            {
//                case MediaPlayerState.Playing:
//                    player.Pause();
//                    break;
//                case MediaPlayerState.Paused:
//                    player.Play();
//                    break;
//            }
//        }



//        public event EventHandler Shutdown;
//        public event EventHandler TrackChanged;
//        public event EventHandler<PlaybackStateEventArgs> PlaybackStateChanged;

//        public async void PlaySong(int id, bool startPlayback = true)
//        {
//            var song = _service.GetSongById(id);
//            await Task.Run(async () =>
//            {
//                if (song == null && !song.IsDownload)
//                {
//                    if (!System.IO.File.Exists(song.AudioUrl))
//                    {
//                        MessageHelpers.DeleteRequestAsync("File doesn't exists at given url.", "Delete", song);
//                        if (_service.CurrentPlaybackQueue.Count > 0)
//                            NextSong();
//                        return;
//                    }

//                }

//                if (string.IsNullOrEmpty(song.AudioUrl) || string.IsNullOrWhiteSpace(song.AudioUrl))
//                {
//                    MessageHelpers.ShowError("Audio url doesn't exists.", "Re match", song: song, IsRematchError: true);
//                    NextSong();
//                    return;
//                }

//                if (song.IsDownload)
//                {
//                    if (!Utilities.InternetConnectionHelper.HasConnection())
//                    {
//                        MessageHelpers.ShowError("Problem while streaming.", "No internet connection");
//                        NextSong();
//                        return;
//                    }

//                    if (!await IsValid(song.AudioUrl))
//                    {
//                        MessageHelpers.ShowError("Audio url is not responding we suggest rematch.", "Re match", song: song, IsRematchError: true);
//                        NextSong();
//                        return;
//                    }
//                }
//            });

//            if (startPlayback)
//                await CompletePlayAsync(song);
//            else
//                ChangeNextTrack(song);
//        }

//        //when only track is changed queue;
//        private async void ChangeNextTrack(Song song)
//        {
//            if (_isShutdown) await AddMediaPlayerEventHandlers();
//            _appSettings.Write(PlayerConstants.CurrentTrack, song.Id);

//            try
//            {
//                MessageService.SendMessageToBackground(new TrackChangedMessage(song.Id));
//            }
//            catch
//            {
//                ShowError();
//            }     
//        }


//        //when only track is changed from else place;
//        int failCount = 0;
//        private async Task CompletePlayAsync(Song song)
//        {
//            if (_isShutdown) await AddMediaPlayerEventHandlers();

//            if(failCount > 2)
//            {
//                ShowError();
//                return;
//            }

//            if(_updatingPlaylist)
//            {
//                failCount += 1;
//                ToastManager.Show("Please wait...");
//                return;
//            }

//            failCount = 0;

//            _updatingPlaylist = true;
//            _appSettings.Write(PlayerConstants.CurrentTrack, song.Id);
//            await _service.ClearQueueAsync();

//            bool hasMore = queueSongs.Count > 1 ? true : false;
//            try
//            {
//                await DispatcherHelper.RunAsync(() =>
//                {
//                    if (queueSongs.Count > 0)
//                    {
//                        Debug.WriteLine("Foreground sending message");
//                        MessageService.SendMessageToBackground(new UpdatePlaylistMessage(queueSongs, song.Id, startPlayback: true));
//                    }
//                });
//            }

//            catch
//            {
//                ShowError();
//            }


//        }


//        protected virtual async void OnPlaybackStateChanged(MediaPlayerState state)
//        {
//            await DispatcherHelper.RunAsync(
//                 () =>
//                 {
//                     var handler = PlaybackStateChanged;
//                     if (handler != null)
//                     {
//                         handler(this, new PlaybackStateEventArgs(state));
//                     }
//                 });
//        }


//        public async Task ShutdownPlayerAsync()
//        {
//            try
//            {
//                var player = SafeMediaPlayer;
//                if (player == null) return;
//                RemoveMediaPlayerEventHandlers(player);
//                BackgroundMediaPlayer.Shutdown();
//                App.Locator.AppSettingsHelper.Write(PlayerConstants.CurrentTrack, null);
//                await Task.Delay(30);
//                _isShutdown = true;
//                RaiseEvent(Shutdown);
//            }
//            catch
//            {
//                //ognore
//            }
//        }


//        private async Task AddMediaPlayerEventHandlers()
//        {
//            var player = SafeMediaPlayer;
//            if (player == null) return;
//            try
//            {
//                // avoid duplicate events
//                RemoveMediaPlayerEventHandlers(player);
//                player.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
//                BackgroundMediaPlayer.MessageReceivedFromBackground +=
//                BackgroundMediaPlayer_MessageReceivedFromBackground;
//                await Task.Delay(30);
//                _isShutdown = false;
//                return;
//            }
//            catch
//            {
//                // ignored
//            }

//            _isShutdown = true;
//        }


//       async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
//        {
//            var message = MessageService.ParseMessage(e.Data);

//            Debug.WriteLine(message);

//            if (message is BackgroundAudioTaskStartedMessage)
//            {
//                // StartBackgroundAudioTask is waiting for this signal to know when the task is up and running
//                // and ready to receive messages
//                Debug.WriteLine("Foreground BackgroundAudioTask started");
//                backgroundAudioTaskStarted.Set();
//            }

//            else if (message is UpdateSuccessMessage)
//            {
//                Debug.WriteLine("Foreground update playlist");
//                await _service.ClearQueueAsync();
//                var currentId = _appSettings.Read<int>(PlayerConstants.CurrentTrack); 
//                await PerformAddToQueue(queueSongs, currentId);
//            }

//            else
//            {
//                var trackChangedMessage = message as TrackChangedMessage;
//                if (trackChangedMessage != null)
//                {
//                    Debug.WriteLine("Foreground Track Changed Message.");
//                    RaiseEvent(TrackChanged);
//                } 
//            }
//        }


//        private void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
//        {
//            OnPlaybackStateChanged(SafePlayerState);
//        }


//        private async void RaiseEvent(EventHandler handler)
//        {
//            await DispatcherHelper.RunAsync(
//                  () =>
//                  {
//                      if (handler != null)
//                      {
//                          handler(this, EventArgs.Empty);
//                      }
//                  });
//        }

//        private void RemoveMediaPlayerEventHandlers(MediaPlayer player)
//        {
//            player.CurrentStateChanged -= MediaPlayer_CurrentStateChanged;
//            BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundMediaPlayer_MessageReceivedFromBackground;
//        }

//        private async Task<bool> IsValid(string url)
//        {
//            WebRequest webRequest = WebRequest.Create(url);
//            WebResponse webResponse;
//            try
//            {
//                webResponse = await webRequest.GetResponseAsync();
//            }
//            catch (Exception) //If exception thrown then couldn't get response from address
//            {
//                return false;
//            }
//            return true;
//        }

//        public async Task PlaySongsAsync(Song song, List<Song> songs)
//        {
//            if (song == null || songs == null || songs.Count == 0) return;
//            if (!song.IsMatched) return;
//            songs = songs.Where(p => p.IsMatched).ToList();

//            if (songs.Count == 0)
//            {
//                ToastManager.ShowError("The songs haven't been matched yet.");
//                return;
//            }

//            if (_currentlyPreparing)
//            {
//                await Task.Delay(20);
//                ToastManager.ShowError("Please wait...");
//                await Task.Delay(2000);
//            }

//            if (SafeMediaPlayer.CurrentState == MediaPlayerState.Playing && SafeMediaPlayer.Position < TimeSpan.FromSeconds(10))
//            {
//                ToastManager.ShowError("Try after few secs..");
//                return;
//            }

//            queueSongs = ConvertToQueueTrack(songs);
//            PlaySong(song.Id);
//        }

//        private async Task PerformAddToQueue(List<QueueSong> queueSong, int id)
//        {        
//            if (_sql.DbConnection.IsInTransaction)
//            {
//                _currentlyPreparing = true;
//                ToastManager.Show("Please wait...");
//                await Task.Delay(2000);
//                if (_sql.DbConnection.IsInTransaction)
//                    await Task.Delay(4000);
//            }

//            var currentTrack = queueSong.FirstOrDefault(p => p.SongId == id);
//            queueSong.Remove(currentTrack);

//            try
//            {
//               _currentlyPreparing = true;
//                _sql.BeginTransaction();

//                await _service.AddToQueueAsync(currentTrack).ConfigureAwait(false);

//                foreach (var track in queueSong)
//                    await _service.AddToQueueAsync(track).ConfigureAwait(false);

//                _sql.Commit();
//                _currentlyPreparing = false;
//            }
//            catch
//            {
//                //ignored
//            }

//            if(queueSongs != null)
//                queueSongs = null;

//            _updatingPlaylist = false;
//        }

//        private List<QueueSong> ConvertToQueueTrack(List<Song> songs)
//        {
//            List<QueueSong> _dummy = new List<QueueSong>();
//            foreach(var song in songs)
//            {
//                try
//                {
//                    _dummy.Add(new QueueSong
//                    {
//                        SongName = song.Name,
//                        ArtistName = song.ArtistName,
//                        AlbumUri = song.Album.Artwork.Uri,
//                        IsStreaming = song.IsStreaming,
//                        AudioUrl = song.AudioUrl,
//                        SongId = song.Id
//                    });
//                }
//                catch
//                {
//                    _dummy.Add(new QueueSong
//                    {
//                        SongName = song.Name,
//                        ArtistName = song.ArtistName,
//                        AlbumUri = new Uri(AppConstant.MissingArtworkAppPath),
//                        IsStreaming = song.IsStreaming,
//                        AudioUrl = song.AudioUrl,
//                        SongId = song.Id
//                    });
//                }
//            }

//            return _dummy;
//        }

//        public async void AddToQueueAsync(Song song)
//        {
//            if(_sql.DbConnection.IsInTransaction)
//            {
//                ToastManager.Show("Please wait...");
//                await Task.Delay(2000);
//                if (_sql.DbConnection.IsInTransaction)
//                    await Task.Delay(4000);
//            }

//            if (!song.IsMatched)
//            {
//                ToastManager.ShowError("Can't add unmatch songs to queue.");
//                return;
//            }

//            if (_currentlyPreparing)
//            {
//                ToastManager.ShowError("Something went wrong.");
//                return;
//            }

//            QueueSong newQueueSong = new QueueSong
//            {
//                SongName = song.Name,
//                ArtistName = song.ArtistName,
//                AlbumUri = song.Album.Artwork.Uri,
//                IsStreaming = song.IsStreaming,
//                AudioUrl = song.AudioUrl,
//                SongId = song.Id
//            };

//            _currentlyPreparing = true;
//            _sql.BeginTransaction();
//            await _service.AddToQueueAsync(newQueueSong).ConfigureAwait(false);
//            _sql.Commit();
//            _currentlyPreparing = false;

//        }

//    }

//    public class PlaybackStateEventArgs : EventArgs
//    {
//        public PlaybackStateEventArgs(MediaPlayerState state)
//        {
//            State = state;
//        }
//        public MediaPlayerState State { get; set; }
//    }
//}
