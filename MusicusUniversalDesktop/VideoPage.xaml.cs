using Musicus.Data.Collection.Model;
using Musicus.Helpers;
using Musicus.Utilities;
using Musicus.ViewModel;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Musicus
{
    public sealed partial class VideoPage : IModalSheetPage
    {
        private DisplayRequest appDisplayRequest = null;

        public VideoPage()
        {
            this.InitializeComponent();
        }
       
        private void PlayerVMStartPlaybackVideo(object sender, VideoModel e)
        {
            if (e == null) return;
            StartControlForPlayback(e);
        }

        bool forcedPause = false;
        private void VideoPlayerClosePlayback()
        {   
            try
            {
                Player.Stop();
                if (Player.Source != null)
                    Player.Source = null;        
            }
            catch
            {
                //ignored
            }
        }

        public Popup Popup { get; private set; }
        public void OnOpened(Popup popup)
        {
            Popup = popup;
          
            App.Locator.VideoPlayer.StartPlaybackVideo += PlayerVMStartPlaybackVideo;
            App.SupressBackEvent += HardwareButtonsOnBackPressedForVideoPage;
            App.Navigator.ShowBack();

            progressRing.IsActive = true;
            Player.AreTransportControlsEnabled = false;

            BeforeStart(ModalSheetUtility.PassingObject);
        }


        void HardwareButtonsOnBackPressedForVideoPage(object sender, BackRequestedEventArgs e)
        {
            SheetUtility.CloseVideoPage();
        }

        public void OnClosed()
        {
            Popup = null;
          
            VideoPlayerClosePlayback();
            App.Locator.VideoPlayer.StartPlaybackVideo -= PlayerVMStartPlaybackVideo;

            UiBlockerUtility.Unblock();
            App.SupressBackEvent -= HardwareButtonsOnBackPressedForVideoPage;

            if (App.Navigator.StackCount <= 0)
                App.Navigator.HideBack();

            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            {
                var player = App.Locator.AudioPlayerHelper.SafeMediaPlayer;
                if (player.CurrentState == MediaPlayerState.Paused && forcedPause)
                    player.Play();
                forcedPause = false;
            }
        }


        async void BeforeStart(object parameter)
        {

            if (App.Locator.Player.IsPlayerActive)
            {
                var player = App.Locator.AudioPlayerHelper.SafeMediaPlayer;
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                    await App.Locator.AudioPlayerHelper.ShutdownPlayerAsync();
                else
                {
                    forcedPause = true;
                    player.Pause();
                }
            }

            if (parameter != null && parameter is Video)
                App.Locator.VideoPlayer.InvokeOffline(parameter as Video);
            else
            {
                await Task.Delay(100);
                App.Locator.VideoPlayer.VideoClickExecuteAfter();
            }
              
        }


        async void StartControlForPlayback(VideoModel videoModel)
        {
            try
            {
                if (videoModel.IsWebVideo)
                {
                    await DispatcherHelper.RunAsync(() => 
                    {
                        Player.Source = new Uri(videoModel.Url);
                        Player.Play();
                    });                   
                }

                else if (!videoModel.IsWebVideo)
                {
                    try
                    {
                        var file = await InitializeFromFilePath(videoModel.Url);
                        IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                        Player.SetSource(stream, file.ContentType);
                        Player.Play();
                    }
                    catch
                    {
                        await DispatcherHelper.RunAsync(async () =>
                        {
                            var file = await InitializeFromFilePath(videoModel.Url);
                            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                            Player.SetSource(stream, file.ContentType);
                            Player.Play();
                        });
                    }
                }

                else
                {
                    try
                    {
                        progressRing.IsActive = false;
                        MessageHelpers.ShowError(Core.StringMessage.SomethinWentWrong, "Error");
                        SheetUtility.CloseVideoPage();
                    }
                    catch
                    {
                        await DispatcherHelper.RunAsync(() =>
                        {
                            SheetUtility.CloseVideoPage();
                        });
                    }
                }
            }
            catch (Exception)
            {
                try
                {
                    progressRing.IsActive = false;
                    MessageHelpers.ShowError(Core.StringMessage.SomethinWentWrong, "Error");
                    SheetUtility.CloseVideoPage();
                }

                catch
                {
                    await DispatcherHelper.RunAsync(() =>
                    {
                        SheetUtility.CloseVideoPage();
                    });
                }
              
            }
        }

        public async Task<StorageFile> InitializeFromFilePath(string path)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);
                return file;
            }
            catch (Exception)
            {
                throw new Exception("no_video_urls_found");
            }
        }




        private void StateChanged(object sender, RoutedEventArgs e)
        {
            switch (Player.CurrentState)
            {
                default:
                    progressRing.IsActive = false;
                    Player.AreTransportControlsEnabled = false;
                    if (appDisplayRequest != null)
                    {
                        // Deactivate the display request and set the var to null.
                        appDisplayRequest.RequestRelease();
                        appDisplayRequest = null;
                    }
                    break;

                case MediaElementState.Playing:
                    progressRing.IsActive = false;
                    Player.AreTransportControlsEnabled = true;
                    if (appDisplayRequest == null)
                    {
                        // This call creates an instance of the DisplayRequest object. 
                        appDisplayRequest = new DisplayRequest();
                        appDisplayRequest.RequestActive();
                    }
                    break;

                case MediaElementState.Paused:
                    progressRing.IsActive = false;
                    Player.AreTransportControlsEnabled = true;
                    if (appDisplayRequest == null)
                    {
                        // This call creates an instance of the DisplayRequest object. 
                        appDisplayRequest = new DisplayRequest();
                        appDisplayRequest.RequestActive();
                    }
                    break;

                case MediaElementState.Stopped:
                    progressRing.IsActive = false;
                    Player.AreTransportControlsEnabled = true;
                    if (appDisplayRequest != null)
                    {
                        // Deactivate the display request and set the var to null.
                        appDisplayRequest.RequestRelease();
                        appDisplayRequest = null;
                    }
                    break;

                case MediaElementState.Buffering:
                case MediaElementState.Opening:
                    progressRing.IsActive = true;
                    Player.AreTransportControlsEnabled = false;
                    if (appDisplayRequest != null)
                    {
                        // Deactivate the display request and set the var to null.
                        appDisplayRequest.RequestRelease();
                        appDisplayRequest = null;
                    }
                    break;

            }
        }

    }
}
