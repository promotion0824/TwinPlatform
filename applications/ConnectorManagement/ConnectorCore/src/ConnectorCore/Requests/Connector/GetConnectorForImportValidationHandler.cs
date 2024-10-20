namespace ConnectorCore.Requests.Connector;

using ConnectorCore.Dtos;
using ConnectorCore.Entities;
using ConnectorCore.Infrastructure.Exceptions;
using ConnectorCore.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

internal class GetConnectorForImportValidationHandler
{
    public static async Task<Results<Ok<ConnectorForImportValidationDto>, BadRequest<ProblemDetails>, NotFound>> HandleAsync([AsParameters] Guid siteId, [FromRoute] Guid connectorId, [FromServices] IConnectorsRepository connectorsRepository, [FromServices] ISchemaColumnsRepository schemaColumnsRepository, [FromServices] ITagsRepository tagsRepository, [FromServices] IPointsRepository pointsRepository, [FromServices] IDevicesRepository devicesRepository, [FromServices] IEquipmentsRepository equipmentsRepository)
    {
        var connectorData = await connectorsRepository.GetConnectorDataForValidation(connectorId, siteId);
        if (connectorData == null)
        {
            throw new NotFoundException();
        }

        var schemaColumnsBySchemaId = (await schemaColumnsRepository.GetBySchemas(connectorData.DeviceMetadataSchemaId,
                connectorData.PointMetadataSchemaId))
            .GroupBy(x => x.SchemaId)
            .ToDictionary(x => x.Key);

        var allTags = await tagsRepository.GetAllAsync();
        var allPointTypes = await pointsRepository.GetAllPointTypesAsync();
        var allExternalPointsForSiteExcludingConnector = await pointsRepository.GetAllExternalPointsForSiteExcludingConnectorAsync(connectorData.SiteId, connectorId);
        var entityIdByPointIdMapping = await pointsRepository.GetEntityIdByPointIdMappingAsync(connectorId);
        var allDeviceIds = await devicesRepository.GetAllIdsForConnectorIdAsync(connectorId);
        var allEquipmentIds = await equipmentsRepository.GetAllIdsForConnectorIdAsync(connectorId);
        var validationDto = new ConnectorForImportValidationDto
        {
            SiteId = connectorData.SiteId,
            DeviceSchemaColumns =
                schemaColumnsBySchemaId.TryGetValue(connectorData.DeviceMetadataSchemaId, out var devicesColumns)
                    ? devicesColumns.ToArray()
                    : new SchemaColumnEntity[0],
            PointSchemaColumns =
                schemaColumnsBySchemaId.TryGetValue(connectorData.PointMetadataSchemaId, out var pointsColumns)
                    ? pointsColumns.ToArray()
                    : new SchemaColumnEntity[0],
            AllTagNames = allTags.Select(x => x.Name).ToList(),
            AllPointTypes = allPointTypes,
            AllExternalPointsForSiteExcludingConnector = allExternalPointsForSiteExcludingConnector,
            EntityIdByPointId = entityIdByPointIdMapping,
            AllDeviceIds = allDeviceIds,
            AllEquipmentIds = allEquipmentIds,
        };

        return TypedResults.Ok(validationDto);
    }
}
