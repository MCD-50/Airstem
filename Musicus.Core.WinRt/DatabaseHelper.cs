#region

using System;
using System.Collections.Generic;
using System.IO;
using Musicus.Core.Utils.Interfaces;
using Musicus.Data.Collection;
using Musicus.Data.Collection.Model;
using Musicus.Data.Collection.RunTime;
using Musicus.Core.WinRt;
using SQLite.Net;


#endregion

// ReSharper disable once CheckNamespace
namespace Musicus
{
    public class DatabaseHelper
    {
        private readonly IAppSettingsHelper _appSettingsHelper;
        private readonly IBitmapFactory _bitmapFactory;
        private readonly IDispatcherHelper _dispatcher;
        private readonly string folderPath;

        public DatabaseHelper(
            IDispatcherHelper dispatcher,
            IAppSettingsHelper appSettingsHelper,
            IBitmapFactory bitmapFactory)
        {
            _dispatcher = dispatcher;
            _appSettingsHelper = appSettingsHelper;
            _bitmapFactory = bitmapFactory;
            folderPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        }

        public ICollectionService CreateCollectionService(ISqlService collectionSqlService, ISqlService playerSqlService)
        {
            return new CollectionService(
                collectionSqlService,  
                playerSqlService,             
                _appSettingsHelper,
                _dispatcher,
                _bitmapFactory,
                AppConstant.MissingArtworkImage,
                AppConstant.LocalStorageAppPath,
                AppConstant.ArtworkPath,
                AppConstant.ArtistsArtworkPath,
                AppConstant.VideoArtworkPath);
        }

        public ISqlService CreateCollectionSqlService(int version, Action<SQLiteConnection, double> onUpdate = null)
        {
            var dbTypes = new List<Type>
                              {
                                  typeof(Artist), 
                                  typeof(Album), 
                                  typeof(Song), 
                                  typeof(Video),
                                  typeof(Playlist), 
                                  typeof(PlaylistSong),
                                  typeof(Lyrics),
                                  typeof(Folder)
                              };
            var config = new SqlServiceConfig
            {
                Tables = dbTypes,
                CurrentVersion = version,
                Path = Path.Combine(folderPath, Core.PlayerConstants.DataBasePath),
                //OnUpdate = onUpdate
            };

            return new SqlService(config);
        }

        public ISqlService CreatePlayerSqlService(int version, Action<SQLiteConnection, double> onUpdate = null)
        {
            var dbTypes = new List<Type>
            {
                typeof(QueueSong)
            };
            var config = new SqlServiceConfig
            {
                Tables = dbTypes,
                CurrentVersion = version,
                Path = Path.Combine(folderPath, Core.PlayerConstants.BgDataBasePath),
                //OnUpdate = onUpdate
            };
            return new SqlService(config);
        }
    }
}

