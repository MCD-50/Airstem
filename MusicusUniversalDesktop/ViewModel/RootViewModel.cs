using System;
using Windows.UI.ViewManagement;

namespace Musicus.ViewModel
{
    public class RootViewModel : Mvvm.Others.ViewModelBase
    {
        private bool _isEnable = false;
        private bool _isCollectionPage = false;
        private bool _isPlaying = default(bool);
        private bool _isPause = default(bool);
        private bool _isFirstRun;
        private int _forwardCount;
        public event EventHandler Select;

  
        public bool IsTabletMode { get; private set; }


    

        public void CheckTabletMode()
        {
            var mode =  UIViewSettings.GetForCurrentView();
            if (mode.UserInteractionMode == UserInteractionMode.Mouse)
                IsTabletMode = false;
            else IsTabletMode = true;
        }

        public bool IsFirstRun
        {
            get { return _isFirstRun; }
            set { Set(ref _isFirstRun, value); }
        }

        public bool IsEnable
        {
            get { return _isEnable; }
            set { Set(ref _isEnable, value); }
        }

        public bool IsCollectionPage
        {
            get { return _isCollectionPage; }
            set { Set(ref _isCollectionPage, value); }
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { Set(ref _isPlaying, value); }
        }

        public bool IsPause
        {
            get { return _isPause; }
            set { Set(ref _isPause, value); }
        }

        public void InvokeSelect()
        {
            Select?.Invoke(this, null);
        }

        public int ForwardCount
        {
            get { return _forwardCount; }
            set { Set(ref _forwardCount, value); }
        }

    }
}
