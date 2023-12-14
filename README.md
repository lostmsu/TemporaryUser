[![NuGet](https://img.shields.io/nuget/v/TemporaryUser.svg)](https://www.nuget.org/packages/TemporaryUser/)

## Usage

```csharp
using var user = new TemporaryUser(name: "TempUser", password: "passw");
File.WriteAllText(path: Path.Join(user.ProfilePath.FullName, "readme.txt"),
                  contents: "Hello, world!");
```