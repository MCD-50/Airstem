#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#endregion

namespace Musicus.TransitionHelpers
{
    public sealed class Navigator
    {
        private readonly Dictionary<Type, PageBase> _pages = new Dictionary<Type, PageBase>();
        private readonly Canvas _rootContainer;
        private readonly Page _rootPage;
        private readonly Stack<Func<bool, PageTransition>> _stack = new Stack<Func<bool, PageTransition>>();

        public Navigator(Page rootPage, Canvas rootContainer)
        {
            _rootPage = rootPage;
            _rootContainer = rootContainer;
            _rootContainer.SizeChanged += rootContainer_SizeChanged;
            rootContainer_SizeChanged(null, null);
            CurrentPage = null;
        }


        public PageBase CurrentPage { get; private set; }

        public int StackCount { get; set; }

        public void AddPage<T>(T page)
            where T : PageBase
        {
            if (_pages.ContainsKey(typeof(T)))
            {
                throw new InvalidOperationException("Pages can only be registered once.");
            }
            _pages.Add(typeof(T), page);

            page.IsHitTestVisible = false;
            page.Width = _rootContainer.ActualWidth;
            page.Height = _rootContainer.ActualHeight;
            Canvas.SetTop(page, 0);
            Canvas.SetLeft(page, 0);
            page.Opacity = 0;
            _rootContainer.Children.Add(page);
        }

        public void Commit(Func<bool, PageTransition> action)
        {
            _stack.Push(action);
        }

        public T GetPage<T>()
            where T : PageBase
        {
            if (!_pages.ContainsKey(typeof(T)))
            {
                AddPage(Activator.CreateInstance<T>());
            }
            return _pages[typeof(T)] as T;
        }


        public bool GoBack()
        {
            if (_stack.Count == 0)
            {
                return false;
            }
            _stack.Pop().Invoke(true);
            return true;
        }

        public void GoTo<TPage, TTransition>(Object parameter, bool includeInBackStack = true)
            where TPage : PageBase
            where TTransition : PageTransition, new()
        {

            PageBase page;
            if (CurrentPage != null && CurrentPage.GetType() == typeof(TPage))
            {
                CurrentPage.NavigatedTo(NavigationMode.Refresh, parameter);
                NavigationClearer();
                return;
            }
            //OnNavigating();
            if (CurrentPage != null)
            {
                CurrentPage.IsHitTestVisible = false;
                CurrentPage.NavigatedFrom(NavigationMode.Forward);
                page = GetPage<TPage>();

        
                page.BeforeNavigateTo();
                var transition = Activator.CreateInstance<TTransition>();

                transition.FromPage = CurrentPage;
                transition.ToPage = page;
                transition.Play(() =>
                {
                    CurrentPage = page;
                    page.IsHitTestVisible = true;
                    var from = _rootContainer.Children.IndexOf(page);
                    var to = _rootContainer.Children.Count - 1;

                    _rootContainer.Children.Move((uint)from, (uint)to);

                    page.NavigatedTo(NavigationMode.Forward, parameter);
                    _rootPage.BottomAppBar = page.Bar;
                    //OnNavigated();
                });

                if (includeInBackStack || _stack.Count > 0)
                {
                    PageTransition navTransition = null;
                    if (!includeInBackStack)
                    {
                        navTransition = _stack.Pop().Invoke(false);
                        navTransition.ToPage = transition.ToPage;
                    }
                    else
                    {
                        navTransition = transition;
                    }
                    Commit(
                        execute =>
                        {
                            if (!execute)
                            {
                                return transition;
                            }

                            navTransition.PlayReverse(
                                () =>
                                {
                                    CurrentPage = navTransition.FromPage;

                                    var from = _rootContainer.Children.IndexOf(navTransition.FromPage);
                                    var to = _rootContainer.Children.Count - 1;

                                    _rootContainer.Children.Move((uint)from, (uint)to);

                                    _rootPage.BottomAppBar = navTransition.FromPage.Bar;
                                    navTransition.ToPage.NavigatedFrom(NavigationMode.Back);
                                    navTransition.ToPage.IsHitTestVisible = false;
                                    navTransition.FromPage.NavigatedTo(NavigationMode.Back, null);
                                    navTransition.FromPage.IsHitTestVisible = true;
                                });
                            return transition;
                        });
                    NavigationClearer();
                    return;
                }
            }
            page = GetPage<TPage>();

         
            page.IsHitTestVisible = true;
            page.BeforeNavigateTo();

            TransitionHelper.Show(page);

            CurrentPage = page;
            _rootPage.BottomAppBar = page.Bar;

            var fromMain = _rootContainer.Children.IndexOf(page);
            var toMain = _rootContainer.Children.Count - 1;

            _rootContainer.Children.Move((uint)fromMain, (uint)toMain);
            page.NavigatedTo(NavigationMode.Forward, parameter);

            
            //OnNavigated();
        }


        public void NavigationClearer()
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") return;

            if ((CurrentPage is HomePage || CurrentPage is CollectionPage || CurrentPage is SettingsPage || 
               CurrentPage is CollectionPlaylistsPage || CurrentPage is DownloadManager 
               || CurrentPage is NowPlayingPage || CurrentPage is SearchPage) && App.BackButtonNavigator != null)
            {
               
                _stack.Clear();
                App.BackButtonNavigator.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                StackCount = 0;

                SearchPage.ClearData();

            }
            else
                ShowBack();
        }

        public void ShowBack()
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") return;
            if (App.BackButtonNavigator != null)
                App.BackButtonNavigator.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        public void HideBack()
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") return;
            if (App.BackButtonNavigator != null)
                App.BackButtonNavigator.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;     
        }


        //private void OnNavigated()
        //{
        //    var eventHandler = Navigated;
        //    if (eventHandler != null)
        //    {
        //        eventHandler.Invoke(this, EventArgs.Empty);
        //    }
        //}

        //private void OnNavigating()
        //{
        //    var eventHandler = Navigating;
        //    if (eventHandler != null)
        //    {
        //        eventHandler.Invoke(this, EventArgs.Empty);
        //    }
        //}

        private void rootContainer_SizeChanged(Object sender, SizeChangedEventArgs e)
        {
            foreach (var page in _pages)
            {
                page.Value.SetSize(new Size(_rootContainer.ActualWidth, _rootContainer.ActualHeight));
            }
        }

        

        //public event EventHandler Navigated;

        //public event EventHandler Navigating;
    }
}