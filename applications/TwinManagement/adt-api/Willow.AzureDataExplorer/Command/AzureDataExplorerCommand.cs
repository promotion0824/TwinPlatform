using Kusto.Data.Common;
using Polly;
using System.Data;
using Willow.AzureDataExplorer.Builders;

namespace Willow.AzureDataExplorer.Command;

public interface IAzureDataExplorerCommand
{
    Task<IDataReader> ExecuteControlCommandAsync(string database, string command);
    Task<IDataReader> ExecuteQueryAsync(string database, string query);
}

public class AzureDataExplorerCommand : IAzureDataExplorerCommand
{
    private readonly IClientBuilder _clientBuilder;

    public AzureDataExplorerCommand(IClientBuilder clientBuilder)
    {
        _clientBuilder = clientBuilder;
    }

    public async Task<IDataReader> ExecuteControlCommandAsync(string database, string command)
    {
        var retryPolicy = Policy.Handle<Kusto.Data.Exceptions.KustoRequestThrottledException>(x => !x.IsPermanent)
            .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(10));

        IDataReader? reader = null;
        await retryPolicy.ExecuteAsync(async () =>
        {
            var cslAdminProvider = await _clientBuilder.GetCslAdminProvider;
            reader = await cslAdminProvider.ExecuteControlCommandAsync(database, command);
        });

        return reader;
    }

    public async Task<IDataReader> ExecuteQueryAsync(string database, string query)
    {
        var retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(1));
        IDataReader? reader = null;
        var clientOptions = new Dictionary<string, object>
            {
                { ClientRequestProperties.OptionNoTruncation, true }
            };

        await retryPolicy.ExecuteAsync(async () =>
        {
            var cslQueryProvider = await _clientBuilder.GetCslQueryProvider;
            reader = await cslQueryProvider.ExecuteQueryAsync(database, query, new ClientRequestProperties(clientOptions, null));
        });

        return reader;
    }
}
