param ($Path, $BuildConfiguration, $OutputDir)

if($Path.EndsWith('*.csproj')) {
    Get-ChildItem -Path $Path -Recurse | ForEach-Object {
        $outputPath = "$OutputDir/$($_.BaseName)"
        New-Item -Path $outputPath -ItemType Directory -Force
        dotnet publish $_.FullName --no-build --no-restore --configuration $BuildConfiguration --output $outputPath
        $outputPaths += $outputPath
    }
} else {
    $outputPath = $OutputDir
    New-Item -Path $outputPath -ItemType Directory -Force
    dotnet publish $_.FullName --no-build --no-restore --configuration $BuildConfiguration --output $outputPath
    $outputPaths += $outputPath
}
