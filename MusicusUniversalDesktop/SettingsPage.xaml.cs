using Musicus.Helpers;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Musicus.Core.WinRt.Common;
using Windows.UI.Xaml;
using Windows.Storage.Pickers;
using Musicus.Data.Collection.Model;
using Windows.Media.Playback;
using Musicus.Data;
using Windows.UI.Popups;

namespace Musicus
{
    public sealed partial class SettingsPage
    {

        public SettingsPage()
        {
            this.InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                MainPivot.Title = "SETTINGS";
                MainPivotItem1.Header = "personalize";
                MainPivotItem2.Header = "about";
            }
                  
        }

        private async void AddNewFolderClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                string text = string.Empty;
                FolderPicker folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;

                //music
                folderPicker.FileTypeFilter.Add(".wma");
                folderPicker.FileTypeFilter.Add(".m4a");
                folderPicker.FileTypeFilter.Add(".mp3");

                //videos
                folderPicker.FileTypeFilter.Add(".flv");
                folderPicker.FileTypeFilter.Add(".mp4");
                folderPicker.FileTypeFilter.Add(".3gp");


                StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {

                    //foreach (var xoo in App.Locator.CollectionService.Folders)
                    //    Debug.WriteLine(StorageApplicationPermissions.FutureAccessList.CheckAccess(await StorageFile.GetFileFromPathAsync(xoo.Url)));

                    var folders = App.Locator.CollectionService.Folders;
                    foreach(Folder i in folders)
                        if (i.Url.Equals(folder.Path, StringComparison.CurrentCultureIgnoreCase)) return;

                   
                    await App.Locator.CollectionService.AddFolderAsync(new Folder { Url = folder.Path }, folder);


                }
                else
                {

                }
            }
            catch (Exception exce)
            {
                ToastManager.ShowError("Something went wrong " + exce + ".");
            }
        }


      

        private void ImportSongs(object sender, RoutedEventArgs e)
        {
            SheetUtility.OpenImportRefreshPage(1);
        }

        async void RepairMusicus(object sender, RoutedEventArgs e)
        {
            if (BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing)
            {
                ToastManager.ShowError("Stop playback first.");
                return;
            }
            else
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                 async () =>
                 {
                  var md = new MessageDialog(
                         "This will delete everything from airstem and import your data again.",
                         "Are you sure you want to refresh?");
                  md.Commands.Add(new UICommand("Ok"));
                  md.Commands.Add(new UICommand("Cancel"));

                  var result = await md.ShowAsync();
                     if (result != null && result.Label.ToLower().Equals("ok"))
                         SheetUtility.OpenImportRefreshPage(3);
                 });
            }
            
        }

      

        private async void ContactClick(object sender, RoutedEventArgs e)
        {
            var mail = new Uri("mailto:?to=airstemapp@gmail.com&subject=Airstem user&body=\nThank You!!");
            await Launcher.LaunchUriAsync(mail);
        }

        private async void RateClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=28a6b450-5d24-4999-9e7c-1297abdbfb48"));

        }

        private void ShareMusicus(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
            var dataManager = DataTransferManager.GetForCurrentView();
            dataManager.DataRequested += DataTransferManagerOnDataRequested;
        }


        private void DataTransferManagerOnDataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            var request = e.Request;
            request.Data.Properties.Title = "Checkout Musicus For Windows Phone";
            request.Data.Properties.Description = "The App You Were Waiting for!!!!!";
            const string url = "\n" + "\n" + "\n" + "http://www.windowsphone.com/s?appid=28a6b450-5d24-4999-9e7c-1297abdbfb48";
            request.Data.SetText(request.Data.Properties.Description + " " + url);
        }


        private async void ArtistArtworkSync(object sender, RoutedEventArgs e)
        {
            ToastManager.Show("Downloading missing artworks.");
            await DownloadArtworks.DownloadAlbumsArtworkAsync();
            if (App.Locator.Setting.SpotifyArtworkSync)
                await DownloadArtworks.DownloadArtistsArtworkAsyncFromSpotify();
            else
                await DownloadArtworks.DownloadArtistsArtworkAsync();
        }

      

        //private void ColorChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        //{
        //    switch (ColorBox.SelectedIndex)
        //    {
        //        case 0://Accent
        //            (Application.Current.Resources["MusicusPivotItemSelectedTextColor"] as SolidColorBrush).Color = (Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush).Color;
        //            (Application.Current.Resources["MusicusPivotTitleTextColor"] as SolidColorBrush).Color = (Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush).Color;
        //            (Application.Current.Resources["MusicusPivotItemUnSelectedTextColor"] as SolidColorBrush).Color = (Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush).Color;
        //            (Application.Current.Resources["MusicusOtherColor"] as SolidColorBrush).Color = (Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush).Color;
        //            App.Locator.AppSettingsHelper.Write(Core.PlayerConstants.AppThemeIndex, 0);
        //            break;

        //        case 1://White
        //            (Application.Current.Resources["MusicusPivotItemSelectedTextColor"] as SolidColorBrush).Color = Colors.White;
        //            (Application.Current.Resources["MusicusPivotTitleTextColor"] as SolidColorBrush).Color = Colors.White;
        //            (Application.Current.Resources["MusicusPivotItemUnSelectedTextColor"] as SolidColorBrush).Color = Colors.White;
        //            (Application.Current.Resources["MusicusOtherColor"] as SolidColorBrush).Color = Colors.White;
        //            App.Locator.AppSettingsHelper.Write(Core.PlayerConstants.AppThemeIndex, 1);
        //            break;
        //    }
        //}

        private void ViewAddedFolderClick(object sender, RoutedEventArgs e)
        {
            SheetUtility.OpenAddedFoldersPage();
        }

        //public async void SendClick(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {

        //        SendButton.IsEnabled = false;

        //        if (!App.Locator.Setting.IsLoggedIn)
        //        {
        //            ToastManager.ShowError("Login required.");
        //            SendButton.IsEnabled = true;
        //            return;
        //        }


        //        if (InternetConnectionHelper.HasConnection())
        //        {
        //            if (string.IsNullOrEmpty(ChatTextBox.Text.Trim())) return;
        //            await ChatAndSpeechEngine.AzureService.SendChats(ChatTextBox.Text);
        //            ChatTextBox.Text = "";
        //            SendButton.IsEnabled = true;
        //            await GetChats();

        //        }
        //    }
        //    catch
        //    {

        //    }
        //}

        //private async Task GetChats()
        //{
        //    try
        //    {
        //        if (!InternetConnectionHelper.HasConnection())
        //        {
        //            MessageTextBlock.Visibility = Visibility.Visible;
        //            MessageTextBlock.Text = Core.StringMessage.NoInternetConnection;
        //            return;
        //        }

        //        List<Data.Collection.AzureModel.ChatTable> _chat = new List<Data.Collection.AzureModel.ChatTable>();
        //        await Task.Run(async () =>
        //        {
        //            _chat = await ChatAndSpeechEngine.AzureService.GetChats();
        //        });
        //        if (_chat != null)
        //        {
        //            ListItems.ItemsSource = _chat;
        //            MessageTextBlock.Visibility = Visibility.Collapsed;
        //        }
        //        else
        //        {
        //            MessageTextBlock.Visibility = Visibility.Visible;
        //            MessageTextBlock.Text = Core.StringMessage.EmptyMessage;
        //        }
        //    }
        //    catch
        //    {

        //    }
        //}

        //bool loaded = false;
        //private async void PivotChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        //{
        //    if (Pivot.SelectedIndex == 1 && !loaded)
        //    {
        //        MessageTextBlock.Text = Core.StringMessage.LoadingPleaseWait;
        //        await GetChats();
        //        loaded = true;
        //    }
        //}

        async void SendPayPal(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.paypal.me/AyushAS"));
        }


        void SendMsgTest(object sender, RoutedEventArgs e)
        {
            //FacebookLoginHelper _loginHelper = new FacebookLoginHelper();
            //_loginHelper.LoginToGoogleAsync();

            //await ChatAndSpeechEngine.FacebookLoginHelper.LoginScreen();
            //smtp.sendgrid.net, Convert.ToInt32(587), false, "azure_7cab7a8cef25f6541031c8d18445949a@azure.com"
            //smtp.gmail.com, 587, true, Core.StringMessage.GmailUsername, Core.StringMessage.Password
            //"smtp-mail.outlook.com", 25, false, Core.StringMessage.OutlookUsername, Core.StringMessage.Password
            //using (SmtpClient client = new SmtpClient("smtp.sendgrid.net", 587, false, ApiKeys.SendGridUser, Core.StringMessage.Password))
            //{
            //    EmailMessage emailMessage = new EmailMessage();
            //    emailMessage.To.Add(new EmailRecipient("ayush_shukla@live.com","User"));
            //    emailMessage.Subject = "Subject line of your message";              
            //    emailMessage.Body = "This is an email sent from a WinRT app!";
            //    await client.SendMail(emailMessage);
            //}
        }


        private async void TwitterClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://twitter.com/AirstemApp"));
        }

        private async void InstagramClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.instagram.com/airstemapp"));
        }

        private async void PrivacyClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://airstemapp.azurewebsites.net/privacy.html"));
        }

        private async void HelpClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://airstemapp.azurewebsites.net/help.html"));
        }

        private async void AirstemClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://airstemapp.azurewebsites.net"));
        }

        private async void ChangeLogClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://airstemapp.azurewebsites.net/change.html"));
        }

        void ImportVideos(object sender, RoutedEventArgs e)
        {
            SheetUtility.OpenImportRefreshPage(2);
        }

        private void ColorChangeClicked(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            SheetUtility.OpenColorViewPage();
        }

        private async void DownloadAirflow(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://play.google.com/store/apps/details?id=com.airstem.airflow.ayush.airflow&hl=en"));
        }
    }
}
