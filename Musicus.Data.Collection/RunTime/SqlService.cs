#region
using Musicus.Data.Collection.Model;
using SQLite.Net;
using SQLite.Net.Interop;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#endregion

namespace Musicus.Data.Collection.RunTime
{
    public class SqlService : ISqlService
    {
        private readonly SqlServiceConfig _config;

        public SqlService(SqlServiceConfig config)
        {
            _config = config;
        }

        public SqlService()
        {

        }

        public SQLiteConnection DbConnection { get; set; }
        bool _error = false;

        public async Task<bool> InitializeAsync()
        {
            return await Task.Run(() => Initialize()).ConfigureAwait(false);
        }

        public bool Initialize(bool walMode = true, bool readOnlyMode = false)
        {
            var flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create;
            if (readOnlyMode)
            {
                flags = SQLiteOpenFlags.ReadOnly;
            }

            // this.DbConnection = new SQLiteConnection(this.config.Path, flags);

            try
            {
                DbConnection = new SQLiteConnection(sqlitePlatform: new SQLitePlatformWinRT(), databasePath: _config.Path, openFlags: flags);
                // using wal so the player and app can access the db without worrying about it being locked
                DbConnection.ExecuteScalar<string>("PRAGMA journal_mode = " + (walMode ? "WAL" : "DELETE"));
                var sqlVersion = DbConnection.ExecuteScalar<int>("PRAGMA user_version");
                if (sqlVersion == _config.CurrentVersion)
                    return _error;
            }
            catch(Exception ex)
            {
                if(ex.HResult == -2146233088 || ex.Message.Equals("unable to open database file",StringComparison.CurrentCultureIgnoreCase) 
                    || ex.Source.Equals("SQLite.Net.Interop.Result.CannotOpen"))
                    _error = true;
                return _error;
            }
                   

            /*
             * Callback function is invoked once for each DELETE, INSERT, or UPDATE operation. 
             * The argument is the number of rows that were changed
             * Turning this off will give a small speed boost 
             */
            DbConnection.ExecuteScalar<string>("PRAGMA count_changes = OFF");
            // Data integrity is not a top priority, performance is.
            DbConnection.ExecuteScalar<string>("PRAGMA synchronous = OFF");
            DbConnection.ExecuteScalar<string>("PRAGMA foreign_keys = ON");
            CreateTablesIfNotExists();
            return _error;
        }

        public bool Error()
        {
            return _error;
        }

        public async Task<bool> InsertAsync(BaseEntry entry)
        {
            return await Task.FromResult(Insert(entry)).ConfigureAwait(false);
        }

        public bool Insert(BaseEntry entry)
        {
            try
            {
                DbConnection.Insert(entry);
                return true;
            }    
            catch
            {
                if (entry is Lyrics)
                    UpdateItem(entry);
                return false;
            }
        }


        public async Task<bool> DeleteItemAsync(BaseEntry item)
        {
            return await Task.FromResult(DeleteItem(item)).ConfigureAwait(false);
        }

        public bool DeleteItem(BaseEntry item)
        {
            try
            {
                DbConnection.Delete(item);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> UpdateItemAsync(BaseEntry item)
        {
            return await Task.FromResult(UpdateItem(item)).ConfigureAwait(false);
        }



        public string UpdateItem(BaseEntry item)
        {
            try
            {
                DbConnection.Update(item);
                return "true";
            }

            catch(Exception exception)
            {
                if(exception.Message.ToLower().Contains("no such column"))
                    return Core.StringMessage.ErrorDatabase;
                return "false";
            }

        }

        public int GetQueueSize(string path)
        {

            using (var db = new SQLiteConnection(sqlitePlatform: new SQLitePlatformWinRT(), databasePath: path))
            {
                return db.Table<QueueSong>().ToList().Count - 1;
            }
        }

        public void Dispose()
        {
            DbConnection.Dispose();
            GC.Collect();
        }

        private void CreateTablesIfNotExists()
        {
            foreach (var type in _config.Tables)
            {
                DbConnection.CreateTable(type);
            }

            UpdateDbVersion(_config.CurrentVersion);
        }

        private void UpdateDbVersion(double version)
        {
            DbConnection.Execute("PRAGMA user_version = " + version);
        }


        public string GetLyrics(string songName, string artistName)
        {
            try
            {
                var o = DbConnection.Table<Lyrics>().ToList();
                if (o != null)
                {
                    var content = o.FirstOrDefault(p => p.SongName.Equals(songName, StringComparison.CurrentCultureIgnoreCase) && p.ArtistName.Equals(artistName, StringComparison.CurrentCultureIgnoreCase));
                    if (content == null)
                        return string.Empty;
                    return content.Content;
                }
            }

            catch
            {
                return string.Empty;
            }

            return string.Empty;
        }

        public List<Song> GetAllTracks()
        {
            try
            {
                return DbConnection.Table<Song>().ToList();
            }
            catch
            {

            }
            return null;
        }


        public List<Folder> GetAllFolders()
        {

            try
            {
                return DbConnection.Table<Folder>().ToList();
            }
            catch
            {

            }
            return null;
        }


        public List<Artist> GetAllArtists()
        {
            try
            {
                return DbConnection.Table<Artist>().ToList();
            }
            catch
            {

            }
            return null;
        }

        public List<Album> GetAllAlbums()
        {
            try
            {
                return DbConnection.Table<Album>().ToList();
            }
            catch
            {

            }
            return null;
        }

        public List<Video> GetAllVideos()
        {
            try
            {
                return DbConnection.Table<Video>().ToList();
            }
            catch
            {

            }
            return null;
        }


        public List<QueueSong> GetAllQueueSong()
        {
            try
            {
                return DbConnection.Table<QueueSong>().ToList();
            }
            catch
            {

            }
            return null;
        }

        public List<Playlist> GetAllPlaylists()
        {
            try
            {
                return DbConnection.Table<Playlist>().ToList();
            }
            catch
            {

            }
            return null;
        }

        public List<PlaylistSong> GetAllPlaylistsTracks()
        {
            try
            {
                return DbConnection.Table<PlaylistSong>().ToList();
            }
            catch
            {

            }
            return null;
        }


        //public async Task<List<T>> SelectAllAsync<T>() where T : class
        //{
        //    return await Task.Factory.StartNew(() => SelectAll<T>()).ConfigureAwait(false);
        //}

        //public List<T> SelectAll<T>() where T : class
        //{
        //    try
        //    {
        //        return DbConnection.Table<T>().ToList();
        //    }
        //    catch
        //    {
        //        // ignored
        //    }

        //    return new List<T>();
        //}

        public QueueSong GetFirstQueueTrack(Func<QueueSong, bool> expression, string path)
        {
            try
            {
                using (var db = new SQLiteConnection(sqlitePlatform: new SQLitePlatformWinRT(), databasePath: path))
                {
                    return db.Table<QueueSong>().FirstOrDefault(expression);
                }
                //return this.DbConnection.Table<QueueSong>().FirstOrDefault(expression);
            }
            catch
            {

            }
            return null;
        }


        public Song GetFirstTrack(Func<Song, bool> expression, string path)
        {
            try
            {
                using (var db = new SQLiteConnection(sqlitePlatform: new SQLitePlatformWinRT(), databasePath: path))
                {
                    return db.Table<Song>().FirstOrDefault(expression);
                }

                //return this.DbConnection.Table<Song>().FirstOrDefault(expression);
            }
            catch
            {

            }
            return null;
        }

        public bool GetFirstArtist(int id, string path)
        {
            try
            {
                using (var db = new SQLiteConnection(sqlitePlatform: new SQLitePlatformWinRT(), databasePath: path))
                {
                    return db.Table<Artist>().FirstOrDefault(p=>p.Id == id).HasArtwork;
                }

                //return this.DbConnection.Table<Artist>().FirstOrDefault(expression);
            }
            catch
            {

            }
            return false;
        }

        public bool GetFirstAlbum(int id, string path)
        {
            try
            {
                using (var db = new SQLiteConnection(sqlitePlatform: new SQLitePlatformWinRT(), databasePath: path))
                {
                    return db.Table<Album>().FirstOrDefault(p=>p.Id == id).HasArtwork;
                }

                //return this.DbConnection.Table<Album>().FirstOrDefault(expression);
            }
            catch
            {

            }
            return false;
        }



        public Task DeleteTableAsync<T>()
        {
            return Task.Run(
                () =>
                {
                    DbConnection.DeleteAll<T>();
                    DbConnection.Execute("DELETE FROM sqlite_sequence WHERE name = '" + typeof(T).Name + "'");
                });
        }

        public void BeginTransaction()
        {
            try
            {
                DbConnection.BeginTransaction();
            }

            catch (InvalidOperationException)
            {

            }

            catch (SQLiteException)
            {

            }
        }

        public void Commit()
        {
            try
            {
                DbConnection.Commit();
            }

            catch (InvalidOperationException)
            {

            }

            catch (SQLiteException)
            {

            }
        }

        
    }


    public class SqlServiceConfig
    {
       // public Action<SQLiteConnection, double> OnUpdate;
        public int CurrentVersion { get; set; }
        public string Path { get; set; }
        public List<Type> Tables { get; set; }
    }

}