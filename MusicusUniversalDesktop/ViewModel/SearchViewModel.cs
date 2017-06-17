#region
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using Musicus.Core.Common;
using Musicus.Core.WinRt.Common;
using Musicus.Data.Collection;
using Musicus.Data.Collection.Model;
using Musicus.Data.Model.Advertisement;
using Musicus.Data.Model.Deezer;
using Musicus.Data.Model.WebData;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Service.Interfaces;
using Musicus.Data.Spotify.Models;
using Musicus.Helpers;
using Musicus.Utilities;
using Musicus.ViewModel.Mvvm.Commands;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
#endregion

namespace Musicus.ViewModel
{
    public class SearchViewModel : Mvvm.Others.ViewModelBase
    {
        private readonly IScrobblerService _service;
        private readonly ISpotifyService _spotify;
        private readonly IDeezerService _deezer;
        private readonly IMiscellaneousService _misc;

        private bool _isLoading;

        private IncrementalObservableCollection<Base> _albumsCollection, _tracks;

        private List<LastArtist> _artistsCollection;

        private IEnumerable<WebVideo> _onlineVideos;

        private readonly CollectionCommandHelper _commands;
        private string _trackMessage;
        private string _albumArtistMessage;
        private string _videoMessage;
        private bool _artistHeader;

        public Command<ItemClickEventArgs> VideoClickCommand { get; set; }
        public Command<KeyRoutedEventArgs> KeyDownRelayCommand { get; set; }
        public Command<ItemClickEventArgs> SongClickRelayCommand { get; set; }


        public SearchViewModel(IScrobblerService service,
                ISpotifyService spotify,
                IMiscellaneousService misc,
                IDeezerService deezer,
                CollectionCommandHelper commands)
        {
            _spotify = spotify;
            _service = service;
            _misc = misc;
            _deezer = deezer;
            _commands = commands;



            KeyDownRelayCommand = new Command<KeyRoutedEventArgs>(KeyDownExecute);
            SongClickRelayCommand = new Command<ItemClickEventArgs>(SongClickExecute);
            VideoClickCommand = new Command<ItemClickEventArgs>(VideoClickExecute);
        }

        public Command<string> SearchCommand { get; }


        public CollectionCommandHelper Commands
        {
            get { return _commands; }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }

        public string TrackMessage
        {
            get { return _trackMessage; }
            set { Set(ref _trackMessage, value); }
        }

        public string VideoMessage
        {
            get { return _videoMessage; }
            set { Set(ref _videoMessage, value); }
        }

        public string AlbumArtistMessage
        {
            get { return _albumArtistMessage; }
            set { Set(ref _albumArtistMessage, value); }
        }

        public bool ArtistHeader
        {
            get { return _artistHeader; }
            set { Set(ref _artistHeader, value); }
        }

        private async void KeyDownExecute(KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && e.KeyStatus.RepeatCount == 1)
            {
                ((PivotItem)((ScrollViewer)((StackPanel)((TextBox)e.OriginalSource).Parent).Parent).Parent).Focus(FocusState.Keyboard);
                var term = ((TextBox)e.OriginalSource).Text.Trim();
                ((TextBox)e.OriginalSource).IsEnabled = false;
                await DispatcherHelper.RunAsync(async () =>
                {
                    await SearchAsync(term);
                    ((TextBox)e.OriginalSource).IsEnabled = true;
                });
            }
        }


        public IEnumerable<WebVideo> OnlineVideos
        {
            get { return _onlineVideos; }
            set { Set(ref _onlineVideos, value); }
        }


        public List<LastArtist> Artists
        {
            get { return _artistsCollection; }
            set { Set(ref _artistsCollection, value); }
        }


        public IncrementalObservableCollection<Base> Tracks
        {
            get { return _tracks; }
            set { Set(ref _tracks, value); }
        }



        public IncrementalObservableCollection<Base> Albums
        {
            get { return _albumsCollection; }
            set { Set(ref _albumsCollection, value); }
        }


        public void ClearSearchViewModelData()
        {
            if (Albums != null) Albums = null;
            if (Artists != null) Artists = null;
            if (Tracks != null) Tracks = null;
            if (OnlineVideos != null) OnlineVideos = null;

            if (_tracksResponse != null) _tracksResponse = null;
            if (_albumsResponse != null) _albumsResponse = null;
            if (_artistsResponse != null) _artistsResponse = null;

            ArtistHeader = false;
        }

        public void SetMessage(string term)
        {
            TrackMessage = term;
            AlbumArtistMessage = term;
            VideoMessage = term;
        }

        private PageResponse<LastTrack> _tracksResponse;
        private PageResponse<LastAlbum> _albumsResponse;
        private PageResponse<LastArtist> _artistsResponse;


        public async Task SearchAsync(string term)
        {

            if (string.IsNullOrEmpty(term))
            {
                ToastManager.ShowError("Please enter something.");
                return;
            }

            SetMessage(Core.StringMessage.LoadingPleaseWait);
            if (!App.Locator.Network.IsActive)
            {
                SetMessage(Core.StringMessage.NoInternetConnection);
                return;
            }


            foundInViez = false;
            IsLoading = true;

            try
            {
                ClearSearchViewModelData();

                var tasks = new List<Task>
                {
                    Task.Run(
                        async () =>
                        {
                            _tracksResponse = await _service.SearchTracksAsync(term, limit:20);
                            if (_tracksResponse != null)
                            {
                                await DispatcherHelper.RunAsync(
                                () =>
                                {

                                    int indexToAdd = -1;
                                    Tracks = CreateLastIncrementalCollection(
                                        "songs",
                                        () => _tracksResponse,
                                        tracks => _tracksResponse = tracks,
                                        async i => await _service.SearchTracksAsync(term, page: i, limit:20));


                                    foreach (var addedOTrack in _tracksResponse.Content)
                                    {
                                        indexToAdd++;

                                        Tracks.Add(Data.Model.WebSongConverter.CreateSong(addedOTrack));
                                         if (indexToAdd == 2 || indexToAdd == 10 || indexToAdd == 22 || indexToAdd == 34 || indexToAdd == 49 ||
                                             indexToAdd == 63 || indexToAdd == 78 || indexToAdd == 88 || indexToAdd == 99)
                                            Tracks.Insert(indexToAdd, new ListAdvert());
                                    }

                                });
                            }
                        }),



                        Task.Run(
                        async () =>
                        {
                            _albumsResponse = await _service.SearchAlbumsAsync(term, limit:20);
                            if (_albumsResponse != null)
                            {

                            await DispatcherHelper.RunAsync(
                                () =>
                                {
                                    int indexToAdd = -1;

                                    Albums = CreateLastIncrementalCollection(
                                        "albums",
                                        () => _albumsResponse,
                                        albums => _albumsResponse = albums,
                                        async i => await _service.SearchAlbumsAsync(term, page: i, limit:20));

                                    foreach (var addedOAlbum in _albumsResponse?.Content)
                                    {

                                        if(!string.IsNullOrEmpty(addedOAlbum.Mbid) || !string.IsNullOrEmpty(addedOAlbum.Id))
                                        {
                                            indexToAdd++;
                                             Albums.Add(Data.Model.WebSongConverter.CreateAlbum(addedOAlbum));
                                             if (indexToAdd == 2 || indexToAdd == 10 || indexToAdd == 22 || indexToAdd == 34 || indexToAdd == 49 ||
                                                indexToAdd == 63 || indexToAdd == 78 || indexToAdd == 88 || indexToAdd == 99)
                                                Albums.Insert(indexToAdd, new Advert());
                                        }
                                    }
                                });
                            }
                        }),


                     Task.Run(
                        async () =>
                        {
                            _artistsResponse = await _service.SearchArtistsAsync(term, limit:1);
                            if (_artistsResponse != null && _artistsResponse.Content.Count > 0)
                                await DispatcherHelper.RunAsync(()=> Artists = new List<LastArtist>(_artistsResponse.Content));
                        }),


                     Task.Run(
                         async () =>
                         {
                               try
                               {
                                  await Task.Factory.StartNew(async () =>
                                    {
                                      await DispatcherHelper.RunAsync(async () =>
                                         {
                                             if (App.Locator.Setting.VideoOnOff)
                                             {
                                                 VideoMessage = Core.StringMessage.LoadingPleaseWait;
                                                 OnlineVideos = await _misc.GetSearchedVideos(20, term);
                                                 if (OnlineVideos != null && OnlineVideos.CountOrDefault() > 0)
                                                     VideoMessage = Core.StringMessage.NoMessge;
                                                 else VideoMessage = Core.StringMessage.EmptyMessage;
                                             }
                                             else
                                             {
                                                 VideoMessage = Core.StringMessage.NoMessge;
                                             }

                                          });
                                      });
                                }

                                catch
                                {
                                    VideoMessage = Core.StringMessage.SomethinWentWrong;
                                }
                         })

                };
                await Task.WhenAll(tasks);
            }
            catch
            {
                SetMessage(Core.StringMessage.SomethinWentWrong);
            }

            if (Tracks == null || Tracks.Count < 1)
                await FindOnViez(term);
            else
                TrackMessage = (Tracks != null && Tracks.Count > 0) ? Core.StringMessage.NoMessge : Core.StringMessage.EmptyMessage;

            if ((Albums != null && Albums.Count > 0) && (Artists != null && Artists.Count > 0))
                ArtistHeader = true;
            else ArtistHeader = false;

            AlbumArtistMessage = ((Albums != null && Albums.Count > 0) || (Artists != null && Artists.Count > 0)) ? Core.StringMessage.NoMessge : Core.StringMessage.EmptyMessage;

            IsLoading = false;
        }


        //private PageResponse<LastTrack> _lastTrackResponse;

        //public async Task FindOnLastFm(string term)
        //{
        //    if (Tracks == null)
        //        Tracks = new IncrementalObservableCollection<Base>();
        //    else
        //        Tracks.Clear();
        //    try
        //    {
        //        _lastTrackResponse = await _service.SearchTracksAsync(term);
        //        if (_lastTrackResponse != null)
        //        {
        //            int indexToAdd = -1;

        //            Tracks = CreateLastIncrementalCollection(
        //               () => _lastTrackResponse,
        //               artists => _lastTrackResponse = artists,
        //               async i => await _service.SearchTracksAsync(term, i));

        //            foreach (var addedOSong in _lastTrackResponse.Content)
        //            {
        //                indexToAdd++;
        //                Tracks.Add(Data.Model.WebSongConverter.CreateSong(addedOSong));
        //                if (indexToAdd == 2 || indexToAdd == 10 || indexToAdd == 22 || indexToAdd == 34 || indexToAdd == 49 ||
        //                    indexToAdd == 63 || indexToAdd == 78 || indexToAdd == 88 || indexToAdd == 99)
        //                    Tracks.Insert(indexToAdd, new ListAdvert());
        //            }
        //        }

        //    }

        //    catch
        //    {
        //        TrackMessage = Core.StringMessage.SomethinWentWrong;
        //    }

        //    if (Tracks == null || Tracks.Count <= 0)
        //        await FindOnViez(term);
        //    else
        //        TrackMessage = (Tracks != null && Tracks.Count > 0) ? Core.StringMessage.NoMessge : Core.StringMessage.EmptyMessage;
        //}


        private bool foundInViez;

        public async Task FindOnViez(string term)
        {
            if (Tracks == null)
                Tracks = new IncrementalObservableCollection<Base>();
            else
                Tracks.Clear();

            List<WebSong> _allTracks;
            try
            {
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                    _allTracks = await _misc.GetViezResultsAsync(term, 20);
                else
                    _allTracks = await _misc.GetViezResultsAsync(term, 30);
                if (_allTracks != null)
                {
                    int indexToAdd = -1;
                    foreach (var oSong in _allTracks)
                    {
                        indexToAdd++;
                        Tracks.Add(oSong);
                        if (indexToAdd == 2 || indexToAdd == 10 || indexToAdd == 22 || indexToAdd == 34 || indexToAdd == 49 ||
                            indexToAdd == 63 || indexToAdd == 78 || indexToAdd == 88 || indexToAdd == 99)
                            Tracks.Insert(indexToAdd, new ListAdvert());
                    }
                }

                TrackMessage = (Tracks != null && Tracks.Count > 0) ? Core.StringMessage.NoMessge : Core.StringMessage.EmptyMessage;

            }
            catch (Exception)
            {
                TrackMessage = Core.StringMessage.SomethinWentWrong;
            }

            foundInViez = true;
        }

        private IncrementalObservableCollection<Base> CreateIncrementalCollection<T>(
            string type,
            Func<DeezerPageResponse<T>> getPageResponse,
            Action<DeezerPageResponse<T>> setPageResponse,
            Func<int, Task<DeezerPageResponse<T>>> searchFunc) where T : new()
        {
            var collection = new IncrementalObservableCollection<Base>
            {
                HasMoreItemsFunc = () =>
                {
                    var page = getPageResponse();
                    if (page != null)
                    {
                        return !string.IsNullOrEmpty(page.next);
                    }
                    return false;
                }
            };

            collection.LoadMoreItemsFunc = count =>
            {
                Func<Task<LoadMoreItemsResult>> taskFunc = async () =>
                {
                    try
                    {
                        if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                        {
                            if (type.Equals("songs") && Tracks != null && Tracks.Count >= 70)
                            {
                                IsLoading = false;
                                setPageResponse(null);
                                return new LoadMoreItemsResult { Count = 0 };
                            }

                            if (type.Equals("albums") && Albums != null && Albums.Count >= 20)
                            {
                                IsLoading = false;
                                setPageResponse(null);
                                return new LoadMoreItemsResult { Count = 0 };
                            }
                        }

                        else
                        {
                            if (type.Equals("songs") && Tracks != null && Tracks.Count >= 120)
                            {
                                IsLoading = false;
                                setPageResponse(null);
                                return new LoadMoreItemsResult { Count = 0 };
                            }

                            if (type.Equals("albums") && Albums != null && Albums.Count >= 40)
                            {
                                IsLoading = false;
                                setPageResponse(null);
                                return new LoadMoreItemsResult { Count = 0 };
                            }
                        }

                        if (type.Equals("artists") && Artists != null && Artists.Count >= 1)
                        {
                            IsLoading = false;
                            setPageResponse(null);
                            return new LoadMoreItemsResult { Count = 0 };
                        }


                        IsLoading = true;
                        var itemsAdded = 0;

                        var pageResp = getPageResponse();

                        var next = pageResp.next.Split('?')[1]?.Split('=');
                        int page = int.Parse(next[next.Length - 1]);


                        var resp = await searchFunc(page);


                        if (resp != null)
                        {
                            foreach (var item in resp.data)
                            {

                                if (item is DeezerAlbum)
                                {
                                    collection.Add(Data.Model.WebSongConverter.CreateDeezerAlbum(item as DeezerAlbum));
                                }
                                else if (item is DeezerSong)
                                    collection.Add(Data.Model.WebSongConverter.CreateDeezerSong(item as DeezerSong));
                            }

                            setPageResponse(resp);
                            itemsAdded = resp.data.Count;
                        }
                        else
                        {
                            setPageResponse(null);
                        }

                        IsLoading = false;

                        return new LoadMoreItemsResult { Count = (uint)itemsAdded };
                    }
                    catch
                    {
                        IsLoading = false;
                        setPageResponse(null);
                        ToastManager.ShowError("Error in loading more results.");
                        return new LoadMoreItemsResult { Count = 0 };
                    }
                };
                var loadMorePostsTask = taskFunc();
                return loadMorePostsTask.AsAsyncOperation();
            };

            return collection;
        }



        public IncrementalObservableCollection<Base> CreateLastIncrementalCollection<T>(
            string type,
            Func<PageResponse<T>> getPageResponse,
            Action<PageResponse<T>> setPageResponse,
            Func<int, Task<PageResponse<T>>> searchFunc) where T : new()
        {

            var collection = new IncrementalObservableCollection<Base>
            {
                HasMoreItemsFunc = () =>
                {
                    var page = getPageResponse();
                    if (page != null)
                    {
                        return page.Page < page.TotalPages;
                    }

                    return false;
                }
            };

            collection.LoadMoreItemsFunc = count =>
            {
                Func<Task<LoadMoreItemsResult>> taskFunc = async () =>
                {
                    try
                    {

                        if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                        {
                            if (type.Equals("songs") && Tracks != null && Tracks.Count >= 70)
                            {
                                IsLoading = false;
                                setPageResponse(null);
                                return new LoadMoreItemsResult { Count = 0 };
                            }

                            if (type.Equals("albums") && Albums != null && Albums.Count >= 20)
                            {
                                IsLoading = false;
                                setPageResponse(null);
                                return new LoadMoreItemsResult { Count = 0 };
                            }
                        }

                        else
                        {
                            if (type.Equals("songs") && Tracks != null && Tracks.Count >= 120)
                            {
                                IsLoading = false;
                                setPageResponse(null);
                                return new LoadMoreItemsResult { Count = 0 };
                            }

                            if (type.Equals("albums") && Albums != null && Albums.Count >= 40)
                            {
                                IsLoading = false;
                                setPageResponse(null);
                                return new LoadMoreItemsResult { Count = 0 };
                            }
                        }

                        if (type.Equals("artists") && Artists != null && Artists.Count >= 1)
                        {
                            IsLoading = false;
                            setPageResponse(null);
                            return new LoadMoreItemsResult { Count = 0 };
                        }


                        IsLoading = true;
                        var itemsAdded = 0;
                        var pageResp = getPageResponse();
                        var resp = await searchFunc(pageResp.Page + 1);

                        if (resp != null)
                        {
                            foreach (var item in resp.Content)
                            {

                                if (item is LastAlbum)
                                {
                                    collection.Add(Data.Model.WebSongConverter.CreateAlbum(item as LastAlbum));
                                }
                                else if (item is LastTrack)
                                    collection.Add(Data.Model.WebSongConverter.CreateSong(item as LastTrack));
                            }

                            setPageResponse(resp);
                            itemsAdded = resp.Content.Count;
                        }
                        else
                        {
                            setPageResponse(null);
                        }


                        IsLoading = false;
                        return new LoadMoreItemsResult { Count = (uint)itemsAdded };
                    }
                    catch
                    {
                        IsLoading = false;
                        setPageResponse(null);
                        ToastManager.ShowError("Error in loading more results.");
                        return new LoadMoreItemsResult { Count = 0 };
                    }
                };
                var loadMorePostsTask = taskFunc();
                return loadMorePostsTask.AsAsyncOperation();
            };

            return collection;
        }


        private async void SongClickExecute(ItemClickEventArgs item)
        {
            var track = item.ClickedItem;
            if (track is Advert || track is ListAdvert) return;

            if (track is WebSong)
            {
                if (foundInViez)
                    SheetUtility.OpenEditTrackMetadataPage(track as WebSong);
                else
                    await SongSavingHelper.SaveViezTrackLevel1(track as WebSong);
            }

            else if (track is FullTrack)
                await SongSavingHelper.SaveSpotifyTrackLevel1(track as FullTrack);
        }


        void VideoClickExecute(ItemClickEventArgs obj)
        {
            App.Locator.VideoPlayer.InvokeOnline(obj.ClickedItem);
        }

    }
}