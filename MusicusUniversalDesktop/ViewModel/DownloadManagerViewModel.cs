
using Musicus.Data.Collection.Model;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Musicus.Data.Collection;
using Musicus.ViewModel.Mvvm.Commands;
using Musicus.Helpers;
using System;
using Musicus.Core.Common;
using System.Threading.Tasks;
using Musicus.ViewModel.Mvvm.Dispatcher;

namespace Musicus.ViewModel
{
    public class DownloadManagerViewModel : Mvvm.Others.ViewModelBase
    {
        private readonly Command<ItemClickEventArgs> _songClickCommand;
        private OptimizedObservableCollection<Song> _downloads;
        private string _message = default(string);
        private readonly ICollectionService _service;

        public DownloadManagerViewModel(ICollectionService service)
        {
            _service = service;
            _songClickCommand = new Command<ItemClickEventArgs>(SongClickExecute);
        }

        public OptimizedObservableCollection<Song> Downloads
        {
            get { return _downloads; }
            set { Set(ref _downloads, value); }
        }

        public string Message
        {
            get { return _message; }
            set { Set(ref _message, value); }
        }

        public Command<ItemClickEventArgs> SongClickCommand
        {
            get { return _songClickCommand; }
        }

        public async void LoadAsync()
        {
            await Task.Factory.StartNew(async () =>
            {
                await DispatcherHelper.RunAsync(() =>
                {
                    Downloads = ConvertToObservable(_service.GetDownloads());
                    Message = (Downloads != null && Downloads.Count() > 0) 
                    ? Core.StringMessage.NoMessge 
                    : Core.StringMessage.EmptyMessage;
                });
            });

            
        }

        public ICollectionService Service
        {
            get { return _service; }
        }

        OptimizedObservableCollection<Song> ConvertToObservable(IEnumerable<Song> enumerable)
        {
            OptimizedObservableCollection<Song> _songs = new OptimizedObservableCollection<Song>();
            foreach(Song song in enumerable)
                _songs.Add(song);
            return _songs;
        }

        private async void SongClickExecute(ItemClickEventArgs e)
        {
            Song s = e.ClickedItem as Song;
            var queueSongs = _downloads.ToList();
            int index = queueSongs.IndexOf(s);
            queueSongs = queueSongs.Skip(index).ToList();

            if (queueSongs != null && queueSongs.Count > 0)
                await PlayAndQueueHelper.PlaySongsAsync(s, queueSongs, true);    
        }      
    }
}