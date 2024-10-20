namespace Connector.Nunit.Tests.Infrastructure
{
    using System.Linq;
    using Connector.Nunit.Tests.Infrastructure.Abstractions;
    using ConnectorCore.Database;
    using ConnectorCore.Infrastructure.HealthCheck;
    using ConnectorCore.Repositories;
    using Microsoft.Extensions.Logging;

    public class DatabaseUpdaterInitializer : DatabaseUpdater
    {
        private readonly ISchemasRepository schemasRepository;
        private readonly ISchemaColumnsRepository columnsRepository;
        private readonly IConnectorTypesRepository connectorTypesRepository;
        private readonly IConnectorsRepository connectorsRepository;
        private readonly IDevicesRepository devicesRepository;
        private readonly IEquipmentsRepository equipmentsRepository;
        private readonly IPointsRepository pointsRepository;
        private readonly ITagsRepository tagsRepository;
        private readonly ITagCategoriesRepository tagCategoriesRepository;
        private readonly ILogsRepository logsRepository;
        private readonly IGatewaysRepository gatewaysRepository;
        private readonly IDatabaseEraser databaseEraser;
        private readonly HealthCheckSql healthCheckSql;

        public DatabaseUpdaterInitializer(
            IDbConnectionStringProvider connectionStringProvider,
            ISchemasRepository schemasRepository,
            ISchemaColumnsRepository columnsRepository,
            IConnectorTypesRepository connectorTypesRepository,
            IConnectorsRepository connectorsRepository,
            IDevicesRepository devicesRepository,
            IEquipmentsRepository equipmentsRepository,
            IPointsRepository pointsRepository,
            ITagsRepository tagsRepository,
            ILogsRepository logsRepository,
            ITagCategoriesRepository tagCategoriesRepository,
            IGatewaysRepository gatewaysRepository,
            IDatabaseEraser databaseEraser,
            HealthCheckSql healthCheckSql)
            : base(connectionStringProvider, healthCheckSql)
        {
            this.schemasRepository = schemasRepository;
            this.columnsRepository = columnsRepository;
            this.connectorTypesRepository = connectorTypesRepository;
            this.connectorsRepository = connectorsRepository;
            this.devicesRepository = devicesRepository;
            this.equipmentsRepository = equipmentsRepository;
            this.pointsRepository = pointsRepository;
            this.tagsRepository = tagsRepository;
            this.tagCategoriesRepository = tagCategoriesRepository;
            this.logsRepository = logsRepository;
            this.gatewaysRepository = gatewaysRepository;
            this.databaseEraser = databaseEraser;
            this.healthCheckSql = healthCheckSql;
        }

        public override void DeployDatabaseChanges(ILoggerFactory loggerFactory, bool isDevEnvironment = true)
        {
            databaseEraser.EraseDb();

            base.DeployDatabaseChanges(loggerFactory, isDevEnvironment);

            foreach (var schemaEntity in TestData.SchemasTestData.Schemas)
            {
                schemasRepository.CreateAsync(schemaEntity).Wait();
            }

            foreach (var schemaColumnEntity in TestData.SchemaColumnsTestData.SchemaColumns)
            {
                columnsRepository.CreateAsync(schemaColumnEntity).Wait();
            }

            foreach (var connectorTypeEntity in TestData.ConnectorsTestData.Types)
            {
                connectorTypesRepository.CreateAsync(connectorTypeEntity).Wait();
            }

            foreach (var connectorEntity in TestData.ConnectorsTestData.Connectors)
            {
                connectorsRepository.CreateAsync(connectorEntity).Wait();
            }

            foreach (var deviceEntity in TestData.DevicesTestData.Devices)
            {
                devicesRepository.CreateAsync(deviceEntity).Wait();
            }

            foreach (var equipmentEntity in TestData.EquipmentsTestData.Equipments)
            {
                equipmentsRepository.CreateAsync(equipmentEntity).Wait();
            }

            foreach (var pointEntity in TestData.PointsTestData.Points)
            {
                pointsRepository.CreateAsync(pointEntity).Wait();
            }

            foreach (var tagEntity in TestData.TagsTestData.Tags)
            {
                tagsRepository.CreateAsync(tagEntity).Wait();
            }

            foreach (var gateway in TestData.GatewaysTestData.Gateways)
            {
                gatewaysRepository.CreateAsync(gateway).Wait();
            }

            pointsRepository.AddTagsToPointAsync(TestData.TagsTestData.PointToTagLinks).Wait();

            equipmentsRepository.AddTagsToEquipmentAsync(TestData.TagsTestData.EquipmentToTagLinks).Wait();
            equipmentsRepository.AddPointsToEquipmentAsync(TestData.EquipmentsTestData.EquipmentsToPoints).Wait();

            foreach (var logRecordEntity in TestData.LogsTestData.LogRecords)
            {
                logsRepository.CreateAsync(logRecordEntity).Wait();
            }

            foreach (var tagCategoryEntity in TestData.TagsTestData.TagCategories)
            {
                tagCategoriesRepository.CreateTagCategoryAsync(tagCategoryEntity).Wait();
            }

            foreach (var linksGroup in TestData.TagsTestData.TagCategoryLinks.GroupBy(l => l.CategoryId))
            {
                var tagIds = linksGroup.Select(l => l.TagId);
                tagCategoriesRepository.AddTagsToCategoryAsync(linksGroup.Key, tagIds).Wait();
            }
        }
    }
}
