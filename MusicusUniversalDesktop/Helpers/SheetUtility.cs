
using Musicus.Data.Collection.Model;
using Musicus.Utilities;
using Windows.UI.Core;

namespace Musicus.Helpers
{
    public static class SheetUtility
    {
        private static NowPlayingSheet _currentSheet;
        public static void OpenNowPlaying()
        {
            if (_currentSheet != null) return;
            _currentSheet = new NowPlayingSheet();
            UiBlockerUtility.BlockNavigation();
            ModalSheetUtility.Show(_currentSheet);
            App.SupressBackEvent += HardwareButtonsOnBackPressed;         
        }

        private static void HardwareButtonsOnBackPressed(object sender, BackRequestedEventArgs e)
        {
            UiBlockerUtility.Unblock();
            App.SupressBackEvent -= HardwareButtonsOnBackPressed;
            CloseNowPlaying();
        }

        public static void CloseNowPlaying()
        {            
            if (_currentSheet == null) return;
            ModalSheetUtility.Hide(_currentSheet);
            _currentSheet = null;
        }


        //open an close add to playlists page.
        private static AddToPlaylistPage _AddToPlaylistPage;
        public static void OpenAddToPlaylistPage()
        {
            if (_AddToPlaylistPage != null) return;
            _AddToPlaylistPage = new AddToPlaylistPage();
            ModalSheetUtility.Show(_AddToPlaylistPage);           
            UiBlockerUtility.BlockNavigation();
        }

        public static void CloseAddToPlaylistPage()
        {
            UiBlockerUtility.Unblock();
            if (_AddToPlaylistPage == null) return;
            ModalSheetUtility.Hide(_AddToPlaylistPage);
            _AddToPlaylistPage = null;
        }


        //open and close collectionSearchPage.
        private static SearchCollectionPage _SearchCollectionPage;
        public static void OpenSearchCollectionPage()
        {
            if (_SearchCollectionPage != null) return;
            _SearchCollectionPage = new SearchCollectionPage();
            UiBlockerUtility.BlockNavigation();
            ModalSheetUtility.Show(_SearchCollectionPage);
            App.SupressBackEvent += HardwareButtonsOnBackPressedForSearchCollection;
        }

        private static void HardwareButtonsOnBackPressedForSearchCollection(object sender, BackRequestedEventArgs e)
        {
            UiBlockerUtility.Unblock();
            App.SupressBackEvent -= HardwareButtonsOnBackPressedForSearchCollection;
            CloseSearchCollectionPage();
        }
        public static void CloseSearchCollectionPage()
        {
            UiBlockerUtility.Unblock();
            if (_SearchCollectionPage == null) return;
            ModalSheetUtility.Hide(_SearchCollectionPage);
            _SearchCollectionPage = null;
        }


        //open and close videomatch.
        private static VideoPage _VideoPage;
        public static void OpenVideoPage(object Object)
        {
            if (_VideoPage != null) return;
            _VideoPage = new VideoPage();
            ModalSheetUtility.PassingObject = Object;
            UiBlockerUtility.BlockNavigation();
            ModalSheetUtility.Show(_VideoPage);
        }

        public static void CloseVideoPage()
        {
            UiBlockerUtility.Unblock();
            if (_VideoPage == null) return;
            ModalSheetUtility.Hide(_VideoPage);
            _VideoPage = null;
        }


        //open and close manualmatch.
        private static ManualMatchPage _ManualMatchPage;
        public static void OpenManualMatchPage(Song song)
        {
            if (_ManualMatchPage != null) return;
            _ManualMatchPage = new ManualMatchPage();
            UiBlockerUtility.BlockNavigation();
            ModalSheetUtility.PassingObject = song;
            //App.Locator.Manual.ReceiveSong(song);
            ModalSheetUtility.Show(_ManualMatchPage);
            App.SupressBackEvent += HardwareButtonsOnBackPressedForManualMatchPage;
        }

        private static void HardwareButtonsOnBackPressedForManualMatchPage(object sender, BackRequestedEventArgs e)
        {
            UiBlockerUtility.Unblock();
            App.SupressBackEvent -= HardwareButtonsOnBackPressedForManualMatchPage;
            CloseManualMatchPage();
        }

        public static void CloseManualMatchPage()
        {
            UiBlockerUtility.Unblock();
            if (_ManualMatchPage == null) return;
            ModalSheetUtility.Hide(_ManualMatchPage);
            _ManualMatchPage = null;
        }

        //open and close EditDetails.
        private static EditTrackMetadata _EditTrackMetadata;
        public static void OpenEditTrackMetadataPage(object Object)
        {

            if (_EditTrackMetadata != null) return;
            _EditTrackMetadata = new EditTrackMetadata();
            ModalSheetUtility.PassingObject = Object;
            UiBlockerUtility.BlockNavigation();
            ModalSheetUtility.Show(_EditTrackMetadata);
        }
  
        public static void CloseEditTrackMetadataPage()
        {
            UiBlockerUtility.Unblock();
            if (_EditTrackMetadata == null) return;
            ModalSheetUtility.Hide(_EditTrackMetadata);
            _EditTrackMetadata = null;
        }

        //open and close AddAPlaylistpage.
        private static AddAPlaylist _AddAPlaylist;
        public static void OpenAddAPlaylistPage()
        {
            if (_AddAPlaylist != null) return;
            _AddAPlaylist = new AddAPlaylist();
            UiBlockerUtility.BlockNavigation();
            ModalSheetUtility.Show(_AddAPlaylist);
            App.SupressBackEvent += HardwareButtonsOnBackPressedForAddAPlaylist;
        }

        private static void HardwareButtonsOnBackPressedForAddAPlaylist(object sender, BackRequestedEventArgs e)
        {
            UiBlockerUtility.Unblock();
            App.SupressBackEvent -= HardwareButtonsOnBackPressedForAddAPlaylist;
            CloseAddAPlaylistPage();
        }
        public static void CloseAddAPlaylistPage()
        {
            UiBlockerUtility.Unblock();
            if (_AddAPlaylist == null) return;
            ModalSheetUtility.Hide(_AddAPlaylist);
            _AddAPlaylist = null;
        }


        //open and close FolderPage.
        private static AddedFolders _AddedFolders;
        public static void OpenAddedFoldersPage()
        {
            if (_AddedFolders != null) return;
            _AddedFolders = new AddedFolders();
            UiBlockerUtility.BlockNavigation();     
            ModalSheetUtility.Show(_AddedFolders);
            App.SupressBackEvent += HardwareButtonsOnBackPressedForAddedFoldersPage;
        }

        private static void HardwareButtonsOnBackPressedForAddedFoldersPage(object sender, BackRequestedEventArgs e)
        {
            UiBlockerUtility.Unblock();
            App.SupressBackEvent -= HardwareButtonsOnBackPressedForAddedFoldersPage;
            CloseAddedFoldersPage();
        }

        public static void CloseAddedFoldersPage()
        {
            UiBlockerUtility.Unblock();
            if (_AddedFolders == null) return;
            ModalSheetUtility.Hide(_AddedFolders);
            _AddedFolders = null;
        }


        //open and close ColorPage.
        private static ColorView _ColorView;
        public static void OpenColorViewPage()
        {
            if (_ColorView != null) return;
            _ColorView = new ColorView();
            UiBlockerUtility.BlockNavigation();
            ModalSheetUtility.Show(_ColorView);
            App.SupressBackEvent += HardwareButtonsOnBackPressedForColorViewPage;
        }

        private static void HardwareButtonsOnBackPressedForColorViewPage(object sender, BackRequestedEventArgs e)
        {
            UiBlockerUtility.Unblock();
            App.SupressBackEvent -= HardwareButtonsOnBackPressedForColorViewPage;
            CloseColorViewPage();
        }

        public static void CloseColorViewPage()
        {
            UiBlockerUtility.Unblock();
            if (_ColorView == null) return;
            ModalSheetUtility.Hide(_ColorView);
            _ColorView = null;
        }


        private static ImportRefreshPage _ImportRefreshPage;
        public static void OpenImportRefreshPage(int i)
        {
            if (_ImportRefreshPage != null) return;
            _ImportRefreshPage = new ImportRefreshPage();
            _ImportRefreshPage.index = i;
            UiBlockerUtility.BlockNavigation();
            ModalSheetUtility.Show(_ImportRefreshPage);
        }

        public static void CloseImportRefreshPage()
        {
            UiBlockerUtility.Unblock();
            if (_ImportRefreshPage == null) return;
            ModalSheetUtility.Hide(_ImportRefreshPage);
            _ImportRefreshPage = null;
        }
       
    }
}