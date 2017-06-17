using System.Threading.Tasks;
using System.Linq;
using Musicus.Core.WinRt;
using System.Net.Http;
using PCLStorage;
using Musicus.Core.Utils;
using Musicus.Data.Collection.Model;
using System;
using Musicus.Data.Spotify;
using Musicus.ViewModel.Mvvm.Dispatcher;

namespace Musicus.Helpers
{
    public class DownloadArtworks
    {

        //Artworks from spotify.......

        public static async Task DownloadArtistsArtworkAsyncFromSpotify(bool missingOnly = true, string artistToSearch = null)
        {
            if (!App.Locator.Network.IsActive) return;

            var artists = App.Locator.CollectionService.Artists.ToList();

            if (missingOnly)
                artists = artists.Where(p => !p.HasArtwork && !p.Name.ToLower().Contains("unknown") && !p.Name.ToLower().Contains("various") && !p.Name.ToLower().Contains("random")).ToList();
            else if(!string.IsNullOrEmpty(artistToSearch))
            {
                artists = artists.Where(p => p.Name.Equals(artistToSearch, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }

            var tasks = artists.Select(
                artist => Task.Factory.StartNew(
                    async () =>
                    {
                        if (artist.ProviderId == "autc.unknown" || string.IsNullOrEmpty(artist.Name)) return;     
                        // don't want to retry getting this pic while we're downloading it
                        var hadArtwork = artist.HasArtwork;
                        artist.HasArtwork = true;


                        try
                        {
                            var spotify = await App.Locator.Spotify.SearchItems(artist.Name, SearchType.ARTIST, 1);
                            string id = "";
                            if (spotify != null && spotify.Artists != null && spotify.Artists.Items.Count > 0)
                            {
                                id = spotify.Artists.Items[0].Id;
                            }

                            var finalimage = await App.Locator.Spotify.GetArtist(id);

                            if (finalimage == null || finalimage.Images == null || finalimage.Images.Count == 0)
                            {
                                artist.HasArtwork = false;
                                artist.NoArtworkFound = true;
                                await App.Locator.SqlService.UpdateItemAsync(artist);
                                await DownloadFromLastFMOnMissingSpotifyCall(artist);
                                return;
                            }

                            if (!string.IsNullOrEmpty(finalimage.Images[0].Url))
                            {
                                var artistFilePath = string.Format(AppConstant.ArtistsArtworkPath, artist.Id);

                                if (await SaveImageAsync(artistFilePath, finalimage.Images[0].Url).ConfigureAwait(false))
                                {

                                    if (!hadArtwork)
                                    {
                                        artist.HasArtwork = true;
                                        await App.Locator.SqlService.UpdateItemAsync(artist).ConfigureAwait(false);
                                    }

                                    await DispatcherHelper.RunAsync(
                                        () =>
                                        {
                                            artist.Artwork =
                                                new PclBitmapImage(new Uri(AppConstant.LocalStorageAppPath + artistFilePath));
                                            artist.Artwork.SetDecodedPixel(App.Locator.CollectionService.ScaledImageSize);
                                        });
                                }
                                else
                                {
                                    artist.HasArtwork = false;
                                }
                            }

                        }
                        catch
                        {
                            artist.HasArtwork = false;
                            //await DownloadFromLastFMOnMissingSpotifyCall(artist);
                        }
                    })).Cast<Task>().ToList();

            App.Locator.SqlService.BeginTransaction();
            foreach (var task in tasks)
            {
                await task.ConfigureAwait(false);
            }

            App.Locator.SqlService.Commit();

        }



        //Artworks from lastfm.......
        public static async Task DownloadArtistsArtworkAsync(bool missingOnly = true, string artistToSearch = null)
        {
            if (!App.Locator.Network.IsActive) return;

            var artists = App.Locator.CollectionService.Artists.ToList();
            if (missingOnly)
                artists = artists.Where(p => !p.HasArtwork && !p.Name.ToLower().Contains("unknown") && !p.Name.ToLower().Contains("various") && !p.Name.ToLower().Contains("random")).ToList();
            else if (!string.IsNullOrEmpty(artistToSearch))
            {
                artists = artists.Where(p => p.Name.Equals(artistToSearch, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }


            var tasks = artists.Select(
                artist => Task.Factory.StartNew(
                    async () =>
                    {

                        if (artist.ProviderId == "autc.unknown" || string.IsNullOrEmpty(artist.Name)) return;
                        // don't want to retry getting this pic while we're downloading it
                        var hadArtwork = artist.HasArtwork;
                        artist.HasArtwork = true;

                        try
                        {
                            var lastArtist = await App.Locator.ScrobblerService.GetDetailArtist(artist.Name);

                            if (lastArtist == null || lastArtist.MainImage == null || lastArtist.MainImage.Largest == null)
                            {
                                artist.HasArtwork = false;

                                // By setting no artwork found we know not to try again, saving precious data!
                                artist.NoArtworkFound = true;
                                await App.Locator.SqlService.UpdateItemAsync(artist);
                                return;
                            }

                            var artistFilePath = string.Format(AppConstant.ArtistsArtworkPath, artist.Id);
                            if (await SaveImageAsync(artistFilePath, lastArtist.MainImage.Largest.AbsoluteUri).ConfigureAwait(false))
                            {

                                if (!hadArtwork)
                                {
                                    artist.HasArtwork = true;
                                    await App.Locator.SqlService.UpdateItemAsync(artist).ConfigureAwait(false);
                                }

                                await DispatcherHelper.RunAsync(
                                    () =>
                                    {
                                        artist.Artwork =
                                            new PclBitmapImage(new Uri(AppConstant.LocalStorageAppPath + artistFilePath));
                                        artist.Artwork.SetDecodedPixel(App.Locator.CollectionService.ScaledImageSize);

                                        if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
                                        {
                                            artist.SmallArtwork = new PclBitmapImage(new Uri(AppConstant.LocalStorageAppPath + artistFilePath));
                                            artist.SmallArtwork.SetDecodedPixel(25);
                                        }

                                    });
                            }
                            else
                            {
                                artist.HasArtwork = false;
                            }

                        }
                        catch
                        {
                            artist.HasArtwork = false;
                            await DownloadFromSpotifyOnMissingLastFMCall(artist);
                        }
                    })).Cast<Task>().ToList();

            App.Locator.SqlService.BeginTransaction();
            foreach (var task in tasks)
            {
                await task.ConfigureAwait(false);
            }

            App.Locator.SqlService.Commit();

        }

        private static async Task DownloadFromLastFMOnMissingSpotifyCall(Artist artist)
        {
            if (!App.Locator.Network.IsActive) return;
            if (artist.ProviderId == "autc.unknown" || string.IsNullOrEmpty(artist.Name)) return;
            // don't want to retry getting this pic while we're downloading it
            var hadArtwork = artist.HasArtwork;
            artist.HasArtwork = true;

            try
            {
                var lastArtist = await App.Locator.ScrobblerService.GetDetailArtist(artist.Name);

                if (lastArtist == null || lastArtist.MainImage == null || lastArtist.MainImage.Largest == null)
                {
                    artist.HasArtwork = false;
                    // By setting no artwork found we know not to try again, saving precious data!
                    artist.NoArtworkFound = true;
                    await App.Locator.SqlService.UpdateItemAsync(artist);
                    return;
                }

                var artistFilePath = string.Format(AppConstant.ArtistsArtworkPath, artist.Id);
                if (await SaveImageAsync(artistFilePath, lastArtist.MainImage.Largest.AbsoluteUri).ConfigureAwait(false))
                {

                    if (!hadArtwork)
                    {
                        artist.HasArtwork = true;
                        await App.Locator.SqlService.UpdateItemAsync(artist).ConfigureAwait(false);
                    }

                    await DispatcherHelper.RunAsync(
                        () =>
                        {
                            artist.Artwork =
                                new PclBitmapImage(new Uri(AppConstant.LocalStorageAppPath + artistFilePath));
                            artist.Artwork.SetDecodedPixel(App.Locator.CollectionService.ScaledImageSize);

                            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
                            {
                                artist.SmallArtwork = new PclBitmapImage(new Uri(AppConstant.LocalStorageAppPath + artistFilePath));
                                artist.SmallArtwork.SetDecodedPixel(25);
                            }

                        });
                }
                else
                {
                    artist.HasArtwork = false;
                }

           }
            catch
            {
                artist.HasArtwork = false;
            }
        }


        //Artworks from spotify in responce to lastfm.......
        private static async Task DownloadFromSpotifyOnMissingLastFMCall(Artist artist)
        {   
            if (!App.Locator.Network.IsActive) return;

            //set true to avoid duplicate searching.
            var hadArtwork = artist.HasArtwork;
            artist.HasArtwork = true;

       
            try
            {
                var spotify = await App.Locator.Spotify.SearchItems(artist.Name, SearchType.ARTIST, 1);
                string id = "";
                if (spotify != null && spotify.Artists != null && spotify.Artists.Items.Count > 0)
                {
                    id = spotify.Artists.Items[0].Id;
                }

                var finalimage = await App.Locator.Spotify.GetArtist(id);

                if (finalimage == null || finalimage.Images == null || finalimage.Images.Count == 0)
                {
                    artist.HasArtwork = false;
                    artist.NoArtworkFound = true;
                    await App.Locator.SqlService.UpdateItemAsync(artist);
                    return;
                }

                if (!string.IsNullOrEmpty(finalimage.Images[0].Url))
                {
                  
                    var artistFilePath = string.Format(AppConstant.ArtistsArtworkPath, artist.Id);

                    if (await
                        SaveImageAsync(artistFilePath, finalimage.Images[0].Url)
                            .ConfigureAwait(false))
                    {

                        if (!hadArtwork)
                        {
                            artist.HasArtwork = true;
                            await App.Locator.SqlService.UpdateItemAsync(artist).ConfigureAwait(false);
                        }

                        await DispatcherHelper.RunAsync(
                            () =>
                            {
                                artist.Artwork =
                                    new PclBitmapImage(new Uri(AppConstant.LocalStorageAppPath + artistFilePath));
                                artist.Artwork.SetDecodedPixel(App.Locator.CollectionService.ScaledImageSize);

                                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
                                {
                                    artist.SmallArtwork = new PclBitmapImage(new Uri(AppConstant.LocalStorageAppPath + artistFilePath));
                                    artist.SmallArtwork.SetDecodedPixel(25);
                                }

                            });
                    }
                    else
                    {
                        artist.HasArtwork = false;
                    }
                }

            }
            catch
            {
                artist.HasArtwork = false;
            }

        }

        public static async Task DownloadAlbumsArtworkAsync(bool missingOnly = true)
        {
            // if (!InternetConnectionHelper.HasConnection()) return;

            if (!App.Locator.Network.IsActive) return;

            var albums = App.Locator.CollectionService.Albums.ToList();

            if (missingOnly)
                albums = albums.Where(p => !p.HasArtwork && !p.Name.ToLower().Contains("unknown") && !p.Name.ToLower().Contains("various") && !p.Name.ToLower().Contains("random")).ToList();

            var tasks = albums.Select(
                album => Task.Factory.StartNew(
                    async () =>
                    {
                        if (string.IsNullOrEmpty(album.Name)) return;
                       
                        // don't want to retry getting this pic while we're downloading it
                        album.HasArtwork = true;

                        try
                        {
                            string artworkUrl;

                            // All spotify albums have artwork
                            if (album.ProviderId.StartsWith("spotify."))
                            {
                                var spotifyAlbum = await App.Locator.Spotify.GetAlbum(album.ProviderId.Replace("spotify.", string.Empty)).ConfigureAwait(false);
                                if (spotifyAlbum == null)
                                {
                                    album.HasArtwork = false;
                                    album.NoArtworkFound = true;
                                    await App.Locator.SqlService.UpdateItemAsync(album).ConfigureAwait(false);
                                    return;
                                }

                                artworkUrl = spotifyAlbum.Images[0].Url;
                            }
                            else
                            {
                                // First, try using Last.FM
                                var lastAlbum =
                                    await
                                    App.Locator.ScrobblerService.GetDetailAlbum(album.Name, album.PrimaryArtist.Name);

                                if (lastAlbum == null ||
                                    lastAlbum.Images == null ||
                                    lastAlbum.Images.Largest == null)
                                {
                                    // Then Deezer
                                    var results =
                                        await
                                        App.Locator.DeezerService.SearchAlbumsAsync(
                                            album.Name + " " + album.PrimaryArtist.Name).ConfigureAwait(false);
                                    var deezerAlbum = results.data.FirstOrDefault();

                                    if (deezerAlbum == null
                                        ||
                                        (!album.Name.ToLower().Contains(deezerAlbum.title.ToLower())
                                         && (!album.PrimaryArtist.Name.ToLower()
                                                   .Contains(deezerAlbum.artist.name.ToLower())
                                             || deezerAlbum.bigCover == null)))
                                    {
                                        album.HasArtwork = false;
                                        album.NoArtworkFound = true;
                                        await App.Locator.SqlService.UpdateItemAsync(album).ConfigureAwait(false);
                                        return;
                                    }

                                    artworkUrl = deezerAlbum.bigCover;
                                }
                                else
                                {
                                    artworkUrl = lastAlbum.Images.Largest.AbsoluteUri;
                                }
                            }

                            await SaveAlbumImageAsync(album, artworkUrl).ConfigureAwait(false);
                        }
                        catch
                        {
                            album.HasArtwork = false;
                        }
                    })).Cast<Task>().ToList();

            App.Locator.SqlService.BeginTransaction();
            foreach (var task in tasks)
            {
                await task.ConfigureAwait(false);
            }

            App.Locator.SqlService.Commit();
        }

        public static async Task SaveAlbumImageAsync(Album album, string url)
        {
            var filePath = string.Format(AppConstant.ArtworkPath, album.Id);
            if (await SaveImageAsync(filePath, url).ConfigureAwait(false))
            {
                album.HasArtwork = true;
                await App.Locator.SqlService.UpdateItemAsync(album).ConfigureAwait(false);
                //album.Songs.All(p => p.HasAlbumArtwork = true);
                await DispatcherHelper.RunAsync(
                    () =>
                    {
                        album.Artwork = new PclBitmapImage(new Uri(AppConstant.LocalStorageAppPath + filePath));
                        album.Artwork.SetDecodedPixel(App.Locator.CollectionService.ScaledImageSize);

                        album.SmallArtwork = new PclBitmapImage(new Uri(AppConstant.LocalStorageAppPath + filePath));
                        if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                            album.SmallArtwork.SetDecodedPixel(10);
                        else
                            album.SmallArtwork.SetDecodedPixel(25);
                    });
              }
        }

        private static async Task<bool> SaveImageAsync(string filePath, string url)
        {
            try
            {
                var file = await StorageHelper.CreateFileAsync(filePath, option: CreationCollisionOption.ReplaceExisting).ConfigureAwait(false);

                using (var client = new HttpClient())
                {
                    using (var fileStream = await file.OpenAsync(FileAccess.ReadAndWrite))
                    {
                        var buffer = await client.GetByteArrayAsync(url).ConfigureAwait(false);
                        await fileStream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    }
                }

                return true;
            }
            catch
            {
            
                return false;
            }
        }


        public static async Task SaveArtworkByUrl(Uri url, string name)
        {
            //if (!InternetConnectionHelper.HasConnection()) return;
            if (!App.Locator.Network.IsActive) return;

            var collAlbum = App.Locator.CollectionService.Albums.FirstOrDefault(p => string.Equals(p.Name.ToLower(), name.ToLower()));
            if (collAlbum != null && !collAlbum.HasArtwork && !collAlbum.NoArtworkFound)
                await SaveAlbumImageAsync(collAlbum, url.ToString());

        }

    }
}
