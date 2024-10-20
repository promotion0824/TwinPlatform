namespace ConnectorCore.Database
{
    using System.Data;
    using System.Threading.Tasks;

    internal interface IDbConnectionProvider
    {
        Task<IDbConnection> GetConnection();
    }
}
