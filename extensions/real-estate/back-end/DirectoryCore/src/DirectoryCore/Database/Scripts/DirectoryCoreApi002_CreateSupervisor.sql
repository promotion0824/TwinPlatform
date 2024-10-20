/****** Object:  Table [dbo].[Supervisors]    Script Date: 12/12112019 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Supervisors](
	[Id] [uniqueidentifier] NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[EmailConfirmationToken] [nvarchar](32) NOT NULL,
	[EmailConfirmationTokenExpiry] [datetime2](7) NOT NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[Mobile] [nvarchar](50) NOT NULL,
	[Status] [int] NOT NULL,
	[Auth0UserId] [nvarchar](50) NOT NULL,
	[AvatarUrl] [nvarchar](250) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[Initials] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_Supervisors] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO