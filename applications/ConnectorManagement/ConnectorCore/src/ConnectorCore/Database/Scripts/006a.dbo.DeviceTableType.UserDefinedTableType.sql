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
	PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO
