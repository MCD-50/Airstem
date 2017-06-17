#region
using Musicus.Core.Utils;
using Musicus.Data.Collection.Model;
using Musicus.Helpers;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System;
using Musicus.Utilities;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls.Primitives;
using Musicus.Core.WinRt.Common;
using System.Collections.Generic;
#endregion

namespace Musicus
{
    public sealed partial class CollectionPage
    {

        private TypedEventHandler<ListViewBase, ContainerContentChangingEventArgs> _delegate;

        public CollectionPage()
        {
            this.InitializeComponent();
        }

        public override void NavigatedTo(NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);
            if (parameter == null) return;
            var pivotIndex = (int)parameter;
            MainPivot.SelectedIndex = pivotIndex;
            //App.Locator.PBar.IsCollectionPage = true;
            //App.Locator.PBar.Select += PBarSelect;
        }

        private void PBarSelect(object sender, EventArgs e)
        {
            OnEdgeTapped(null, null);
        }

        public override void NavigatedFrom(NavigationMode mode)
        {
            base.NavigatedFrom(mode);
            //App.Locator.PBar.IsCollectionPage = false;
            //App.Locator.PBar.Select -= PBarSelect;
            HardwareButtonsOnBackPressed(null, null);
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


        private void OpenSearchCollection(object sender, RoutedEventArgs e)
        {
            SheetUtility.OpenSearchCollectionPage();
        }

        private async void ShuffleAll(object sender, RoutedEventArgs e)
        {
            App.Locator.PBar.IsEnable = true;
            S1.IsEnabled = false;
            S2.IsEnabled = false;
            S3.IsEnabled = false;
            S4.IsEnabled = false;
            await DispatcherHelper.RunAsync(async () =>
            {
                var list = App.Locator.CollectionService.Songs.ToList().Shuffle().ToList();
                await PlayAndQueueHelper.PlaySongsAsync(list[0], list);
                S1.IsEnabled = true;
                S2.IsEnabled = true;
                S3.IsEnabled = true;
                S4.IsEnabled = false;
                App.Locator.PBar.IsEnable = false;
            });
        }

        private void OnEdgeTapped(object sender, RoutedEventArgs e)
        {
            if (SongList.SelectionMode == ListViewSelectionMode.Multiple)
            {
                SongList.SelectionMode = ListViewSelectionMode.None;
                OnSelectionChanged();
            }
            else
            {
                SongList.SelectionMode = ListViewSelectionMode.Multiple;
                OnSelectionChanged(true);
            }
        }

        private void OnSelectionChanged(bool isMulti = false)
        {

            if (isMulti)
            {
                App.SupressBackEvent += HardwareButtonsOnBackPressed;
                UiBlockerUtility.BlockNavigation(false);
                SongList.IsItemClickEnabled = false;
                OptionButton.Visibility = Visibility.Visible;
                SongViewer._showFlyout = false;
                SelectCancelTextBlock.Text = "Cancel";
                //SongList.SelectionMode = ListViewSelectionMode.Multiple;
            }

            else
            {
                App.SupressBackEvent -= HardwareButtonsOnBackPressed;
                UiBlockerUtility.Unblock();
                SongList.IsItemClickEnabled = true;
                OptionButton.Visibility = Visibility.Collapsed;
                SongViewer._showFlyout = true;
                SelectCancelTextBlock.Text = "Select";
                //SongList.SelectionMode = ListViewSelectionMode.None;
            }
        }



        void HardwareButtonsOnBackPressed(object sender, BackRequestedEventArgs e)
        {
            SongList.SelectionMode = ListViewSelectionMode.None;
            OnSelectionChanged();
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

        private void MainPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HardwareButtonsOnBackPressed(null, null);
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            HardwareButtonsOnBackPressed(null, null);
        }
    }
}