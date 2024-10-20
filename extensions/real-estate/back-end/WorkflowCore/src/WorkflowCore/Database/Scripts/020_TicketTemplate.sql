
/*************************************************/
/****** Object:  Table [dbo].[WF_TicketTemplate] */
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WF_TicketTemplate]
(
	[Id]				[uniqueidentifier] NOT NULL,
	[CustomerId]		[uniqueidentifier] NOT NULL,
	[SiteId]			[uniqueidentifier] NOT NULL,
	[FloorCode]			[nvarchar](64) NOT NULL,
	[SequenceNumber]	[nvarchar](64) NOT NULL,

	[Priority]			[int] NOT NULL,
	[Status]			[int] NOT NULL,

	[Summary]			[nvarchar](512) NOT NULL,
	[Description]		[nvarchar](MAX) NOT NULL,

	[ReporterId]		[uniqueidentifier] NULL,
	[ReporterName]		[nvarchar](64) NOT NULL,
	[ReporterPhone]		[nvarchar](32) NOT NULL,
	[ReporterEmail]		[nvarchar](64) NOT NULL,
	[ReporterCompany]	[nvarchar](64) NOT NULL,

	[CreatedDate]		[datetime2] NOT NULL,
	[UpdatedDate]		[datetime2] NOT NULL,
	[ClosedDate]		[datetime2] NULL,

	[SourceType]		[int] NOT NULL,
	[AssigneeType]		[int] NOT NULL,
	[AssigneeId]		[uniqueidentifier] NULL,

	[Recurrence]	    [nvarchar](MAX) NOT NULL,
	[OverdueThreshold]	[nvarchar](32) NOT NULL,
	[Assets]			[nvarchar](MAX) NULL,
	[Tasks]				[nvarchar](MAX) NULL,
	[Attachments]	    [nvarchar](MAX) NULL,

	CONSTRAINT [PK_WF_TicketTemplate] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/*************************************************/
ALTER TABLE [dbo].[WF_TicketTemplate]  WITH CHECK ADD  CONSTRAINT [Unique_WF_TicketTemplate_SequenceNumber] UNIQUE([SequenceNumber])
GO

/*************************************************/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WF_TicketTemplateNextNumber]
(
	[Prefix] [nvarchar](16) NOT NULL,
	[NextNumber] [bigint] NOT NULL,

	CONSTRAINT [PK_TicketTemplateNumber] PRIMARY KEY CLUSTERED 
	(
		[Prefix] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

) ON [PRIMARY]
GO

/*************************************************/
ALTER TABLE [dbo].[WF_Ticket]
  ADD [ScheduledDate] datetime2 NULL;
GO
