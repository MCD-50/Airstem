using Musicus.Data.Collection.Model;
using Musicus.Helpers;
using System.Collections.Generic;
using System.Linq;
using Musicus.Data.Collection;
using System;
using System.Threading.Tasks;
using Musicus.ViewModel.Mvvm.Dispatcher;

namespace Musicus.ViewModel
{
    public class CollectionSearchViewModel : Mvvm.Others.ViewModelBase
    {
        private IEnumerable<Song> _songs;
        private IEnumerable<Album> _albums;
        private Artist _artists;
      
        private readonly CollectionCommandHelper _commands;
        private readonly ICollectionService _service;

        private string message;

        public CollectionSearchViewModel(CollectionCommandHelper commands, ICollectionService service)
        {
            _commands = commands;
            _service = service;
        }


        public IEnumerable<Song> Songs
        {
            get { return _songs; }
            set { Set(ref _songs, value); }
        }

        public IEnumerable<Album> Albums
        {
            get { return _albums; }
            set { Set(ref _albums, value); }
        }

        public Artist Artists
        {
            get { return _artists; }
            set { Set(ref _artists, value); }
        }

        public string Message
        {
            get { return message; }
            set { Set(ref message, value); }
        }


        public CollectionCommandHelper Commands
        {
            get { return _commands; }
        }

        //private async void KeyDownExecute(KeyRoutedEventArgs e)
        //{
        //    if (e.Key == VirtualKey.Enter && e.KeyStatus.RepeatCount == 1)
        //    {
        //        ((PivotItem)((ScrollViewer)((StackPanel)((TextBox)e.OriginalSource).Parent).Parent).Parent).Focus(FocusState.Keyboard);
        //        var term = ((TextBox)e.OriginalSource).Text.Trim();
        //        await DispatcherHelper.RunAsync(() =>
        //        {
        //            Message = Core.StringMessage.LoadingPleaseWait;
        //            ((TextBox)e.OriginalSource).IsEnabled = false;
        //            System.Threading.Tasks.Task.Delay(1000);
        //            SearchInCollectionPhase1(term);
        //            ((TextBox)e.OriginalSource).IsEnabled = true;
        //        });
        //    }

        //}


        public void SetMessage(string message)
        {
            Message = message;
        }

        public void ClearCollectionSeachData()
        {
            if (Songs != null) Songs = null;
            if (Artists != null) Artists = null;
            if (Albums != null) Albums = null;
        }

        public void SearchInCollectionPhase1(string term)
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                SearchInCollectionPahse2(term, 5);
            else
                SearchInCollectionPahse2(term, 10);
        }

        async void SearchInCollectionPahse2(string query ,int count)
        {
            try
            {
                await Task.Factory.StartNew(async () =>
                {
                    await DispatcherHelper.RunAsync(() =>
                    {
                        Songs = _service.GetMatchingTracks(query, count);
                        Albums = _service.GetMatchingAlbums(query, count);
                        Artists = _service.GetMatchingArtists(query);
                        if ((Songs != null && Songs.Count() > 0) || (Albums != null && Albums.Count() > 0) || Artists != null)
                            Message = Core.StringMessage.NoMessge;
                        else
                            Message = Core.StringMessage.EmptyMessage;

                       

                    });
                });
            }
            catch
            {
                Message = Core.StringMessage.SomethinWentWrong;
            }
        }
    }
}
