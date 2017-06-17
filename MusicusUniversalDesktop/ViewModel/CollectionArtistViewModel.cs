#region
using Musicus.Data.Collection;
using Musicus.Data.Collection.Model;
using Musicus.Helpers;
using Musicus.ViewModel.Mvvm.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#endregion

namespace Musicus.ViewModel
{
    public class CollectionArtistViewModel : Mvvm.Others.ViewModelBase
    {
        private readonly ICollectionService _service;
        private readonly CollectionCommandHelper _commands;
        private Artist _artist;
        private string _name;
        private readonly Command<ItemClickEventArgs> _songClickCommand;
        public SettingViewModel SettingViewModel { get; set; }
        //private object _sortedSongs;
        private SolidColorBrush _backgroundBrush;

        public CollectionArtistViewModel(ICollectionService service, CollectionCommandHelper commands, SettingViewModel settingViewModel)
        {
            _service = service;
            _commands = commands;
            _songClickCommand = new Command<ItemClickEventArgs>(SongClickExecute);
            SettingViewModel = settingViewModel;
        }

        public CollectionCommandHelper Commands
        {
            get { return _commands; }
        }

        private async void SongClickExecute(ItemClickEventArgs e)
        {
            Song s = e.ClickedItem as Song;
            var queueSongs = Artist.Songs.ToList();
            int index = queueSongs.IndexOf(s);
            queueSongs = queueSongs.Skip(index).ToList();
            if (queueSongs != null && queueSongs.Count > 0)
                await PlayAndQueueHelper.PlaySongsAsync(s, queueSongs, true);
        }

        public Command<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickCommand; }
        }

        public Artist Artist
        {
            get { return _artist; }
            set { Set(ref _artist, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        public void SetArtist(int id)
        {
            Artist = _service.GetArtist(id);
            Name = Artist.Name.ToUpper();
        }
    }
}