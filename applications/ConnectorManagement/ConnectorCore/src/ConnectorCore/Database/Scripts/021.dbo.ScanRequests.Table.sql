CREATE TABLE [dbo].[ScanRequests](
	[Id] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[ConnectorId] [uniqueidentifier] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[LastUpdatedAt] [datetime2](7) NULL,
	[LastUpdatedBy] [uniqueidentifier] NULL,
	[DeviceId] [uniqueidentifier] NULL,
	[ScanType] [nvarchar](128) NOT NULL,
	[Status] [nvarchar](128) NOT NULL
 CONSTRAINT [PK_ScanRequests] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ScanRequests]  WITH CHECK ADD  CONSTRAINT [FK_ScanRequests_Connector] FOREIGN KEY([ConnectorId])
REFERENCES [dbo].[Connector] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ScanRequests]  WITH CHECK ADD  CONSTRAINT [FK_ScanRequests_Device] FOREIGN KEY([DeviceId])
REFERENCES [dbo].[Device] ([Id])
ON DELETE CASCADE
GO
create index [Idx_ScanRequests_SiteId] on [ScanRequests]([SiteId]);
GO
create index [Idx_ScanRequests_ConnectorId] on [ScanRequests]([ConnectorId]);
GO
create index [Idx_ScanRequests_DeviceId] on [ScanRequests]([DeviceId]);
GO
create index [Idx_ScanRequests_Status] on [ScanRequests]([Status]);
GO