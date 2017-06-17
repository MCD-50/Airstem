#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#endregion

namespace Musicus.Core.Utils
{
    public static class CollectionExtensions
    {
        private static readonly Random Random = new Random();

        public static void Sort<T>(this ObservableCollection<T> observable, Comparison<T> comparison)
        {
            var sorted = observable.ToList();
            sorted.Sort(comparison);

            var ptr = 0;
            while (ptr < sorted.Count)
            {
                if (!observable[ptr].Equals(sorted[ptr]))
                {
                    var t = observable[ptr];
                    observable.RemoveAt(ptr);
                    observable.Insert(sorted.IndexOf(t), t);
                }
                else
                {
                    ptr++;
                }
            }
        }

        public static void AddRange<T>(this IList<T> collection, IEnumerable<T> items)
        {
            try
            {
                foreach (var item in items)
                    collection.Add(item);
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// Shuffles the specified list using an implementation of the Fisher-Yates shuffle.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            if (list.Count < 2) return list;

            var arr = list.ToArray();
            Shuffle(arr);
            return arr;
        }

        private static void Shuffle<T>(T[] array)
        {
            var n = array.Length;
            for (var i = 0; i < n; i++)
            {
                var r = i + (int) (Random.NextDouble()*(n - i));
                var t = array[r];
                array[r] = array[i];
                array[i] = t;
            }
        }


        //private static double val = 0.0;
        //public static double CollectionGridSize(this double obj)
        //{

        //    if (val < 1)
        //    {
        //        val = (Windows.UI.Xaml.Window.Current.Bounds.Width - (PlayerConstants.OptimizedWidth + PlayerConstants.FiveItems)) / 5;
        //        return val;
        //    }
        //    else return val;
        //}


        //private static double val1 = 0.0;
        //public static double SpotifyGridWidth(this double obj)
        //{
        //    if (val1 < 1)
        //    {
        //        val1 = (Windows.UI.Xaml.Window.Current.Bounds.Width - (PlayerConstants.OptimizedWidth + PlayerConstants.ThreeItems)) / 3;
        //        return val1;
        //    }
        //    else return val1;
        //}


        //private static double val2 = 0.0;
        //public static double SpotifyGridHeight(this double obj)
        //{
        //    if (val2 < 1)
        //    {
        //        val2 = (Windows.UI.Xaml.Window.Current.Bounds.Width - (PlayerConstants.OptimizedWidth + PlayerConstants.ThreeItems)) / 4;
        //        return val2;
        //    }
        //    else return val2;
        //}
    }
}