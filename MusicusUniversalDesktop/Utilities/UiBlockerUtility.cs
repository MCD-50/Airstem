using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace Musicus.Utilities
{
    public static class UiBlockerUtility
    {
        private static Popup _popup;
        private static CommandBar _commandBar;
        private static Visibility _commandPrevVisibility;
        public static bool SupressBackEvents { get; private set; }
        public static void BlockNavigation(bool hideNavBar = true)
        {
            SupressBackEvents = true;
            if (!hideNavBar) return;
            _commandBar = (App.RootFrame.Content as Page).BottomAppBar as CommandBar;
            if (_commandBar != null)
            {
                _commandPrevVisibility = _commandBar.Visibility;
                _commandBar.Visibility = Visibility.Collapsed;
            }
        }


        private static bool _subscribed = false;
        private static Grid grid;
        private static TextBlock TextBlock;
        public static void Block(string header)
        {
            if (_popup != null) return;
            TextBlock = new TextBlock();
            TextBlock.Text = string.Empty;  
           
            const double opacity = 255 * 0.95;
            var size = Window.Current.Bounds;

            TextBlock.FontSize = 20;
            TextBlock.FontWeight = Windows.UI.Text.FontWeights.Normal;
            TextBlock.TextAlignment = TextAlignment.Center;
            TextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            TextBlock.VerticalAlignment = VerticalAlignment.Center;
            TextBlock.TextWrapping = TextWrapping.Wrap;

            TextBlock block = new TextBlock()
            {
                FontSize = 23,
                Text = header,
                FontWeight = Windows.UI.Text.FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 20)
            };

            ProgressRing ProgressRing = new ProgressRing()
            {
                IsActive = true,
                IsEnabled = true,
                Height = 50,
                Width = 50,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 20, 0, 20)
            };



            StackPanel StackPanel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center 
            };

            StackPanel.Children.Add(block);
            StackPanel.Children.Add(TextBlock);
            StackPanel.Children.Add(ProgressRing);
         
            grid = new Grid()
            {
                Width = size.Width,
                Height = size.Height,
                Background = new SolidColorBrush(Color.FromArgb(Convert.ToByte
                (opacity), 0x0, 0x0, 0x0))
                
            };

            grid.Children.Add(StackPanel);

            _popup = new Popup()
            {
                Child = grid,
                IsOpen = true
            };
            BlockNavigation();


            _subscribed = true;
            Window.Current.SizeChanged += Current_SizeChanged;
        }




        private static void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            var size = e.Size;//Window.Current.Bounds;
            grid.Width = size.Width;
            grid.Height = size.Height;
        }

        public static void SetMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
                TextBlock.Text = message;
        }

        public static void Unblock()
        {

            SupressBackEvents = false;
            if (_commandBar != null)
            {
                _commandBar.Visibility = _commandPrevVisibility;
                _commandBar = null;
            }
            if (_popup == null) return;
            _popup.IsOpen = false;
            _popup = null;

            if (_subscribed)
            {
                Window.Current.SizeChanged -= Current_SizeChanged;
                _subscribed = false;
            }
        }
    }
}
