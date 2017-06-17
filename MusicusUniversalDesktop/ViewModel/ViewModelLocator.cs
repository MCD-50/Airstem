#region
using Microsoft.Practices.ServiceLocation;
using Musicus.Core.Utils;
using Musicus.Core.Utils.Interfaces;
using Musicus.Core.WinRt;
using Musicus.Core.WinRt.Utilities;
using Musicus.Data;
using Musicus.Data.Collection;
using Musicus.Data.Service.Interfaces;
using Musicus.Data.Service.RunTime;
using Musicus.Data.Spotify;
using Musicus.Helpers;
using Musicus.Helpers.Triggers;
using Musicus.PlayerHelpers;
using Musicus.Utilities;
using Musicus.ViewModel.Mvvm.Dispatcher;
using Musicus.ViewModel.Mvvm.SimpleIoc;

#endregion

namespace Musicus.ViewModel
{

    public class ViewModelLocator
    {

        public ViewModelLocator()
        {

            //saving and registering viewmodels in container.
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<ICredentialHelper, PclCredentialHelper>();
            SimpleIoc.Default.Register<IAppSettingsHelper, AppSettingsHelper>();
            SimpleIoc.Default.Register<IBitmapFactory, PclBitmapFactory>();
            SimpleIoc.Default.Register<IMp3SearchService, Mp3SearchService>();

            SimpleIoc.Default.Register<SpotifyWebApi>();
            SimpleIoc.Default.Register<ISpotifyService, SpotifyService>();
            SimpleIoc.Default.Register<IMiscellaneousService, MiscellaneousService>();
            SimpleIoc.Default.Register<IVideoUrlService, VideoUrlService>();
            SimpleIoc.Default.Register<IDispatcherHelper>(() => new PclDispatcherHelper(DispatcherHelper.UIDispatcher));
            SimpleIoc.Default.Register<IDeezerService, DeezerService>();
            SimpleIoc.Default.Register<IScrobblerService, ScrobblerService>();

            SimpleIoc.Default.Register<ITriggerValue, NetworkConnectionStateTrigger>();

            var factory = new DatabaseHelper(PclDispatcherHelper, AppSettingsHelper, BitmapFactory);

            SimpleIoc.Default.Register(() => factory.CreateCollectionSqlService(11, async (connection, d) =>
            {
                if (!(d > 0) || !(d < 8)) return;

                if (App.Locator.CollectionService.IsLibraryLoaded)
                    await CollectionHelper.MigrateAsync();
                else
                    App.Locator.CollectionService.LibraryLoaded += async (sender, args) =>
                      await CollectionHelper.MigrateAsync();
            }));

            SimpleIoc.Default.Register(() => factory.CreatePlayerSqlService(6), "BackgroundSql");
            SimpleIoc.Default.Register(() => factory.CreateCollectionService(SqlService, BgSqlService));
          

            SimpleIoc.Default.Register<ISongDownloadService>(() => new SongDownloadService(CollectionService, SqlService));
            SimpleIoc.Default.Register<Mp3MatchEngine>();
            SimpleIoc.Default.Register<CollectionCommandHelper>();
            SimpleIoc.Default.Register<AudioPlayerHelper>();
            SimpleIoc.Default.Register<CollectionViewModel>();
            SimpleIoc.Default.Register<CollectionSearchViewModel>();
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<NowPlayingViewModel>();
            SimpleIoc.Default.Register<PlayerViewModel>();
            SimpleIoc.Default.Register<VideoPlayerViewModel>();
            SimpleIoc.Default.Register<CollectionAlbumViewModel>();
            SimpleIoc.Default.Register<CollectionArtistViewModel>();
            SimpleIoc.Default.Register<CollectionPlaylistViewModel>();
            SimpleIoc.Default.Register<SearchViewModel>();
            SimpleIoc.Default.Register<RootViewModel>();
            SimpleIoc.Default.Register<SettingViewModel>();
            SimpleIoc.Default.Register<SpotifyAlbumViewModel>();
            SimpleIoc.Default.Register<SpotifyArtistViewModel>();
            SimpleIoc.Default.Register<DownloadManagerViewModel>();
            SimpleIoc.Default.Register<ManualMatchViewModel>();

        }


        public IMiscellaneousService MiscallaneousService
        {
            get { return ServiceLocator.Current.GetInstance<IMiscellaneousService>(); }
        }

        public SpotifyWebApi Spotify
        {
            get { return ServiceLocator.Current.GetInstance<SpotifyWebApi>(); }
        }

        public IAppSettingsHelper AppSettingsHelper
        {
            get { return ServiceLocator.Current.GetInstance<IAppSettingsHelper>(); }
        }

        public ITriggerValue Network
        {
            get { return ServiceLocator.Current.GetInstance<ITriggerValue>(); }
        }

        public IDeezerService DeezerService
        {
            get { return ServiceLocator.Current.GetInstance<IDeezerService>(); }
        }

        public IDispatcherHelper PclDispatcherHelper
        {
            get { return ServiceLocator.Current.GetInstance<IDispatcherHelper>(); }
        }

        public IBitmapFactory BitmapFactory
        {
            get { return ServiceLocator.Current.GetInstance<IBitmapFactory>(); }
        }

        public ICredentialHelper CredentialHelper
        {
            get { return ServiceLocator.Current.GetInstance<ICredentialHelper>(); }
        }

        public IMp3SearchService Mp3Search
        {
            get { return SimpleIoc.Default.GetInstance<IMp3SearchService>(); }
        }

        public Mp3MatchEngine Mp3MatchEngine
        {
            get { return ServiceLocator.Current.GetInstance<Mp3MatchEngine>(); }
        }

        public RootViewModel PBar
        {
            get { return SimpleIoc.Default.GetInstance<RootViewModel>(); }
        }

        public MainViewModel HomePage
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        public CollectionSearchViewModel CollectionSearch
        {
            get { return ServiceLocator.Current.GetInstance<CollectionSearchViewModel>(); }
        }

 

        public SettingViewModel Setting
        {
            get { return ServiceLocator.Current.GetInstance<SettingViewModel>(); }
        }

        public NowPlayingViewModel NowPlaying
        {
            get { return ServiceLocator.Current.GetInstance<NowPlayingViewModel>(); }
        }

        public CollectionAlbumViewModel CollectionAlbum
        {
            get { return ServiceLocator.Current.GetInstance<CollectionAlbumViewModel>(); }
        }

        public ManualMatchViewModel Manual
        {
            get { return ServiceLocator.Current.GetInstance<ManualMatchViewModel>(); }
        }

        public CollectionPlaylistViewModel CollectionPlaylist
        {
            get { return ServiceLocator.Current.GetInstance<CollectionPlaylistViewModel>(); }
        }

        public PlayerViewModel Player
        {
            get { return ServiceLocator.Current.GetInstance<PlayerViewModel>(); }
        }

        public VideoPlayerViewModel VideoPlayer
        {
            get { return ServiceLocator.Current.GetInstance<VideoPlayerViewModel>(); }
        }

        public AudioPlayerHelper AudioPlayerHelper
        {
            get { return ServiceLocator.Current.GetInstance<AudioPlayerHelper>(); }
        }

     
        public CollectionArtistViewModel CollectionArtist
        {
            get { return ServiceLocator.Current.GetInstance<CollectionArtistViewModel>(); }
        }

        public SearchViewModel Search
        {
            get { return ServiceLocator.Current.GetInstance<SearchViewModel>(); }
        }

        public DownloadManagerViewModel DownloadHelper
        {
            get { return ServiceLocator.Current.GetInstance<DownloadManagerViewModel>(); }
        }


        public CollectionViewModel Collection
        {
            get { return ServiceLocator.Current.GetInstance<CollectionViewModel>(); }
        }

        public SpotifyAlbumViewModel SpotifyAlbum
        {
            get { return ServiceLocator.Current.GetInstance<SpotifyAlbumViewModel>(); }
        }

        public SpotifyArtistViewModel SpotifyArtist
        {
            get { return ServiceLocator.Current.GetInstance<SpotifyArtistViewModel>(); }
        }



        public ICollectionService CollectionService
        {
            get { return SimpleIoc.Default.GetInstance<ICollectionService>(); }
        }

        public IScrobblerService ScrobblerService
        {
            get { return SimpleIoc.Default.GetInstance<IScrobblerService>(); }
        }

        public ISongDownloadService Download
        {
            get { return SimpleIoc.Default.GetInstance<ISongDownloadService>(); }
        }

        public ISqlService SqlService
        {
            get { return SimpleIoc.Default.GetInstance<ISqlService>(); }
        }

        public ISqlService BgSqlService
        {
            get { return SimpleIoc.Default.GetInstance<ISqlService>("BackgroundSql"); }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}