

using Musicus.Data.Model.Advertisement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Musicus.Helpers
{
    public class DataTemplateHelper : DataTemplateSelector
    {
        public DataTemplate ListAdvertTemplate { get; set; }
        public DataTemplate AdvertTemplate { get; set; }
        public DataTemplate NormalTemplate { get; set; }
       
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is Advert)
                return AdvertTemplate;
            else if (item is ListAdvert)
                return ListAdvertTemplate;
            return NormalTemplate;
        }
    }
}
