

using Musicus.Core;
using Musicus.Core.WinRt.Utilities;
using System;
using System.Diagnostics;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Playback;
using Windows.UI.Notifications;

namespace Musicus.BackgroundPlayer
{
    internal enum ForegroundAppStatus
    {
        Active,
        Suspended,
        Unknown
    }

    public sealed class MusicTask : IBackgroundTask
    {
        #region Private fields, properties

        internal static ManualResetEvent TaskStarted { get; } = new ManualResetEvent(false);
        private BackgroundTaskDeferral _deferral; // Used to keep task alive
        private AppSettingsHelper _appSettingsHelper;
        private bool _backgroundtaskrunning = false;

        private ForegroundAppStatus ForegroundAppState
        {
            get
            {
                var value = _appSettingsHelper.Read(PlayerConstants.AppState);
                if (value == null)
                    return ForegroundAppStatus.Unknown;
                return (ForegroundAppStatus)Enum.Parse(typeof(ForegroundAppStatus), value);
            }
        }

        private QueueManager _queueManager;
        private SystemMediaTransportControls _systemmediatransportcontrol;

        /// <summary>
        ///     Property to hold current playlist
        /// </summary>
        private QueueManager QueueManager
        {
            get
            {
                if (_queueManager != null) return _queueManager;
                _queueManager = new QueueManager(_appSettingsHelper);
                return _queueManager;
            }
        }

        #endregion

        #region IBackgroundTask and IBackgroundTaskInstance Interface Members and handlers

        /// <summary>
        ///     The Run method is the entry point of a background task.
        /// </summary>
        /// <param name="taskInstance"></param>
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _appSettingsHelper = new AppSettingsHelper();
            Debug.WriteLine("Background Audio Task " + taskInstance.Task.Name + " starting...");
            // InitializeAsync SMTC object to talk with UVC. 
            //Note that, this is intended to run after app is paused and 
            //hence all the logic must be written to run in background process

            _systemmediatransportcontrol = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            _systemmediatransportcontrol.ButtonPressed += systemmediatransportcontrol_ButtonPressed;
            // _systemmediatransportcontrol.PropertyChanged += systemmediatransportcontrol_PropertyChanged;
            _systemmediatransportcontrol.IsEnabled = true;
            _systemmediatransportcontrol.IsPauseEnabled = true;
            _systemmediatransportcontrol.IsPlayEnabled = true;
            _systemmediatransportcontrol.IsNextEnabled = true;
            _systemmediatransportcontrol.IsPreviousEnabled = true;



            //InitializeAsync message channel 
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;
            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;

            //Send information to foreground that background task has been started if app is active
            if (ForegroundAppState == ForegroundAppStatus.Active)
            {
                var message = new ValueSet { { PlayerConstants.BackgroundTaskStarted, "" } };
                BackgroundMediaPlayer.SendMessageToForeground(message);
            }

            _appSettingsHelper.Write(PlayerConstants.BackgroundTaskState, PlayerConstants.BackgroundTaskRunning);

            // This must be retrieved prior to subscribing to events below which use it
            _deferral = taskInstance.GetDeferral();

            // Mark the background task as started to unblock SMTC Play operation (see related WaitOne on this signal)
            TaskStarted.Set();

            taskInstance.Task.Completed += Taskcompleted;
            taskInstance.Canceled += OnCanceled;
            //Add handlers for MediaPlayer
            //Add handlers for playlist trackchanged
            QueueManager.TrackChanged += playList_TrackChanged;
            QueueManager.ErrorHandler += QueueManager_ErrorHandler;
            _backgroundtaskrunning = true;
        }


        /// <summary>
        ///     Indicate that the background task is completed.
        /// </summary>
        private void Taskcompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Debug.WriteLine("Audio Task " + sender.TaskId + " Completed...");
            TileUpdateManager.CreateTileUpdaterForApplication("App").Clear();
            _deferral.Complete();

        }

        /// <summary>
        ///     Handles background task cancellation. Task cancellation happens due to :
        ///     1. Another Media app comes into foreground and starts playing music
        ///     2. Resource pressure. Your task is consuming more CPU and memory than allowed.
        ///     In either case, save state so that if foreground app resumes it can know where to start.
        /// </summary>
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            // You get some time here to save your state before process and resources are reclaimed
            Debug.WriteLine("MyBackgroundAudioTask " + sender.Task.TaskId + " Cancel Requested...");
            try
            {
                TaskStarted.Reset();
                if (_queueManager != null)
                {
                    QueueManager.TrackChanged -= playList_TrackChanged;
                    _queueManager = null;
                }

                _appSettingsHelper.Write(PlayerConstants.BackgroundTaskState,
                    PlayerConstants.BackgroundTaskCancelled);

                _backgroundtaskrunning = false;
                //unsubscribe event handlers
                _systemmediatransportcontrol.ButtonPressed -= systemmediatransportcontrol_ButtonPressed;

                BackgroundMediaPlayer.Shutdown(); // shutdown media pipeline
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            if (_deferral != null)
            {
                _deferral.Complete(); // signals task completion. 
                Debug.WriteLine("AudioPlayer Cancel complete...");
            }

            QueueManager.ErrorHandler -= QueueManager_ErrorHandler;
            TileUpdateManager.CreateTileUpdaterForApplication("App").Clear();
        }

        #endregion

        #region SysteMediaTransportControls related functions and handlers

        /// <summary>
        ///     Update UVC using SystemMediaTransPortControl apis
        /// </summary>
        private void UpdateUvcOnNewTrack()
        {
            _systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Playing;
            _systemmediatransportcontrol.DisplayUpdater.Type = MediaPlaybackType.Music;
            _systemmediatransportcontrol.DisplayUpdater.MusicProperties.Title = QueueManager.CurrentTrack.Song.Name;
            _systemmediatransportcontrol.DisplayUpdater.MusicProperties.Artist = QueueManager.CurrentTrack.Song.ArtistName;
            _systemmediatransportcontrol.DisplayUpdater.Update();
        }






        /// <summary>
        ///     This function controls the button events from UVC.
        ///     This code if not run in background process, will not be able to handle button pressed events when app is suspended.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void systemmediatransportcontrol_ButtonPressed(SystemMediaTransportControls sender,
                SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    Debug.WriteLine("UVC play button pressed");
                    // If music is in paused state, for a period of more than 5 minutes, 
                    //app will get task cancellation and it cannot run code. 
                    //However, user can still play music by pressing play via UVC unless a new app comes in clears UVC.
                    //When this happens, the task gets re-initialized and that is asynchronous and hence the wait
                    if (!_backgroundtaskrunning)
                    {
                        var result = TaskStarted.WaitOne(5000);
                        if (!result)
                            throw new Exception("Background Task didn't initialize in time");
                        StartPlayback();
                    }
                    else
                    {
                        BackgroundMediaPlayer.Current.Play();
                    }
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    Debug.WriteLine("UVC pause button pressed");
                    try
                    {
                        BackgroundMediaPlayer.Current.Pause();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                    break;
                case SystemMediaTransportControlsButton.Next:
                    Debug.WriteLine("UVC next button pressed");
                    SkipToNext();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Debug.WriteLine("UVC previous button pressed");
                    if (BackgroundMediaPlayer.Current.Position.TotalSeconds > 20)
                    {
                        //StartPlayback();
                        BackgroundMediaPlayer.Current.Position = TimeSpan.Zero;
                        BackgroundMediaPlayer.Current.Play();
                    }
                    else
                        SkipToPrevious();

                    break;
            }
        }

        #endregion

        #region Playlist management functions and handlers

        /// <summary>
        ///     Start playlist and change UVC state
        /// </summary>
        private void StartPlayback()
        {
            try
            {
                QueueManager.StartMusicInternal(QueueManager.GetCurrentQueueSong());
                //Data.Collection.BackgroundQueueHelper.SizeOfQueue = bac
            }
            catch (Exception)
            {
                Debug.WriteLine("exception");
            }
        }

        /// <summary>
        ///     Fires when playlist changes to a new track
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void playList_TrackChanged(QueueManager sender, object args)
        {
            _appSettingsHelper.Write(PlayerConstants.CurrentTrack, sender.CurrentTrack.SongId);
            _appSettingsHelper.Write(PlayerConstants.CurrentQueueTrackIndex, sender.CurrentTrack.Id);


            if (ForegroundAppState == ForegroundAppStatus.Active)
            {
                //Message channel that can be used to send messages to foreground
                var message = new ValueSet { { PlayerConstants.Trackchanged, sender.CurrentTrack.Id } };
                BackgroundMediaPlayer.SendMessageToForeground(message);
            }
                     
            UpdateUvcOnNewTrack();
        }


        private void QueueManager_ErrorHandler(object sender, UnauthorizedAccessException e)
        {
            if (ForegroundAppState == ForegroundAppStatus.Active)
            {
                //Message channel that can be used to send messages to foreground
                var message = new ValueSet { { PlayerConstants.Error, e.Message } };
                BackgroundMediaPlayer.SendMessageToForeground(message);
            }
        }



        ///// <summary>
        /////     Skip track and update UVC via SMTC
        ///// </summary>
        private void SkipToPrevious()
        {
            _systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Changing;
            QueueManager.SkipToPrevious();
        }

        /// <summary>
        ///     Skip track and update UVC via SMTC
        /// </summary>
        private void SkipToNext()
        {
            _systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Changing;
            QueueManager.SkipToNext();
        }

        #endregion

        #region Background Media Player Handlers
        private void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (sender.CurrentState == MediaPlayerState.Playing)
            {
                //QueueManager.SetTile();
                _systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Playing;
            }
            else if (sender.CurrentState == MediaPlayerState.Paused)
            {
               // TileUpdateManager.CreateTileUpdaterForApplication("App").Clear();
                _systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Paused;
            }
            //else if(sender.CurrentState == MediaPlayerState.Stopped || sender.CurrentState == MediaPlayerState.Closed)
             //   TileUpdateManager.CreateTileUpdaterForApplication("App").Clear();
        }


        /// <summary>
        ///     Fires when a message is recieved from the foreground app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender,
            MediaPlayerDataReceivedEventArgs e)
        {
            foreach (var key in e.Data.Keys)
            {
                switch (key.ToLower())
                {
                    case PlayerConstants.AppSuspended:
                        Debug.WriteLine("App suspending");
                        // App is suspended, you can save your task state at this point
                        _appSettingsHelper.Write(PlayerConstants.CurrentTrack, QueueManager.CurrentTrack.SongId);
                        _appSettingsHelper.Write(PlayerConstants.CurrentQueueTrackIndex, QueueManager.CurrentTrack.Id);
                        break;

                    case PlayerConstants.AppResumed:
                        Debug.WriteLine("App resuming");// App is resumed, now subscribe to message channel
                        break;

                    case PlayerConstants.StartPlayback:
                        //Foreground App process has signalled that it is ready for playback
                        Debug.WriteLine("Starting Playback");
                        StartPlayback();
                        break;
                    case PlayerConstants.SkipNext: // User has chosen to skip track from app context.
                        Debug.WriteLine("Skipping to next");
                        SkipToNext();
                        break;
                    case PlayerConstants.SkipPrevious: // User has chosen to skip track from app context.
                        Debug.WriteLine("Skipping to previous");
                        SkipToPrevious();
                        break;
                }
            }
        }

        #endregion
    }
}
