
using IF.Lastfm.Core.Objects;
using Musicus.Data.Collection;
using Musicus.Data.Collection.Model;
using Musicus.Data.Model.Advertisement;
using Musicus.Data.Model.WebData;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Service.Interfaces;
using Musicus.Helpers;
using Musicus.ViewModel.Mvvm.Commands;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;


namespace Musicus.ViewModel
{
    public class NowPlayingViewModel : Mvvm.Others.ViewModelBase
    {
        private readonly IScrobblerService _service;
        private readonly ICollectionService _collection;
        private bool _isLoading;
        private Command<ItemClickEventArgs> _songClickRelayCommand;
        private IEnumerable<WebArtist> _simArtists;
        private ObservableCollection<Base> _simTracks;
        
        private readonly CollectionCommandHelper _commands;
        private string _message;

        private bool _similarTrackVisible;
        private bool _similarArtistVisible;

        public NowPlayingViewModel(IScrobblerService service, ICollectionService collection, CollectionCommandHelper commands)
        {
            _service = service;
            _commands = commands;
            _collection = collection;
            _songClickRelayCommand = new Command<ItemClickEventArgs>(SongClickExecute);
            SimilarTracks = new ObservableCollection<Base>();
        }

        public Command<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickRelayCommand; }
        }


        public ObservableCollection<Base> SimilarTracks
        {
            get { return _simTracks; }
            set { Set(ref _simTracks, value); }
        }

        public IEnumerable<WebArtist> SimilarArtists
        {
            get { return _simArtists; }
            set { Set(ref _simArtists, value); }
        }

       
        public bool SimilarTrackVisible
        {
            get { return _similarTrackVisible; }
            set { Set(ref _similarTrackVisible, value); }
        }

        public bool SimilarArtistVisible
        {
            get { return _similarArtistVisible; }
            set { Set(ref _similarArtistVisible, value); }
        }


       
        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }

        public string Message
        {
            get { return _message; }
            set { Set(ref _message, value); }
        }


        public CollectionCommandHelper Commands
        {
            get { return _commands; }
        }

        private async void SongClickExecute(ItemClickEventArgs item)
        {
            await SongSavingHelper.SaveViezTrackLevel1(item.ClickedItem as WebSong);
        }


        public void ClearNowPlayingViewModelData()
        {
            if (SimilarArtists != null) SimilarArtists = null;
            if (SimilarTracks != null) SimilarTracks = null;
            SimilarArtistVisible = SimilarTrackVisible = false;
        }


        public async void LoadTopTracks(string songName, string similarTrackArtistName, int id)
        {
            await DispatcherHelper.RunAsync(async() =>
            {
                int count = 10;
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
                    count = 20;

                ClearNowPlayingViewModelData();

                Message = Core.StringMessage.LoadingPleaseWait;
                IsLoading = true;
                SimilarTrackVisible = true;
                SimilarArtistVisible = false;
                // viewModelCleared = false;

                SimilarTracks = new ObservableCollection<Base>();

                try
                {                
                    var webResults = await _service.GetSimilarTracksAsync(songName, similarTrackArtistName, count);
                    
                    if (webResults == null || webResults.Songs.Count < 1)
                    {
                        SimilarTrackVisible = false;
                        SimilarArtistVisible = true;
                        var o = await _service.GetSimilarArtistsAsync(similarTrackArtistName, 10);
                        SimilarArtists = o?.Artists;
                        if (o != null) o = null;
                    }
                    else
                    {
                        int indexToAdd = -1;
                        foreach (var simpleTrack in webResults.Songs)
                        {
                            indexToAdd++;
                            SimilarTracks.Add(simpleTrack);
                            if (indexToAdd == 2 || indexToAdd == 10 || indexToAdd == 22 || indexToAdd == 34 || indexToAdd == 49 ||
                                indexToAdd == 63 || indexToAdd == 78 || indexToAdd == 88 || indexToAdd == 99)
                                SimilarTracks.Insert(indexToAdd, new ListAdvert());
                        }
                    }

                    if (webResults != null) webResults = null;
                }
                catch (Exception)
                {
                    Message = Core.StringMessage.SomethinWentWrong;
                }

                if ((SimilarArtists != null && SimilarArtists.Count() > 0) 
                || (SimilarTracks != null && SimilarTracks.Count > 0))
                    Message = Core.StringMessage.NoMessge;
                else
                    Message = Core.StringMessage.EmptyMessage;

                IsLoading = false;
            });
        }
    }
}