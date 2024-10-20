using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Willow.Api.Client;
using Willow.Common;
using Willow.Data;

namespace Willow.Data.Rest
{
    public class RestRepositoryReader<TID, TVALUE> : IReadRepository<TID, TVALUE> 
    {
        private readonly IRestApi            _restApi;
        private readonly Func<TID, string>   _getEndPoint;
        protected readonly Func<TID, string> _getListEndpoint;

        public RestRepositoryReader(IRestApi directoryApi, Func<TID, string> getEndPoint, Func<TID, string> getListEndpoint)
        {
            _restApi         = directoryApi;
            _getEndPoint     = getEndPoint;
            _getListEndpoint = getListEndpoint;
        }

        public async Task<TVALUE> Get(TID id)
        {
            var endPoint = _getEndPoint(id);
            var result = await _restApi.Get<TVALUE>(endPoint);

            if(result == null)
                throw new Exception();

            return result;
        }

        public async virtual IAsyncEnumerable<TVALUE> Get(IEnumerable<TID> ids)
        {
            var result = await GetAll(ids);

            foreach(var item in result)
            { 
                yield return item;
            }
        }

        private async Task<IEnumerable<TVALUE>> GetAll(IEnumerable<TID> ids)
        {
            var query = ids.AsParallel().WithDegreeOfParallelism(8).Select( async id=> 
            {
                try
                { 
                    return await Get(id);
                }
                catch
                {
                    return default(TVALUE);
                }

            });

            var result = await Task.WhenAll(query);

            return result.Where( i=> i != null );
        }
    }
}
