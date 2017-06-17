#region

using Musicus.Data.Model.WebData;
using Musicus.Helpers;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

#endregion

namespace Musicus
{
    public sealed partial class SpotifyAlbumPage : IDisposable
    {
        public SpotifyAlbumPage()
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
            App.Locator.SpotifyAlbum.ReceivedId(id);
        }

        public override void NavigatedFrom(NavigationMode mode)
        {
            base.NavigatedFrom(mode);
            if (mode == NavigationMode.Back)
            {
                ClearData();
            }

         
          

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
                await DispatcherHelper.RunAsync(() => App.Locator.SpotifyAlbum.ClearSpotifyAlbumViewModelData());
            });

            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}

