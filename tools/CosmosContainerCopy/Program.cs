using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using CosmosContainerCopy.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = new CommandLineBuilder(new RootCommand("Copy data.")
			{
				new CopyContainerCommand()
			})
			.UseDefaults()
			.UseHost(_ => Host.CreateDefaultBuilder(args), builder =>
			{
				builder.UseCommandHandler<CopyContainerCommand, CopyContainerCommand.Handler>();
				builder.ConfigureServices((context, services) =>
				{
					services.AddLogging();
				});
			}).Build();

return await builder.InvokeAsync(args);
