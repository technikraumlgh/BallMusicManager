# Ball Music Manager
Software zu erstellen und abspielen der LGH-Ball-Playlisten sowie der Info-Tablets.

## [Bedienung](docs/README.md)

## Developer Guide
### Build
```bash
cd '<path to project>'
dotnet publish -o '<output path>' -p:DebugType=None
```
### Update .NET
change `DotNetVersion` in [Directory.Build.props](Directory.Build.props) to the latest version of .NET (should be done every year)<br>
e.g. `<DotNetVersion>net10.0</DotNetVersion>`<br>
Update all used packages. Account for all breaking changes.  
Update SignalR in [display.html](BallMusic.Server/src/display.html)

### Optional Types
Types like `Option<T>`, `Result<T>` and `ErrorState` are optional types. They can be either a success or an error and have values stored in them respectively. This way errors can be propagated through the call chain without constant checks. Check out [Ametrin.Optional](https://github.com/BarionLP/Ametrin.Optional) for more details and examples.