dotnet watch --project .\test\WorkflowCore.Test\WorkflowCore.Test.csproj ^
test --filter Category!=UseSqlServer ^
/p:CollectCoverage=true ^
/p:CoverletOutputFormat=lcov ^
/p:Exclude=\"[*]WorkflowCore.Program,[*]Microsoft.*,[*]System.*,[*]Willow.Infrastructure.*,[*]WorkflowCore.Database.*,[*]WorkflowCore.Services.NotificationService.*,[xunit*]*\" ^
/p:CoverletOutput=.\lcov.info