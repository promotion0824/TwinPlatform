SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SetPointCommandStatus] (
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_SetPointCommandStatus] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT INTO [dbo].[SetPointCommandStatus] 
	([Id] ,[Name])
VALUES (0 , 'Created'),
	(1 , 'ActivationFailed'),
	(2 , 'Active'),
	(3 , 'ResetFailed'),
	(4 , 'Completed'),
	(5 , 'Deleted')
GO

CREATE TABLE [dbo].[SetPointCommand] (
	[Id] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[ConnectorId] [uniqueidentifier] NOT NULL,
	[EquipmentId] [uniqueidentifier] NOT NULL,
	[InsightId] [uniqueidentifier] NOT NULL,
	[PointId] [uniqueidentifier] NOT NULL,
	[SetPointId] [uniqueidentifier] NOT NULL,
	[OriginalValue] [decimal](18, 6) NOT NULL,
	[DesiredValue] [decimal](18, 6) NOT NULL,
	[DesiredDurationSeconds] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[LastUpdatedAt] [datetime2](7) NOT NULL,
	[ErrorDescription] [nvarchar](512) NULL,
 CONSTRAINT [PK_SetPointCommand] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SetPointCommand]  WITH CHECK ADD  CONSTRAINT [FK_SetPointCommand_Connector] FOREIGN KEY([ConnectorId])
REFERENCES [dbo].[Connector] ([Id])
GO

ALTER TABLE [dbo].[SetPointCommand] CHECK CONSTRAINT [FK_SetPointCommand_Connector]
GO

ALTER TABLE [dbo].[SetPointCommand]  WITH CHECK ADD  CONSTRAINT [FK_SetPointCommand_SetPointCommandStatus] FOREIGN KEY([Status])
REFERENCES [dbo].[SetPointCommandStatus] ([Id])
GO

ALTER TABLE [dbo].[SetPointCommand] CHECK CONSTRAINT [FK_SetPointCommand_SetPointCommandStatus]
GO
