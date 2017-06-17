using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Musicus.Extras
{
    public class ColorPicker
    {
        public List<ColorName> colorList;
        public ColorPicker()
        {
            colorList = new List<ColorName>()
            {
                new ColorName { Index = 1, Name = "Accent", ColorBrush = (Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush)},
                new ColorName { Index = 2, ColorBrush = new SolidColorBrush(Colors.Aqua)},
                new ColorName { Index = 3, ColorBrush = new SolidColorBrush(Colors.Aquamarine)},
                new ColorName { Index = 4, ColorBrush = new SolidColorBrush(Colors.BlueViolet)},
                new ColorName { Index = 5, ColorBrush = new SolidColorBrush(Colors.Brown)},
                new ColorName { Index = 6, ColorBrush = new SolidColorBrush(Colors.BurlyWood)},
                new ColorName { Index = 7, ColorBrush = new SolidColorBrush(Colors.CadetBlue)},
                new ColorName { Index = 8, ColorBrush = new SolidColorBrush(Colors.Chocolate)},
                new ColorName { Index = 9, ColorBrush = new SolidColorBrush(Colors.CornflowerBlue)},
                new ColorName { Index = 10, ColorBrush = new SolidColorBrush(Colors.Crimson)},
                new ColorName { Index = 11, ColorBrush = new SolidColorBrush(Colors.Cyan)},

                new ColorName { Index = 12, ColorBrush = new SolidColorBrush(Colors.DarkCyan)},
                new ColorName { Index = 13, ColorBrush = new SolidColorBrush(Colors.DarkRed)},
                new ColorName { Index = 14, ColorBrush = new SolidColorBrush(Colors.DarkSlateBlue)},
                new ColorName { Index = 15, ColorBrush = new SolidColorBrush(Colors.Green)},
                new ColorName { Index = 16, ColorBrush = new SolidColorBrush(Colors.IndianRed)},


                new ColorName { Index = 17, ColorBrush = new SolidColorBrush(Colors.LightCoral)},
                new ColorName { Index = 18, ColorBrush = new SolidColorBrush(Colors.LightSeaGreen)},
                new ColorName { Index = 19, ColorBrush = new SolidColorBrush(Colors.Lime)},
                new ColorName { Index = 20, ColorBrush = new SolidColorBrush(Colors.Maroon)},
                new ColorName { Index = 21, ColorBrush = new SolidColorBrush(Colors.MediumSpringGreen)},


                new ColorName { Index = 22, ColorBrush = new SolidColorBrush(Colors.Olive)},
                new ColorName { Index = 23, ColorBrush = new SolidColorBrush(Colors.OrangeRed)},
                new ColorName { Index = 24, ColorBrush = new SolidColorBrush(Colors.PaleVioletRed)},
                new ColorName { Index = 15, ColorBrush = new SolidColorBrush(Colors.SeaGreen)},
                new ColorName { Index = 26, ColorBrush = new SolidColorBrush(Colors.Teal)},
            };
        }

        public string CurrentColor { get; set; }

        private SolidColorBrush GetColorFromHexString(string hexValue)
        {
            //var a = Convert.ToByte(hexValue.Substring(0, 2), 16);
            var r = Convert.ToByte(hexValue.Substring(0, 2), 16);
            var g = Convert.ToByte(hexValue.Substring(2, 2), 16);
            var b = Convert.ToByte(hexValue.Substring(4, 2), 16);
            return new SolidColorBrush(Color.FromArgb(1, r, g, b));
        }


        public void ChangeColor(ColorName colorName = null, int index = 1)
        {
            if (colorName == null)
                colorName = colorList.FirstOrDefault(p => p.Index == index);
            (Application.Current.Resources["MusicusPivotItemSelectedTextColor"] as SolidColorBrush).Color = colorName.ColorBrush.Color;
            (Application.Current.Resources["MusicusPivotTitleTextColor"] as SolidColorBrush).Color = colorName.ColorBrush.Color;
            (Application.Current.Resources["MusicusPivotItemUnSelectedTextColor"] as SolidColorBrush).Color = colorName.ColorBrush.Color;
            (Application.Current.Resources["MusicusOtherColor"] as SolidColorBrush).Color = colorName.ColorBrush.Color;
            App.Locator.AppSettingsHelper.Write(Core.PlayerConstants.AppThemeIndex, colorName.Index);
            CurrentColor = colorName.Name;
        }

    }

    public class ColorName
    {
       public SolidColorBrush ColorBrush { get; set; }
       public string Name { get; set; }
       public int Index { get; set; } 
    }
}
