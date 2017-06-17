
using Musicus.Core.Utils.Interfaces;
using Musicus.Helpers;
using System.Collections.Generic;
using Windows.UI.ViewManagement;
using System;
using Musicus.ViewModel.Mvvm.Commands;

namespace Musicus.ViewModel
{
    public class SettingViewModel : Mvvm.Others.ViewModelBase
    {
        private IAppSettingsHelper _appSettingsHelper;
        private ICredentialHelper _credential;
        private List<string> _folders = new List<string>();
        private string _userName;
        private string _token;
        //public Command LoginClickRelay { get; set; }
        public string _glyph;
      
        public SettingViewModel(IAppSettingsHelper _helper, ICredentialHelper credential)
        {
            _appSettingsHelper = _helper;
            _credential = credential;
            //LoginClickRelay = new Command(LoginButtonClicked);
            Glyph = "\uE1E2";
            LoginAsync();
        }

        private void LoginAsync()
        {
            var creds = _credential.GetCredentials("facebook");
            if (creds != null)
            {
                Username = creds.GetUsername();
                _token = creds.GetPassword();
                //IsLoggedIn = true;
                Glyph = "\uE1E0";
            }
        }

        public bool IsLoggedIn
        {
            get { return true; }
            //set { Set(ref _isLoggedIn, value); }
        }

        public string Username
        {
            get { return _userName; }
            set { Set(ref _userName, value); }
        }

        public string Glyph
        {
            get { return _glyph; }
            set { Set(ref _glyph, value); }
        }

        public bool Background
        {
            get { return _appSettingsHelper.Read("IsBackgroundOnOrOff", true, SettingsStrategy.Roaming); }
            set
            {
                _appSettingsHelper.Write("IsBackgroundOnOrOff", value, SettingsStrategy.Roaming);
                RaisePropertyChanged();
            }
        }

        public bool LyricsOnOrOff
        {
            get { return _appSettingsHelper.Read("LyricsOnOrOff", true, SettingsStrategy.Roaming); }
            set
            {
                _appSettingsHelper.Write("LyricsOnOrOff", value, SettingsStrategy.Roaming);
                RaisePropertyChanged();
            }
        }

        public bool HdVideoOnOff
        {
            get { return _appSettingsHelper.Read("HdVideoOnOff", true, SettingsStrategy.Roaming); }
            set
            {
                _appSettingsHelper.Write("HdVideoOnOff", value, SettingsStrategy.Roaming);
                RaisePropertyChanged();
            }
        }

        public bool VideoOnOff
        {
            get { return _appSettingsHelper.Read<bool>("VideoOnOff", false, SettingsStrategy.Roaming); }
            set
            {
                _appSettingsHelper.Write("VideoOnOff", value, SettingsStrategy.Roaming);
                RaisePropertyChanged();
            }
        }

        public bool Notification
        {
            get { return _appSettingsHelper.Read("NotificationOnOrOff", false, SettingsStrategy.Roaming); }
            set
            {
                _appSettingsHelper.Write("NotificationOnOrOff", value, SettingsStrategy.Roaming);
                RaisePropertyChanged();
            }
        }

        public bool TurboMode
        {
            get { return _appSettingsHelper.Read("TurboModeOnOrOff", false, SettingsStrategy.Roaming); }
            set
            {
                _appSettingsHelper.Write("TurboModeOnOrOff", value, SettingsStrategy.Roaming);
                RaisePropertyChanged();
            }
        }


        public bool SpotifyArtworkSync
        {
            get { return _appSettingsHelper.Read("SpotifyArtworkSyncOnOrOff", true, SettingsStrategy.Roaming); }
            set
            {
                _appSettingsHelper.Write("SpotifyArtworkSyncOnOrOff", value, SettingsStrategy.Roaming);
                RaisePropertyChanged();
            }
        }


        public bool SafeStart
        {
            get { return _appSettingsHelper.Read("SafeStartOnOrOff", true, SettingsStrategy.Roaming); }
            set
            {
                _appSettingsHelper.Write("SafeStartOnOrOff", value, SettingsStrategy.Roaming);
                RaisePropertyChanged();
            }
        }


        public bool Enable
        {
            get { return _appSettingsHelper.Read("EnableOnOrOff", true, SettingsStrategy.Roaming); }
            set
            {
                _appSettingsHelper.Write("EnableOnOrOff", value, SettingsStrategy.Roaming);
                RaisePropertyChanged();
                if (Enable)
                    ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            }
        }

        public bool TrackView
        {
            get { return _appSettingsHelper.Read("TrackViewOnOrOff", true, SettingsStrategy.Roaming); }
            set
            {
                _appSettingsHelper.Write("TrackViewOnOrOff", value, SettingsStrategy.Roaming);
                RaisePropertyChanged();
            }
        }


        //public List<string> Folders
        //{
        //    get { return _folders; }
        //    set { Set(ref _folders, value); }
        //}

        //public async void SetIntoFolders()
        //{
        //    try
        //    {
        //        string text = " " + await StorageFileHelper.GetText("AddedFolder");
        //        Folders.Clear();
        //        if (string.IsNullOrEmpty(text))  return;
        //        foreach (string obj in text.Trim().Split(','))
        //            if (!string.IsNullOrEmpty(obj))
        //                Folders.Add(obj);
        //    }
        //    catch { }
        //}


        //public void LoginButtonClicked()
        //{
        //    if (!App.Locator.Network.IsActive)
        //    {
        //        Core.WinRt.Common.ToastManager.ShowError(Core.StringMessage.SomethinWentWrong);
        //        return;
        //    }
        //    if (IsLoggedIn)
        //    {
        //        ClearData(false);
        //    }
        //    else
        //    {
        //        //await DispatcherHelper.RunAsync(() => SheetUtility.OpenLoginToAirstemPage());
        //        //ChatAndSpeechEngine.GoogleLoginHelper.LoginAsync();
        //        //await ChatAndSpeechEngine.FacebookLoginHelper.Login();
        //    }
        //}

        //public async Task SaveData(string accessToken)
        //{
        //    var values = new String[7]; //await ChatAndSpeechEngine.FacebookLoginHelper.Set(accessToken);
        //    if (values == null)
        //    {
        //        ClearData();
        //        return;
        //    }

        //    Username = values[1];
        //    //IsLoggedIn = true;
           
        //    _credential.SaveCredentials("facebook", Username, accessToken);
        //    Glyph = "\uE1E0";

        //    Core.WinRt.Common.ToastManager.ShowError("Welcome " + Username + ".");
        //   // await Task.Factory.StartNew(() => ChatAndSpeechEngine.AzureService.PushData(values[0], values[1], values[2]));
        //}

        //public void ClearData(bool error = true)
        //{
        //    Username = string.Empty;
        //    //IsLoggedIn = false;
        //    _token = string.Empty;
        //    _credential.DeleteCredentials("facebook");
        //    Glyph = "\uE1E2";
        //    if (error)
        //        Core.WinRt.Common.ToastManager.ShowError(Core.StringMessage.SomethinWentWrong);
        //    else
        //        Core.WinRt.Common.ToastManager.ShowError("Logged out.");
        //}


        //public void SaveGoogleData(MobileServiceUser user)
        //{
            
        //}

    }
}
