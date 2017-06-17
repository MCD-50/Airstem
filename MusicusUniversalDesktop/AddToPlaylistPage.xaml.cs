
using Musicus.Data.Collection.Model;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using System.Linq;
using System.Collections.Generic;
using Musicus.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Core;
using Musicus.Utilities;

namespace Musicus
{
    public sealed partial class AddToPlaylistPage : IModalSheetPage
    {
        public AddToPlaylistPage()
        {
            this.InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                MainPivot.Title = "PICKER";
                MainPivotItem1.Header = "choose to add";
            }
        }

        public static List<Song> songs;
        public Popup Popup { get; private set; }
        public void OnOpened(Popup popup)
        {
           
            Popup = popup;
            App.SupressBackEvent += HardwareButtons_BackPressed;

            if (App.Locator.CollectionService.Playlists.Count > 0)
                MessageTextBlock.Visibility = Visibility.Collapsed;
            else MessageTextBlock.Visibility = Visibility.Visible;
            PlaylistsList.ItemsSource = App.Locator.CollectionService.Playlists;

            App.Navigator.ShowBack();
        }

        public void OnClosed()
        {
            Popup = null;
            if (App.Navigator.StackCount > 0) return;
            App.Navigator.HideBack();
        }

        private static void HardwareButtons_BackPressed(object sender, BackRequestedEventArgs e)
        {
            UiBlockerUtility.Unblock();
            App.SupressBackEvent -= HardwareButtons_BackPressed;
            SheetUtility.CloseAddToPlaylistPage();
        }

        private async void playlistitemclick(object sender, ItemClickEventArgs e)
        {
            var playlist = e.ClickedItem as Playlist;
            for (var i = 0; i < songs.Count; i++)
            {
                var song = songs[i];
                // only add if is not there already
                if (playlist.Songs.FirstOrDefault(p => p.Song.Id == song.Id) == null)
                    await App.Locator.CollectionService.AddToPlaylistAsync(playlist, song).ConfigureAwait(false);
            }
            SheetUtility.CloseAddToPlaylistPage();
        }
    }
}
