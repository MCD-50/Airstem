
using Musicus.Data.Model.WebData;
using Musicus.Utilities;
using Musicus.ViewModel.Mvvm.Dispatcher;
using Windows.UI.Xaml.Navigation;
using System;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Model.Advertisement;

namespace Musicus
{
    public sealed partial class NewTrackAlbumPage
    {
        public NewTrackAlbumPage()
        {
            this.InitializeComponent();
            array[0] = "Top tracks";
            array[1] = "Top albums";
            array[2] = "Discover the list of top tracks from all around the world." + "\n" + "Top trending tracks of this week.";
            array[3] = "Discover the list of top albums from all around the world." + "\n" + "Top trending albums of the day.";
        }

        public override void NavigatedTo(NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);
            if (parameter == null) return;
            var pivotIndex = (int)parameter;
         
            if(pivotIndex == 0)
            {
                HeaderTextBlock.Text = array[0];
                ContentTextBlock.Text = array[2];
                TrackBackground.Visibility = Windows.UI.Xaml.Visibility.Visible;
                AlbumBackground.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                TracksAndAlbumsGrid.ItemsSource = App.Locator.HomePage.Popular;
                TracksAndAlbumsGrid.ItemTemplateSelector = 
                    (Windows.UI.Xaml.Controls.DataTemplateSelector)this.Resources["NewTrackPagePcSelectorForTracks"];
            }

            else
            {
                HeaderTextBlock.Text = array[1];
                ContentTextBlock.Text = array[3];
                TrackBackground.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                AlbumBackground.Visibility = Windows.UI.Xaml.Visibility.Visible;

                TracksAndAlbumsGrid.ItemsSource = App.Locator.HomePage.TopAlbums;
                TracksAndAlbumsGrid.ItemTemplateSelector =
                    (Windows.UI.Xaml.Controls.DataTemplateSelector)this.Resources["NewTrackPagePcSelectorForAlbums"];
            }

            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") return;
            App.Navigator.StackCount += 1;
            App.Navigator.ShowBack();
        }

        string[] array = new string[5];

        public override void NavigatedFrom(NavigationMode mode)
        {
            base.NavigatedFrom(mode);      
            if (mode == NavigationMode.Back && Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            {
                App.Navigator.StackCount -= 1;
                if (App.Navigator.StackCount <= 0)
                    App.Navigator.HideBack();
            }
        }

    
        private async void ItemClicked(object sender, Windows.UI.Xaml.Controls.ItemClickEventArgs e)
        {
            var o = e.ClickedItem;
            if (o is Advert || o is ListAdvert) return;

            if (o is WebAlbum)
            {
                await DispatcherHelper.RunAsync(() =>
                {
                    App.Navigator.GoTo<DeezerAlbumPage, ZoomInTransition>(o as WebAlbum);
                });
            }

            else if(o is WebSong)
            {
                await Helpers.SongSavingHelper.SaveViezTrackLevel1(o as WebSong);
            }
      
        }
    }
}
