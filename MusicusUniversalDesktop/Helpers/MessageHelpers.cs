using Musicus.Data.Collection.Model;
using Musicus.Utilities;
using Musicus.ViewModel.Mvvm.Dispatcher;
using System;
using System.Collections.Generic;
using Musicus.Core.WinRt.Common;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Popups;
using Musicus.Data.Collection;

namespace Musicus.Helpers
{
    public static class MessageHelpers
    {
        public static async Task DeleteConfirm(Song song)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
             async () =>
             {
                 var md = new MessageDialog(
                     "If you delete " + song.Name.ToLower() + " it won't be on this app and device anymore.",
                     "Are you sure you want to delete this?");
                 md.Commands.Add(new UICommand("Delete"));
                 md.Commands.Add(new UICommand("Cancel"));

                 var result = await md.ShowAsync();
                 if (result != null && result.Label.Equals("Delete"))
                 {
                     await DispatcherHelper.RunAsync(async () =>
                     {
                         await CollectionHelper.DeleteEntryAsync(song);                         
                         if (!song.IsDownload)
                             await CollectionHelper.DeleteFromDevice(song);
                     });
                 }
             });

          
        }


        public async static Task DeleteConfirm(BaseEntry item)
        {
            try
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                async () =>
                {

                  if (item is Album)
                  {
                      Album album = item as Album;
                      var md = new MessageDialog(
                      "If you delete " + album.Name.ToLower() + " it won't be on this app anymore.",
                      "Are you sure you want to delete this?");
                      md.Commands.Add(new UICommand("Delete"));
                      md.Commands.Add(new UICommand("Cancel"));

                      var result = await md.ShowAsync();
                      if (result != null && result.Label.Equals("Delete"))
                      {
                          await DispatcherHelper.RunAsync(async () =>
                          {
                              await CollectionHelper.DeleteEntryAsync(album);
                          });
                      }
                  }
                  else if(item is Artist)
                  {
                      Artist artist = item as Artist;
                      var md = new MessageDialog(
                          "If you delete " + artist.Name.ToLower() + " it won't be on this app anymore.",
                          "Are you sure you want to delete this?");
                      md.Commands.Add(new UICommand("Delete"));
                      md.Commands.Add(new UICommand("Cancel"));

                      var result = await md.ShowAsync();
                      if (result != null && result.Label.Equals("Delete"))
                      {
                          await DispatcherHelper.RunAsync(async () =>
                          {
                              await CollectionHelper.DeleteEntryAsync(artist);
                          });
                      }
                  }

                  else if(item is Playlist)
                  {
                        Playlist play = item as Playlist;
                        var md = new MessageDialog(
                            "If you delete " + play.Name.ToLower() + " it won't be on this app anymore.",
                            "Are you sure you want to delete this?");

                        md.Commands.Add(new UICommand("Delete"));
                        md.Commands.Add(new UICommand("Cancel"));
                        var result = await md.ShowAsync();
                        if (result != null && result.Label.Equals("Delete"))
                        {
                            await DispatcherHelper.RunAsync(async () =>
                            {
                                await CollectionHelper.DeleteEntryAsync(play);
                            });
                        }
                    }

                  else if(item is Video)
                  {
                        Video vid = item as Video;
                        var md = new MessageDialog(
                            "If you delete " + vid.Title.ToLower() + " it won't be on this app anymore.",
                            "Are you sure you want to delete this?");

                        md.Commands.Add(new UICommand("Delete"));
                        md.Commands.Add(new UICommand("Cancel"));
                        var result = await md.ShowAsync();
                        if (result != null && result.Label.Equals("Delete"))
                        {
                            await DispatcherHelper.RunAsync(async () =>
                            {
                                await CollectionHelper.DeleteEntryAsync(vid);
                            });
                        }
                    }

                  else
                  {
                     ToastManager.Show("Something went wrong.");
                  }
               });
            }
            catch
            {
                ToastManager.Show("Something went wrong.");
            }
        }



        public static async Task DeleteConfirm(List<Song> songs)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
              async () =>
              {
                  var md = new MessageDialog(
                      "If you delete these tracks they won't be on this app and device anymore.",
                      "Are you sure you want to delete this?");
                  md.Commands.Add(new UICommand("Delete"));
                  md.Commands.Add(new UICommand("Cancel"));

                  var result = await md.ShowAsync();

                  if (result != null && result.Label.ToLower().Equals("delete"))
                      await AddDeleteShareManager.DeletePhaseConfirmed(songs);
              });
        }


        public static async Task RateAndReviewReminder()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
               async () =>
               {
                   var md = new MessageDialog(
                       "Love airstem, rate us 5 star.",
                       "Rate airstem");
                   md.Commands.Add(new UICommand("Rate"));
                   md.Commands.Add(new UICommand("Cancel"));

                   var result = await md.ShowAsync();

                   if (result != null && result.Label.ToLower().Equals("rate"))
                       await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=28a6b450-5d24-4999-9e7c-1297abdbfb48"));
               });
        }


     
        public static async Task LoginRequired()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
               async () =>
               {
                   var md = new MessageDialog(
                       "Login from settings page to your facebook account.",
                       "Login to facebook");
                   md.Commands.Add(new UICommand("Login"));
                   md.Commands.Add(new UICommand("Cancel"));

                   var result = await md.ShowAsync();
                   //if (result != null && result.Label.ToLower().Equals("login"))
                       //App.Locator.Setting.LoginButtonClicked();
                
               });

        }

        public static async void DeleteRequestAsync(string message, string title, Song song)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
             async () =>
             {
                 var md = new MessageDialog(message, title);
                 md.Commands.Add(new UICommand("Delete"));
                 md.Commands.Add(new UICommand("Close"));
                 var result = await md.ShowAsync();
                
                 if (result != null && result.Label.ToLower().Equals("delete"))
                     await DeleteConfirm(song);
             });
        }



        public static async void ShowError(string message, string title, Song song = null, bool IsRematchError = false)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
              async () =>
                {
                    var md = new MessageDialog(message, title);
                    if (IsRematchError) md.Commands.Add(new UICommand("Ok"));
                    else md.Commands.Add(new UICommand("Close"));
                    var result = await md.ShowAsync();
                    if (IsRematchError)
                    {
                        if (result != null && result.Label.ToLower().Equals("ok"))
                        {
                            song.AudioUrl = string.Empty;
                            song.SongState = SongState.Matching;
                            CollectionHelper.MatchSong(song);
                           
                        }
                    }
                });
        }

        public static async Task RefreshMusicus()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                async () =>
                {
                    var md = new MessageDialog(
                                  "Error while booting, something went wrong.",
                                  "Error!!!");
                    md.Commands.Add(new UICommand("Repair"));
                    md.Commands.Add(new UICommand("Cancel"));
                    var result = await md.ShowAsync();
                    if (result != null && result.Label.ToLower().Equals("repair"))
                    {
                        SheetUtility.OpenImportRefreshPage(2);
                    }
                });
        }

        public static async Task ErrorOnImportOrRefresh()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                async () =>
                {
                    var md = new MessageDialog(
                                  "Error while looking for files, something went wrong.",
                                  "Error!!!");
                    md.Commands.Add(new UICommand("Close"));
                    var result = await md.ShowAsync();
                    if (result != null && result.Label.ToLower().Equals("close"))
                    {
                        SheetUtility.CloseImportRefreshPage();
                    }
                });
        }

    }
}
