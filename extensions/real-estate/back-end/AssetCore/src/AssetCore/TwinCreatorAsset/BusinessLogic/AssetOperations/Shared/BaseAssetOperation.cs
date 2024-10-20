using AutoMapper;
using AssetCoreTwinCreator.Domain;
using Microsoft.Extensions.Logging;

namespace AssetCoreTwinCreator.BusinessLogic.AssetOperations.Shared
{
    public class BaseAssetOperation
    {
        protected readonly AssetDbContext _dbContext;
        protected readonly ILogger _logger;
        protected readonly IMapper _mapper;

        public BaseAssetOperation(AssetDbContext dbContext, ILogger logger, IMapper mapper)
        {
            _dbContext = dbContext;
            _logger = logger;
            _mapper = mapper;
        }

    }
}
