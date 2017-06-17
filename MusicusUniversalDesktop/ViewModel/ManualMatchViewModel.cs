
using Musicus.Core.Utils.Interfaces;
using Musicus.Data.Collection.Model;
using Musicus.Data.Model.Advertisement;
using Musicus.Data.Model.WebSongs;
using Musicus.Data.Service.Interfaces;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Musicus.ViewModel
{
    public class ManualMatchViewModel : Mvvm.Others.ViewModelBase
    {
        private readonly IDispatcherHelper dispatcherHelper;
        private readonly IMp3SearchService searchService;
        public Song CurrentSong { get; set; }
        private ObservableCollection<Base> soundCloud, pleer, mpglu, youtube, mpz;
        private bool isscLoading;
        private bool isplLoading;
        private bool iswtLoading;
        private bool isytLoading;
        private bool ismpzLoading;

        public int count = 5;
        private bool isLoading;
        

        public ManualMatchViewModel(IMp3SearchService _service, IDispatcherHelper dispatcherHelper)
        {
            this.dispatcherHelper = dispatcherHelper;
            searchService = _service;
        }

        public bool IsLoading
        {
            get { return isLoading; }
            set { Set(ref isLoading, value); }
        }


        public ObservableCollection<Base> Pleer
        {
            get
            {
                return pleer;
            }

            set { Set(ref pleer, value); }
        }

        public ObservableCollection<Base> Mpz
        {
            get
            {
                return mpz;
            }

            set { Set(ref mpz, value); }
        }


        public ObservableCollection<Base> SoundCloud
        {
            get
            {
                return soundCloud;
            }

            set { Set(ref soundCloud, value); }
        }


        public ObservableCollection<Base> YouTube
        {
            get
            {
                return youtube;
            }

            set { Set(ref youtube, value); }
        }


        public ObservableCollection<Base> WapTrack
        {
            get
            {
                return mpglu;
            }

            set { Set(ref mpglu, value); }
        }


        public bool IsSCLoading
        {
            get { return isscLoading; }
            set { Set(ref isscLoading, value); }
        }


        public bool IsWTLoading
        {
            get { return iswtLoading; }
            set { Set(ref iswtLoading, value); }
        }


        public bool IsYTLoading
        {
            get { return isytLoading; }
            set { Set(ref isytLoading, value); }
        }


        public bool IsPLLoading
        {
            get { return isplLoading; }
            set { Set(ref isplLoading, value); }
        }


        public bool IsMPZLoading
        {
            get { return ismpzLoading; }
            set { Set(ref ismpzLoading, value); }
        }

        private bool viewModelCleared = false;
        public void CleanManualMatchViewModel()
        {
            if (!viewModelCleared)
            {
                if (WapTrack != null) WapTrack = null;
                if (YouTube != null) YouTube = null;
                if (SoundCloud != null) SoundCloud = null;
                if (Mpz != null) Mpz = null;
                viewModelCleared = true;
            }
        }

      
        public async void ReceiveSong(Song song)
        {
            if (!App.Locator.Network.IsActive) return;

            try
            {
                CurrentSong = song;

                viewModelCleared = false;

                IsLoading = true;

             
                var tasks = new List<Task>
                {
                    Task.Factory.StartNew(
                    async () =>
                    {
                        await DispatcherHelper.RunAsync(() => IsSCLoading = true);

                        var results =
                            await
                            searchService.SearchSoundCloud(title: CurrentSong.Name, artist:CurrentSong.Artist.Name, limit:5);

                       await DispatcherHelper.RunAsync(() =>
                            {
                                count--;
                                 if(count == 0)
                                    IsLoading = false;
                                IsSCLoading = false;
                                if (results == null)return;
                                SoundCloud = new ObservableCollection<Base>();

                                if(results.Count > 0)
                                {
                                      int indexToAdd = -1;
                                      foreach (var item in results)
                                      {
                                         indexToAdd++;
                                         SoundCloud.Add(item);
                                         if (indexToAdd == 2 || indexToAdd == 10 || indexToAdd == 22 || indexToAdd == 34 || indexToAdd == 49 ||
                                             indexToAdd == 63 || indexToAdd == 78 || indexToAdd == 88 || indexToAdd == 99)
                                                 SoundCloud.Insert(indexToAdd, new ListAdvert());
                                      }
                                }

                            });
                    }),

                    Task.Factory.StartNew(
                    async () =>
                    {
                        await DispatcherHelper.RunAsync(() => IsWTLoading = true);


                        var results = await searchService.SearchMp3Glu(CurrentSong.Name,CurrentSong.Artist.Name, limit:5);
                        await dispatcherHelper.RunAsync(
                            () =>
                            {
                                 count--;
                                  if(count == 0)
                                    IsLoading = false;
                                IsWTLoading= true;
                                if (results == null)return;
                                WapTrack = new ObservableCollection<Base>();

                                if(results.Count > 0)
                                {
                                      int indexToAdd = -1;
                                      foreach (var item in results)
                                      {
                                         indexToAdd++;
                                         WapTrack.Add(item);
                                         if (indexToAdd == 2 || indexToAdd == 10 || indexToAdd == 22 || indexToAdd == 34 || indexToAdd == 49 ||
                                             indexToAdd == 63 || indexToAdd == 78 || indexToAdd == 88 || indexToAdd == 99)
                                                 WapTrack.Insert(indexToAdd, new ListAdvert());
                                      }
                                }

                            });
                    }),

                    Task.Factory.StartNew(
                    async () =>
                    {
                        await DispatcherHelper.RunAsync(() => IsPLLoading= true);

                        var results =
                            await
                            searchService.SearchMp3lio(title: CurrentSong.Name, artist: CurrentSong.Artist.Name, limit:5);

                        await dispatcherHelper.RunAsync(
                            () =>
                            {
                                count--;
                                 if(count == 0)
                                    IsLoading = false;
                                IsPLLoading= false;
                                if (results == null) return;
                                Pleer = new ObservableCollection<Base>(results);
                                if(Pleer.Count > 1)
                                {
                                      int indexToAdd = -1;
                                      foreach (var item in Pleer)
                                      {
                                         indexToAdd++;
                                         if (indexToAdd == 2 || indexToAdd == 10 || indexToAdd == 22 || indexToAdd == 34 || indexToAdd == 49 ||
                                             indexToAdd == 63 || indexToAdd == 78 || indexToAdd == 88 || indexToAdd == 99)
                                                 Pleer.Insert(indexToAdd, new ListAdvert());
                                      }
                                }
                            });
                    }),

                    Task.Factory.StartNew(
                    async () =>
                    {
                        await DispatcherHelper.RunAsync(() => IsYTLoading = true);


                       var results =
                            await
                            searchService.SearchYoutube(title: CurrentSong.Name, artist: CurrentSong.Artist.Name, limit:5, includeAudioTag:true);

                        await dispatcherHelper.RunAsync(
                            () =>
                            {
                                count--;
                                 if(count == 0)
                                    IsLoading = false;
                                IsYTLoading= false;
                                if (results == null)return;
                                YouTube = new ObservableCollection<Base>();
                           
                                if(results.Count > 0)
                                {
                                      int indexToAdd = -1;
                                      foreach (var item in results)
                                      {
                                         indexToAdd++;
                                         YouTube.Add(item);
                                         if (indexToAdd == 2 || indexToAdd == 10 || indexToAdd == 22 || indexToAdd == 34 || indexToAdd == 49 ||
                                             indexToAdd == 63 || indexToAdd == 78 || indexToAdd == 88 || indexToAdd == 99)
                                                 YouTube.Insert(indexToAdd, new ListAdvert());
                                      }
                                }

                            });
                    }),

               

                    Task.Factory.StartNew(
                    async () =>
                    {
                        await DispatcherHelper.RunAsync(() => IsMPZLoading = true);


                        var results = await searchService.SearchMp3Pm(CurrentSong.Name,CurrentSong.Artist.Name, limit:5);
                        await dispatcherHelper.RunAsync(
                            () =>
                            {
                                count--;
                                  if(count == 0)
                                    IsLoading = false;
                                IsMPZLoading = false;
                                if (results == null)return;
                                Mpz = new ObservableCollection<Base>();
                             
                                if(results.Count > 0)
                                {
                                      int indexToAdd = -1;
                                      foreach (var item in results)
                                      {
                                         indexToAdd++;
                                         Mpz.Add(item);
                                         if (indexToAdd == 2 || indexToAdd == 10 || indexToAdd == 22 || indexToAdd == 34 || indexToAdd == 49 ||
                                             indexToAdd == 63 || indexToAdd == 78 || indexToAdd == 88 || indexToAdd == 99)
                                                 Mpz.Insert(indexToAdd, new ListAdvert());
                                      }
                                }

                            });
                    }),
                };

                await Task.WhenAll(tasks);
            }
            catch (Exception)
            {
                IsLoading = false;
            }
            if (count == 0) IsLoading = false;
        }
    }
}