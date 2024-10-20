/****** Object:  Table [dbo].[WF_Schema]    Script Date: 16/04/2019 11:28:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WF_Schema](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Type] [nvarchar](64) NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_WF_Schema] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WF_SchemaColumn]    Script Date: 16/04/2019 11:28:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WF_SchemaColumn](
	[Id] [uniqueidentifier] NOT NULL,
	[SchemaId] [uniqueidentifier] NOT NULL,
	[IsRequired] [bit] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[DataType] [nvarchar](64) NOT NULL,
	[IsDetail] [bit] NOT NULL,
	[GroupName] [nvarchar](64) NOT NULL,
	[OrderInGroup] [int] NOT NULL,
	[ReferenceColumn] [nvarchar](max) NULL,
 CONSTRAINT [PK_WF_SchemaColumn] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WF_Ticket]    Script Date: 16/04/2019 11:28:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WF_Ticket](
	[Id] [uniqueidentifier] NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[FloorCode] [nvarchar](64) NOT NULL,
	[SequenceNumber] [nvarchar](64) NOT NULL,

	[Priority] [int] NOT NULL,
	[Status] [int] NOT NULL,

	[IssueType] [int] NOT NULL,
	[IssueId] [uniqueidentifier] NULL,
	[IssueName] [nvarchar](64) NOT NULL,

	[Description] [nvarchar](1024) NOT NULL,
	[Cause] [nvarchar](1024) NOT NULL,
	[Solution] [nvarchar](1024) NOT NULL,

	[ReporterId] [uniqueidentifier] NULL,
	[ReporterName] [nvarchar](64) NOT NULL,
	[ReporterPhone] [nvarchar](32) NOT NULL,
	[ReporterEmail] [nvarchar](64) NOT NULL,
	[ReporterCompany] [nvarchar](64) NOT NULL,

	[AssigneeContractorId] [uniqueidentifier] NULL,
	[DueDate] [datetime2] NULL,
	[CreatedDate] [datetime2] NOT NULL,
	[UpdatedDate] [datetime2] NOT NULL,
	[ResolvedDate] [datetime2] NULL,
	[ClosedDate] [datetime2] NULL,

	[SourceType] [int] NOT NULL,
	[SourceId] [uniqueidentifier] NULL,
	[ExternalId] [nvarchar](128) NOT NULL,
	[ExternalStatus] [nvarchar](64) NOT NULL,
	[ExternalMetadata] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_WF_Ticket] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WF_Attachment]    Script Date: 09/06/2019 01:20:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WF_Attachment](
    [Id] [uniqueidentifier] NOT NULL,
    [TicketId] [uniqueidentifier] NOT NULL,
    [Type] [int] NOT NULL,
    [FileName] [nvarchar](256) NOT NULL,
    [CreatedDate] [datetime2] NOT NULL,
CONSTRAINT [PK_WF_Attachment] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WF_Comment]    Script Date: 09/06/2019 01:20:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WF_Comment](
    [Id] [uniqueidentifier] NOT NULL,
    [TicketId] [uniqueidentifier] NOT NULL,
	[Text] [nvarchar](2048) NOT NULL,
	[CreatorType] [int] NOT NULL,
    [CreatorId] [uniqueidentifier] NOT NULL,
    [CreatedDate] [datetime2] NOT NULL,
CONSTRAINT [PK_WF_Comment] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WF_Reporter]    Script Date: 09/06/2019 01:20:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WF_Reporter](
	[Id] [uniqueidentifier] NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](64) NOT NULL,
	[Phone] [nvarchar](32) NOT NULL,
	[Email] [nvarchar](64) NOT NULL,
	[Company] [nvarchar](64) NOT NULL,
CONSTRAINT [PK_WF_Reporter] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WF_TicketNextNumber]    Script Date: 22/05/2019 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WF_TicketNextNumber](
	[Prefix] [nvarchar](16) NOT NULL,
	[NextNumber] [bigint] NOT NULL,
 CONSTRAINT [PK_TicketNextNumber] PRIMARY KEY CLUSTERED 
(
	[Prefix] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[WF_SchemaColumn]  WITH CHECK ADD  CONSTRAINT [FK_WF_SchemaColumn_WF_Schema] FOREIGN KEY([SchemaId])
REFERENCES [dbo].[WF_Schema] ([Id])
GO
ALTER TABLE [dbo].[WF_SchemaColumn] CHECK CONSTRAINT [FK_WF_SchemaColumn_WF_Schema]
GO

ALTER TABLE [dbo].[WF_Ticket]  WITH CHECK ADD  CONSTRAINT [Unique_WF_Ticket_SequenceNumber] UNIQUE([SequenceNumber])
GO

ALTER TABLE [dbo].[WF_Attachment] WITH CHECK ADD CONSTRAINT [FK_WF_Attachment_WF_Ticket] FOREIGN KEY([TicketId])
REFERENCES [dbo].[WF_Ticket] ([Id])
GO
ALTER TABLE [dbo].[WF_Attachment] CHECK CONSTRAINT [FK_WF_Attachment_WF_Ticket]
GO

ALTER TABLE [dbo].[WF_Comment] WITH CHECK ADD CONSTRAINT [FK_WF_Comment_WF_Ticket] FOREIGN KEY([TicketId])
REFERENCES [dbo].[WF_Ticket] ([Id])
GO
ALTER TABLE [dbo].[WF_Comment] CHECK CONSTRAINT [FK_WF_Comment_WF_Ticket]
GO
