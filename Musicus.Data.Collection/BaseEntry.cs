#region

using SQLite.Net.Attributes;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#endregion

namespace Musicus.Data.Collection
{
    public class BaseEntry : INotifyPropertyChanged
    {
        public BaseEntry()
        {
            // CreatedAt = DateTime.UtcNow;
        }

        //[Microsoft.WindowsAzure.MobileServices.CreatedAt]
        //public DateTime CreatedAt { get; set; }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}