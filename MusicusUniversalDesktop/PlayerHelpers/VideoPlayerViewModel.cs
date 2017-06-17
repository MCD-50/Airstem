using Musicus.Data.Collection;
using Musicus.Data.Collection.Model;
using Musicus.Data.Model.Advertisement;
using Musicus.Data.Model.WebData;
using Musicus.Data.Service.Interfaces;
using Musicus.Helpers;
using Musicus.ViewModel.Mvvm.Dispatcher;
using Musicus.ViewModel.Mvvm.Others;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Musicus.ViewModel
{
    public class VideoPlayerViewModel : ViewModelBase
    {
        public event EventHandler<VideoModel> StartPlaybackVideo;
        private string currentVideoTitle;
        
        private readonly ICollectionService _collectionService;
        private readonly IVideoUrlService _videoUrlService;
        private readonly IMiscellaneousService _misc;

        private ObservableCollection<Base> _similarVideos;

        private WebVideo _vidInfo;
        private string _message;
        private SolidColorBrush _backgroundBrush;
    
     
        public VideoPlayerViewModel(ICollectionService collectionService, IMiscellaneousService misc, IVideoUrlService videoUrlService)
        {
            _collectionService = collectionService;
            _videoUrlService = videoUrlService;
            _misc = misc;
            SimilarVideos = new ObservableCollection<Base>();
        }

        public ObservableCollection<Base> SimilarVideos
        {
            get { return _similarVideos; }
            set { Set(ref _similarVideos, value); }
        }

        public string CurrentVideoTitle
        {
            get { return currentVideoTitle; }
            set { Set(ref currentVideoTitle, value); }
        }

        public WebVideo VideoInfo
        {
            get { return _vidInfo; }
            set { Set(ref _vidInfo, value); }
        }

        public string Message
        {
            get { return _message; }
            set { Set(ref _message, value); }
        }

        public SolidColorBrush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set { Set(ref _backgroundBrush, value); }
        }

        private bool viewModelCleared = false;
        public void ClearVideoPlayerViewModel()
        {
            if (!viewModelCleared)
            {
                if (VideoInfo != null) VideoInfo = null;
                if (SimilarVideos != null) SimilarVideos = null;

                viewModelCleared = true;
            }
        }

        public void InvokeOffline(Video parameter)
        {
            try
            {
                var id = parameter;
                if (id == null) return;
                VideoModel videoModel = new VideoModel(id.VideoUrl, false);
                StartPlaybackVideo?.Invoke(this, videoModel);
            }
            catch
            {

            }
        }

        public async void InvokeOnline(object parameter)
        {
            
            if (!App.Locator.Network.IsActive)
            {
                Message = Core.StringMessage.NoInternetConnection;
                return;
            }

            Message = Core.StringMessage.LoadingPleaseWait;

         
            try
            {          
                if (parameter is string)
                {
                    string id = parameter as string;
                    if (string.IsNullOrEmpty(id))
                    {
                        var currentSong = App.Locator.Player.CurrentSong;
                        id = await YouTubeVideo(currentSong.Name, currentSong.ArtistName);
                    }
                    OpenAndLoadInfo(id);
                }

                else if (parameter is WebVideo)
                {
                    var id = parameter as WebVideo;
                    if (id == null) return;
                    OpenAndLoadInfo(id.VideoId);
                }
            }
            catch
            {
                Message = Core.StringMessage.SomethinWentWrong;
            }
        }

        async void OpenAndLoadInfo(string id)
        {
            if (App.Navigator.CurrentPage is VideoInformationPage)
            {
                
            }
            else
            {
                await DispatcherHelper.RunAsync(() =>
                {
                    App.Navigator.GoTo<VideoInformationPage, Utilities.ZoomInTransition>(null);
                });
            }

            viewModelCleared = false;
            VideoInfo = await _misc.GetYVideoFromId(id);


            if (SimilarVideos != null)
            {
                SimilarVideos = null;
                SimilarVideos = new ObservableCollection<Base>();
            }
            else
                SimilarVideos = new ObservableCollection<Base>();


            try
            {
                await Task.Factory.StartNew(async () =>
                {
                    await DispatcherHelper.RunAsync(async () =>
                    {
                        int count = 10;
                        if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
                            count = 20;
                        var o = await _misc.GetRelatedYVideoFromId(count, id);
                        int indexToAdd = -1;
                        foreach (var addedOVideo in o)
                        {
                            indexToAdd++;
                            SimilarVideos.Add(addedOVideo);
                            //if (indexToAdd == 2 || indexToAdd == 10 || indexToAdd == 22 || indexToAdd == 34 || indexToAdd == 49 ||
                            //    indexToAdd == 63 || indexToAdd == 78 || indexToAdd == 88 || indexToAdd == 99)
                            //    SimilarVideos.Insert(indexToAdd, new Advert());
                        }

                        if (o != null) o = null;

                        if (SimilarVideos != null && SimilarVideos.Count > 0)
                            Message = Core.StringMessage.NoMessge;
                        else Message = Core.StringMessage.EmptyMessage;
                    });
                });
            }
            catch
            {
                Message = Core.StringMessage.SomethinWentWrong;
            }    
        }



        public async void VideoClickExecuteAfter()
        {
            try
            {
                await Task.Factory.StartNew(async () =>
                {

                    string url = await GetOnlineUrl(VideoInfo.VideoId);
                    if (string.IsNullOrEmpty(url))
                    {
                        await DispatcherHelper.RunAsync(() =>
                        {
                            SheetUtility.CloseVideoPage();
                            MessageHelpers.ShowError("Url not found.", "Error");
                        });

                    }
                    else
                    {
                        VideoModel videoModel = new VideoModel(url, true);
                        StartPlaybackVideo?.Invoke(this, videoModel);
                    }
                });
            }

            catch
            {
                SheetUtility.CloseVideoPage();
            }
        }
      

        public async Task<string> YouTubeVideo(string title, string artist)
        {
            var query = ((title + " " + artist).Trim()).Trim();
            string url = await App.Locator.MiscallaneousService.GetVideo(query);
            if (string.IsNullOrEmpty(url))
                return string.Empty;
            else
                return url;
        }


        public async Task<string> GetOnlineUrl(string id)
        {
            try
            {
                if(App.Locator.Setting.HdVideoOnOff)
                    return await _videoUrlService.GetUrlAsync(id, true);
                return await _videoUrlService.GetUrlAsync(id, false);
            }
            catch
            {
                return string.Empty;
            }
        }



    }

    public class VideoModel
    {
        public VideoModel(string url,bool isWeb)
        {
            Url = url;
            IsWebVideo = isWeb;
        }

        public string Url { get; set; }
        public bool IsWebVideo { get; set; }
    }
}
