/****** Object:  Table [dbo].[WF_Checks]    Script Date: 9/11/2020 1:17:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WF_Checks](
	[Id] [uniqueidentifier] NOT NULL,
	[InspectionId] [uniqueidentifier] NOT NULL,
	[SortOrder] [int] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Type] [int] NOT NULL,
	[TypeValue] [nvarchar](512) NOT NULL,
	[DecimalPlaces] [int] NOT NULL,
	[MinValue] [float] NULL,
	[MaxValue] [float] NULL,
	[DependencyId] [uniqueidentifier] NULL,
	[DependencyValue] [nvarchar](50) NULL,
	[PauseStartDate] [datetime] NULL,
	[PauseEndDate] [datetime] NULL,
	[LastRecordId] [uniqueidentifier] NULL,
	[LastSubmittedRecordId] [uniqueidentifier] NULL,
	[isArchived] [bit] NOT NULL,
 CONSTRAINT [PK_WF_Checks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WF_CheckRecords]    Script Date: 9/11/2020 1:17:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WF_CheckRecords](
	[Id] [uniqueidentifier] NOT NULL,
	[InspectionId] [uniqueidentifier] NOT NULL,
	[CheckId] [uniqueidentifier] NOT NULL,
	[InspectionRecordId] [uniqueidentifier] NOT NULL,
	[Status] [int] NOT NULL,
	[SubmittedUserId] [uniqueidentifier] NULL,
	[SubmittedDate] [datetime] NULL,
	[SubmittedSiteLocalDate] [datetime] NULL,
	[NumberValue] [float] NULL,
	[StringValue] [nvarchar](256) NULL,
	[Notes] [nvarchar](1024) NULL,
	[InsightId] [uniqueidentifier] NULL,
	[EffectiveDate] [datetime] NOT NULL,
 CONSTRAINT [PK_WF_CheckRecords] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WF_Inspections]    Script Date: 9/11/2020 1:17:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WF_Inspections](
	[Id] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[FloorCode] [nvarchar](10) NOT NULL,
	[ZoneId] [uniqueidentifier] NOT NULL,
	[AssetId] [uniqueidentifier] NOT NULL,
	[AssignedWorkgroupId] [uniqueidentifier] NOT NULL,
	[FrequencyInHours] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NULL,
	[LastRecordId] [uniqueidentifier] NULL,
	[NextEffectiveDate] [datetime] NOT NULL,
	[isArchived] [bit] NOT NULL,
 CONSTRAINT [PK_WF_Inspections] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WF_InspectionRecords]    Script Date: 9/11/2020 1:17:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WF_InspectionRecords](
	[Id] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[InspectionId] [uniqueidentifier] NOT NULL,
	[EffectiveDate] [datetime] NOT NULL,
 CONSTRAINT [PK_WF_InspectionRecords] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WF_Zones]    Script Date: 9/11/2020 1:17:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WF_Zones](
	[Id] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
 CONSTRAINT [PK_WF_Zones] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[WF_Checks]  WITH CHECK ADD  CONSTRAINT [FK_WF_Checks_WF_Inspections] FOREIGN KEY([InspectionId])
REFERENCES [dbo].[WF_Inspections] ([Id])
GO
ALTER TABLE [dbo].[WF_Checks] CHECK CONSTRAINT [FK_WF_Checks_WF_Inspections]
GO
ALTER TABLE [dbo].[WF_CheckRecords]  WITH CHECK ADD  CONSTRAINT [FK_WF_CheckRecords_WF_Checks] FOREIGN KEY([CheckId])
REFERENCES [dbo].[WF_Checks] ([Id])
GO
ALTER TABLE [dbo].[WF_CheckRecords] CHECK CONSTRAINT [FK_WF_CheckRecords_WF_Checks]
GO
ALTER TABLE [dbo].[WF_CheckRecords]  WITH CHECK ADD  CONSTRAINT [FK_WF_CheckRecords_WF_Inspections] FOREIGN KEY([InspectionId])
REFERENCES [dbo].[WF_Inspections] ([Id])
GO
ALTER TABLE [dbo].[WF_CheckRecords] CHECK CONSTRAINT [FK_WF_CheckRecords_WF_Inspections]
GO
ALTER TABLE [dbo].[WF_CheckRecords]  WITH CHECK ADD  CONSTRAINT [FK_WF_CheckRecords_WF_InspectionRecords] FOREIGN KEY([InspectionRecordId])
REFERENCES [dbo].[WF_InspectionRecords] ([Id])
GO
ALTER TABLE [dbo].[WF_CheckRecords] CHECK CONSTRAINT [FK_WF_CheckRecords_WF_InspectionRecords]
GO
ALTER TABLE [dbo].[WF_Inspections]  WITH CHECK ADD  CONSTRAINT [FK_WF_Inspections_WF_Workgroups] FOREIGN KEY([AssignedWorkgroupId])
REFERENCES [dbo].[WF_Workgroups] ([Id])
GO
ALTER TABLE [dbo].[WF_Inspections] CHECK CONSTRAINT [FK_WF_Inspections_WF_Workgroups]
GO
ALTER TABLE [dbo].[WF_Inspections]  WITH CHECK ADD  CONSTRAINT [FK_WF_Inspections_WF_Zones] FOREIGN KEY([ZoneId])
REFERENCES [dbo].[WF_Zones] ([Id])
GO
ALTER TABLE [dbo].[WF_Inspections] CHECK CONSTRAINT [FK_WF_Inspections_WF_Zones]
GO
ALTER TABLE [dbo].[WF_InspectionRecords]  WITH CHECK ADD  CONSTRAINT [FK_WF_InspectionRecords_WF_Inspections] FOREIGN KEY([InspectionId])
REFERENCES [dbo].[WF_Inspections] ([Id])
GO
ALTER TABLE [dbo].[WF_InspectionRecords] CHECK CONSTRAINT [FK_WF_InspectionRecords_WF_Inspections]
GO
CREATE NONCLUSTERED INDEX [IX_WF_CheckRecords_InspectionRecordId] ON [dbo].[WF_CheckRecords]
(
	[InspectionRecordId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[WF_InspectionRecords]  WITH CHECK ADD CONSTRAINT [UC_WF_InspectionRecords_InspectionId_EffectiveDate] UNIQUE([InspectionId], [EffectiveDate])
GO
ALTER TABLE [dbo].[WF_CheckRecords]  WITH CHECK ADD CONSTRAINT [UC_WF_InspectionRecords_InspectionId_CheckId_EffectiveDate] UNIQUE([InspectionId], [CheckId], [EffectiveDate])
GO