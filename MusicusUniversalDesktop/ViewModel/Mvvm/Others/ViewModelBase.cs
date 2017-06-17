using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.Xaml.Navigation;

namespace Musicus.ViewModel.Mvvm.Others
{
    public abstract class ViewModelBase : ObservableObject
    {
        public bool IsInDesignMode => DesignMode.DesignModeEnabled;
        public string PageKey { get; set; }

        public static CoreDispatcher Dispatcher = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().CoreWindow.Dispatcher;
        public virtual void OnNavigatedTo(object parameter, NavigationMode mode, Dictionary<string, object> state)
        {
        }

        public virtual void OnSaveState(bool suspending, Dictionary<string, object> state)
        {
        }

        public virtual void OnNavigatedFrom()
        {
            
        }

        public virtual string SimplifiedParameter(object parameter)
        {
            return parameter?.ToString();
        }
    }
}