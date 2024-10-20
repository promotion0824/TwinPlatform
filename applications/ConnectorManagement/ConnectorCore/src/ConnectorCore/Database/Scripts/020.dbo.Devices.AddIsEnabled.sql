alter table [Device] add [IsEnabled] bit;
GO
update [Device] set [IsEnabled] = 1;
GO
alter table [Device] alter column [IsEnabled] bit not null;
GO

alter table [Point] add [IsEnabled] bit;
GO
update [Point] set [IsEnabled] = 1;
GO
alter table [Point] alter column [IsEnabled] bit not null;
GO

alter table [Connector] drop column [IsActive];
GO

alter table [Connector] add [ErrorThreshold] int;
GO
update [Connector] set [ErrorThreshold] = 0;
GO
alter table [Connector] alter column [ErrorThreshold] int not null;
GO

alter table [Connector] add [IsEnabled] bit;
GO
update [Connector] set [IsEnabled] = 1;
GO
alter table [Connector] alter column [IsEnabled] bit not null;
GO

alter table [Connector] add [IsLoggingEnabled] bit;
GO
update [Connector] set [IsLoggingEnabled] = 1;
GO
alter table [Connector] alter column [IsLoggingEnabled] bit not null;
GO

DROP TYPE [dbo].[DeviceTableType];
GO

CREATE TYPE [dbo].[DeviceTableType] AS TABLE(
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](64) NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[ExternalDeviceId] [nvarchar](max) NOT NULL,
	[RegistrationId] [nvarchar](64) NULL,
	[RegistrationKey] [nvarchar](256) NULL,
	[Metadata] [nvarchar](max) NOT NULL,
	[IsDetected] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[LastUpdatedAt] [datetime2](7) NULL,
	[LastUpdatedBy] [uniqueidentifier] NULL,
	[ConnectorId] [uniqueidentifier] NOT NULL,
	[IsEnabled] [bit] NOT NULL
	PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO

DROP TYPE [dbo].[PointTableType];
GO

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
	[IsEnabled] [bit] NOT NULL
	PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO
