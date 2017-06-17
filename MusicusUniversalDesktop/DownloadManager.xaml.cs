using Musicus.Core.WinRt.Common;
using Musicus.Data.Collection.Model;
using Musicus.Helpers;
using Musicus.Utilities;
using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace Musicus
{
    public sealed partial class DownloadManager
    {
        private TypedEventHandler<ListViewBase, ContainerContentChangingEventArgs> _delegate;      
        public DownloadManager()
        {
            this.InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                MainPivot.Title = "MANAGE";
                MainPivotItem1.Header = "downloads";
            }

            //SongList.SelectionChanged += OnSelectionChanged;
        }


    

        private void PBarSelect(object sender, EventArgs e)
        {
            OnEdgeTapped(null, null);
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
                        songViewer.ShowPlaceholder(args.Item as Song);
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


            args.Handled = true;

        }

        private void OpenNowPlaying(object sender, RoutedEventArgs e)
        {
            AddDeleteShareManager.OpenNowPlaying();
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
                OptionButton.Visibility = Visibility.Visible;
                SongList.SelectionMode = ListViewSelectionMode.Multiple;
            }

            else //if (!SongList.IsItemClickEnabled)
            {
                App.SupressBackEvent -= HardwareButtonsOnBackPressed;
                UiBlockerUtility.Unblock();
                SongList.IsItemClickEnabled = true;
                OptionButton.Visibility = Visibility.Collapsed;
                SongList.SelectionMode = ListViewSelectionMode.None;
            }
        }


        void HardwareButtonsOnBackPressed(object sender, BackRequestedEventArgs e)
        {
            SongList.SelectionMode = ListViewSelectionMode.None;
        }

        private void ShowFlyoutClick(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(FlyoutPanel);
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