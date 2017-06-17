using System;

namespace Musicus.Data.Spotify
{
    public class StringAttribute : Attribute
    {
        public String Text {get;set;}
        public StringAttribute(String text)
        {
            this.Text = text;
        }
    }
}
