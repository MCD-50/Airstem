using IF.Lastfm.Core.Objects;
using Musicus.Data.Collection;
using Musicus.Data.Collection.Model;
using Musicus.Data.Model.Advertisement;
using Musicus.Data.Model.WebData;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Service.Interfaces;
using Musicus.Helpers;
using Musicus.PlayerHelpers;
using Musicus.Utilities;
using Musicus.ViewModel.Mvvm.Commands;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Musicus.ViewModel
{
    public class MainViewModel : Mvvm.Others.ViewModelBase
    {
        private readonly CollectionCommandHelper _commands;
        private bool _isLoading;
        private readonly ICollectionService _service;
        private readonly IScrobblerService _scroble;
        private readonly IMiscellaneousService _misc;

        private ObservableCollection<Base> _newReleases, _popular, _deezerAlbum;
        private List<Base> _smallPopular, _smallDeezerAlbum;

        private IEnumerable<Song> _history;
        private IEnumerable<WebArtist> _lastArtistsCollection;

        public PlayerViewModel PlayerModel { get; set; }

        private string _message = default(string);
        private string _whatsNewMessage = default(string);

        private Command<ItemClickEventArgs> _songClickCommand;

        public MainViewModel(ICollectionService service, IMiscellaneousService misc, CollectionCommandHelper commands, Data.Service.Interfaces.IScrobblerService scroble, 
             PlayerViewModel _playerModel)
        {
            _service = service;
            _commands = commands;
            _scroble = scroble;
            _misc = misc;

            PlayerModel = _playerModel;
            Message = Core.StringMessage.LoadingPleaseWait;
            WhatsNewMessage = Core.StringMessage.LoadingPleaseWait;

            _songClickCommand = new Command<ItemClickEventArgs>(SongClickExecute);
            _service.LibraryLoaded += _service_LibraryLoaded;
        }

        public Command<ItemClickEventArgs> SongClickCommand
        {
            get { return _songClickCommand; }
        }

        public string Message
        {
            get { return _message; }
            set { Set(ref _message, value); }
        }

        public string WhatsNewMessage
        {
            get { return _whatsNewMessage; }
            set { Set(ref _whatsNewMessage, value); }
        }

        public CollectionCommandHelper Commands
        {
            get { return _commands; }
        }

        public IEnumerable<WebArtist> TopArtists
        {
            get { return _lastArtistsCollection; }
            set { Set(ref _lastArtistsCollection, value); }
        }

        public ObservableCollection<Base> TopAlbums
        {
            get { return _deezerAlbum; }
            set { Set(ref _deezerAlbum, value); }
        }

        public ObservableCollection<Base> NewReleases
        {
            get { return _newReleases; }
            set { Set(ref _newReleases, value); }
        }


        public ObservableCollection<Base> Popular
        {
            get { return _popular; }
            set { Set(ref _popular, value); }
        }

        public List<Base> SmallTopAlbums
        {
            get { return _smallDeezerAlbum; }
            set { Set(ref _smallDeezerAlbum, value); }
        }

        public List<Base> SmallPopular
        {
            get { return _smallPopular; }
            set { Set(ref _smallPopular, value); }
        }


        public IEnumerable<Song> History
        {
            get { return _history; }
            set { Set(ref _history, value); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }


        private void _service_LibraryLoaded(object sender, EventArgs e)
        {
            _service.LibraryLoaded -= _service_LibraryLoaded;
            SongColletionChanged();
        }


        public void SongColletionChanged()
        {
            _service.Songs.CollectionChanged -= SongsOnCollectionChanged;
            _service.Songs.CollectionChanged += SongsOnCollectionChanged;
            SongsOnCollectionChanged(null, null);
        }

        public void LoadHistory()
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
                History = _service.GetLastPlayed(20);
            else
                History = _service.GetLastPlayed(10);
        }


        private bool _isLoaded = false;
        private async void SongsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                LoadHistory();
                App.Locator.DownloadHelper.LoadAsync();
            }
            catch (Exception)
            {
                Message = Core.StringMessage.SomethinWentWrong;
            }

            if (History != null && History.Count() > 0)
                Message = Core.StringMessage.NoMessge;
            else
                Message = Core.StringMessage.EmptyMessage;

            if (!_isLoaded)
            {
                await LoadWhatsNewData();
                _isLoaded = true;
            }
        }


        int loadedCount = 0;
        int countToLoad = 4;
        private async Task LoadWhatsNewData()
        {
            if (!App.Locator.Network.IsActive)
            {
                WhatsNewMessage = Core.StringMessage.NoInternetConnection;
                return;
            }

            IsLoading = true;

            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
                countToLoad = 10;

            try
            {
                await LoadDataForAlbumAndTracks(countToLoad);
                var _lastArtist = await _scroble.GetTopArtistsAsync(limit: 5);
                if (_lastArtist != null && _lastArtist.Artists != null && _lastArtist.Artists.Count > 0)
                     TopArtists = _lastArtist.Artists;

                if (_lastArtist != null) _lastArtist = null;

                loadedCount++;

            }
            catch (Exception)
            {
                WhatsNewMessage = Core.StringMessage.SomethinWentWrong;
            }

            if(loadedCount == 4 && TopArtists != null)
                WhatsNewMessage = Core.StringMessage.NoMessge;
            else
                WhatsNewMessage = Core.StringMessage.EmptyMessage;

            IsLoading = false;
        }



        public async Task LoadDataForAlbumAndTracks(int count = 5)
        {        
            Popular = new ObservableCollection<Base>();
            NewReleases = new ObservableCollection<Base>();
            TopAlbums = new ObservableCollection<Base>();
      
            try
            {
                var o = await _misc.GetTopTracksAsync(count);//GetNewRelesedSong(count);
                if (o != null && o.Songs.Count > 0)
                {
                    int indexToAdd = -1;
                    foreach (var oSong in o.Songs)
                    {
                        indexToAdd++;
                        Popular.Add(oSong);
                        if (indexToAdd == 2 || indexToAdd == 10 || indexToAdd == 22 || indexToAdd == 34 || indexToAdd == 49 ||
                            indexToAdd == 63 || indexToAdd == 78 || indexToAdd == 88 || indexToAdd == 99)
                            Popular.Insert(indexToAdd, new Advert());
                    }
                }
                else
                {
                    Popular = new ObservableCollection<Base>();
                }

                if (o != null) o = null;

                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
                    SmallPopular = Popular.Take(3).ToList();
                loadedCount++;
            }
            catch
            {
                //ignored
            }

            await Task.Delay(50);

            try
            {
                var newR = await _misc.GetNewRelesedSong(count);
                if (newR != null && newR.Count > 0)
                {
                    int indexToAdd = -1;
                    foreach (var oSong in newR)
                    {
                        indexToAdd++;
                        NewReleases.Add(oSong);
                        if (indexToAdd == 2 || indexToAdd == 10 || indexToAdd == 22 || indexToAdd == 34 || indexToAdd == 49 ||
                            indexToAdd == 63 || indexToAdd == 78 || indexToAdd == 88 || indexToAdd == 99)
                            NewReleases.Insert(indexToAdd, new ListAdvert());
                    }
                 }

                else
                {
                    NewReleases = new ObservableCollection<Base>();
                }

                if (newR != null) newR = null;

                loadedCount++;
            }
            catch
            {
                //ignored          
            }

            try
            {
                var o = await _misc.GetTopAlbumsAsync(count);
                if (o!= null && o.Albums != null && o.Albums.Count > 0)
                {
                    int indexToAdd = -1;
                    foreach (var oAlbum in o.Albums)
                    {
                        indexToAdd++;
                        TopAlbums.Add(oAlbum);
                        if (indexToAdd == 2 || indexToAdd == 10 || indexToAdd == 22 || indexToAdd == 34 || indexToAdd == 49 ||
                            indexToAdd == 63 || indexToAdd == 78 || indexToAdd == 88 || indexToAdd == 99)
                            TopAlbums.Insert(indexToAdd, new Advert());
                    }
                }
                else
                {
                    TopAlbums = new ObservableCollection<Base>();
                }


                if (o != null) o = null;

                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
                    SmallTopAlbums = TopAlbums.Take(3).ToList();

                loadedCount++;
            }

            catch
            {
                //ignored
            }        
        }

        private async void SongClickExecute(ItemClickEventArgs obj)
        {
            var track = obj.ClickedItem;
            if (track is Advert || track is ListAdvert) return;
     
            if (Popular.Contains(track as WebSong))
                await SongSavingHelper.SaveViezTrackLevel1(track as WebSong);
            else
                SheetUtility.OpenEditTrackMetadataPage(track as WebSong);
        }


    }
}