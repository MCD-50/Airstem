using Musicus.Core.WinRt.Common;
using Musicus.Utilities;
using Musicus.ViewModel.Mvvm.Dispatcher;
using Windows.UI;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Musicus
{
    public sealed partial class SidePanel
    {
        public SidePanel()
        {
            InitializeComponent();
            //home.Background = new SolidColorBrush(Color.FromArgb(255, 38, 38, 38));
            OnFirstLaunch();
        }

        private void OnFirstLaunch()
        {
            PerformOperations();
            HomeIcon.Opacity = 1;
        }

        private void PerformOperations()
        {
            ChangeOpacity();
            //SearchPage.ClearDataOnNavigatedFrom();
        }
      
        
        private void ChangeOpacity()
        {
            HomeIcon.Opacity = .4;
            SongIcon.Opacity = .4;
            //VideoIcon.Opacity = .4;
            PlaylistIcon.Opacity = .4;
            DownloadIcon.Opacity = .4;
            //SettingIcon.Opacity = .4;
            SettingIcon.Opacity = .4;
            PlayingPanel.Opacity = .4;
            //SearchIcon.Opacity = .4;    
        }

       
        private async void OpenSongList(object sender, RoutedEventArgs e)
        {
            if (App.Navigator.CurrentPage is CollectionPage) return;
            await DispatcherHelper.RunAsync(() =>
            {
                App.Navigator.GoTo<CollectionPage, ZoomInTransition>(0);
                PerformOperations();
                SongIcon.Opacity = 1;
            });
        }


        //private async void OpenVideoList(object sender, RoutedEventArgs e)
        //{
        //    if (App.Navigator.CurrentPage is CollectionPage) return;
        //    else
        //    {
        //        await DispatcherHelper.RunAsync(() =>
        //        {
        //            App.Navigator.GoTo<CollectionPage, ZoomInTransition>(3);
        //            PerformOperations();
        //            VideoIcon.Opacity = 1;
        //        });
        //    }
        //}


        private async void OpenPlaylistList(object sender, RoutedEventArgs e)
        {
            if (App.Navigator.CurrentPage is CollectionPlaylistsPage) return;
            else
            {
                await DispatcherHelper.RunAsync(() =>
                {
                    App.Navigator.GoTo<CollectionPlaylistsPage, ZoomInTransition>(null);
                    PerformOperations();
                    PlaylistIcon.Opacity = 1;
                });
            }
        }

        //private async void OpenSearchPage(object sender, RoutedEventArgs e)
        //{
        //    if (App.Navigator.CurrentPage is SearchPage) return;
        //    else
        //    {
        //        await DispatcherHelper.RunAsync(() =>
        //        {
        //            if (App.Locator.PBar.IsCollectionPage)
        //            {
        //                Helpers.SheetUtility.OpenSearchCollectionPage();
        //                return;
        //            }
        //            else
        //                App.Navigator.GoTo<SearchPage, ZoomInTransition>(null);
        //            PerformOperations();
        //            SearchIcon.Opacity = 1;
        //         });
        //    }
        //}


        private async void OpenSettingsPage(object sender, RoutedEventArgs e)
        {

            if (App.Navigator.CurrentPage is SettingsPage) return;
            else
            {
               await DispatcherHelper.RunAsync(() =>
                {
                    App.Navigator.GoTo<SettingsPage, ZoomInTransition>(null);
                    PerformOperations();
                    SettingIcon.Opacity = 1;
                 });
            }
        }

        private async void OpenDownloadsPage(object sender, RoutedEventArgs e)
        {
            if (App.Navigator.CurrentPage is DownloadManager)
                return;
            else
            {
                await DispatcherHelper.RunAsync(() =>
                {
                    App.Navigator.GoTo<DownloadManager, ZoomInTransition>(null);
                    PerformOperations();
                    DownloadIcon.Opacity = 1;
               });      
            }
        }

        private async void OpenNowPlayingPage(object sender, RoutedEventArgs e)
        {
            if (App.Navigator.CurrentPage is NowPlayingPage) return;
            else
            {
                if (App.Locator.Player.IsPlayerActive)
                {
                    await DispatcherHelper.RunAsync(() =>
                    {
                        App.Navigator.GoTo<NowPlayingPage, ZoomInTransition>(null);
                        PerformOperations();
                        PlayingPanel.Opacity = 1;
                    });               
                }
                else
                    ToastManager.ShowError("Play any song before.");
            }
        }

     

        private async void OpenHomePage(object sender, RoutedEventArgs e)
        {
            if (App.Navigator.CurrentPage is HomePage) return;
            else
            {
                await DispatcherHelper.RunAsync(() =>
                {
                    App.Navigator.GoTo<HomePage, ZoomInTransition>(null);
                    OnFirstLaunch();
                }); 
            }
        }

    }
}
