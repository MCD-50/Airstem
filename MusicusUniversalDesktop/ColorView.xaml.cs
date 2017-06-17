


using Musicus.Extras;
using Musicus.Helpers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Musicus
{
    public sealed partial class ColorView : IModalSheetPage
    {
        ColorPicker _colorPicker;
        public ColorView()
        {
            this.InitializeComponent();
            _colorPicker = new ColorPicker();
        }

        private void ColorListViewClicked(object sender, ItemClickEventArgs e)
        {
            _colorPicker.ChangeColor(e.ClickedItem as ColorName);
            SheetUtility.CloseColorViewPage();
        }

        public Popup Popup { get; private set; }
        public void OnOpened(Popup popup)
        {
            Popup = popup;
            ColorListView.ItemsSource = _colorPicker.colorList;
            App.Navigator.ShowBack();
        }

        public void OnClosed()
        {
            Popup = null;
            if (App.Navigator.StackCount > 0) return;
            App.Navigator.HideBack();
        }
    }
}
