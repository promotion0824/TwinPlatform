dotnet watch --project .\test\ImageHub.Test\ImageHub.Test.csproj test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:Exclude=\"[*]ImageHub.Program,[*]ImageHub.Services.BlobService,[*]Microsoft.*,[*]System.*,[*]Willow.Infrastructure.*,[xunit*]*\" /p:CoverletOutput=.\lcov.info
