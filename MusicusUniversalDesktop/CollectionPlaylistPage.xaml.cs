
using Musicus.Data.Collection.Model;
using Musicus.Helpers;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using System;
using Musicus.Utilities;
using Windows.UI.Core;
using Musicus.Core.Utils;
using Musicus.Core.WinRt.Common;
using Windows.UI.Xaml.Navigation;

namespace Musicus
{
    public sealed partial class CollectionPlaylistPage
    {

        private TypedEventHandler<ListViewBase, ContainerContentChangingEventArgs> _delegate;
        public CollectionPlaylistPage()
        {
            this.InitializeComponent();
        }


        public override void NavigatedTo(NavigationMode mode, object e)
        {
            base.NavigatedTo(mode, e);
            var id = e as int?;
            if (id == null) return;

            ExecuteRemaining(id);

            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            {
                App.Navigator.StackCount += 1;
                App.Navigator.ShowBack();
            }
        }

        private async void ExecuteRemaining(int? id)
        {
            App.Locator.CollectionPlaylist.SetPlaylist((int)id);
            FadeInAnimation.Begin();
            await System.Threading.Tasks.Task.Delay(1);
        }


        public override void NavigatedFrom(NavigationMode mode)
        {
            base.NavigatedFrom(mode);
            if (mode == NavigationMode.Back && 
                Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            {
                App.Navigator.StackCount -= 1;
                if (App.Navigator.StackCount <= 0)
                    App.Navigator.HideBack();
            }
        }


        private TypedEventHandler<ListViewBase, ContainerContentChangingEventArgs> ContainerContentChangingDelegate
        {
            get { return _delegate ?? (_delegate = ItemListView_ContainerContentChanging); }
        }

        private void ItemListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var songViewer = args.ItemContainer.ContentTemplateRoot as SongViewer;

            if (songViewer == null)
                return;

            if (args.InRecycleQueue)
            {
                songViewer.ClearData();
            }
            else
                switch (args.Phase)
                {
                    case 0:
                        songViewer.ShowPlaceholder((args.Item as PlaylistSong).Song, playlistMode: true);
                        args.RegisterUpdateCallback(ContainerContentChangingDelegate);
                        break;
                    case 1:
                        songViewer.ShowTitle();
                        args.RegisterUpdateCallback(ContainerContentChangingDelegate);
                        break;
                    case 2:
                        songViewer.ShowRest();
                        break;
                }

            // For imporved performance, set Handled to true since app is visualizing the data item 
            args.Handled = true;
        }


        private async void PlayAll(object sender, RoutedEventArgs e)
        {
            var queueSongs = App.Locator.CollectionPlaylist.Playlist.Songs.Select(p => p.Song).ToList();
            if (queueSongs != null && queueSongs.Count > 0)
                await PlayAndQueueHelper.PlaySongsAsync(queueSongs[0], queueSongs, true);
        }

        private void OpenNowPlaying(object sender, RoutedEventArgs e)
        {
            AddDeleteShareManager.OpenNowPlaying();
        }

        private async void ShuffleAll(object sender, RoutedEventArgs e)
        {
            App.Locator.PBar.IsEnable = true;
            await DispatcherHelper.RunAsync(async () =>
            {
                var list = App.Locator.CollectionPlaylist.Playlist.Songs.Select(p=>p.Song).ToList().Shuffle().ToList();
                await PlayAndQueueHelper.PlaySongsAsync(list[0], list);
                App.Locator.PBar.IsEnable = false;
            });
        }

        private void OnEdgeTapped(object sender, RoutedEventArgs e)
        {
            if (SongList.SelectionMode == ListViewSelectionMode.Multiple)
            {
                SongList.SelectionChanged -= OnSelectionChanged;
                SongList.SelectionMode = ListViewSelectionMode.None;
            }
            else
            {
                SongList.SelectionChanged += OnSelectionChanged;
                SongList.SelectionMode = ListViewSelectionMode.Multiple;
                OnSelectionChanged(null, null);
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var bar = Bar as CommandBar;
            if (SongList.SelectionMode == ListViewSelectionMode.Multiple)
            {
                App.SupressBackEvent += HardwareButtonsOnBackPressed;
                UiBlockerUtility.BlockNavigation(false);
                SongList.IsItemClickEnabled = false;
                //OptionButton.Visibility = Visibility.Visible;
                SongList.SelectionMode = ListViewSelectionMode.Multiple;
            }

            else if (!SongList.IsItemClickEnabled)
            {
                App.SupressBackEvent -= HardwareButtonsOnBackPressed;
                UiBlockerUtility.Unblock();
                SongList.IsItemClickEnabled = true;
                //OptionButton.Visibility = Visibility.Collapsed;
                SongList.SelectionMode = ListViewSelectionMode.None;
            }
        }


        void HardwareButtonsOnBackPressed(object sender, BackRequestedEventArgs e)
        {
            SongList.SelectionMode = ListViewSelectionMode.None;
        }

        private void ShowFlyoutClick(object sender, RoutedEventArgs e)
        {
            //FlyoutBase.ShowAttachedFlyout(FlyoutPanel);
        }

        private void AddANewPlaylist_OnClick(object sender, RoutedEventArgs e)
        {
            SheetUtility.OpenAddAPlaylistPage();
        }

        async void PlayClicked(object sender, RoutedEventArgs e)
        {
            if (SongList.SelectedItems.Count > 0)
            {
                var tracks = SongList.SelectedItems.Cast<Song>().ToList();
                await PlayAndQueueHelper.PlaySongsAsync(tracks[0], tracks, true);
                HardwareButtonsOnBackPressed(null, null);
                if (tracks != null)
                    tracks = null;
            }
            else
                ToastManager.ShowError("Select an item first.");
        }

        private void AddToPlaylistClick(object sender, RoutedEventArgs e)
        {
            if (SongList.SelectedItems.Count > 0)
            {
                var tracks = SongList.SelectedItems.Cast<Song>().ToList();
                HardwareButtonsOnBackPressed(null, null);
                AddDeleteShareManager.AddToPlaylist(tracks);
                if (tracks != null)
                    tracks = null;
            }
            else
                ToastManager.ShowError("Select an item first.");
        }

        private void DeleteClicked(object sender, RoutedEventArgs e)
        {
            if (SongList.SelectedItems.Count > 0)
            {
                AddDeleteShareManager.Delete(SongList.SelectedItems.Cast<Song>().ToList());
                HardwareButtonsOnBackPressed(null, null);
            }
            else
                ToastManager.ShowError("Select an item first.");
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            HardwareButtonsOnBackPressed(null, null);
        }

    }
}
