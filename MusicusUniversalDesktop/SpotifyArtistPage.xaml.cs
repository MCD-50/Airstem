using System;
using Windows.UI.Xaml.Navigation;
using Musicus.Data.Spotify.Models;
using Musicus.Utilities;
using Musicus.ViewModel.Mvvm.Dispatcher;
using Windows.UI.Xaml.Controls;
using Musicus.Helpers;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Model.Advertisement;
using Musicus.Data.Model.WebData;

namespace Musicus
{
    public sealed partial class SpotifyArtistPage : IDisposable
    {

        public SpotifyArtistPage()
        {
            this.InitializeComponent();
        }

        public override void NavigatedTo(NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);

            var id = parameter as string;
            if (id == null) return;
            ExecuteAsync(id);

            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") return;
            App.Navigator.StackCount += 1;
            App.Navigator.ShowBack();
        }

        private void ExecuteAsync(string id)
        {
            App.Locator.SpotifyArtist.ReceivedName(id);
        }

        public override void NavigatedFrom(NavigationMode mode)
        {
            base.NavigatedFrom(mode);
            if (mode == NavigationMode.Back)
                ClearData();

         
            if (mode == NavigationMode.Back && Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            {
                App.Navigator.StackCount -= 1;
                if (App.Navigator.StackCount <= 0)
                    App.Navigator.HideBack();
            }
        }

        private async void ClearData()
        {
            await System.Threading.Tasks.Task.Delay(800);
            await System.Threading.Tasks.Task.Factory.StartNew(async () =>
            {
                await DispatcherHelper.RunAsync(() => App.Locator.SpotifyArtist.ClearSpotifyArtistViewModelData());
            });

            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }


        //private void FollowToggle(object sender, RoutedEventArgs e)
        //{
        //    App.Locator.SpotifyArtist.FollowToggle(App.Locator.SpotifyArtist.Name);
        //}


        private async void AlbumClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem;
            if (album is Advert || album is ListAdvert) return;
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<SpotifyAlbumPage, ZoomInTransition>(((WebAlbum)album).Id);
            });
        }

    }
}


