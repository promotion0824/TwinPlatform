using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Willow.Common;

namespace Willow.Data
{
   public interface IReadRepository<TID, TVALUE>
    {
        Task<TVALUE>             Get(TID id);
        IAsyncEnumerable<TVALUE> Get(IEnumerable<TID> ids);
    }
}
