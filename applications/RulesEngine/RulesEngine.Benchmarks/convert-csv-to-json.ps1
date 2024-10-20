#### To add new tests, add the csv report output name here, and replace either the micro (μs) or nano (ns) second symbol with blank. ######
 
[io.file]::readalltext(“BenchmarkDotNet.Artifacts\results\RulesEngine.Benchmarks.TemplateBenchmarks-report.csv”).replace(“ μs”,””) | Out-File BenchmarkDotNet.Artifacts\results\RulesEngine.Benchmarks.TemplateBenchmarks-report.csv;
[io.file]::readalltext(“BenchmarkDotNet.Artifacts\results\RulesEngine.Benchmarks.ParserBenchmarks-report.csv”).replace(“ ns”,””) | Out-File BenchmarkDotNet.Artifacts\results\RulesEngine.Benchmarks.ParserBenchmarks-report.csv;


Import-Csv "BenchmarkDotNet.Artifacts\results\RulesEngine.Benchmarks.TemplateBenchmarks-report.csv" |
    ConvertTo-Json |
    Add-Content -Path "BenchmarkDotNet.Artifacts\results\RulesEngine.Benchmarks.TemplateBenchmarks-report.json";


Import-Csv "BenchmarkDotNet.Artifacts\results\RulesEngine.Benchmarks.ParserBenchmarks-report.csv" |
    ConvertTo-Json |
    Add-Content -Path "BenchmarkDotNet.Artifacts\results\RulesEngine.Benchmarks.ParserBenchmarks-report.json"