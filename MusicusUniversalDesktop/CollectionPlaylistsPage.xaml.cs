
#region
using Musicus.Helpers;
using Musicus.Utilities;
using System;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

#endregion

namespace Musicus
{
    public sealed partial class CollectionPlaylistsPage
    {

        public CollectionPlaylistsPage()
        {
            this.InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                MainPivot.Title = "COLLECTION";
                MainPivotItem1.Header = "playlists";
            }
        }

        private void OpenNowPlaying(object sender, RoutedEventArgs e)
        {
            AddDeleteShareManager.OpenNowPlaying();
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

        private void NewPlaylistClick(object sender, RoutedEventArgs e)
        {
            SheetUtility.OpenAddAPlaylistPage();
        }
    }
}




