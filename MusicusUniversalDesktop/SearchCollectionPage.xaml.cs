
using Musicus.Data.Collection.Model;
using Musicus.Helpers;
using Musicus.Utilities;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Musicus
{

    public sealed partial class SearchCollectionPage : IModalSheetPage
    {
        public SearchCollectionPage()
        {
            this.InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                MainPivot.Title = "COLLECTION";
                MainPivotItem1.Header = "results";
            }

        }
  

        public Popup Popup { get; private set; }
        public void OnOpened(Popup popup)
        {
            Popup = popup;
            App.Locator.CollectionSearch.SetMessage("Looks great, search away...");
            App.Navigator.ShowBack();         
        }

        public void OnClosed()
        {
            Popup = null;           
            if (App.Navigator.StackCount <= 0)
             App.Navigator.HideBack();
            
            App.Locator.CollectionSearch.ClearCollectionSeachData();
        }

        private async void songsSearchListClicked(object sender, ItemClickEventArgs e)
        {
            await DispatcherHelper.RunAsync(async () =>
            {
                SheetUtility.CloseSearchCollectionPage();

                Song s = e.ClickedItem as Song;
                var queueSongs = App.Locator.CollectionSearch.Songs.ToList();
                int index = queueSongs.IndexOf(s);
                queueSongs = queueSongs.Skip(index).ToList();

                if (queueSongs != null && queueSongs.Count > 0)
                    await PlayAndQueueHelper.PlaySongsAsync(s, queueSongs, true);
            });
        }

        private async void albumsSearchListClicked(object sender, ItemClickEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                SheetUtility.CloseSearchCollectionPage();
                App.Navigator.GoTo<CollectionAlbumPage, ZoomInTransition>(((Album)e.ClickedItem).Id);
            });
        }


        private async void artistsSearchListClicked(object sender, ItemClickEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                SheetUtility.CloseSearchCollectionPage();
                App.Navigator.GoTo<CollectionArtistPage, ZoomInTransition>(((Artist)e.ClickedItem).Id);
            });
        }

        //private void TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        //{
        //    //We only want to get results when it was a user typing, 
        //    //otherwise we assume the value got filled in by TextMemberPath 
        //    //or the handler for SuggestionChosen
        //    if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        //        App.Locator.CollectionSearch.SearchInCollectionPhase1(sender.Text);
        //}



        private async void artistClicked(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                SheetUtility.CloseSearchCollectionPage();
                App.Navigator.GoTo<CollectionArtistPage, ZoomInTransition>(App.Locator.CollectionSearch.Artists.Id);
            });
        }

        private void StartSearch(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && e.KeyStatus.RepeatCount == 1)
            {
                App.Locator.CollectionSearch.SearchInCollectionPhase1(SearchTextBox.Text);
            }
        }
    }
}