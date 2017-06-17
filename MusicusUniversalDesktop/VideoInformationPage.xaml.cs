
using Musicus.ViewModel.Mvvm.Dispatcher;
using Windows.UI.Xaml.Navigation;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using Musicus.Data.Model.Advertisement;

namespace Musicus
{
    public sealed partial class VideoInformationPage : System.IDisposable
    {
      
        public VideoInformationPage()
        {
            this.InitializeComponent();
        }


        public override void NavigatedTo(NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") return;
            App.Navigator.StackCount += 1;
            App.Navigator.ShowBack();
        }


        public override void NavigatedFrom(NavigationMode mode)
        {
            base.NavigatedFrom(mode);

            if (mode == NavigationMode.Back)
            {
                ClearData();

                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
                {
                    App.Navigator.StackCount -= 1;
                    if (App.Navigator.StackCount <= 0)
                        App.Navigator.HideBack();
                } 
            }
        }


        private async void ClearData()
        {
            await Task.Delay(800);
            await Task.Factory.StartNew(async () =>
            {
                await DispatcherHelper.RunAsync(() =>
                     App.Locator.VideoPlayer.ClearVideoPlayerViewModel()
                );
            });

            Dispose();
        }

        public void Dispose()
        {
            GC.Collect();
        }

        private void OnPivotChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainPivot.SelectedIndex == 1)
                BackgroundImage.Visibility = Visibility.Collapsed;
            else
                BackgroundImage.Visibility = Visibility.Visible;
        }

        private void SimilarVideosClicked(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ListAdvert || e.ClickedItem is Advert) return;
            MainPivot.SelectedIndex = 0;
            App.Locator.VideoPlayer.InvokeOnline(e.ClickedItem);
        }

       
        private async void PlayVideoClicked(object sender, RoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                Helpers.SheetUtility.OpenVideoPage(null);
            });
        }
    }
}
