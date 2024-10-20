/****** Object:  Table [dbo].[Contractors]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Contractors](
	[Id] [uniqueidentifier] NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[EmailConfirmationToken] [nvarchar](32) NOT NULL,
	[EmailConfirmationTokenExpiry] [datetime2](7) NOT NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[Mobile] [nvarchar](50) NOT NULL,
	[Status] [int] NOT NULL,
	[Auth0UserId] [nvarchar](250) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[Initials] [nvarchar](20) NOT NULL,
	[AvatarId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Contractors] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[ContractorAssignments]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContractorAssignments](
	[ContractorId] [uniqueidentifier] NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_ContractorAssignments] PRIMARY KEY CLUSTERED 
(
	[ContractorId] ASC,
	[CustomerId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ContractorAssignments]  WITH CHECK ADD  CONSTRAINT [FK_ContractorAssignments_Customers_CustomerId] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ContractorAssignments] CHECK CONSTRAINT [FK_ContractorAssignments_Customers_CustomerId]
GO

ALTER TABLE [dbo].[ContractorAssignments]  WITH CHECK ADD  CONSTRAINT [FK_ContractorAssignments_Contractors_ContractorId] FOREIGN KEY([ContractorId])
REFERENCES [dbo].[Contractors] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ContractorAssignments] CHECK CONSTRAINT [FK_ContractorAssignments_Contractors_ContractorId]
GO