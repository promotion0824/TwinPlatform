### This script uses git to download a benchmarkdotnet results comparer application, which is then built and used to compare 2 sets of benchmarkdotnet results.
### This script parses the results of the comparison to search for text indicating better or same performance, and produces a fail for worse performance.

####################################################
#### Clone results comparer and build solution #####
####################################################

Write-Host "Cloning Repo from https://github.com/dotnet/performance -> ./dotnet_perf"
git clone https://github.com/dotnet/performance dotnet_perf --quiet

# Write-Host "Goto folder to build ResultsComparer"
pushd dotnet_perf\src\tools\ResultsComparer

Write-Host "Building benchmarkdotnet results comparer app..."
dotnet build -c release

# Write-Host "back to root"
popd
# View CLI options
# dotnet ./dotnet_perf/artifacts/bin/ResultsComparer/Release/net6.0/ResultsComparer.dll -h


###########################################
#### ParserBenchmarks Comparison Here #####
###########################################

Write-Host "
Comparing Parser Benchmarks with previous results..."
$results = dotnet ./dotnet_perf/artifacts/bin/ResultsComparer/Release/net6.0/ResultsComparer.dll --base $env:Agent_BuildDirectory\ParserBenchmarks-report-full.json --diff D:\a\1\s\BenchmarkDotNet.Artifacts\results\RulesEngine.Benchmarks.ParserBenchmarks-report-full.json --threshold 10%

# Convert results to a string for comparisons - results could be a string or an object, but below line works irrespective
$results = Out-String -InputObject $results

Write-Host "Comparison results: $($results)"

# Trim down the results for the comparison
$results = $results.SubString(0,16)
Write-Host "Test results: $($results)"

# Set the text we need to appear for a pass
$tests_passed = "summary:
better"
Write-Host "For a pass, results should be: $($tests_passed)"

# Comparison
if ($tests_passed -eq $results)
{
Write-Host "Expressions Parser Benchmark.net load tests -> Passed"
}
Else
{
Write-Host "The Benchmark.net load tests have detected a slowdown in the Expressions Parser greater than 10%."
Write-Host "Benchmark.net load tests -> Failed"
[Environment]::Exit(1) #to exit with error code 1 and stop script from progressing
}


################################################
#### TeamplateBenchmarks Comparison Here ######
################################################

Write-Host "
Now comparing Rule Template Benchmarks with previous results..."
$results = dotnet ./dotnet_perf/artifacts/bin/ResultsComparer/Release/net6.0/ResultsComparer.dll --base $env:Agent_BuildDirectory\TemplateBenchmarks-report-full.json --diff D:\a\1\s\BenchmarkDotNet.Artifacts\results\RulesEngine.Benchmarks.TemplateBenchmarks-report-full.json --threshold 10%

# Convert results to a string for comparisons - results could be a string or an object, but below line works irrespective
$results = Out-String -InputObject $results

Write-Host "Comparison results: $($results)"

# Trim down the results for the comparison
$results = $results.SubString(0,16)
Write-Host "Test results: $($results)"

# Set the text we need to appear for a pass
$tests_passed = "summary:
better"
Write-Host "For a pass, results should be: $($tests_passed)"

# Comparison
if ($tests_passed -eq $results)
{
Write-Host "Rule Template Benchmark.net load tests -> Passed"
}
Else
{
Write-Host "The Benchmark.net load tests have detected a slowdown in Rule Template execution greater than 10%."
Write-Host "Benchmark.net load tests -> Failed"
[Environment]::Exit(1) #to exit with error code 1 and stop script from progressing
}