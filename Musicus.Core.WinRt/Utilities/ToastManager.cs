
// Copyright (c) 2016 Airstem inc.
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.





#region

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

#endregion

namespace Musicus.Core.WinRt.Common
{
    public class ToastManager
    {

        private double _millisecondsToHide = 1500;
        private static ToastManager _current;
        private Popup _popup;
     
        public ToastManager(string msg, bool isError = false)
        {
            CreatePopUp(msg);      
        }

        public void Dismiss()
        {
            try
            {
                if (_popup != null)
                    _popup.IsOpen = false;
                _popup = null;
                _current = null;
            }
            catch
            {
            }
        }

        public static ToastManager Show(string msg)
        {
            return Show(msg, null, null);
        }

        public static ToastManager Show(string msg, params object[] args)
        {
            return Show(null, TimeSpan.FromSeconds(1.5), msg, args);
        }

        public static ToastManager Show(Action action, string msg, params object[] args)
        {
            return Show(action, TimeSpan.FromSeconds(1.5), msg, args);
        }

        public static ToastManager Show(Action action, TimeSpan duration, string msg, params object[] args)
        {
            if (args != null)
            {
                msg = string.Format(msg, args);
            }

            if (_current != null)
                _current.Dismiss();

            var curtain = new ToastManager(msg) { _millisecondsToHide = duration.TotalMilliseconds };
            _current = curtain;
            return curtain;
        }

        public static ToastManager ShowError(string msg)
        {
            return ShowError(null, msg);
        }

        public static ToastManager ShowError(string msg, params object[] args)
        {
            return ShowError(3500, null, msg, args);
        }

        public static ToastManager ShowError(Action action, string msg, params object[] args)
        {
            return ShowError(3500, action, msg, args);
        }

        public static ToastManager ShowError(int milliToHide, Action action, string msg, params object[] args)
        {
            if (args != null)
            {
                msg = string.Format(msg, args);
            }

            if (_current != null)
                _current.Dismiss();


            var curtain = new ToastManager(msg, true)
            { _millisecondsToHide = milliToHide };
            _current = curtain;
            return curtain;
        }



        private void CreatePopUp(string message)
        {
            double leftRight = (Window.Current.Bounds.Width - 250) / 2;
            double top = (Window.Current.Bounds.Height - 120);

            _popup = new Popup
            {
                Width = 250,
                Height = 50,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(leftRight, top, leftRight,0)
            };

            Border Border = new Border
            {
                Width = 250,
                Height = 50,
                Background = (Application.Current.Resources["MusicusNotificationColor"] as SolidColorBrush),
                Margin = new Thickness(10, 10, 10, 50),
                //CornerRadius = new CornerRadius(8),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom  
            };

            TextBlock Msg = new TextBlock
            {
                Text = message,
                FontSize = 14,
                Margin = new Thickness(10, 10, 10, 10),
                IsTextScaleFactorEnabled = false,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap
            };

            Border.Child = Msg;
            _popup.Child = Border;
            _popup.IsOpen = true;
        
            ShowPopUp(_popup);
        }

        private void ShowPopUp(Popup _popup)
        {
            DoubleAnimation Animation = new DoubleAnimation
            {
                From = 0,
                To = .9,
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            };

            var sb = new Storyboard();
            sb.Children.Add(Animation);
            Storyboard.SetTarget(Animation, _popup);
            Storyboard.SetTargetProperty(Animation, "Opacity");
            sb.Begin();
            HidePopUp(_popup);
        }

     

        private async void HidePopUp(Popup _popup)
        {
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(_millisecondsToHide));
            DoubleAnimation Animation = new DoubleAnimation
            {
                From = .9,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            };

            var sb = new Storyboard();
            sb.Children.Add(Animation);
            Storyboard.SetTarget(Animation, _popup);
            Storyboard.SetTargetProperty(Animation, "Opacity");

            sb.Begin();
            await System.Threading.Tasks.Task.Delay(200);
            Dismiss();
        }
    }
}