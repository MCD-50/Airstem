#region
using Musicus.Data.Collection;
using Musicus.Data.Collection.Model;
using Musicus.Helpers;
using Musicus.ViewModel.Mvvm.Commands;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;



#endregion

namespace Musicus.ViewModel
{
    public class CollectionPlaylistViewModel : Mvvm.Others.ViewModelBase
    {     
        private readonly Command<ItemClickEventArgs> _songClickCommand;
        private Playlist _playlist;
        private readonly ICollectionService _service;

        public CollectionPlaylistViewModel(ICollectionService service)
        {
            _service = service;
            _songClickCommand = new Command<ItemClickEventArgs>(SongClickExecute);
        }

        public Playlist Playlist
        {
            get { return _playlist; }
            set { Set(ref _playlist, value); }
        }

        public Command<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickCommand; }
        }

        public async void SetPlaylist(int id)
        {
            if (Playlist != null) Playlist = null;
            await Task.Factory.StartNew(async () =>
            {
                await DispatcherHelper.RunAsync(() => 
                {
                    Playlist = _service.GetPlayList(id);
                });
            });  
        }

  




        //private int _prevIndex = -1;
        //private async void SongsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.Action == NotifyCollectionChangedAction.Add && _prevIndex != -1)
        //    {
        //        //an item was move using reorder
        //        await _service.MovePlaylistFromToAsync(Playlist, _prevIndex, e.NewStartingIndex);
        //    }

        //    if (e.Action == NotifyCollectionChangedAction.Remove)
        //        _prevIndex = e.OldStartingIndex;
        //    else
        //        _prevIndex = -1;
        //}


        private async void SongClickExecute(ItemClickEventArgs e)
        {
            var queueSongs = _playlist.Songs.Select(p => p.Song).ToList();
            var clickedsong = (e.ClickedItem as PlaylistSong).Song;

            if (queueSongs != null && queueSongs.Count > 0)
                await PlayAndQueueHelper.PlaySongsAsync(clickedsong, queueSongs, true);
        }
 
    }
}