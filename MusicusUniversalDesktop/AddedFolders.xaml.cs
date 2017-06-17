using Musicus.Data.Collection.Model;
using Musicus.Helpers;
using System.Collections.Generic;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Musicus
{
    public sealed partial class AddedFolders : IModalSheetPage
    {
        public AddedFolders()
        {
            this.InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                MainPivot.Title = "SETTINGS";
                MainPivotItem1.Header = "added folders";
            }
        }

        public Popup Popup { get; private set; }
        public void OnOpened(Popup popup)
        {
            App.Navigator.ShowBack();
            Popup = popup;
        }

        public void OnClosed()
        {
            Popup = null;
            if (App.Navigator.StackCount > 0) return;
            App.Navigator.HideBack();
        }

        async void RemoveFlyoutClicked(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuFlyoutItem).DataContext as Folder;
            await App.Locator.CollectionService.DeleteFolderAsync(item);
            //StorageApplicationPermissions.FutureAccessList.Remove(item.Id.ToString());
        }
    }
}
