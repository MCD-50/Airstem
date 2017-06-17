#region

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Microsoft.Xaml.Interactivity;
#endregion

namespace Musicus.Utilities
{
    public class ShowFlyoutAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {

            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            return null;
            //var element = sender as FrameworkElement;
            //if (element != null)
            //{
            //    FlyoutBase.ShowAttachedFlyout(element);
            //}

            //return sender;
        }
    }
}