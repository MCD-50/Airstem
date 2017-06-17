using System;
using System.Collections.Generic;
using Musicus.Data.Model.WebData;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Provider.Deezer.Model;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Musicus.Data.Provider.Deezer;
using System.Linq;
using Musicus.Core.Extensions;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using Musicus.Core.Utils;
using Musicus.Data.Service.Interfaces;
using Google.Apis.YouTube.v3.Data;

namespace Musicus.Data.Service.RunTime
{
    //spell error
    public class MiscellaneousService : IMiscellaneousService, IDisposable
    {

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }


        //getting top tracks
        public async Task<WebResults> GetTopTracksAsync(int limit = 20, string pageToken = null)
        {
            int offset;
            int.TryParse(pageToken, out offset);

            using (var response = await new DeezerChartRequest<JToken>(WebResults.Type.Song).Limit(limit).Offset(offset).ToResponseAsync().DontMarshall())
            {
                if (!response.HasData) return null;
                var results = CreateResults(response.Data, limit, offset);
                results.Songs = response.Data.Data?.Select(CreateSong).Take(limit).ToList();
                return results;
            }
        }


        private WebSong CreateSong(JToken token)
        {
            var song = token.ToObject<DeezerSong>();
            return CreateSong(song);
        }

        private WebSong CreateSong(DeezerSong deezerSong)
        {
            var song = new WebSong()
            {
                Name = deezerSong.Title,
                Id = deezerSong.Id,
                Artist = deezerSong.Artist != null ? deezerSong.Artist.Name : "Unknown Artist",
                Album = deezerSong.Album != null ? deezerSong.Album.Title : "Unknown Album"
            };
            if (deezerSong.Album != null)
            {
                var image = deezerSong.Album.CoverBig;
                if (image != null)
                    song.ArtworkImage = new Uri(image);
             }

            return song;
        }


        //gettiing top albums
        public async Task<WebResults> GetTopAlbumsAsync(int limit = 20, string pageToken = null)
        {
            int offset;
            int.TryParse(pageToken, out offset);
            using (var response = await new DeezerChartRequest<JToken>(WebResults.Type.Album).Limit(20).Offset(offset).ToResponseAsync().DontMarshall())
            {
                if (!response.HasData) return null;
                var results = CreateResults(response.Data, limit, offset);
                results.Albums = response.Data.Data?.Select(CreateAlbum).OrderByDescending(q=> q.ReleaseDate).Take(limit).ToList();
                return results;
            }
        }


        private WebAlbum CreateAlbum(JToken token)
        {
            var album = token.ToObject<DeezerAlbum>();
            return CreateAlbum(album);
        }

        private WebAlbum CreateAlbum(DeezerAlbum album)
        {
            var webAlbum = new WebAlbum()
            {
                Title = album.Title,
                Id = album.Id.ToString(),
                ReleaseDate = album.ReleaseDate
            };

            var image = album.CoverBig;
            if (image != null)
            {
                webAlbum.Artwork = new Uri(image);
                webAlbum.HasArtwork = true;
            }
            else webAlbum.HasArtwork = false;

            if (album.Artist != null)
                webAlbum.ArtistName = album.Artist.Name;

            if (album.Tracks != null)
            {
                webAlbum.Tracks = convertToList(album.Tracks.Data.ToList(), album.Title, album.CoverBig);
            }
            return webAlbum;
        }

        private List<WebSong> convertToList(List<DeezerSong> list, string albumName, string artwork)
        {
            List<WebSong> songs = new List<WebSong>();
            foreach (var deezerSong in list)
            {
                songs.Add(new WebSong()
                {
                    Name = deezerSong.Title,
                    Artist = deezerSong.Artist.Name,
                    Id = deezerSong.Id,
                    Album = albumName,
                    ArtworkImage = new Uri(artwork, UriKind.RelativeOrAbsolute),
                    IsDeezerTrack = true
                });
            }
            return songs;
        }

        //getting top album songs
        public async Task<WebAlbum> GetDeezerAlbumSongsAsync(string albumToken)
        {
            using (var response = await new DeezerAlbumRequest(albumToken)
                .ToResponseAsync().DontMarshall())
            {
                if (string.IsNullOrEmpty(response.Data.Title))
                {
                    var text = await response.HttpResponse.Content.ReadAsStringAsync();
                    if (text != null)
                    {
                        if (JToken.Parse(text)["error"] != null)
                            throw new NotImplementedException();
                    }
                }
                if (response.HasData)
                    return CreateAlbum(response.Data);
                throw new NotImplementedException();
            }
        }



        private WebResults CreateResults<T>(DeezerPageResponse<T> paging, int limit, int currentOffset)
        {
            return new WebResults
            {
                HasMore = paging?.Next != null,
                PageToken = paging == null ? null : (currentOffset + limit).ToString()
            };
        }



        int youtubeCount = 0;
        YouTubeService CreateClient()
        {
            YouTubeService youtubeService;
            if (youtubeCount < 10)
            {
                youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = ApiKeys.YouTubeIdMusicus,
                    ApplicationName = "Musicus1" //"Musicus1"
                });
                youtubeCount++;
            }
            else
            {
                youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = ApiKeys.YoutubeId,
                    ApplicationName = "Airstem" //"Musicus1"
                });
                youtubeCount = 0;
            }

            return youtubeService;
        }


        VideosResource.ListRequest CreateVideoResouceListResouceNewReleases(int count)
        {
            VideosResource.ListRequest request = CreateClient().Videos.List("snippet");
            request.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;
            request.VideoCategoryId = "10";
            request.MaxResults = count;
            return request;
        }




        public async Task<List<WebSong>> GetNewRelesedSong(int count)
        {
            try
            {
                List<WebSong> songs = new List<WebSong>();
                VideoListResponse searchListResult;

                try
                {
                     searchListResult = await CreateVideoResouceListResouceNewReleases(count)
                                              .ExecuteAsync();
                }
                catch
                {
                    searchListResult = await CreateVideoResouceListResouceNewReleases(count)
                                             .ExecuteAsync();
                }


                foreach (var searchResult in searchListResult.Items)
                {
                    await Task.Delay(1);
                    int i = new Random().Next(0, 15);
                    if (searchResult.Snippet.Title.Contains("-"))
                    {
                        songs.Add(new WebSong()
                        {
                            Name = searchResult.Snippet.Title.GetSongName().Trim(),
                            Artist = searchResult.Snippet.Title.GetArtistName().Trim(),
                            Id = searchResult.Id.ToString(),
                            ReleaseDate = searchResult.Snippet.PublishedAt.ToString(),
                            FileAuthor = searchResult.Snippet.ChannelTitle,
                            Provider = Mp3Provider.YouTube
                        });
                    }
                }

                if (searchListResult != null) searchListResult = null;

                if (songs.Count < 0 || songs == null) return null;
                var _websong = new List<WebSong>();
                foreach (var item in songs)
                {
                    if (item.Name != "Unknown Song" && item.Artist != "Unknown Artist")
                        _websong.Add(item);
                }
                return _websong.OrderByDescending(p => p.ReleaseDate).Take(count).ToList();
            }

            catch
            {
                return null;
            }


        }

        SearchResource.ListRequest CreateVideoResouceListResouceViezSearch(string term, int count)
        {
            SearchResource.ListRequest request = CreateClient().Search.List("snippet");
            request.Q = term;
            request.VideoCategoryId = "10";
            request.MaxResults = count;
            request.Type = "video";
            return request;
        }





        public async Task<List<WebSong>> GetViezResultsAsync(string term, int count)
        {

            try
            {
                List<WebSong> songs = new List<WebSong>();

                SearchListResponse searchListResult;

                try
                {
                    searchListResult = await CreateVideoResouceListResouceViezSearch(term, count)
                                             .ExecuteAsync();
                }
                catch
                {
                    searchListResult = await CreateVideoResouceListResouceViezSearch(term, count)
                                             .ExecuteAsync();
                }


                foreach (var searchResult in searchListResult.Items)
                {
                    switch (searchResult.Id.Kind)
                    {
                        case "youtube#video":
                            if (searchResult.Snippet.Title.Contains("-"))
                            {
                                songs.Add(new WebSong()
                                {
                                    Name = searchResult.Snippet.Title.GetSongName().Trim(),
                                    Artist = searchResult.Snippet.Title.GetArtistName().Trim(),
                                    Id = searchResult.Id.VideoId.ToString(),
                                    ReleaseDate = searchResult.Snippet.PublishedAt.ToString(),
                                    FileAuthor = searchResult.Snippet.ChannelTitle,
                                    Provider = Mp3Provider.YouTube
                                });
                            }
                            break;
                    }
                }

                if (searchListResult != null) searchListResult = null;

                if (songs.Count < 0 || songs == null) return null;
                var _websong = new List<WebSong>();
                foreach (var item in songs)
                {
                    if (item.Name != "Unknown Song" && item.Artist != "Unknown Artist")
                        _websong.Add(item);
                }
                return _websong.OrderByDescending(p => p.ReleaseDate).ToList();
            }
            catch
            {
                return null;
            }
        }


        SearchResource.ListRequest CreateVideoResouceListResouceGetVideo(string term)
        {
            SearchResource.ListRequest request = CreateClient().Search.List("snippet");
            request.Q = term;
            request.VideoCategoryId = "10";
            request.MaxResults = 1;
            request.Type = "video";
            return request;
        }



        public async Task<string> GetVideo(string term)
        {      
            try
            {
                SearchListResponse searchListResult;

                try
                {
                    searchListResult = await CreateVideoResouceListResouceGetVideo(term)
                                         .ExecuteAsync();
                }
                catch
                {
                    searchListResult = await CreateVideoResouceListResouceGetVideo(term)
                                             .ExecuteAsync();
                }

                return searchListResult.Items.FirstOrDefault().Id.VideoId;

            }
            catch
            {
                return string.Empty;
            }
        }



        //public async Task<IEnumerable<WebVideo>> GetPopularCategoryVideos(int count, string cat)
        //{
        //    YouTubeService youtubeService;
        //    if (youtubeCount < 10)
        //    {
        //        youtubeService = new YouTubeService(new BaseClientService.Initializer()
        //        {
        //            ApiKey = ApiKeys.YouTubeIdMusicus,
        //            ApplicationName = "Musicus1" //"Musicus1"
        //        });
        //        youtubeCount++;
        //    }
        //    else
        //    {
        //        youtubeService = new YouTubeService(new BaseClientService.Initializer()
        //        {
        //            ApiKey = ApiKeys.YoutubeId,
        //            ApplicationName = "Airstem" //"Musicus1"
        //        });
        //        youtubeCount = 0;
        //    }

        //    var popular = youtubeService.Videos.List("snippet");
        //    popular.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;
        //    popular.VideoCategoryId = cat;
        //    popular.MaxResults = count;

        //    try
        //    {
        //        List<WebVideo> videos = new List<WebVideo>();



        //        var searchListResult = await popular.ExecuteAsync();

        //        foreach (var item in searchListResult.Items)
        //        {
        //            switch (item.Kind)
        //            {
        //                case "youtube#video":
        //                    videos.Add(new WebVideo()
        //                    {
        //                        Title = item.Snippet.Title,
        //                        Author = item.Snippet.ChannelTitle,
        //                        //Views = item.Statistics.ViewCount.GetValueOrDefault(),
        //                        VideoId = item.Id,
        //                        //Length = item.ContentDetails.Duration.GetFineLength(),
        //                        Date = item.Snippet.PublishedAt.GetValueOrDefault().ToString("dd/MM/yyyy"),
        //                        Artwork = GetUri(item.Snippet.Thumbnails)
        //                        //new Uri(item.Snippet.Thumbnails.High.Url, UriKind.RelativeOrAbsolute)
        //                    });
        //                    break;

        //            }
        //        }

        //        return videos;
        //    }

        //    catch
        //    {
        //        return null;
        //    }
        //}


        SearchResource.ListRequest CreateVideoResouceListResouceGetSearchedVideo(string term, int count)
        {
            SearchResource.ListRequest request = CreateClient().Search.List("snippet");
            request.Q = term;
            request.MaxResults = count;
            request.Type = "video";
            return request;
        }



        public async Task<IEnumerable<WebVideo>>GetSearchedVideos(int count, string term)
        {
            try
            {
                List<WebVideo> videos = new List<WebVideo>();

                SearchListResponse searchListResult;

                try
                {
                    searchListResult = await CreateVideoResouceListResouceGetSearchedVideo(term, count)
                                        .ExecuteAsync();
                }
                catch
                {
                    searchListResult = await CreateVideoResouceListResouceGetSearchedVideo(term, count)
                                          .ExecuteAsync();
                }

                foreach (var item in searchListResult.Items)
                {
                    videos.Add(new WebVideo()
                    {
                        Title = item.Snippet.Title,
                        Author = item.Snippet.ChannelTitle,
                        VideoId = item.Id.VideoId,
                        //Length = string.Empty,
                        Date = item.Snippet.PublishedAt.GetValueOrDefault().ToString("dd/MM/yyyy"),
                        Artwork = GetUri(item.Snippet.Thumbnails)
                    });
                }

                if (searchListResult != null) searchListResult = null;

                return videos;
            }

            catch
            {
                return null;
            }
        }


        VideosResource.ListRequest CreateVideoResouceListResouceGetYVideoFromId(string id)
        {
            VideosResource.ListRequest request = CreateClient().Videos.List("snippet,statistics");
            request.Id = id;
            request.MaxResults = 1;
            return request;
        }

        public async Task<WebVideo> GetYVideoFromId(string id)
        {

            try
            {
                VideoListResponse searchListResult;
                try
                {
                    searchListResult = await CreateVideoResouceListResouceGetYVideoFromId(id)
                                            .ExecuteAsync();
                }
                catch
                {
                    searchListResult = await CreateVideoResouceListResouceGetYVideoFromId(id)
                                            .ExecuteAsync();
                }
            
                var item = searchListResult.Items.FirstOrDefault();
                if (searchListResult != null) searchListResult = null;

                return new WebVideo()
                {
                    Title = item.Snippet.Title,
                    Author = item.Snippet.ChannelTitle,
                    VideoId = item.Id,
                    Info = item.Snippet.Description,                   
                    Date = item.Snippet.PublishedAt.GetValueOrDefault().ToString("dd/MM/yyyy"),
                    Artwork = GetUri(item.Snippet.Thumbnails) 
                };

                //  ViewCount = item.Player., LikeCount = item.Statistics.LikeCount.GetValueOrDefault(),
                //  DislikeCount = item.Statistics.DislikeCount.GetValueOrDefault(),
            }

            catch
            {
                return null;
            }

        }


        SearchResource.ListRequest CreateVideoResouceListResouceGetRelatedYVideoFromId(string id, int count)
        {
            SearchResource.ListRequest request = CreateClient().Search.List("snippet");
            request.RelatedToVideoId = id;
            request.MaxResults = count;
            request.Type = "video";
            return request;
        }


        public async Task<IEnumerable<WebVideo>> GetRelatedYVideoFromId(int count, string id)
        {
            try
            {
                List<WebVideo> videos = new List<WebVideo>();
                SearchListResponse searchListResult;

                try
                {
                    searchListResult = await CreateVideoResouceListResouceGetRelatedYVideoFromId(id, count)
                                            .ExecuteAsync();
                }
                catch
                {
                    searchListResult = await CreateVideoResouceListResouceGetRelatedYVideoFromId(id, count)
                                             .ExecuteAsync();
                }


               
                foreach (var item in searchListResult.Items)
                {
                    videos.Add(new WebVideo()
                    {
                        Title = item.Snippet.Title,
                        Author = item.Snippet.ChannelTitle,
                        VideoId = item.Id.VideoId,
                        //Length = string.Empty,
                        Date = item.Snippet.PublishedAt.GetValueOrDefault().ToString("dd/MM/yyyy"),
                        Artwork = GetUri(item.Snippet.Thumbnails)
                    });
                }


                if (searchListResult != null) searchListResult = null;

                return videos;
            }

            catch
            {
                return null;
            }
        }

        private Uri GetUri(ThumbnailDetails thumbnails)
        {
            if (thumbnails.Maxres != null)
                return new Uri(thumbnails.Maxres.Url, UriKind.RelativeOrAbsolute);
            else if (thumbnails.High != null)
                return new Uri(thumbnails.High.Url, UriKind.RelativeOrAbsolute);
            else if (thumbnails.Medium != null)
                return new Uri(thumbnails.Medium.Url, UriKind.RelativeOrAbsolute);
            else if (thumbnails.Default__ != null)
                return new Uri(thumbnails.Default__.Url, UriKind.RelativeOrAbsolute);
            return new Uri(string.Empty, UriKind.RelativeOrAbsolute);
        }


    }
}
