dotnet watch --project .\test\WorkflowCore.Test\WorkflowCore.Test.csproj test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:Exclude=\"[*]WorkflowCore.Program,[*]Microsoft.*,[*]System.*,[*]Willow.Infrastructure.*,[*]WorkflowCore.Database.*,[xunit*]*\" /p:CoverletOutput=.\lcov.info
