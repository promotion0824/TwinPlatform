/****** Object:  Table [dbo].[Portfolios]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Portfolios](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Portfolios] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Add PortfolioId to Sites ******/
ALTER TABLE dbo.Sites
ADD PortfolioId UNIQUEIDENTIFIER NULL;
GO

/****** Object:  Table [dbo].[Roles]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Roles](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Permissions]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Permissions](
	[Id] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](200) NOT NULL,
 CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[RolePermission]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RolePermission](
	[RoleId] [uniqueidentifier] NOT NULL,
	[PermissionId] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_RolePermission] PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC,
	[PermissionId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Assignments]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Assignments](
	[PrincipalId] [uniqueidentifier] NOT NULL,
	[PrincipalType] [int] NOT NULL,
	[RoleId] [uniqueidentifier] NOT NULL,
	[ResourceId] [uniqueidentifier] NOT NULL,
	[ResourceType] [int] NOT NULL,
 CONSTRAINT [PK_Assignments] PRIMARY KEY CLUSTERED 
(
	[PrincipalId] ASC,
	[RoleId] ASC,
	[ResourceId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Portfolios]  WITH CHECK ADD  CONSTRAINT [FK_Portfolios_Customers_CustomerId] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Portfolios] CHECK CONSTRAINT [FK_Portfolios_Customers_CustomerId]
GO

ALTER TABLE [dbo].[RolePermission]  WITH CHECK ADD  CONSTRAINT [FK_RolePermission_Roles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RolePermission] CHECK CONSTRAINT [FK_RolePermission_Roles_RoleId]
GO

ALTER TABLE [dbo].[RolePermission]  WITH CHECK ADD  CONSTRAINT [FK_RolePermission_Permissions_PermissionId] FOREIGN KEY([PermissionId])
REFERENCES [dbo].[Permissions] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RolePermission] CHECK CONSTRAINT [FK_RolePermission_Permissions_PermissionId]
GO

ALTER TABLE [dbo].[Assignments]  WITH CHECK ADD  CONSTRAINT [FK_Assignments_Roles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Assignments] CHECK CONSTRAINT [FK_Assignments_Roles_RoleId]
GO
