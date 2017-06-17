#region

using Musicus.Data.Model.Advertisement;
using Musicus.Data.Model.WebData;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Service.Interfaces;
using Musicus.Data.Spotify.Models;
using Musicus.Helpers;
using Musicus.Utilities;
using Musicus.ViewModel.Mvvm.Commands;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using IF.Lastfm.Core.Objects;


#endregion

namespace Musicus.ViewModel
{
    public class SpotifyAlbumViewModel : Mvvm.Others.ViewModelBase
    {
        private readonly ISpotifyService _spotify;
        private readonly IScrobblerService _service;
        private readonly IMiscellaneousService _misc;
        private LastAlbum _album;
        private WebAlbum _deezerAlbum;
        private ObservableCollection<Base> _deezerTracks, _tracks;
        private bool _isLoading;
        private Command<ItemClickEventArgs> _songClickRelayCommand;
        //private ObservableCollection<SimpleTrack> _tracks;
        private string _message;
        private readonly CollectionCommandHelper _commands;

        private string _name;

        public SpotifyAlbumViewModel(ISpotifyService spotify,
                IScrobblerService service,
                IMiscellaneousService misc,
                CollectionCommandHelper commands)
        {
            _service = service;
            _misc = misc;
            _commands = commands;
            _songClickRelayCommand = new Command<ItemClickEventArgs>(SongClickExecute);
        }

        public LastAlbum Album
        {
            get { return _album; }
            set { Set(ref _album, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        public WebAlbum DeezerAlbum
        {
            get { return _deezerAlbum; }
            set { Set(ref _deezerAlbum, value); }
        }

        public ObservableCollection<Base> Tracks
        {
            get { return _tracks; }
            set { Set(ref _tracks, value); }
        }

        public ObservableCollection<Base> DeezerTracks
        {
            get { return _deezerTracks; }
            set { Set(ref _deezerTracks, value); }
        }

        public Command<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickRelayCommand; }
        }


        public CollectionCommandHelper Commands
        {
            get { return _commands; }
        }

        public string Message
        {
            get { return _message; }
            set { Set(ref _message, value); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }

        public void ReceivedId(string id)
        {
            Name = "Unknown Album";
            if (Album != null && id == Album.Id) return;
            LoadData(id);
        }

        public void ReceivedId(WebAlbum _webAlbum)
        {
            Name = "Unknown Album";
            if (Album != null && _webAlbum.Id == Album.Id) return;
            LoadDataDeezer(_webAlbum.Id);
        }


        public void ClearSpotifyAlbumViewModelData()
        {
            if (Album != null) Album = null;
            if (Tracks != null) Tracks = null;
            if (DeezerAlbum != null) DeezerAlbum = null;
            if (DeezerTracks != null) DeezerTracks = null;
        }


        private async void LoadData(string id)
        {
            Message = Core.StringMessage.LoadingPleaseWait;
            if (!App.Locator.Network.IsActive)
            {
                Message = Core.StringMessage.NoInternetConnection;
                return;
            }

            ClearSpotifyAlbumViewModelData();

            Tracks = new ObservableCollection<Base>();

            IsLoading = true;

            try
            {
                var o = await _service.GetDetailAlbumByMbid(id);
                int indexToAdd = -1;

                foreach (var simpleTrack in o?.Tracks)
                {
                    simpleTrack.ArtistName = o.ArtistName;
                    indexToAdd++;
                    Tracks.Add(Data.Model.WebSongConverter.CreateSong(simpleTrack as LastTrack));
                    if (indexToAdd == 2 || indexToAdd == 10 || indexToAdd == 22 || indexToAdd == 34 || indexToAdd == 49 ||
                        indexToAdd == 63 || indexToAdd == 78 || indexToAdd == 88 || indexToAdd == 99)
                        Tracks.Insert(indexToAdd, new ListAdvert());
                }


                Name = o.Name;
                Album = o;

            }
            catch
            {
                Message = Core.StringMessage.SomethinWentWrong;
            }

            if (Album != null && (Tracks != null && Tracks.Count > 0))
                Message = Core.StringMessage.NoMessge;
            else
                Message = Core.StringMessage.EmptyMessage;
            IsLoading = false;
        }

        private async void LoadDataDeezer(string id)
        {
            Message = Core.StringMessage.LoadingPleaseWait;
            if (!App.Locator.Network.IsActive)
            {
                Message = Core.StringMessage.NoInternetConnection;
                return;
            }

            ClearSpotifyAlbumViewModelData();

            DeezerTracks = new ObservableCollection<Base>();

            IsLoading = true;

            try
            {
                var o = await _misc.GetDeezerAlbumSongsAsync(id);
                string artist = o.ArtistName;
                int indexToAdd = -1;
                foreach (var simpleTrack in o.Tracks)
                {
                    simpleTrack.Artist = artist;
                    indexToAdd++;
                    DeezerTracks.Add(simpleTrack);
                    if (indexToAdd == 2 || indexToAdd == 10 || indexToAdd == 22 || indexToAdd == 34 || indexToAdd == 49 ||
                        indexToAdd == 63 || indexToAdd == 78 || indexToAdd == 88 || indexToAdd == 99)
                        DeezerTracks.Insert(indexToAdd, new ListAdvert());
                }

                Name = o.Title;

                DeezerAlbum = o;
            }
            catch
            {
                Message = Core.StringMessage.SomethinWentWrong;
            }

            if (DeezerAlbum != null && (DeezerTracks != null && DeezerTracks.Count > 0))
                Message = Core.StringMessage.NoMessge;
            else
                Message = Core.StringMessage.EmptyMessage;
            IsLoading = false;
        }

        private async void SongClickExecute(ItemClickEventArgs item)
        {
            var track = item.ClickedItem;
            if (track is ListAdvert || track is Advert) return;
            if (track is WebSong)
                await SongSavingHelper.SaveViezTrackLevel1(track as WebSong);
            //else
            //    await SongSavingHelper.SaveSpotifyTrackLevel2((SimpleTrack)item.ClickedItem, Album);
        }
    }
}