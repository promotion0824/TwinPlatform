IF NOT EXISTS (SELECT  object_id FROM sys.tables WHERE name = 'DataQualityData') 
BEGIN
CREATE TABLE [dbo].[DataQualityData](
	[Id] [uniqueidentifier] NOT NULL,
	[TwinId] [nvarchar](250) NOT NULL Index IX_DataQualityData_TwinID NONCLUSTERED ,
	[ResultSource] [nvarchar](50) NULL,
	[ResultType] [nvarchar](50) NULL,
	[ModelId] [nvarchar](256) NULL,
	[TwinInfo] [nvarchar](max) NULL,
	[ResultInfo] [nvarchar](max) NULL,
	[Location] [nvarchar](max) NULL,
	[Score] [SMALLINT] NULL,
	[CreatedDate] [datetime2] NOT Null,
	 CONSTRAINT [PK_DataQualityData] PRIMARY KEY CLUSTERED 
(
	Id
)
) ON [PRIMARY]
END



IF NOT EXISTS (SELECT  object_id FROM sys.tables WHERE name = 'FloorDataQuality') 
BEGIN
CREATE TABLE [dbo].[FloorDataQuality](
	[FloorId] [uniqueidentifier] NOT NULL,
	[System] [nvarchar](250) NOT NULL,
	[StaticScore] [SMALLINT] NULL,
	[ConnectivityScore] [SMALLINT] NULL,
	[OverallHealthScore] [SMALLINT] NULL,
 CONSTRAINT [PK_FloorDataQuality] PRIMARY KEY CLUSTERED 
(
	FloorId,System 
)
) ON [PRIMARY]
END



IF NOT EXISTS (SELECT  object_id FROM sys.tables WHERE name = 'SiteDataQuality') 
BEGIN

CREATE TABLE [dbo].[SiteDataQuality](
	[SiteId] [uniqueidentifier] NOT NULL,
	[StaticScore] [SMALLINT] NULL,
	[ConnectivityScore] [SMALLINT] NULL,
	[OverallHealthScore] [SMALLINT] NULL,
 CONSTRAINT [PK_SiteDataQuality] PRIMARY KEY CLUSTERED 
(
	[SiteId] ASC
)
) ON [PRIMARY]

END



IF NOT EXISTS (SELECT  object_id FROM sys.tables WHERE name = 'TwinDataQuality') 
BEGIN

CREATE TABLE [dbo].[TwinDataQuality](
	[TwinId] [nvarchar](250) NOT NULL,
	[FloorId] [uniqueidentifier] NULL,
	[SiteId] [uniqueidentifier] NULL,
	[System] [nvarchar](250) NULL,
	[AttributeScore] [SMALLINT] NULL,
	[SensorDefinedScore] [SMALLINT] NULL,
	[SensorsReadingDataScore] [SMALLINT] NULL,
	[StaticScore] [SMALLINT] NULL,
	[ConnectivityScore] [SMALLINT] NULL,
	[OverallHealthScore] [SMALLINT] NULL,
	[MissingProperties] [nvarchar](max) NULL,
	[MissingSensors] [nvarchar](max) NULL,
 CONSTRAINT [PK_TwinDataQuality] PRIMARY KEY CLUSTERED 
(
	[TwinId] ASC
)
) ON [PRIMARY]

END