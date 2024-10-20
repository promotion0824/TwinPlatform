/****** Object:  Table [dbo].[CustomerUserTimeSeries] ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CustomerUserTimeSeries](
	[CustomerUserId] [uniqueidentifier] NOT NULL,
	[State] [nvarchar](max) NULL,
	[Favorites] [nvarchar](max) NULL,
	[RecentAssets] [nvarchar](max) NULL,
	[ExportedCsvs] [nvarchar](max) NULL,
 CONSTRAINT [PK_CustomerUserTimeSeries] PRIMARY KEY CLUSTERED 
(
	[CustomerUserId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


