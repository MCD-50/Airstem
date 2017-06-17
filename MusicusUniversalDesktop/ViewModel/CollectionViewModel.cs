using Musicus.Core.Common;
using Musicus.Data.Collection;
using Musicus.Data.Collection.Model;
using Musicus.Data.Model.Advertisement;
using Musicus.Data.Model.WebData;
using Musicus.Data.Service.Interfaces;
using Musicus.Helpers;
using Musicus.ViewModel.Mvvm.Commands;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization.Collation;
using Windows.UI.Xaml.Controls;

namespace Musicus.ViewModel
{
    public class CollectionViewModel : Mvvm.Others.ViewModelBase
    {
        private readonly CollectionCommandHelper _commands;
        private readonly ICollectionService _service;
        private readonly IMiscellaneousService _misc;

        private ObservableCollection<AlphaKeyGroup<Album>> _sortedAlbums;
        private ObservableCollection<AlphaKeyGroup<Artist>> _sortedArtists;
        private ObservableCollection<AlphaKeyGroup<Song>> _sortedSongs;
        private ObservableCollection<AlphaKeyGroup<Video>> _sortedVideos;
        private string _message;    
     


        public CollectionViewModel(ICollectionService service, IMiscellaneousService misc, CollectionCommandHelper commands)
        {
            _service = service;
            _commands = commands;
            _misc = misc;

            SongClickCommand = new Command<ItemClickEventArgs>(SongClickExecute);
            VideoClickCommand = new Command<ItemClickEventArgs>(VideoClickExecute);

            SortedSongs = AlphaKeyGroup<Song>.CreateGroups(Service.Songs.Where(p => !p.IsTemp).ToList(),
               CultureInfo.CurrentCulture, item => item.Name, true);

            SortedArtists = AlphaKeyGroup<Artist>.CreateGroups(Service.Artists.Where(p => p.Songs.Count > 0).ToList(),
                CultureInfo.CurrentCulture, item => item.Name, true);
            SortedAlbums = AlphaKeyGroup<Album>.CreateGroups(Service.Albums.Where(p => p.Songs.Count > 0).ToList(),
                CultureInfo.CurrentCulture, item => item.Name, true);

            SortedVideos = AlphaKeyGroup<Video>.CreateGroups(Service.Videos, CultureInfo.CurrentCulture, item => item.Title, true);//Service.Videos;


            InitAsync();
        }

        void InitAsync()
        {
            Service.Songs.CollectionChanged += OnCollectionChanged;
            Service.Albums.CollectionChanged += OnCollectionChanged;
            Service.Artists.CollectionChanged += OnCollectionChanged;
            Service.Videos.CollectionChanged += OnCollectionChanged;
        }

      
        public Command<ItemClickEventArgs> SongClickCommand { get; set; }
        public Command<ItemClickEventArgs> VideoClickCommand { get; set; }

        private async void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs arg)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                BaseEntry item;
                var removed = false;

                switch (arg.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        if (sender is OptimizedObservableCollection<Song>)
                            SortedSongs = AlphaKeyGroup<Song>.CreateGroups(Service.Songs.Where(p => !p.IsTemp).ToList(),
                                CultureInfo.CurrentCulture, i => i.Name, true);
                        else if (sender is OptimizedObservableCollection<Album>)
                            SortedAlbums = AlphaKeyGroup<Album>.CreateGroups(Service.Albums.Where(p => p.Songs.Count > 0).ToList(),
                                CultureInfo.CurrentCulture, i => i.Name, true);
                        else if (sender is OptimizedObservableCollection<Artist>)
                            SortedArtists = AlphaKeyGroup<Artist>.CreateGroups(Service.Artists.Where(p => p.Songs.Count > 0).ToList(),
                                CultureInfo.CurrentCulture, i => i.Name, true);
                        else if (sender is OptimizedObservableCollection<Video>)
                            SortedVideos = AlphaKeyGroup<Video>.CreateGroups(Service.Videos, CultureInfo.CurrentCulture, i => i.Title, true);//Service.Videos;
                        return;
                    case NotifyCollectionChangedAction.Add:
                        item = (BaseEntry)arg.NewItems[0];
                        break;
                    default:
                        item = (BaseEntry)arg.OldItems[0];
                        removed = true;
                        break;
                }

                if (item is Song)
                {
                    var song = item as Song;
                    if (!song.IsTemp || removed)
                        UpdateSortedCollection(song, removed, song.Name, () => SortedSongs);
                }
                else if (item is Artist)
                {
                    var artist = item as Artist;
                    if (artist.Songs.Count > 0 || removed)
                        UpdateSortedCollection(artist, removed, artist.Name, () => SortedArtists);
                }
                else if (item is Album)
                {
                    var album = item as Album;
                    if (album.Songs.Count > 0 || removed)
                        UpdateSortedCollection(album, removed, album.Name, () => SortedAlbums);
                }
                else if (item is Video)
                {
                    var video = item as Video;
                    if (removed)
                        UpdateSortedCollection(video, removed, video.Title, () => SortedVideos);
                }
            });
        }

        public static void UpdateSortedCollection<T>(T item, bool removed, string key,
        Func<ObservableCollection<AlphaKeyGroup<T>>> getSorted)
        {
           
            if (string.IsNullOrEmpty(key)) return;

            var sortedGroups = getSorted();
            var charKey = new CharacterGroupings().Lookup(key);
            string charkey = charKey.ToUpper();

            AlphaKeyGroup<T> group;

            try
            { 
                group = sortedGroups.First(a => a.Key.Equals(charkey, StringComparison.CurrentCultureIgnoreCase));
            }
            catch
            {
                group = sortedGroups.FirstOrDefault();
            }

            bool zero;
            if (removed)
            {
                group.Remove(item);
                zero = group.Count == 0;
            }

            else
            {
                zero = group.Count == 0;
                var index = 0;

                //if the group is not empty, then insert acording to sort
                if (!zero)
                {
                    var list = group.ToList();
                    list.Add(item);
                    list.Sort(
                        (x, y) => String.Compare(group.OrderKey(x), group.OrderKey(y), StringComparison.Ordinal));
                    index = list.IndexOf(item);
                }
                group.Insert(index, item);
            }

            if (!zero) return;

            //removing and readding to update the groups collection in the listview
            var groupIndex = sortedGroups.IndexOf(group);
            sortedGroups.Remove(group);
            sortedGroups.Insert(groupIndex, group);
        }


        private async void SongClickExecute(ItemClickEventArgs e)
        {
            Song s = e.ClickedItem as Song;
            var queueSongs = _service.Songs.OrderBy(p => p.Name).ToList();
            int index = queueSongs.IndexOf(s);
            queueSongs = queueSongs.Skip(index).ToList();
          
            if (queueSongs != null && queueSongs.Count > 0)
                await PlayAndQueueHelper.PlaySongsAsync(s, queueSongs, true);
        }


        void VideoClickExecute(ItemClickEventArgs obj)
        {
            if(obj.ClickedItem is Video)
            {
                SheetUtility.OpenVideoPage(obj.ClickedItem as Video);
            }
            else
            {
                App.Locator.VideoPlayer.InvokeOnline(obj.ClickedItem);
            } 
        }

        public string Message
        {
            get { return _message; }
            set { Set(ref _message, value); }
        }

        //public async Task GetVideoForCategory(string id)
        //{

        //    Message = Core.StringMessage.LoadingPleaseWait;

        //    if (!App.Locator.Network.IsActive)
        //    {
        //        Message = Core.StringMessage.NoInternetConnection;
        //        return;
        //    }
        //    //await Task.Delay(50);
           
        //    try
        //    { 
        //        await Task.Factory.StartNew(async () =>
        //        {
        //            await DispatcherHelper.RunAsync(async () =>
        //            {
        //                var o = await _misc.GetPopularCategoryVideos(20, id);
        //                int indexToAdd = -1;
        //                foreach (var addedOVideo in o)
        //                {
        //                    indexToAdd++;
        //                    Videos.Add(addedOVideo);
        //                    if (indexToAdd == 2 || indexToAdd == 10 || indexToAdd == 22 || indexToAdd == 34 || indexToAdd == 49 ||
        //                        indexToAdd == 63 || indexToAdd == 78 || indexToAdd == 88 || indexToAdd == 99)
        //                        Videos.Add(new Advert());
        //                }

        //                if (Videos != null && Videos.Count() > 0)
        //                    Message = Core.StringMessage.NoMessge;  
        //                else Message = Core.StringMessage.EmptyMessage;
        //            });
        //        });    
        //    }
        //    catch
        //    {
        //        Message = Core.StringMessage.SomethinWentWrong;
        //    }
        //}

        //public CollectionViewSource ViewSource
        //{
        //    get{ return _viewSource; }
        //    set{ Set(ref _viewSource, value); }
        //}

        //public int SelectedIndex
        //{
        //    get { return _selectedIndex; }
        //    set { Set(ref _selectedIndex, value); }
        //}

        public ObservableCollection<AlphaKeyGroup<Song>> SortedSongs
        {
            get { return _sortedSongs; }
            set { Set(ref _sortedSongs,value); }
        }

        public ObservableCollection<AlphaKeyGroup<Video>> SortedVideos
        {
            get { return _sortedVideos; }
            set { Set(ref _sortedVideos, value); }
        }

        public ObservableCollection<AlphaKeyGroup<Album>> SortedAlbums
        {
            get { return _sortedAlbums; }
            set { Set(ref _sortedAlbums, value); }
        }

        public ObservableCollection<AlphaKeyGroup<Artist>> SortedArtists
        {
            get { return _sortedArtists; }
            set { Set(ref _sortedArtists, value); }
        }

        public ICollectionService Service
        {
            get { return _service; }
        }

        public CollectionCommandHelper Commands
        {
            get { return _commands; }
        }
  
    }

}


