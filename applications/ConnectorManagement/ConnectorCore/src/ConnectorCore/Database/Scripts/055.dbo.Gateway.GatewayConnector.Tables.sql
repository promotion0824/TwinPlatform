/****** Object:  Table [dbo].[Gateway]    Script Date: 29/10/2020 08:58:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Gateway](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[Host] [nvarchar](128) NOT NULL,
	[IsOnline] [bit] NULL,
	[IsEnabled] [bit] NOT NULL,
	[LastUpdatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Gateway] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


CREATE TABLE [dbo].[GatewayConnector](
	[GatewayId] [uniqueidentifier] NOT NULL,
	[ConnectorId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_GatewayConnector] PRIMARY KEY CLUSTERED 
(
	[GatewayId] ASC,
	[ConnectorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[GatewayConnector]  WITH CHECK ADD  CONSTRAINT [FK_GatewayConnector_Connector] FOREIGN KEY([ConnectorId])
REFERENCES [dbo].[Connector] ([Id])
GO

ALTER TABLE [dbo].[GatewayConnector] CHECK CONSTRAINT [FK_GatewayConnector_Connector]
GO

ALTER TABLE [dbo].[GatewayConnector]  WITH CHECK ADD  CONSTRAINT [FK_GatewayConnector_Gateway] FOREIGN KEY([GatewayId])
REFERENCES [dbo].[Gateway] ([Id])
GO

ALTER TABLE [dbo].[GatewayConnector] CHECK CONSTRAINT [FK_GatewayConnector_Gateway]
GO

