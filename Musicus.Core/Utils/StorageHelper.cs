﻿#region License

// Copyright (c) 2014 Harold Martinez-Molina <hanthonym@outlook.com>
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

#endregion

#region

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PCLStorage;

#endregion

namespace Musicus.Core.Utils
{
    // based on http://codepaste.net/gtu5mq
    public class StorageHelper
    {
        #region Private Methods

        private static IFolder GetFolderFromStrategy(StorageStrategy location)
        {
            switch (location)
            {
                case StorageStrategy.Roaming:
                    return FileSystem.Current.RoamingStorage;
                default:
                    return FileSystem.Current.LocalStorage;
            }
        }

        public static async Task<IFile> GetIfFileExistsAsync(string path, StorageStrategy strategy = StorageStrategy.Local)
        {
            return await GetIfFileExistsAsync(path, GetFolderFromStrategy(strategy)).ConfigureAwait(false);
        }
        public static async Task<IFile> GetIfFileExistsAsync(string path, IFolder folder)
        {
            var parts = path.Split('/');

            var fileName = parts.Last();

            if (parts.Length > 1)
            {
                folder =
                    await GetFolderAsync(path.Substring(0, path.Length - fileName.Length), folder).ConfigureAwait(false);
            }

            if (folder == null)
            {
                return null;
            }
            return await folder.TryGetFileAsync(fileName).ConfigureAwait(false);
        }

        private static async Task<IFolder> _EnsureFolderExistsAsync(string name, IFolder parent)
        {
            if (parent == null) throw new ArgumentNullException("parent");
            return
                await
                    parent.CreateFolderAsync(name, CreationCollisionOption.OpenIfExists).ConfigureAwait(false);
        }

        #endregion

        #region Public Methods

        public static async Task<bool> FileExistsAsync(string path, StorageStrategy location = StorageStrategy.Local)
        {
            return await FileExistsAsync(path, GetFolderFromStrategy(location)).ConfigureAwait(false);
        }

        public static async Task<bool> FileExistsAsync(string path, IFolder folder)
        {
            return (await GetIfFileExistsAsync(path, folder).ConfigureAwait(false)) != null;
        }

        public static async Task<IFolder> EnsureFolderExistsAsync(string path,
            StorageStrategy location = StorageStrategy.Local)
        {
            return await EnsureFolderExistsAsync(path, GetFolderFromStrategy(location)).ConfigureAwait(false);
        }

        public static async Task<IFolder> EnsureFolderExistsAsync(string path, IFolder parentFolder)
        {
            var parent = parentFolder;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var name in path.Trim('/').Split('/'))
            {
                parent = await _EnsureFolderExistsAsync(name, parent).ConfigureAwait(false);
            }

            return parent; // now points to innermost folder
        }

        public static async Task<bool> DeleteFileAsync(string path, StorageStrategy location = StorageStrategy.Local)
        {
            return await DeleteFileAsync(path, GetFolderFromStrategy(location));
        }

        public static async Task<bool> DeleteFileAsync(string path, IFolder folder)
        {
            var file = await GetIfFileExistsAsync(path, folder).ConfigureAwait(false);

            if (file != null)
                await file.DeleteAsync();

            return !(await FileExistsAsync(path, folder).ConfigureAwait(false));
        }

        public static async Task<IFolder> GetFolderAsync(string path,
            StorageStrategy location = StorageStrategy.Local)
        {
            return await GetFolderAsync(path, GetFolderFromStrategy(location)).ConfigureAwait(false);
        }

        public static async Task<IFolder> GetFolderAsync(string path, IFolder parentFolder)
        {
            var parent = parentFolder;

            foreach (var name in path.Trim('/').Split('/'))
            {
                parent = await _GetFolderAsync(name, parent).ConfigureAwait(false);

                if (parent == null) return null;
            }

            return parent; // now points to innermost folder
        }

        private static async Task<IFolder> _GetFolderAsync(string name, IFolder parent)
        {
            var item = await parent.TryGetFolderAsync(name).ConfigureAwait(false);
            return item;
        }

        public static async Task<BinaryReader> GetReaderForFileAsync(string path,
            StorageStrategy location = StorageStrategy.Local)
        {
            return await GetReaderForFileAsync(path, GetFolderFromStrategy(location)).ConfigureAwait(false);
        }

        public static async Task<BinaryReader> GetReaderForFileAsync(string path, IFolder folder)
        {
            var file = await CreateFileAsync(path, folder).ConfigureAwait(false);

            var stream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite).ConfigureAwait(false);

            return new BinaryReader(stream);
        }

        public static async Task<BinaryWriter> GetWriterForFileAsync(string path,
            StorageStrategy location = StorageStrategy.Local)
        {
            return await GetWriterForFileAsync(path, GetFolderFromStrategy(location)).ConfigureAwait(false);
        }

        public static async Task<BinaryWriter> GetWriterForFileAsync(string path, IFolder folder)
        {
            var file = await CreateFileAsync(path, folder).ConfigureAwait(false);

            var stream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite).ConfigureAwait(false);

            return new BinaryWriter(stream);
        }

        public static async Task<IFile> CreateFileAsync(string path,
            StorageStrategy location = StorageStrategy.Local,
            CreationCollisionOption option = CreationCollisionOption.OpenIfExists)
        {
            return await CreateFileAsync(path, GetFolderFromStrategy(location), option);
        }

        public static async Task<IFile> CreateFileAsync(string path, IFolder folder,
            CreationCollisionOption option = CreationCollisionOption.OpenIfExists)
        {
            if (path.StartsWith("/") || path.StartsWith("\\"))
                path = path.Substring(1);
            var parts = path.Split('/');

            var fileName = parts.Last();

            if (parts.Length > 1)
            {
                folder =
                    await
                        EnsureFolderExistsAsync(path.Substring(0, path.Length - fileName.Length), folder)
                            .ConfigureAwait(false);
            }

            return await folder.CreateFileAsync(fileName, option).ConfigureAwait(false);
        }

        public static async Task<IFile> GetFileAsync(string path,
            StorageStrategy location = StorageStrategy.Local)
        {
            return await CreateFileAsync(path, GetFolderFromStrategy(location));
        }

        public static async Task<IFile> GetFileAsync(string path,
            IFolder folder)
        {
            return await CreateFileAsync(path, folder);
        }

        #endregion

        #region Nested types

        public enum StorageStrategy
        {
            /// <summary>Local, isolated folder</summary>
            Local,

            /// <summary>Cloud, isolated folder. 100k cumulative limit.</summary>
            Roaming,
        }

        #endregion
    }
}