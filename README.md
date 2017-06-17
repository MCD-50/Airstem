Airstem
=========

The All-in-one, powerful music app, powered by Last.FM and Deezer.

## Requirements

Most dependencies are obtained from Nuget and automatically restored on build.  The only external dependency you'll have install is the [Windows Advertising SDK](https://visualstudiogallery.msdn.microsoft.com/401703a0-263e-4949-8f0f-738305d6ef4b) SDK and [SQLite for Universal App Platform](https://visualstudiogallery.msdn.microsoft.com/4913e7d5-96c9-4dde-a1a1-69820d615936) SDK.

## Building

Make sure you have the necessary tools for building [Windows Universal Apps]
(https://dev.windows.com/en-us/develop/building-universal-Windows-apps).

Simply clone the repo

    git clone https://github.com/mcd-50/airstem

Open the solution file `MusicusUniversalDesktop.sln` in Visual Studio. Then right-click and click Build on the solution. Nuget should download all missing packages, if not open the package manager and click `Restore Missing Packages`.
