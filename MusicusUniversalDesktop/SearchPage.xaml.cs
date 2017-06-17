
using IF.Lastfm.Core.Objects;
using Musicus.Data.Collection.Model;
using Musicus.Data.Model.Advertisement;
using Musicus.Data.Model.WebData;
using Musicus.Data.Spotify.Models;
using Musicus.Helpers;
using Musicus.Utilities;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Musicus
{
    public sealed partial class SearchPage : IDisposable
    {
        private readonly PivotItem _similarPivotItem;
        public static bool _forward = false;

        public SearchPage()
        {
            this.InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                MainPivot.Title = "SEARCH";
                MainPivotItem1.Header = "music";
                MainPivotItem2.Header = "videos";
                MainPivotItem3.Header = "more";
            }
            App.Locator.Search.SetMessage("Looks great, search away...");
            _similarPivotItem = MainPivotItem2;
            //App.Locator.Search.SetMessage("Looks great, search away...");
        }


        public override void NavigatedTo(NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);

            if (App.Locator.Setting.VideoOnOff && !MainPivot.Items.Contains(_similarPivotItem))
                MainPivot.Items.Insert(1,_similarPivotItem);

            else if (!App.Locator.Setting.VideoOnOff && MainPivot.Items.Contains(_similarPivotItem))
                MainPivot.Items.Remove(_similarPivotItem);
        }


        public override void NavigatedFrom(NavigationMode mode)
        {
            base.NavigatedFrom(mode);
            if(mode == NavigationMode.Back)
                ClearData();

            Dispose();
        }


        public static async void ClearData()
        {
            await Task.Delay(2000);
            await Task.Factory.StartNew(async () =>
            {
                await DispatcherHelper.RunAsync(() =>
                {
                    App.Locator.Search.ClearSearchViewModelData();
                    App.Locator.Search.SetMessage("Looks great, search away...");
                });
            });

        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }


        private async void SpotifyAlbumClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem;
            if (album is Advert || album is ListAdvert) return;
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<SpotifyAlbumPage, ZoomInTransition>(((WebAlbum)album).Id);
            });
        }


        private async void SpotifyArtistClick(object sender, ItemClickEventArgs e)
        {
            var artist = e.ClickedItem;
            if (artist is Advert || artist is ListAdvert) return;
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<SpotifyArtistPage, ZoomInTransition>(((LastArtist)artist).Mbid);
            });
        }

    }
}