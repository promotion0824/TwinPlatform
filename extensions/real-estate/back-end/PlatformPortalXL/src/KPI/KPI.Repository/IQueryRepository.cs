using System.Collections.Generic;
using System.Threading.Tasks;

namespace Willow.KPI.Repository
{
    public interface IQueryRepository
    {
        Task<IEnumerable<IEnumerable<object>>> Query(string query, object[] parms = null);
    }
}
