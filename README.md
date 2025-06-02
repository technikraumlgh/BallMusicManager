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