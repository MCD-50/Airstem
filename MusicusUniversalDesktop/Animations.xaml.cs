
namespace Musicus
{
    public sealed partial class Animations
    {
        public Animations()
        {
            this.InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                startMobileAnimation();
            else startPcAnimation();         
        }

        private void startMobileAnimation()
        {
            Rect1StoryBoard.Begin();
            Rect2StoryBoard.Begin();
            Rect3StoryBoard.Begin();
            Rect4StoryBoard.Begin();
        }

        private void startPcAnimation()
        {
            Rect1StoryBoardPc.Begin();
            Rect2StoryBoardPc.Begin();
            Rect3StoryBoardPc.Begin();
            Rect4StoryBoardPc.Begin();
        }
    }
}
