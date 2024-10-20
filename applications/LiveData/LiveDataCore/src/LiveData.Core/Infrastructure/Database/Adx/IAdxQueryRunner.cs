namespace Willow.LiveData.Core.Infrastructure.Database.Adx;

using System;
using System.Data;
using System.Threading.Tasks;

internal interface IAdxQueryRunner
{
    Task<IDataReader> QueryAsync(Guid? clientId, string kqlQuery);

    Task<IDataReader> ControlQueryAsync(Guid? clientId, string kqlQuery);
}
