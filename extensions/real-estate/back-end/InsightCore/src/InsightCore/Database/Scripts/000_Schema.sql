SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Insights] (
	[Id] [uniqueidentifier] NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[SequenceNumber] [nvarchar](64) NOT NULL,
	[FloorCode] [nvarchar](64) NOT NULL,
	[EquipmentId] [uniqueidentifier] NULL,
	[Type] [int] NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Description] [nvarchar](512) NOT NULL,
	[Priority] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[CreatedDate] [datetime2] NOT NULL,
	[UpdatedDate] [datetime2] NOT NULL,
	[OccurredDate] [datetime2] NOT NULL,
	[DetectedDate] [datetime2] NOT NULL,
	[SourceType] [int] NOT NULL,
	[SourceId] [uniqueidentifier] NULL,
	[ExternalId] [nvarchar](128) NOT NULL,
	[ExternalStatus] [nvarchar](64) NOT NULL,
	[ExternalMetadata] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Insights] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[InsightNextNumber](
    [Prefix] [nvarchar](16) NOT NULL,
    [NextNumber] [bigint] NOT NULL,
 CONSTRAINT [PK_TicketNextNumber] PRIMARY KEY CLUSTERED 
(
    [Prefix] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO