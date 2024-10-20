SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Scan](
	[Id] [uniqueidentifier] NOT NULL,
	[ConnectorId] [uniqueidentifier] NOT NULL,
    [Status] [nvarchar](32) NULL,
    [Message] [nvarchar](1024) NULL,
    [CreatedBy] [nvarchar](64) NOT NULL,
    [CreatedAt] [datetime2] NOT NULL,
    [StartTime] [datetime2]  NULL,
    [EndTime] [datetime2]  NULL,
    [DevicesToScan] [nvarchar](max)  NULL,
    [ErrorCount] [int] NULL,
    [ErrorMessage] [varchar](max) NULL,
    CONSTRAINT [PK_Scans] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Scan]  WITH CHECK ADD  CONSTRAINT [FK_Scan_Connector] FOREIGN KEY([ConnectorId])
    REFERENCES [dbo].[Connector] ([Id])
    GO
ALTER TABLE [dbo].[Scan] CHECK CONSTRAINT [FK_Scan_Connector]
    GO
