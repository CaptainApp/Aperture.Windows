# Aperture.Windows
![version: 0.1](https://img.shields.io/badge/version-0.1-blue.svg)
![license: BSD 2-Clause](https://img.shields.io/badge/license-BSD_2--Clause-brightgreen.svg)
> Windows screen capturing and recording library

## What's this?
Aperture is a .NET library for capturing the screen and recording video on Windows platforms using diverse technologies.

It tries to be as performant as possible and to use hardware-accelerated capture and encoding whenever feasible.

## Compatibility
The library is designed to work with Windows Vista SP2 onwards. Windows XP support is not present nor planned, as
we depend upon the .NET Framework 4.5, which is not compatible with this platform.

Take into account that performance could be negatively affected in _older_ Windows versions (Windows < 8) for the
lack of [Desktop Duplication API](https://msdn.microsoft.com/en-us/library/windows/desktop/hh404487(v=vs.85).aspx).

## Building
```
$ git clone https://github.com/CaptainApp/Aperture.Windows
$ cd Aperture.Windows
$ nuget restore
$ devenv Aperture.Windows.csproj /Build
```

## Open-source code
This software depends upon awesome open-source software without which it could not be possible.

### Third-party
These open-source projects are also being used (albeit with no actual source code of these being included):
- **Multiple [SharpDX](http://sharpdx.org/) libraries**, as NuGet package dependencies.  
  `Copyright (c) 2010-2015 SharpDX - Alexandre Mutel`