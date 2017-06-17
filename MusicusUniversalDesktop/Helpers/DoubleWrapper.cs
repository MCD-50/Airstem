using System.Diagnostics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Musicus.Helpers
{
    public class DoubleWrapper : DependencyObject
    {
        public DoubleWrapper()
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                MarginValue = new Thickness(-10, 1, -10, 1);
                //OddRowColor = new SolidColorBrush(Colors.Transparent);
            }

            else
            {
                MarginValue = new Thickness(0, 1, 0, 1);
                //OddRowColor = new SolidColorBrush(ColorHelper.FromArgb(0, 19, 19, 19));
            }

            Debug.WriteLine(MarginValue);
        }

       

        public double ExactWidth
        {
            get { return (double)GetValue(ExactWidthProperty); }
            set { SetValue(ExactWidthProperty, value); }
        }

        public double ExactHeight
        {
            get { return (double)GetValue(ExactHeightProperty); }
            set { SetValue(ExactHeightProperty, value); }
        }

        public double VideoWidth
        {
            get { return (double)GetValue(VideoWidthProperty); }
            set { SetValue(VideoWidthProperty, value); }
        }

        public double VideoHeight
        {
            get { return (double)GetValue(VideoHeightProperty); }
            set { SetValue(VideoHeightProperty, value); }
        }

        public double AlbumHeight
        {
            get { return (double)GetValue(AlbumHeightProperty); }
            set { SetValue(AlbumHeightProperty, value); }
        }

        public double OnlySearchPageAlbumWidth
        {
            get { return (double)GetValue(OnlySearchPageAlbumWidthProperty); }
            set { SetValue(OnlySearchPageAlbumWidthProperty, value); }
        }

        public double OnlySearchPageAlbumHeight
        {
            get { return (double)GetValue(OnlySearchPageAlbumHeightProperty); }
            set { SetValue(OnlySearchPageAlbumHeightProperty, value); }
        }

        public double OnlineHeight
        {
            get { return (double)GetValue(OnlineHeightProperty); }
            set { SetValue(OnlineHeightProperty, value); }
        }

        public double OnlineWidth
        {
            get { return (double)GetValue(OnlineWidthProperty); }
            set { SetValue(OnlineWidthProperty, value); }
        }

        public Thickness MarginValue
        {
            get { return (Thickness)GetValue(MarginValueProperty); }
            set { SetValue(MarginValueProperty, value); }
        }

        public SolidColorBrush OddRowColor
        {
            get { return (SolidColorBrush)GetValue(OddRowColorProperty); }
            set { SetValue(OddRowColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnlySearchPageAlbumWidthProperty =
            DependencyProperty.Register("OnlySearchPageAlbumWidth", typeof(double), typeof(DoubleWrapper), new PropertyMetadata(0.0));

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnlySearchPageAlbumHeightProperty =
            DependencyProperty.Register("OnlySearchPageAlbumHeight", typeof(double), typeof(DoubleWrapper), new PropertyMetadata(0.0));

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnlineWidthProperty =
            DependencyProperty.Register("OnlineWidth", typeof(double), typeof(DoubleWrapper), new PropertyMetadata(0.0));

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnlineHeightProperty =
            DependencyProperty.Register("OnlineHeight", typeof(double), typeof(DoubleWrapper), new PropertyMetadata(0.0));

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExactWidthProperty =
            DependencyProperty.Register("ExactWidth", typeof(double), typeof(DoubleWrapper), new PropertyMetadata(0.0));

         // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExactHeightProperty =
           DependencyProperty.Register("ExactHeight", typeof(double), typeof(DoubleWrapper), new PropertyMetadata(0.0));

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VideoWidthProperty =
           DependencyProperty.Register("VideoWidth", typeof(double), typeof(DoubleWrapper), new PropertyMetadata(0.0));

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VideoHeightProperty =
           DependencyProperty.Register("VideoHeight", typeof(double), typeof(DoubleWrapper), new PropertyMetadata(0.0));
     
        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AlbumHeightProperty =
            DependencyProperty.Register("AlbumHeight", typeof(double), typeof(DoubleWrapper), new PropertyMetadata(0.0));


        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarginValueProperty =
           DependencyProperty.Register("MarginValue", typeof(Thickness), typeof(DoubleWrapper), new PropertyMetadata(0.0));

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OddRowColorProperty =
           DependencyProperty.Register("OddRowColor", typeof(SolidColorBrush), typeof(DoubleWrapper), new PropertyMetadata(0.0));

    }
}
