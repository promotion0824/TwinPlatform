CREATE TYPE [dbo].[PointTableType] AS TABLE(
	[Id] [uniqueidentifier] NOT NULL,
	[EntityId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](64) NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[Unit] [nvarchar](64) NOT NULL,
	[Type] [int] NOT NULL,
	[ExternalPointId] [nvarchar](max) NOT NULL,
	[Category] [nvarchar](max) NULL,
	[Metadata] [nvarchar](max) NOT NULL,
	[IsDetected] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[LastUpdatedAt] [datetime2](7) NULL,
	[LastUpdatedBy] [uniqueidentifier] NULL,
	[DeviceId] [uniqueidentifier] NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO
