#region

using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Musicus.Core.Utils.Interfaces;

#endregion

namespace Musicus.Core.WinRt
{
    public class PclDispatcherHelper : IDispatcherHelper
    {
        private readonly CoreDispatcher _dispatcher;

        public PclDispatcherHelper(CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async Task RunAsync(Action action)
        {
            await _dispatcher
                .RunAsync(CoreDispatcherPriority.Normal,
                    new DispatchedHandler(action));
        }
    }
}