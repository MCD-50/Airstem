using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;


namespace Musicus.Helpers.CustomContols
{
    public class FlipViewIndicator : ListBox
    {
        public FlipViewIndicator()
        {
            this.DefaultStyleKey = typeof(FlipViewIndicator);
        }

        /// <summary>
        /// Gets or sets the flip view.
        /// </summary>
        public FlipView FlipView
        {
            get { return (FlipView)GetValue(FlipViewProperty); }
            set { SetValue(FlipViewProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="FlipView"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FlipViewProperty =
            DependencyProperty.Register("FlipView", typeof(FlipView), typeof(FlipViewIndicator), new PropertyMetadata(null, (depobj, args) =>
            {
                FlipViewIndicator fvi = (FlipViewIndicator)depobj;
                FlipView fv = (FlipView)args.NewValue;

                // this is a special case where ItemsSource is set in code
                // and the associated FlipView's ItemsSource may not be available yet
                // if it isn't available, let's listen for SelectionChanged 
                fv.SelectionChanged += (s, e) =>
                {
                    fvi.ItemsSource = fv.ItemsSource;
                };

                fvi.ItemsSource = fv.ItemsSource;

                // create the element binding source
                Binding eb = new Binding();
                eb.Mode = BindingMode.TwoWay;
                eb.Source = fv;
                eb.Path = new PropertyPath("SelectedItem");

                // set the element binding to change selection when the FlipView changes
                fvi.SetBinding(FlipViewIndicator.SelectedItemProperty, eb);
            }));
    }
}
