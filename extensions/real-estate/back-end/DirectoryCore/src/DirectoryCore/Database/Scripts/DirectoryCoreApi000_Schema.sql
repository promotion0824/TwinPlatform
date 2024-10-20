/****** Object:  Table [dbo].[Customers]    Script Date: 11/20/2019 2:46:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customers](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Address1] [nvarchar](200) NOT NULL,
	[Address2] [nvarchar](200) NOT NULL,
	[Suburb] [nvarchar](50) NOT NULL,
	[Postcode] [nvarchar](20) NOT NULL,
	[Country] [nvarchar](50) NOT NULL,
	[Type] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[State] [nvarchar](50) NOT NULL,
	[LogoUrl] [nvarchar](250) NOT NULL,
 CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CustomerUsers]    Script Date: 11/20/2019 2:46:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerUsers](
	[UserId] [uniqueidentifier] NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_CustomerUsers] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Sites]    Script Date: 11/20/2019 2:46:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sites](
	[Id] [uniqueidentifier] NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Code] [nvarchar](100) NOT NULL,
	[Address] [nvarchar](250) NOT NULL,
	[State] [nvarchar](50) NOT NULL,
	[Postcode] [nvarchar](20) NOT NULL,
	[Country] [nvarchar](50) NOT NULL,
	[NumberOfFloors] [int] NOT NULL,
	[Contact] [nvarchar](100) NOT NULL,
	[ContactNumber] [nvarchar](100) NOT NULL,
	[ContactEmail] [nvarchar](100) NOT NULL,
	[ImageUrl] [nvarchar](250) NOT NULL,
	[Status] [int] NOT NULL,
	[HidAccessFlag] [bit] NOT NULL,
	[Introduction] [nvarchar](max) NOT NULL,
	[Latitude] [decimal](18, 0) NULL,
	[Longitude] [decimal](18, 0) NULL,
	[Timezone] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Sites] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SiteUsers]    Script Date: 11/20/2019 2:46:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SiteUsers](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[IsTenantAdmin] [bit] NOT NULL,
	[BuildingAccess] [bit] NOT NULL,
	[CarParkAccess] [bit] NOT NULL,
	[BadgeNumber] [nvarchar](50) NOT NULL,
	[LicensePlateNumber] [nvarchar](50) NOT NULL,
	[InvitationCode] [nvarchar](256) NOT NULL,
	[AccessIsolation] [bit] NOT NULL,
	[TenantRequest] [nvarchar](max) NOT NULL,
	[MeetingRoomBooking] [bit] NOT NULL,
	[HelpDeskAccess] [bit] NOT NULL,
	[AccessStatus] [int] NOT NULL,
	[LockerUserId] [nvarchar](128) NULL,
	[PhysicalAccessCardNumber] [bigint] NULL,
	[CardholderID] [bigint] NULL,
	[AcceptedTermsConditions] [bit] NULL,
	[AcceptedTermsConditionsDate] [datetime] NULL,
	[BookingAccess] [bit] NOT NULL,
 CONSTRAINT [PK_SiteUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 11/20/2019 2:46:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [uniqueidentifier] NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[Gender] [nvarchar](20) NOT NULL,
	[Email] [nvarchar](450) NOT NULL,
	[EmailConfirmationToken] [nvarchar](256) NOT NULL,
	[EmailConfirmationTokenExpiry] [datetime2](7) NOT NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[Mobile] [nvarchar](50) NOT NULL,
	[Status] [int] NOT NULL,
	[Auth0UserId] [nvarchar](50) NOT NULL,
	[AvatarUrl] [nvarchar](500) NOT NULL,
	[BuildingAddress] [nvarchar](250) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[Initials] [nvarchar](20) NOT NULL,
	[IsSuperUser] [bit] NOT NULL,
	[IsCustomerUser] [bit] NOT NULL,
	[IsSiteUser] [bit] NOT NULL,
	[DeviceId] [nvarchar](128) NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Zones]    Script Date: 11/20/2019 2:46:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Zones](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Type] [int] NOT NULL,
	[Description] [nvarchar](250) NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Zones] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[SiteUsers] ADD  DEFAULT ((0)) FOR [AcceptedTermsConditions]
GO
ALTER TABLE [dbo].[CustomerUsers]  WITH CHECK ADD  CONSTRAINT [FK_CustomerUsers_Customers_CustomerId] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CustomerUsers] CHECK CONSTRAINT [FK_CustomerUsers_Customers_CustomerId]
GO
ALTER TABLE [dbo].[CustomerUsers]  WITH CHECK ADD  CONSTRAINT [FK_CustomerUsers_Sites_SiteId] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([Id])
GO
ALTER TABLE [dbo].[CustomerUsers] CHECK CONSTRAINT [FK_CustomerUsers_Sites_SiteId]
GO
ALTER TABLE [dbo].[CustomerUsers]  WITH CHECK ADD  CONSTRAINT [FK_CustomerUsers_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CustomerUsers] CHECK CONSTRAINT [FK_CustomerUsers_Users_UserId]
GO
ALTER TABLE [dbo].[Sites]  WITH CHECK ADD  CONSTRAINT [FK_Sites_Customers_CustomerId] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Sites] CHECK CONSTRAINT [FK_Sites_Customers_CustomerId]
GO
ALTER TABLE [dbo].[SiteUsers]  WITH CHECK ADD  CONSTRAINT [FK_SiteUsers_Sites_SiteId] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SiteUsers] CHECK CONSTRAINT [FK_SiteUsers_Sites_SiteId]
GO
ALTER TABLE [dbo].[SiteUsers]  WITH CHECK ADD  CONSTRAINT [FK_SiteUsers_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SiteUsers] CHECK CONSTRAINT [FK_SiteUsers_Users_UserId]
GO
ALTER TABLE [dbo].[Zones]  WITH CHECK ADD  CONSTRAINT [FK_Zones_Sites_SiteId] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([Id])
GO
ALTER TABLE [dbo].[Zones] CHECK CONSTRAINT [FK_Zones_Sites_SiteId]
GO
