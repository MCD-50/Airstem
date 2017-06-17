
using Musicus.Core.Utils.Interfaces;
using Musicus.Core.WinRt;
using Musicus.Data.Collection;
using Musicus.Data.Collection.Model;
using Musicus.Helpers.BackgroundHelpers;
using Musicus.Utilities;
using Musicus.ViewModel;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Musicus.PlayerHelpers
{
    public class PlayerViewModel : ViewModel.Mvvm.Others.ViewModelBase
    {

        private readonly AudioPlayerHelper _helper;
        private readonly ICollectionService _service;
        private readonly Helpers.CollectionCommandHelper _commands;
        public SettingViewModel SettingViewModel { get; set; }
        private readonly RootViewModel _root;
        private readonly IAppSettingsHelper _appSettingsHelper;
        public NowPlayingViewModel NowPlaying { get; set; }
        public EventHandler<Song> InternalTrackChanged;


        private DispatcherTimer _timer;
        private TimeSpan _duration;
        private bool _isLoading;


        private Symbol _playPauseIcon;
        private Song _currentSong;
        private bool _isPlayerActive;
        private TimeSpan _position;
       
     
        public PlayerViewModel(AudioPlayerHelper helper, ICollectionService service, RootViewModel root, SettingViewModel settingViewModel, 
            Helpers.CollectionCommandHelper commands, IAppSettingsHelper appSettingsHelper, NowPlayingViewModel _nowPlaying)
        {
            _helper = helper;
            _service = service;
            _root = root;
            _commands = commands;
            _appSettingsHelper = appSettingsHelper;
            SettingViewModel = settingViewModel;
            NowPlaying = _nowPlaying;


            Add();
        }

        public bool IsShuffle
        {
            get { return _appSettingsHelper.Read<bool>("Shuffle"); }
            set
            {
                _appSettingsHelper.Write("Shuffle", value);
                RaisePropertyChanged();
                _helper.OnShuffleChanged();
            }
        }


        public bool IsRepeat
        {
            get { return _appSettingsHelper.Read<bool>("Repeat"); }

            set
            {
                _appSettingsHelper.Write("Repeat", value);
                RaisePropertyChanged();
            }
        }

        public TimeSpan Duration
        {
            get { return _duration; }
            set { Set(ref _duration, value); }
        }

        public TimeSpan Position
        {
            get { return _position; }
            set { Set(ref _position, value); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }

        public bool IsPlayerActive
        {
            get { return _isPlayerActive; }
            set { Set(ref _isPlayerActive, value); }
        }

        public Symbol PlayPauseIcon
        {
            get { return _playPauseIcon; }
            set { Set(ref _playPauseIcon, value); }
        }


        public Helpers.CollectionCommandHelper Commands
        {
            get { return _commands; }
        }

        public Song CurrentSong
        {
            get { return _currentSong; }
            set { Set(ref _currentSong, value); }
        }

        public ICollectionService CollectionService
        {
            get { return _service; }
        }

        public AudioPlayerHelper AudioPlayerHelper
        {
            get { return _helper; }
        }

        private void HelperOnPlaybackStateChanged(object sender, PlaybackStateEventArgs playbackStateEventArgs)
        {
            IsLoading = false;
            switch (playbackStateEventArgs.State)
            {
                default:
                    PlayPauseIcon = Symbol.Play;
                    if (_timer.IsEnabled)
                        _timer.Stop();
                    break;
                case MediaPlayerState.Playing:
                    PlayPauseIcon = Symbol.Pause;
                    _root.IsPause = false;
                    _root.IsEnable = false;
                    _timer.Start();
                    break;

                case MediaPlayerState.Paused:
                    PlayPauseIcon = Symbol.Play;
                    _root.IsPause = true;
                    break;

                case MediaPlayerState.Buffering:
                case MediaPlayerState.Opening:
                    _root.IsEnable = true;
                    _root.IsPause = true;
                    IsLoading = true;
                    if (_timer.IsEnabled)
                        _timer.Stop();

                    break;
            }
        }



        private bool _init = true;
        public void OnAppActive()
        {
            if (_init) return;
            _init = true;
            _helper.TrackChanged -= HelperOnTrackChanged;
            _helper.PlaybackStateChanged -= HelperOnPlaybackStateChanged;
            _helper.Shutdown -= HelperOnShutdown;
            Add();   
        }

        public void OnAppSuspending()
        {
            if (!_init) return;
            _init = false;
            Remove();
        }

        async void Remove()
        {
            _helper.TrackChanged -= HelperOnTrackChanged;
            _helper.PlaybackStateChanged -= HelperOnPlaybackStateChanged;
            _helper.Shutdown -= HelperOnShutdown;
            await DispatcherHelper.RunAsync(() =>
            {
                if (_timer.IsEnabled)
                    _timer.Stop();

                _timer.Tick -= TimerOnTick;
                _timer = null;
                Remove();
            });
        }

        async void Add()
        {
            _helper.TrackChanged += HelperOnTrackChanged;
            _helper.PlaybackStateChanged += HelperOnPlaybackStateChanged;
            _helper.Shutdown += HelperOnShutdown;
            await DispatcherHelper.RunAsync(() =>
            {
                _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                _timer.Tick += TimerOnTick;
            });


            //UnRegister();

        }

        private void HelperOnShutdown(object sender, EventArgs eventArgs)
        {
            CurrentSong = null;
            IsPlayerActive = false;
            _root.IsPlaying = IsPlayerActive;
            if (App.Navigator.CurrentPage is NowPlayingPage && NowPlayingPage._isActive)
            {
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                    App.Navigator.GoBack();
                else  App.Navigator.GoTo<HomePage, ZoomInTransition>(null);
            }
                
        }

        private void HelperOnTrackChanged(object sender, EventArgs e)
        {
            if (_helper.SafeMediaPlayer == null) return;
            var track = _service.GetSongById(_appSettingsHelper.Read<int>(Core.PlayerConstants.CurrentTrack));
            if (track != null)
                PerformTask(track);
        }


        private void PerformTask(Song newSong)
        {
            Duration = _helper.SafeMediaPlayer.NaturalDuration;
            if (_helper.SafePlayerState != MediaPlayerState.Closed && _helper.SafePlayerState != MediaPlayerState.Stopped)
            {
                IsPlayerActive = true;
                _root.IsPlaying = IsPlayerActive;

                if (CurrentSong != newSong)
                {
                    CurrentSong = newSong;
                    InternalTrackChanged?.Invoke(this, CurrentSong);
                    Update(CurrentSong);
                }  
            }
            else
            {
                IsPlayerActive = false;
                _root.IsPlaying = IsPlayerActive;
                CurrentSong = null;
                if (App.Navigator.CurrentPage is NowPlayingPage && NowPlayingPage._isActive)
                    App.Navigator.GoBack();
            }

            if (Duration == TimeSpan.MinValue)
                Duration = TimeSpan.Zero;

        }


        private async void Update(Song track)
        {
            string message = string.Empty;
            await DispatcherHelper.RunAsync(async () =>
            {
                track.LastPlayed = DateTime.Now;
                track.PlayCount += 1;
                if (!App.Locator.SqlService.DbConnection.IsInTransaction)
                    message = await App.Locator.SqlService.UpdateItemAsync(track);
                else
                {
                    await Task.Delay(1000);
                    message = await App.Locator.SqlService.UpdateItemAsync(track);
                }

                App.Locator.HomePage.LoadHistory();

                if (message.Equals(Core.StringMessage.ErrorDatabase))
                    Helpers.MessageHelpers.ShowError(message, "Reinstall airstem.");
            });
        }

        public void NextSong()
        {
        
            _helper.NextSong();
          
        }

        public void PlayPauseToggle()
        {
            _helper.PlayPauseToggle();
        }

        public void PrevSong()
        {
         
            _helper.PrevSong();
        }

        private void TimerOnTick(object sender, object o)
        {
            try
            {
               Position = BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Opening 
                          ? TimeSpan.Zero 
                          : BackgroundMediaPlayer.Current.Position;
            }
            catch
            {
                // pertaining to the following (random) error: Server execution failed (Exception from HRESULT: 0x80080005 (CO_E_SERVER_EXEC_FAILURE))
            }
        }

        private void UnRegister()
        {
            BackgroundTaskUtility.UnregisterBackgroundTasks(BackgroundTaskUtility.ServicingCompleteTaskName);
            Register();
        }

        private void Register()
        {
            var taskRegistered = false;
            var exampleTaskName = "AudioTask";

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == exampleTaskName)
                {
                    taskRegistered = true;
                    break;
                }
            }

            if (!taskRegistered)
            {
                BackgroundTaskUtility.RegisterBackgroundTask(
                    BackgroundTaskUtility.ServicingCompleteTaskEntryPoint, BackgroundTaskUtility.ServicingCompleteTaskName,
                    new SystemTrigger(SystemTriggerType.ServicingComplete, false), null);
            }
        }
    }
}