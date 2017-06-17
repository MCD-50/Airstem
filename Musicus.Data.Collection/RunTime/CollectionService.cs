using Musicus.Core.Common;
using Musicus.Core.Utils;
using Musicus.Core.Utils.Interfaces;
using Musicus.Data.Collection.Model;
using PCLStorage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;

namespace Musicus.Data.Collection.RunTime
{
    public class CollectionService : INotifyPropertyChanged, ICollectionService
    {
        private readonly IAppSettingsHelper _appSettingsHelper;
        private readonly string _artistArtworkFilePath;
        private readonly string _artworkFilePath;
        private readonly string _videoArtworkFilePath;

        private readonly IBitmapFactory _bitmapFactory;
        private readonly IDispatcherHelper _dispatcher;

        private readonly List<Album> _inProgressAlbums = new List<Album>();
        private readonly List<Artist> _inProgressArtists = new List<Artist>();
        private readonly List<Video> _inProgressVideos = new List<Video>();

        private readonly ConcurrentDictionary<long, QueueSong> _lookupMap = new ConcurrentDictionary<long, QueueSong>();
        private readonly string _localFilePrefix;
        private readonly IBitmapImage _missingArtwork;
        private readonly ISqlService _sqlService;
        private readonly ISqlService _bgSqlService;

        public CollectionService(
            ISqlService sqlService,
            ISqlService bgSqlService,
            IAppSettingsHelper appSettingsHelper,
            IDispatcherHelper dispatcher,
            IBitmapFactory bitmapFactory,
            IBitmapImage missingArtwork,
            string localFilePrefix,
            string artworkFilePath,
            string artistArtworkFilePath,
            string videoArtworkFilePath)
        {
            _sqlService = sqlService;
            _bgSqlService = bgSqlService;
            _dispatcher = dispatcher;
            _appSettingsHelper = appSettingsHelper;
            _bitmapFactory = bitmapFactory;
            _missingArtwork = missingArtwork;
            _localFilePrefix = localFilePrefix;
            _artworkFilePath = artworkFilePath;
            _artistArtworkFilePath = artistArtworkFilePath;
            _videoArtworkFilePath = videoArtworkFilePath;
            PlaybackQueue = new OptimizedObservableCollection<QueueSong>();
            ShufflePlaybackQueue = new OptimizedObservableCollection<QueueSong>();
            Init();
        }

        private void Init()
        {
            Songs = new OptimizedObservableCollection<Song>();
            Artists = new OptimizedObservableCollection<Artist>();
            Albums = new OptimizedObservableCollection<Album>();
            Videos = new OptimizedObservableCollection<Video>();
            Playlists = new OptimizedObservableCollection<Playlist>();
            Folders = new OptimizedObservableCollection<Folder>();
        }

        private bool IsShuffle
        {
            get { return _appSettingsHelper.Read<bool>("Shuffle"); }
        }

        public bool IsLibraryLoaded { get; private set; }
        public event EventHandler LibraryLoaded;
        public OptimizedObservableCollection<Song> Songs { get; set; }
        public OptimizedObservableCollection<Album> Albums { get; set; }
        public OptimizedObservableCollection<Artist> Artists { get; set; }
        public OptimizedObservableCollection<Video> Videos { get; set; }
        public OptimizedObservableCollection<Playlist> Playlists { get; set; }
        public OptimizedObservableCollection<Folder> Folders { get; set; }
        public OptimizedObservableCollection<QueueSong> CurrentPlaybackQueue
        {
            get { return IsShuffle ? ShufflePlaybackQueue : PlaybackQueue; }
        }

        public OptimizedObservableCollection<QueueSong> PlaybackQueue { get; }
        public OptimizedObservableCollection<QueueSong> ShufflePlaybackQueue { get; }


        public int ScaledImageSize { get; set; }

  
        public async void LoadLibrary()
        {
            if (IsLibraryLoaded)
            {
                return;
            }


            /*
             * Sqlite makes a transaction to create a shared lock
             * Wrapping it in one single transactions assures it is only lock and release once
             */


            _sqlService.BeginTransaction();
            var songs = _sqlService.GetAllTracks().OrderByDescending(p => p.Id).ToList();
            var artists = _sqlService.GetAllArtists().OrderByDescending(p => p.Id).ToList();
            var albums = _sqlService.GetAllAlbums().OrderByDescending(p => p.Id).ToList();
            var videos = _sqlService.GetAllVideos().OrderByDescending(p => p.Id).ToList();
            _sqlService.Commit();

            var isForeground = _dispatcher != null;

            foreach (var song in songs)
            {
                song.Artist = artists.FirstOrDefault(p => p.Id == song.ArtistId);
                song.Album = albums.FirstOrDefault(p => p.Id == song.AlbumId);
            }

            if (isForeground)
            {
                _dispatcher.RunAsync(() => Songs.AddRange(songs)).Wait();
            }
            else
            {
                Songs.AddRange(songs);
            }

            foreach (var album in albums)
            {
                album.Songs.AddRange(songs.Where(p => !p.IsTemp && p.AlbumId == album.Id).OrderBy(p => p.TrackNumber));
                album.PrimaryArtist = artists.FirstOrDefault(p => p.Id == album.PrimaryArtistId);
                if (isForeground)
                {
                    _dispatcher.RunAsync(
                        () =>
                        {
                            var artworkPath = string.Format(_artworkFilePath, album.Id);
                            if (album.HasArtwork)
                            {
                                var path = _localFilePrefix + artworkPath;

                                album.Artwork = _bitmapFactory.CreateImage(new Uri(path));

                                if (ScaledImageSize != 0)
                                {
                                    album.Artwork.SetDecodedPixel(ScaledImageSize);
                                    album.SmallArtwork = _bitmapFactory.CreateImage(new Uri(path));
                                    album.SmallArtwork.SetDecodedPixel(5);
                                }
                            }
                            else
                            {
                                album.Artwork = _missingArtwork;
                                album.SmallArtwork = _missingArtwork;
                            }
                        }).Wait();
                }
            }

            if (isForeground)
            {
                _dispatcher.RunAsync(() => Albums.AddRange(albums)).Wait();
            }
            else
            {
                Albums.AddRange(albums);
            }

            foreach (var artist in artists)
            {
                artist.Songs.AddRange(songs.Where(p => !p.IsTemp && p.ArtistId == artist.Id).OrderBy(p => p.Name));
                artist.Albums.AddRange(
                    albums.Where(p => p.PrimaryArtistId == artist.Id && p.Songs.Count > 0).OrderBy(p => p.Name));

                var songsAlbums = artist.Songs.Select(p => p.Album);
                artist.Albums.AddRange(songsAlbums.Where(p => p.Songs.Count > 0 && !artist.Albums.Contains(p)));
                if (isForeground)
                {
                    _dispatcher.RunAsync(
                        () =>
                        {
                            var artworkPath = string.Format(_artistArtworkFilePath, artist.Id);
                            artist.Artwork = artist.HasArtwork
                                ? _bitmapFactory.CreateImage(
                                    new Uri(_localFilePrefix + artworkPath))
                                : null;
                         
                            if (ScaledImageSize != 0 && artist.Artwork != null)
                            {
                                artist.Artwork.SetDecodedPixel(ScaledImageSize);

                                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
                                {
                                    artist.SmallArtwork = artist.HasArtwork ? _bitmapFactory.CreateImage(                         
                                        new Uri(_localFilePrefix + artworkPath)) : null;
                                    if (artist.SmallArtwork != null)
                                        artist.SmallArtwork.SetDecodedPixel(5);
                                }
                            }

                            else
                            {
                                artist.Artwork = _missingArtwork;
                                artist.SmallArtwork = _missingArtwork;
                            }
                        }).Wait();
                }
            }

            if (isForeground)
            {
                _dispatcher.RunAsync(() => Artists.AddRange(artists)).Wait();
            }
            else
            {
                Artists.AddRange(artists);
            }

            foreach (var video in videos)
            {
                if (isForeground)
                {
                    _dispatcher.RunAsync(
                        () =>
                        {
                            var artworkPath = string.Format(_videoArtworkFilePath, video.Id);
                            if (video.HasArtwork)
                            {
                                var path = _localFilePrefix + artworkPath;
                                video.Artwork = _bitmapFactory.CreateImage(new Uri(path));         
                                if (ScaledImageSize != 0)
                                    video.Artwork.SetDecodedPixel(ScaledImageSize);
                            }
                            else
                                video.Artwork = _missingArtwork;
                        }).Wait();
                }
            }

            if (isForeground)
            {
                _dispatcher.RunAsync(() => Videos.AddRange(videos)).Wait();
            }
            else
            {
                Videos.AddRange(videos);
            }



            IsLibraryLoaded = true;
       
            if (!isForeground) return;

           await LoadRestAsync(songs,albums,artists);
        }

        private async Task LoadRestAsync(IEnumerable<Song> songs, IEnumerable<Album> albums, IEnumerable<Artist> artists)
        {
            await Task.Delay(50);

            LoadQueue(songs);
            LoadPlaylists();
            LoadFolders();
            try
            {
                CleanupFiles(albums, artists);
            }
            catch
            {
                // ignored;
            }

            _dispatcher.RunAsync(() =>LibraryLoaded?.Invoke(this, null)).Wait();
            await DeleteCurruptSongs(songs);
        }

       


        
        public async Task DeleteCurruptSongs(IEnumerable<Song> songs)
        {
            await Task.Delay(50);

            try
            {
                var corruptSongs = songs.Where(p => string.IsNullOrEmpty(p.Name) || p.Album == null || p.Artist == null);
                foreach (var corruptSong in corruptSongs)
                    DeleteSongAsync(corruptSong).Wait();

                //foreach (var song in songs)
                //    if (!song.IsDownload && (!System.IO.File.Exists(song.AudioUrl)))
                //        DeleteSongAsync(song).Wait();
            }

            catch
            {
                //ignored;
            }

        }

        public Task LoadLibraryAsync()
        {
            // just return non async as a task
            return Task.Factory.StartNew(LoadLibrary);
        }

        public bool SongAlreadyExists(string localSongPath)
        {
            return
                Songs.FirstOrDefault(
                    p =>
                        (p.SongState == SongState.Local || p.SongState == SongState.Downloaded)
                        && localSongPath == p.AudioUrl) != null;
        }


        public bool SongAlreadyExists(string name, string album, string artist)
        {
            if (string.IsNullOrEmpty(album))
                return
                Songs.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                (p.ArtistName.Equals(artist, StringComparison.OrdinalIgnoreCase) || p.Artist.Name.Equals(artist, StringComparison.OrdinalIgnoreCase))) != null;

            return
                Songs.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                p.Album.Name.Equals(album, StringComparison.OrdinalIgnoreCase) &&
                (p.ArtistName.Equals(artist, StringComparison.OrdinalIgnoreCase) || p.Artist.Name.Equals(artist, StringComparison.OrdinalIgnoreCase))) != null;
        }

        public bool VideoAlreadyExists(string localSongPath)
        {
            return Videos.FirstOrDefault(p => p.VideoState == VideoState.Local && localSongPath == p.VideoUrl) != null;
        }

        public bool VideoAlreadyExists(string title, string author)
        {
            if (!string.IsNullOrEmpty(author))
                return Videos.FirstOrDefault(p => p.Title.Equals(title, StringComparison.OrdinalIgnoreCase) && p.Author.Equals(author, StringComparison.OrdinalIgnoreCase)) != null;
            return false;
        }

        //public async void ShuffleModeChanged()
        //{
        //    OnPropertyChanged("CurrentPlaybackQueue");
        //    // Only reshuffle when the user turns it off
        //    // So the next time he turns it back on, a new shuffle queue is ready
        //    if (!IsShuffle)
        //        await ShuffleCurrentQueueAsync().ConfigureAwait(false);
        //}

        public bool ViezSongAlreadyExists(string name, string album, string artist)
        {
            if (string.IsNullOrEmpty(album))
                album = "";
            if (string.IsNullOrEmpty(artist))
                artist = "";
            return Songs.FirstOrDefault(p => (p.Name == name && (p.Artist.Name == artist || p.ArtistName == artist) && p.Album.Name == album)) != null;
        }


        public IEnumerable<Song> GetMatchingTracks(string query,int count = 5)
        {
            return Songs.Where(c => c.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > -1 || c.ArtistName.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > -1 || c.Album.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > -1)
                .OrderByDescending(x => x.Name.StartsWith(query, StringComparison.CurrentCultureIgnoreCase))
                .ThenByDescending(c => c.ArtistName.StartsWith(query, StringComparison.CurrentCultureIgnoreCase))
                .ThenByDescending(c=>c.Album.Name.StartsWith(query,StringComparison.CurrentCultureIgnoreCase)).Take(count);    
        }

        public IEnumerable<Album> GetMatchingAlbums(string query, int count = 5)
        {
            return Albums.Where(c => c.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > -1 || c.PrimaryArtist.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > -1)
                .OrderByDescending(x => x.Name.StartsWith(query, StringComparison.CurrentCultureIgnoreCase))
                .ThenByDescending(c => c.PrimaryArtist.Name.StartsWith(query, StringComparison.CurrentCultureIgnoreCase)).Take(count);
        }

        public Artist GetMatchingArtists(string query)
        {
            return Artists.Where(c => c.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > -1).OrderByDescending(x => x.Name.StartsWith(query, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
        }

        public IEnumerable<Video> GetMatchingVideos(string query, int count = 5)
        {
            return Videos.Where(c => c.Title.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > -1)
                .OrderByDescending(x => x.Title.StartsWith(query, StringComparison.CurrentCultureIgnoreCase)).Take(count);
        }


        //public void SetImage()
        //{
        //    foreach (var playlist in Playlists)
        //       SetImageOnNavigtedTo(playlist);
        //}

        //const string MissingArtworkAppPath = "ms-appx:///" + "Assets/MissingArtwork.png";

        //public void SetImageOnNavigtedTo(Playlist playlist)
        //{

        //    if (playlist.Songs.Count == 0)
        //    {
        //        playlist.ArtworkOne = new Uri(MissingArtworkAppPath);
        //        playlist.ArtworkTwo = new Uri(MissingArtworkAppPath);
        //        return;
        //    }

        //    try
        //    {
        //        if ((playlist.Songs.Count > 0 && playlist.ArtworkOne == null) || playlist.ArtworkOne.Equals(MissingArtworkAppPath))
        //        {
        //            var fSong = GetSongById(playlist.Songs.FirstOrDefault().SongId);
        //            var lSong = GetSongById(playlist.Songs.LastOrDefault().SongId);
        //            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
        //            {

        //                if (fSong.Album.HasArtwork)
        //                    playlist.ArtworkOne = fSong.Album.Artwork.Uri;
        //                else if (lSong.Album.HasArtwork)
        //                    playlist.ArtworkOne = lSong.Album.Artwork.Uri;
        //                else
        //                    playlist.ArtworkOne = new Uri(MissingArtworkAppPath);
        //            }
        //            else
        //            {
        //                if (fSong.Album.HasArtwork && lSong.Album.HasArtwork)
        //                {
        //                    playlist.ArtworkOne = fSong.Album.Artwork.Uri;
        //                    playlist.ArtworkTwo = lSong.Album.Artwork.Uri;
        //                }

        //                else if (fSong.Album.HasArtwork && !lSong.Album.HasArtwork)
        //                {
        //                    playlist.ArtworkOne = fSong.Album.Artwork.Uri;
        //                    playlist.ArtworkTwo = new Uri(MissingArtworkAppPath);
        //                }

        //                else if (!fSong.Album.HasArtwork && lSong.Album.HasArtwork)
        //                {
        //                    playlist.ArtworkOne = new Uri(MissingArtworkAppPath);
        //                    playlist.ArtworkTwo = lSong.Album.Artwork.Uri;
        //                }

        //                else
        //                {
        //                    playlist.ArtworkOne = new Uri(MissingArtworkAppPath);
        //                    playlist.ArtworkTwo = new Uri(MissingArtworkAppPath);
        //                }
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        //ignored
        //    }
        //}

        public QueueSong GetQueueSong(int songId)
        {
            return IsShuffle ? ShufflePlaybackQueue.FirstOrDefault(p => p.SongId == songId)
                : PlaybackQueue.FirstOrDefault(q => q.SongId == songId);
        }

        public bool CheckIfExist(int id)
        {
            if (CurrentPlaybackQueue.FirstOrDefault(x => x.SongId == id) != null)
                return true;
            return false;
        }

        public IEnumerable<Song> GetLastPlayed(int count = 5)
        {
            return Songs.OrderByDescending(p => p.LastPlayed).Take(count);
        }

        public IEnumerable<Song> GetLastAdded(int count = 5)
        {
            return Songs.OrderByDescending(p => p.Id).Take(count);
        }

        public IEnumerable<Song> GetDownloads()
        {
            return Songs.Where(p => p.IsDownload);
        }

        public Playlist GetPlayList(int id)
        {
            return Playlists.FirstOrDefault(p => p.Id == id);
        }

        public Artist GetArtist(int id)
        {
            return Artists.FirstOrDefault(p => p.Id == id);
        }

        public Album GetAlbum(int id)
        {
            return Albums.FirstOrDefault(p => p.Id == id);
        }

        public Artist GetArtistByName(string name)
        {
            return Artists.FirstOrDefault(c => c.Name.IndexOf(name, StringComparison.CurrentCultureIgnoreCase) > -1);
        }

        public bool HasArtistArtwork(int id)
        {
            return Artists.FirstOrDefault(p => p.Id == id).HasArtwork;
        }

        public bool HasAlbumArtwork(int id)
        {
            return Albums.FirstOrDefault(p => p.Id == id).HasArtwork;
        }

        public Uri GetArtistImage(int id)
        {
           return Artists.FirstOrDefault(p=>p.Id == id).Artwork.Uri;
        }

        public Uri GetAlbumImage(int id)
        {
            return Albums.FirstOrDefault(p => p.Id == id).Artwork.Uri;
        }

        public Song GetSongById(int id)
        {
            return Songs.FirstOrDefault(p => p.Id == id);
        }

        public Song GetSongByArtistId(int id)
        {
            return Songs.FirstOrDefault(p => p.ArtistId == id);
        }

        public async Task DeleteSongAsync(Song song)
        {

            var queueSong = CurrentPlaybackQueue.FirstOrDefault(p => p.SongId == song.Id);
            if (queueSong != null)
                await DeleteFromQueueAsync(queueSong);

            // remove it from artist, albums and playlists songs
            var playlists = Playlists.Where(p => p.Songs.Count(pp => pp.Song.Id == song.Id) > 0).ToList();

            foreach (var playlist in playlists)
            {
                var songs = playlist.Songs.Where(p => p.Song.Id == song.Id).ToList();
                foreach (var playlistSong in songs)
                {
                    await DeleteFromPlaylistAsync(playlist, playlistSong);
                }

                if (playlist.Songs.Count <= 0)
                {
                    await DeletePlaylistAsync(playlist);
                }
            }


            if (song.Album != null)
            {
                song.Album.Songs.Remove(song);

                // If the album is empty, make sure that is not being used by any temp song

                //&& Songs.Count(p => p.AlbumId == song.AlbumId) < 2
                if (song.Album.Songs.Count <= 0)
                {
                    await _sqlService.DeleteItemAsync(song.Album);
                    await _dispatcher.RunAsync(
                        () =>
                        {
                            Albums.Remove(song.Album);
                            if (song.Artist != null)
                                song.Artist.Albums.Remove(song.Album);
                        });
                }
            }

            if (song.Artist != null)
            {
                song.Artist.Songs.Remove(song);
                // && Songs.Count(p => p.ArtistId == song.ArtistId) < 2
                if (song.Artist.Songs.Count <= 0)
                {
                    await _sqlService.DeleteItemAsync(song.Artist);
                    await _dispatcher.RunAsync(
                        () => { Artists.Remove(song.Artist); });
                }
            }


            // good, now lets delete it from the db
            await _sqlService.DeleteItemAsync(song);
            await _dispatcher.RunAsync(() => Songs.Remove(song));
        }

        private void LoadFolders()
        {
            var folders = _sqlService.GetAllFolders();
            foreach (Folder f in folders)
                Folders.Add(f);
        }

        public async Task AddFolderAsync(Folder f, Windows.Storage.StorageFolder sFolder)
        {
            // good, now lets add it from the db
            await _sqlService.InsertAsync(f);
            await _dispatcher.RunAsync(() => Folders.Add(f));
            StorageApplicationPermissions.FutureAccessList.AddOrReplace(f.Id.ToString(), sFolder);

        }

        public async Task DeleteFolderAsync(Folder f)
        {
            // good, now lets delete it from the db
            await _sqlService.DeleteItemAsync(f);
            await _dispatcher.RunAsync(() => Folders.Remove(f));
           
        }

        public async Task DeleteVideoAsync(Video video)
        {
            // good, now lets delete it from the db
            await _sqlService.DeleteItemAsync(video);
            await _dispatcher.RunAsync(() => Videos.Remove(video));
        }


        public async Task AddSongAsync(Song song, Tag tags = null, bool isLastTrack = false, bool isLocalTrack = false, bool isDeezerTrack = false)
        {
            var primaryArtist = (song.Album == null ? song.Artist : song.Album.PrimaryArtist)
                                ?? new Artist { Name = "Unknown Artist", ProviderId = "autc.unknown" };
       
            var artist =
            _inProgressArtists.Union(Artists).FirstOrDefault(
                entry => string.Equals(entry.Name.ToLower(), primaryArtist.Name.ToLower()));

            if (artist == null)
            {
                await _sqlService.InsertAsync(primaryArtist);
                _inProgressArtists.Add(primaryArtist);

                song.Artist = primaryArtist;
                song.ArtistId = primaryArtist.Id;

                if (song.Album != null)
                {
                    song.Album.PrimaryArtistId = song.Artist.Id;
                    song.Album.PrimaryArtist = song.Artist;
                }
            }
            else
            {
                song.Artist = artist;

                if (song.Album != null)
                {
                    song.Album.PrimaryArtistId = artist.Id;
                    song.Album.PrimaryArtist = artist;
                }
            }

            song.ArtistId = song.Artist.Id;

            if (song.Album == null)
            {
                song.Album = new Album
                {
                    PrimaryArtistId = song.ArtistId,
                    Name = song.Name,
                    PrimaryArtist = song.Artist,
                    ProviderId = "autc.single." + song.ProviderId
                };
            }
            Album album;

          //  if (isLastTrack || isLocalTrack || isDeezerTrack)
                album = _inProgressAlbums.Union(Albums).FirstOrDefault(p => string.Equals(p.Name.ToLower(), song.Album.Name.ToLower())
                && string.Equals(p.PrimaryArtist.Name.ToLower(), song.Album.PrimaryArtist.Name.ToLower()));

        //    else 
              //  album = _inProgressAlbums.Union(Albums).FirstOrDefault(p => p.ProviderId == song.Album.ProviderId);

            if (album != null)
            {
                song.Album = album;
            }
            else
            {
                await _sqlService.InsertAsync(song.Album);
                _inProgressAlbums.Add(song.Album);
                await _dispatcher.RunAsync(() =>
                {
                    song.Album.Artwork = _missingArtwork;
                   
                    song.Album.SmallArtwork = _missingArtwork;
                });

                if (tags != null && tags.Pictures != null && tags.Pictures.Length > 0)
                {
                    var albumFilePath = string.Format(_artworkFilePath, song.Album.Id);
                    Stream artwork = null;

                    var image = tags.Pictures.FirstOrDefault();
                    if (image != null)
                    {
                        artwork = new MemoryStream(image.Data.Data);
                    }

                    if (artwork != null)
                    {
                        using (artwork)
                        {
                            try
                            {
                                var file =
                                    await
                                        StorageHelper.CreateFileAsync(
                                            albumFilePath,
                                            option: CreationCollisionOption.ReplaceExisting);

                                using (var fileStream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
                                {
                                    var bytes = tags.Pictures[0].Data.Data;
                                    await fileStream.WriteAsync(bytes, 0, bytes.Length);
                                    song.Album.HasArtwork = true;
                                    await _sqlService.UpdateItemAsync(song.Album);
                                }
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }

                    // set it
                    if (song.Album.HasArtwork)
                    {
                        await _dispatcher.RunAsync(
                            () =>
                            {
                                var path = _localFilePrefix + albumFilePath;

                                song.Album.Artwork = _bitmapFactory.CreateImage(new Uri(path));

                                if (ScaledImageSize == 0)
                                {
                                    return;
                                }

                                song.Album.Artwork.SetDecodedPixel(ScaledImageSize);


                                song.Album.SmallArtwork = _bitmapFactory.CreateImage(new Uri(path));
                                song.Album.SmallArtwork.SetDecodedPixel(50);
                            });
                    }
                }
            }

            song.AlbumId = song.Album.Id;

            if (string.IsNullOrEmpty(song.ArtistName)) song.ArtistName = song.Artist.Name;

            await _sqlService.InsertAsync(song);

            await _dispatcher.RunAsync(
                () =>
                {
                    if (!song.IsTemp)
                    {
                        var orderedAlbumSong = song.Album.Songs.ToList();
                        orderedAlbumSong.Add(song);
                        orderedAlbumSong = orderedAlbumSong.OrderBy(p => p.TrackNumber).ToList();

                        var index = orderedAlbumSong.IndexOf(song);
                        song.Album.Songs.Insert(index, song);


                        var orderedArtistSong = song.Artist.Songs.ToList();
                        orderedArtistSong.Add(song);
                        orderedArtistSong = orderedArtistSong.OrderBy(p => p.Name).ToList();

                        index = orderedArtistSong.IndexOf(song);
                        song.Artist.Songs.Insert(index, song);

                        #region Order artist album

                        if (!song.Artist.Albums.Contains(song.Album))
                        {
                            var orderedArtistAlbum = song.Artist.Albums.ToList();
                            orderedArtistAlbum.Add(song.Album);
                            orderedArtistAlbum = orderedArtistAlbum.OrderBy(p => p.Name).ToList();

                            index = orderedArtistAlbum.IndexOf(song.Album);
                            song.Artist.Albums.Insert(index, song.Album);
                        }

                        #endregion
                    }

                    _inProgressAlbums.Remove(song.Album);
                    _inProgressArtists.Remove(song.Artist);

                    if (!Albums.Contains(song.Album))
                        Albums.Add(song.Album);
                    else if (song.Album.Songs.Count == 1)
                    {
                        // This means the album was added with a temp song
                        // Have to remove and readd it to get it to show up
                        Albums.Remove(song.Album);
                        Albums.Add(song.Album);
                    }

                    if (!Artists.Contains(song.Artist))
                        Artists.Add(song.Artist);
                    else if (song.Artist.Songs.Count == 1)
                    {
                        // This means the album was added with a temp song
                        // Have to remove and readd it to get it to show up
                        Artists.Remove(song.Artist);
                        Artists.Add(song.Artist);
                    }

                    Songs.Add(song);
                });
        }

        public async Task AddVideoAsync(Video video, Tag tags = null, StorageItemThumbnail thumb = null)
        {
            if (string.IsNullOrEmpty(video.Author)) video.Author = video.Title;

            _inProgressVideos.Add(video);
            await _sqlService.InsertAsync(video);
          
            var filePath = string.Format(_videoArtworkFilePath, video.Id);

            try
            {
                if (thumb != null && thumb.Type == ThumbnailType.Image)
                {
                    var file = await StorageHelper.CreateFileAsync(filePath, option: CreationCollisionOption.ReplaceExisting);
                    Windows.Storage.Streams.Buffer MyBuffer = new Windows.Storage.Streams.Buffer(Convert.ToUInt32(thumb.Size));
                    Windows.Storage.Streams.IBuffer iBuf = await thumb.ReadAsync(MyBuffer, MyBuffer.Capacity, Windows.Storage.Streams.InputStreamOptions.None);
                    using (var fileStream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
                    {
                        await fileStream.WriteAsync(iBuf.ToArray(), 0, iBuf.ToArray().Length);//WriteAsync(bytes, 0, bytes.Length);
                        video.HasArtwork = true;
                    }
                }
            }
            catch
            {
                video.HasArtwork = false;
            }

            if (tags != null && tags.Pictures != null && tags.Pictures.Length > 0)
            {
                Stream artwork = null;
                var image = tags.Pictures.FirstOrDefault();
                if (image != null)
                    artwork = new MemoryStream(image.Data.Data);

                if (artwork != null)
                {
                    using (artwork)
                    {
                        try
                        {
                            var file = await StorageHelper.CreateFileAsync(filePath, option: CreationCollisionOption.ReplaceExisting);
                            using (var fileStream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
                            {
                                var bytes = tags.Pictures[0].Data.Data;
                                await fileStream.WriteAsync(bytes, 0, bytes.Length);
                                video.HasArtwork = true;
                            }
                        }
                        catch
                        {
                            video.HasArtwork = false;
                            // ignored
                        }
                    }


                }
            }

            await _sqlService.UpdateItemAsync(video);

            // not working
            await _dispatcher.RunAsync(() =>
            {
                _inProgressVideos.Remove(video);
                Videos.Add(video);
            }); 
            
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async void CleanupFiles(IEnumerable<Album> albums, IEnumerable<Artist> artists)
        {
            var artworkFolder = await StorageHelper.GetFolderAsync("artworks");

            if (artworkFolder == null) return;
            var artworks = await artworkFolder.GetFilesAsync();

            foreach (var file in from file in artworks
                                 let id = file.Name.Replace(".jpg", string.Empty)
                                 where
                                     albums.FirstOrDefault(p => p.Id.ToString() == id) == null
                                     && artists.FirstOrDefault(p => p.ProviderId == id) == null
                                 select file)
            {
                try
                {
                    await file.DeleteAsync();
                    Debug.WriteLine("Deleted file: {0}", file.Name);
                }
                catch
                {
                    // ignored
                }
            }
        }

        #region Playback Queue

        public async Task ClearQueueAsync()
        {
            if (PlaybackQueue.Count == 0)
            {
                return;
            }

            await _bgSqlService.DeleteTableAsync<QueueSong>();

            _lookupMap.Clear();

            await _dispatcher.RunAsync(
                () =>
                {
                    PlaybackQueue.Clear();
                    ShufflePlaybackQueue.Clear();
                });
        }

        public async Task ShuffleCurrentQueueAsync()
        {
            var newShuffle = await Task.Factory.StartNew(() => PlaybackQueue.ToList().Shuffle()).ConfigureAwait(false);
            if (newShuffle.Count > 0)
            {
                await _dispatcher.RunAsync(() => ShufflePlaybackQueue.SwitchTo(newShuffle)).ConfigureAwait(false);
                for (var i = 0; i < newShuffle.Count; i++)
                {
                    var queueSong = newShuffle[i];
                    queueSong.ShufflePrevId = i == 0 ? 0 : newShuffle[i - 1].Id;
                    if (i + 1 < newShuffle.Count)
                        queueSong.ShuffleNextId = newShuffle[i + 1].Id;
                    await _bgSqlService.UpdateItemAsync(queueSong).ConfigureAwait(false);
                }
            }
        }

        public async Task<QueueSong> AddToQueueAsync(Song song, QueueSong position = null)
        {
            if (song == null)
                return null;

            var rnd = new Random(DateTime.Now.Millisecond);
            QueueSong prev = null;
            QueueSong shufflePrev = null;
            QueueSong next = null;
            QueueSong shuffleNext = null;
            var shuffleIndex = -1;
            var normalIndex = -1;

            if (position != null)
            {
                shuffleIndex = ShufflePlaybackQueue.IndexOf(position) + 1;
                normalIndex = PlaybackQueue.IndexOf(position) + 1;
            }

            var insert = normalIndex > -1 && normalIndex < PlaybackQueue.Count;
            var insertShuffle = shuffleIndex > -1;
            var shuffleLastAdd = shuffleIndex == ShufflePlaybackQueue.Count;

            if (insert)
            {
                next = PlaybackQueue.ElementAtOrDefault(normalIndex);
                if (next != null)
                {
                    _lookupMap.TryGetValue(next.PrevId, out prev);
                }
            }
            else
            {
                prev = PlaybackQueue.LastOrDefault();
            }

            if (insertShuffle)
            {
                if (shuffleLastAdd)
                {
                    shufflePrev = ShufflePlaybackQueue.ElementAtOrDefault(ShufflePlaybackQueue.Count - 1);
                }
                else
                {
                    shuffleNext = ShufflePlaybackQueue.ElementAtOrDefault(shuffleIndex);
                    if (shuffleNext != null)
                    {
                        _lookupMap.TryGetValue(shuffleNext.ShufflePrevId, out shufflePrev);
                    }
                }
            }
            else
            {
                if (ShufflePlaybackQueue.Count > 1)
                {
                    shuffleIndex = rnd.Next(1, ShufflePlaybackQueue.Count - 1);
                    shuffleNext = ShufflePlaybackQueue.ElementAt(shuffleIndex);

                    _lookupMap.TryGetValue(shuffleNext.ShufflePrevId, out shufflePrev);
                }
                else
                {
                    shuffleLastAdd = true;
                    shufflePrev = prev;
                }
            }

            // Create the new queue entry
            var newQueue = new QueueSong
            {
                SongId = song.Id,
                NextId = next == null ? 0 : next.Id,
                PrevId = prev == null ? 0 : prev.Id,
                ShuffleNextId = shuffleNext == null ? 0 : shuffleNext.Id,
                ShufflePrevId = shufflePrev == null ? 0 : shufflePrev.Id,
                Song = song
            };

            // Add it to the database
            await _bgSqlService.InsertAsync(newQueue).ConfigureAwait(false);

            if (next != null)
            {
                // Update the prev id of the queue that was replaced
                next.PrevId = newQueue.Id;
                await _bgSqlService.UpdateItemAsync(next).ConfigureAwait(false);
            }

            if (prev != null)
            {
                // Update the next id of the previous tail
                prev.NextId = newQueue.Id;
                await _bgSqlService.UpdateItemAsync(prev).ConfigureAwait(false);
            }

            if (shuffleNext != null)
            {
                shuffleNext.ShufflePrevId = newQueue.Id;
                await _bgSqlService.UpdateItemAsync(shuffleNext).ConfigureAwait(false);
            }

            if (shufflePrev != null)
            {
                shufflePrev.ShuffleNextId = newQueue.Id;
                await _bgSqlService.UpdateItemAsync(shufflePrev).ConfigureAwait(false);
            }

            // Add the new queue entry to the collection and map
            await _dispatcher.RunAsync(
                () =>
                {
                    if (insert)
                    {
                        try
                        {
                            PlaybackQueue.Insert(normalIndex, newQueue);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            PlaybackQueue.Add(newQueue);
                        }
                    }
                    else
                    {
                        PlaybackQueue.Add(newQueue);
                    }

                    if (shuffleLastAdd)
                    {
                        ShufflePlaybackQueue.Add(newQueue);
                    }
                    else
                    {
                        try
                        {
                            ShufflePlaybackQueue.Insert(shuffleIndex, newQueue);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            ShufflePlaybackQueue.Add(newQueue);
                        }
                    }
                }).ConfigureAwait(false);

            _lookupMap.TryAdd(newQueue.Id, newQueue);

            return newQueue;
        }


        public async Task DeleteFromQueueAsync(QueueSong songToRemove)
        {
            QueueSong previousModel;

            if (songToRemove == null)
            {
                return;
            }

            if (_lookupMap.TryGetValue(songToRemove.PrevId, out previousModel))
            {
                previousModel.NextId = songToRemove.NextId;
                await _bgSqlService.UpdateItemAsync(previousModel);
            }

            if (_lookupMap.TryGetValue(songToRemove.ShufflePrevId, out previousModel))
            {
                previousModel.ShuffleNextId = songToRemove.ShuffleNextId;
                await _bgSqlService.UpdateItemAsync(previousModel);
            }

            QueueSong nextModel;

            if (_lookupMap.TryGetValue(songToRemove.NextId, out nextModel))
            {
                nextModel.PrevId = songToRemove.PrevId;
                await _bgSqlService.UpdateItemAsync(nextModel);
            }

            if (_lookupMap.TryGetValue(songToRemove.ShuffleNextId, out nextModel))
            {
                nextModel.ShufflePrevId = songToRemove.ShufflePrevId;
                await _bgSqlService.UpdateItemAsync(nextModel);
            }

            await _dispatcher.RunAsync(
                () =>
                {
                    PlaybackQueue.Remove(songToRemove);
                    ShufflePlaybackQueue.Remove(songToRemove);
                });
            _lookupMap.TryRemove(songToRemove.Id, out songToRemove);

            // Delete from database
            await _bgSqlService.DeleteItemAsync(songToRemove);
        }




        private async void LoadQueue(IEnumerable<Song> songs)
        {
            if (_bgSqlService.Error())
            {
                await ClearQueueAsync();
                return;
            }

            var queue = _bgSqlService.GetAllQueueSong();
            QueueSong head = null;
            QueueSong shuffleHead = null;
            foreach (var queueSong in queue)
            {
                queueSong.Song = songs.FirstOrDefault(p => p.Id == queueSong.SongId);

                _lookupMap.TryAdd(queueSong.Id, queueSong);

                if (queueSong.ShufflePrevId == 0)
                {
                    shuffleHead = queueSong;
                }

                if (queueSong.PrevId == 0)
                {
                    head = queueSong;
                }
            }

            if (head != null)
            {
                for (var i = 0; i < queue.Count; i++)
                {
                    if (_dispatcher != null)
                    {
                        _dispatcher.RunAsync(() => PlaybackQueue.Add(head)).Wait();
                    }
                    else
                    {
                        PlaybackQueue.Add(head);
                    }

                    QueueSong value;
                    if (head.NextId != 0 && _lookupMap.TryGetValue(head.NextId, out value))
                    {
                        head = value;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (shuffleHead != null)
            {
                for (var i = 0; i < queue.Count; i++)
                {
                    if (_dispatcher != null)
                    {
                        _dispatcher.RunAsync(() => ShufflePlaybackQueue.Add(shuffleHead)).Wait();
                    }
                    else
                    {
                        ShufflePlaybackQueue.Add(shuffleHead);
                    }

                    QueueSong value;
                    if (shuffleHead.ShuffleNextId != 0 && _lookupMap.TryGetValue(shuffleHead.ShuffleNextId, out value))
                    {
                        shuffleHead = value;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }



        //public async Task<QueueSong> AddToQueueAsync(Song song)
        //{
        //    try
        //    {
        //        if (song == null) return null;
        //        QueueSong newQueueSong = new QueueSong
        //        {
        //            SongId = song.Id,
        //            Song = song,
        //            NextId = ,
        //            PrevId = (PlaybackQueue.Count - 1)
        //        };
        //        // Add it to the database
        //        await _bgSqlService.InsertAsync(newQueueSong).ConfigureAwait(false);

        //        await _dispatcher.RunAsync(() =>
        //        {
        //            PlaybackQueue.Add(newQueueSong);
        //        });

        //        return newQueueSong;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}



        //public async Task DeleteFromQueueAsync(QueueSong songToRemove)
        //{
        //    if (songToRemove == null) return;
        //    await _dispatcher.RunAsync(
        //        () =>
        //        {
        //            int n, p;
        //            n = songToRemove.NextId;
        //            p = songToRemove.PrevId;
        //            PlaybackQueue.Where(pre => pre.NextId == songToRemove.Id).Select(val => val.NextId = n);
        //            PlaybackQueue.Where(pre => pre.NextId == songToRemove.Id).Select(val => val.NextId = n);
        //            PlaybackQueue.Remove(songToRemove);
        //        });
        //    // Delete from database
        //    await _bgSqlService.DeleteItemAsync(songToRemove);
        //}


        //private void LoadQueue(IEnumerable<Song> songs)
        //{
        //    int i = 0;
        //    var queue = _bgSqlService.GetAllQueueSong();
        //    foreach (var queueSong in queue)
        //    {
        //        var song = songs.FirstOrDefault(p => p.Id == queueSong.SongId);
        //        QueueSong newQueueSong = new QueueSong
        //        {
        //            SongId = song.Id,
        //            Song = song,
        //            NextId = i % queue.Count,
        //            PrevId = PlaybackQueue.Count == 0 ? 0 : (PlaybackQueue.Count - 1)
        //        };

        //        i += 1;

        //        if (_dispatcher != null)
        //        {
        //            _dispatcher.RunAsync(() =>
        //            {
        //                PlaybackQueue.Add(newQueueSong);
        //            });
        //        }
        //        else
        //        {
        //            PlaybackQueue.Add(newQueueSong);
        //        }
        //    }
        //}




        #endregion

        #region Playlist

        public async Task<Playlist> CreatePlaylistAsync(string name)
        {
            if (Playlists.Count(p => p.Name == name) > 0)
            {
                throw new ArgumentException(name);
            }

            var playlist = new Playlist
            {
                Name = name,
                CreatedOn = DateTime.Now.ToString("dd/MM/yyyy")
            };

            await _sqlService.InsertAsync(playlist);

            Playlists.Insert(Playlists.Count, playlist);

            return playlist;
        }

        public async Task DeletePlaylistAsync(Playlist playlist)
        {
            await _sqlService.DeleteItemAsync(playlist);
            Playlists.Remove(playlist);
            foreach(var playlistSong in playlist.Songs)
            {
                await _sqlService.DeleteItemAsync(playlistSong);
            }
        }

        public async Task AddToPlaylistAsync(Playlist playlist, Song song)
        {
        
            // Create the new queue entry
            var newSong = new PlaylistSong
            {  
                SongId = song.Id,
                PlaylistId = playlist.Id,
                Song = song
            };

            // Add it to the database
            await _sqlService.InsertAsync(newSong);

           

            // Add the new queue entry to the collection and map
            playlist.Songs.Add(newSong);
           
        }

        public async Task DeleteFromPlaylistAsync(Playlist playlist, PlaylistSong songToRemove)
        {
            await _sqlService.DeleteItemAsync(songToRemove);
            await _dispatcher.RunAsync(() => playlist.Songs.Remove(songToRemove));
        }

        private async void LoadPlaylists()
        {
            var playlists = _sqlService.GetAllPlaylists().OrderByDescending(p => p.Id);
            var playlistSongs = _sqlService.GetAllPlaylistsTracks();

            foreach (var playlist in playlists)
            {

                var songs = playlistSongs.Where(p => p.PlaylistId == playlist.Id).ToList();
                foreach (var playlistSong in songs)
                {
                    playlistSong.Song = Songs.FirstOrDefault(p => p.Id == playlistSong.SongId);
                    playlist.Songs.Add(playlistSong);
                }

                if (_dispatcher != null)
                {
                    await _dispatcher.RunAsync(() => Playlists.Add(playlist));
                }
                else
                {
                    Playlists.Add(playlist);
                }
            }
        }

        #endregion
    }
}