#region
using Musicus.Data.Service.Interfaces;
using Musicus.Helpers;
using Musicus.Utilities;
using Musicus.ViewModel.Mvvm.Commands;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using System.Linq;
using Windows.UI.Xaml.Media;
using Musicus.Core.Utils;
using Windows.UI;
using Musicus.Data.Model.Advertisement;
using System.Collections.ObjectModel;
using Musicus.Data.Model.WebSongs;
using IF.Lastfm.Core.Objects;
using IF.Lastfm.Core.Api.Helpers;
#endregion
namespace Musicus.ViewModel
{
    public class SpotifyArtistViewModel : Mvvm.Others.ViewModelBase
    {
        private readonly ISpotifyService _service;
        private readonly IScrobblerService _scrobbler;

        private LastArtist _artist;
        private bool _isLoading;
        private Command<ItemClickEventArgs> _songClickRelayCommand;
        private ObservableCollection<Base> _topAlbums;
        private ObservableCollection<Base> _topTracks;
        private string _overViewMessage;
        private string _trackMessage;
        private string _albumMessage;
        private string _bio = default(string);

        public SettingViewModel SettingViewModel { get; set; }
        private SolidColorBrush _backgroundBrush;

        private string _name;

        public SpotifyArtistViewModel(ISpotifyService service, IScrobblerService scrobbler, SettingViewModel settingViewModel)
        {
            _service = service;
            _scrobbler = scrobbler;
            _songClickRelayCommand = new Command<ItemClickEventArgs>(SongClickExecute);
            SettingViewModel = settingViewModel;
        }

        public Command<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickRelayCommand; }
        }

        public LastArtist Artist
        {
            get { return _artist; }
            set { Set(ref _artist, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        public ObservableCollection<Base> TopTracks
        {
            get { return _topTracks; }
            set { Set(ref _topTracks, value); }
        }

        public ObservableCollection<Base> TopAlbums
        {
            get { return _topAlbums; }
            set { Set(ref _topAlbums, value); }
        }

        //public LastArtist LastArtist
        //{
        //    get { return _lastArtist; }
        //    set { Set(ref _lastArtist, value); }
        //}


        public string Bio
        {
            get { return _bio; }
            set { Set(ref _bio, value); }
        }





        //public string Text
        //{
        //    get { return _text; }
        //    set { Set(ref _text, value); }
        //}

        public SolidColorBrush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set { Set(ref _backgroundBrush, value); }
        }

        public string TrackMessage
        {
            get { return _trackMessage; }
            set { Set(ref _trackMessage, value); }
        }

        public string OverViewMessage
        {
            get { return _overViewMessage; }
            set { Set(ref _overViewMessage, value); }
        }

        public string AlbumMessage
        {
            get { return _albumMessage; }
            set { Set(ref _albumMessage, value); }
        }

        //public string RelatedMessage
        //{
        //    get { return _relatedMessage; }
        //    set { Set(ref _relatedMessage, value); }
        //}

        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }



        //private async void FirstTime(string name)
        //{
        //    string text = await StorageFileHelper.GetText("Artist");
        //    if ((string.IsNullOrEmpty(Text) || string.IsNullOrWhiteSpace(Text)) && !text.Contains(name.ToUpper()))
        //    {
        //        Text = "FOLLOW";
        //    }

        //    else
        //    {
        //        Text = "FOLLOWING";
        //    }
        //}


        //public async void FollowToggle(string name)
        //{
        //    if (string.IsNullOrEmpty(name)) return;
        //    string text = await StorageFileHelper.GetText("Artist");

        //    if (Text.Equals("FOLLOW"))
        //    {

        //        try
        //        {
        //            if (text.Contains(name.ToUpper()))
        //            {
        //                Text = "FOLLOWING";
        //                return;
        //            }
        //            StorageFileHelper.SaveText(text.Append(name.ToUpper()), "Artist");
        //            Text = "FOLLOWING";
        //        }
        //        catch
        //        {
        //            //ignored
        //        }
        //    }
        //    else if (Text.Equals("FOLLOWING"))
        //    {
        //        try
        //        {
        //            if (text.Contains(name.ToUpper()))
        //            {
        //                text.Replace(name.ToUpper(), string.Empty);
        //                StorageFileHelper.SaveText(text, "Artist");
        //            }
        //            Text = "FOLLOW";
        //        }
        //        catch
        //        {
        //            //ignored
        //        }
        //    }

        //}

        public void ReceivedName(string id)
        {
            BackgroundBrush = new SolidColorBrush(Colors.Transparent);
            Name = "Unknown Artist";
            IsLoading = true;
            if (Artist != null && id == Artist.Name) return;
            LoadData(id);
        }

        public void ClearSpotifyArtistViewModelData()
        {
            if (Artist != null) Artist = null;
            if (TopAlbums != null) TopAlbums = null;
            if (TopTracks != null) TopTracks = null;
            if (Bio != null) Bio = null;
            // if (LastArtist != null) LastArtist = null;
        }

        public void SetMessage(string message)
        {
            // RelatedMessage = message;
            TrackMessage = message;
            AlbumMessage = message;
            OverViewMessage = message;
        }

        private async void LoadData(string id)
        {
            await Mvvm.Dispatcher.DispatcherHelper.RunAsync(async () =>
            {
                SetMessage(Core.StringMessage.LoadingPleaseWait);
                if (!App.Locator.Network.IsActive)
                {
                    SetMessage(Core.StringMessage.NoInternetConnection);
                    return;
                }

                ClearSpotifyArtistViewModelData();

                TopTracks = new ObservableCollection<Base>();
                TopAlbums = new ObservableCollection<Base>();

                try
                {
                    Artist = await _scrobbler.GetDetailArtistByMbid(id);
                    if (Artist == null)
                    {
                        Artist = await _scrobbler.GetDetailArtist(id);
                    }

                    Name = Artist.Name;

                    var track = await _scrobbler.GetArtistTopTracks(Name);
                    var albums = await _scrobbler.GetArtistTopAlbums(Name);




                    if (track != null)
                    {

                        int indexToAdd = -1;
                        foreach (var oSong in track)
                        {
                            indexToAdd++;
                            if (indexToAdd == 9
                                && Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                            {
                                break;
                            }

                            else if (indexToAdd >= 20)
                            {
                                break;
                            }

                            else
                            {
                                TopTracks.Add(Data.Model.WebSongConverter.CreateSong(oSong));
                                if (indexToAdd == 2 || indexToAdd == 10 || indexToAdd == 22 || indexToAdd == 34 || indexToAdd == 49 ||
                                    indexToAdd == 63 || indexToAdd == 78 || indexToAdd == 88 || indexToAdd == 99)
                                    TopTracks.Insert(indexToAdd, new ListAdvert());
                            }

                        }

                    }

                    if (albums != null)
                    {

                        int indexToAdd = -1;
                        foreach (var oAlbum in albums?.Content)
                        {

                            if (indexToAdd == 9 && Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                            {
                                break;
                            }

                            else if (indexToAdd >= 20)
                            {
                                break;
                            }

                            if (!string.IsNullOrEmpty(oAlbum.Mbid) || !string.IsNullOrEmpty(oAlbum.Id))
                            {
                                indexToAdd++;
                                TopAlbums.Add(Data.Model.WebSongConverter.CreateAlbum(oAlbum));
                            }
                        }
                    }

                    if (track != null) track = null;
                    if (albums != null) albums = null;

                }
                catch (Exception)
                {
                    IsLoading = false;
                    SetMessage(Core.StringMessage.SomethinWentWrong);
                    return;
                }

                if (Artist != null && ((TopAlbums != null && TopAlbums.Count > 0)
                   || (TopTracks != null && TopTracks.Count > 0)))
                    OverViewMessage = Core.StringMessage.NoMessge;
                else
                    OverViewMessage = Core.StringMessage.EmptyMessage;

                if (Artist != null && TopAlbums != null && TopAlbums.Count > 0)
                    AlbumMessage = Core.StringMessage.NoMessge;
                else
                    AlbumMessage = Core.StringMessage.EmptyMessage;

                if (Artist != null && TopTracks != null && TopTracks.Count > 0)
                    TrackMessage = Core.StringMessage.NoMessge;
                else
                    TrackMessage = Core.StringMessage.EmptyMessage;

                IsLoading = false;
            });
        }

        public async System.Threading.Tasks.Task LoadBio()
        {
            if (!string.IsNullOrEmpty(Bio)) return;

            Bio = Core.StringMessage.LoadingPleaseWait;

            if (!App.Locator.Network.IsActive)
            {
                Bio = Core.StringMessage.NoInternetConnection;
                return;
            }

            try
            {
                var lastArtist = await App.Locator.ScrobblerService.GetDetailArtist(Artist.Name);
                if (lastArtist != null)
                {
                    string bio = lastArtist.Bio.Content;
                    if (!string.IsNullOrEmpty(bio))
                    {
                        Bio = bio;
                        return;
                    }
                    Bio = Core.StringMessage.EmptyMessage;
                    return;
                }
            }
            catch
            {
                Bio = Core.StringMessage.SomethinWentWrong;
            }

            Bio = Core.StringMessage.SomethinWentWrong;
        }


        private async void SongClickExecute(ItemClickEventArgs item)
        {
            var track = item.ClickedItem;
            if (track is Advert || track is ListAdvert) return;
            await SongSavingHelper.SaveViezTrackLevel1(track as WebSong);
        }

    }
}