#region
using Musicus.Data.Collection;
using Musicus.Data.Collection.Model;
using Musicus.Helpers;
using Musicus.ViewModel.Mvvm.Commands;
using System.Linq;
using Windows.UI.Xaml.Controls;


#endregion

namespace Musicus.ViewModel
{
    public class CollectionAlbumViewModel : Mvvm.Others.ViewModelBase
    {
        private readonly ICollectionService _service;
        private Album _album;
        private readonly Command<ItemClickEventArgs> _songClickCommand;
        private string _name;
   
        public CollectionAlbumViewModel(ICollectionService service)
        {
            _service = service;
            _songClickCommand = new Command<ItemClickEventArgs>(SongClickExecute);
        }

        private async void SongClickExecute(ItemClickEventArgs e)
        {
            Song s = e.ClickedItem as Song;
            var queueSongs = Album.Songs.ToList();
            int index = queueSongs.IndexOf(s);
            queueSongs = queueSongs.Skip(index).ToList();

            if (queueSongs != null && queueSongs.Count > 0)
                await PlayAndQueueHelper.PlaySongsAsync(s, queueSongs, true);
        }

        public Album Album
        {
            get { return _album; }
            set { Set(ref _album, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        public Command<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickCommand; }
        }
      
        public void SetAlbum(int id)
        {
            Album = _service.GetAlbum(id);
            Name = Album.Name.ToUpper();
        }
    }
}