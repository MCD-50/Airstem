using Musicus.Helpers;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace Musicus
{
    public sealed partial class AddAPlaylist : IModalSheetPage
    {
        public AddAPlaylist()
        {
            this.InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                MainPivot.Title = "COLLECTION";
                MainPivotItem1.Header = "add a playlist";
            }
        }

        public Popup Popup { get; private set; }
        public void OnOpened(Popup popup)
        {
            Popup = popup;
            App.Navigator.ShowBack();
        }

        public void OnClosed()
        {
            Popup = null;
            PlaylistTextBox.Text = string.Empty;
            if (App.Navigator.StackCount > 0) return;
            App.Navigator.HideBack();       
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            SheetUtility.CloseAddAPlaylistPage();   
        }

        private async void Create(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PlaylistTextBox.Text))
            {
                Core.WinRt.Common.ToastManager.ShowError("Enter a name.");
            }
            else
            {
                if (App.Locator.CollectionService.
                    Playlists.FirstOrDefault(p =>
                        string.Equals(p.Name, PlaylistTextBox.Text, StringComparison.CurrentCultureIgnoreCase)) != null)
                {
                    PlaylistTextBox.Text = "";
                    Core.WinRt.Common.ToastManager.ShowError("Already exists.");
                }
                else
                {                  
                    var playlist = await App.Locator.CollectionService.CreatePlaylistAsync(FirstCharToUpper(PlaylistTextBox.Text));
                    SheetUtility.CloseAddAPlaylistPage();
                }
            }
        }

        public static string FirstCharToUpper(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            char[] array = value.ToCharArray();
            // Handle the first letter in the string.
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ')
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }
            return new string(array);
        }
    }
}
