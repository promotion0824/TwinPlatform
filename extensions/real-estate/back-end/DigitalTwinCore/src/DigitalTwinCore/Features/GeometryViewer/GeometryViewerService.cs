using DigitalTwinCore.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Infrastructure.Exceptions;

namespace DigitalTwinCore.Features.GeometryViewer
{
    public interface IGeometryViewerService
    {
        Task AddGeometryViewerModel(GeometryViewerModel request);
        Task RemoveGeometryViewerModel(string urn);
        Task<bool> ExistsGeometryViewerModel(string urn);

        Task<List<GeometryViewerModel>> GetGeometryViewerModelsByUrn(string urn);
        Task<List<GeometryViewerModel>> GetGeometryViewerModelsByTwinId(string twinId);
    }

    public class GeometryViewerService : IGeometryViewerService
    {
        private readonly DigitalTwinDbContext _dtDbContext;

        public GeometryViewerService(DigitalTwinDbContext dtDbContext)
        {
            _dtDbContext = dtDbContext;
        }

        public async Task AddGeometryViewerModel(GeometryViewerModel request) 
        {
            var models = await _dtDbContext.GeometryViewerModels.Where(x => x.Urn == request.Urn).ToListAsync();

            if (models.Any())
            {
                throw new PreconditionFailedException($"Model {request.Urn} already exists");
            }

            var model = GeometryViewerModel.MapTo(request);

            await _dtDbContext.GeometryViewerModels.AddAsync(model);

            await _dtDbContext.SaveChangesAsync();
        }

        public async Task RemoveGeometryViewerModel(string urn)
        {
            var models = await _dtDbContext.GeometryViewerModels.Include(x => x.References).Where(x => x.Urn == urn).ToListAsync();

            if (models.Any())
            {
                var references = models.SelectMany(x => x.References);
                if (references.Any())
                {
                    _dtDbContext.GeometryViewerReferences.RemoveRange(references);
                }

                _dtDbContext.GeometryViewerModels.RemoveRange(models);
                await _dtDbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsGeometryViewerModel(string urn)
        {
            return await _dtDbContext.GeometryViewerModels.AnyAsync(x => x.Urn == urn);
        }

        public async Task<List<GeometryViewerModel>> GetGeometryViewerModelsByUrn(string urn)
        {
            var models = await _dtDbContext.GeometryViewerModels.Include(x => x.References)
                .Where(x => x.Urn == urn)
                .ToListAsync();

            return GeometryViewerModel.MapFrom(models).ToList();
        }

        public async Task<List<GeometryViewerModel>> GetGeometryViewerModelsByTwinId(string twinId)
        {
            var models = await _dtDbContext.GeometryViewerModels.Include(x => x.References)
                .Where(x => x.TwinId == twinId)
                .ToListAsync();

            return GeometryViewerModel.MapFrom(models).ToList();
        }
    }
}
