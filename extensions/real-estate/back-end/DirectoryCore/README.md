# Auto-formatting

This project uses [CSharpier](https://csharpier.com/) to automatically format code.

The CI build will fail if you have not formatted your code with CSharpier.

You should set up the pre-commit hook to automatically format your code on commit:

1. From the `extensions/real-estate/back-end/DirectoryCore` directory, run `dotnet tool restore`
2. From the root directory of TwinPlatform, run `dotnet husky install`

Optionally, you can install an editor plugin to format your code via a keyboard shortcut.
See https://csharpier.com/docs/Editors . Make sure not to have your editor reformat
files in other projects where this is not expected.

## dotnet husky errors
If Visual Studio displays the following error 

| Severity       | Code   | Description                            | Project       | File                                                                 | Line |
|----------------|--------|----------------------------------------|---------------|----------------------------------------------------------------------|------|
| Error (active) | MSB3073| The command "dotnet husky install" exited with code 1. | DirectoryCore | C:\TwinPlatform\extensions\real-estate\back-end\DirectoryCore\src\DirectoryCore\DirectoryCore.csproj | 57   |

Or if running `dotnet husky install` results in the message `Run "dotnet tool restore" to make the "husky" command available.`


Run the following commands from the `extensions/real-estate/back-end/DirectoryCore` directory:

`dotnet tool install husky`

`dotnet tool install csharpier`


Run the following commands from the root directory of TwinPlatform:


`dotnet tool install husky`

`dotnet husky install`

# Build Docker

Building docker image requires a AzureDevops pat token with permission to read artifact feeds

```powershell
docker build --build-arg FEED_ACCESSTOKEN=$env:FEED_ACCESSTOKEN -t directorycore:latest -f .\src\DirectoryCore\Dockerfile .
```
