
using Musicus.Data.Model.WebData;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System;
using Windows.UI.Xaml.Navigation;

namespace Musicus
{
    public sealed partial class DeezerAlbumPage : IDisposable
    {
        public DeezerAlbumPage()
        {
            this.InitializeComponent();
        }

        public override void NavigatedTo(NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);

            var id = parameter as WebAlbum;
            if (id == null) return;

            ExecuteRemaining(id);

            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") return;
            App.Navigator.StackCount += 1;
            App.Navigator.ShowBack();
        }

        private async void ExecuteRemaining(WebAlbum id)
        {
            App.Locator.SpotifyAlbum.ReceivedId(id);
            await System.Threading.Tasks.Task.Delay(1);
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
                await DispatcherHelper.RunAsync(() =>
                {
                    App.Locator.SpotifyAlbum.ClearSpotifyAlbumViewModelData();
                });
            });
          
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
