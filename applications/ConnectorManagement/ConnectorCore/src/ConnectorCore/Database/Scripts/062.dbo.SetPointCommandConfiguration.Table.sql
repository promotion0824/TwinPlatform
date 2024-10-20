SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SetPointCommandConfiguration] (
	[Id] [int] NOT NULL,
	[Type] [int] NOT NULL,
	[Description] [nvarchar](128) NOT NULL,
	[InsightName] [nvarchar](128) NOT NULL,
	[PointTags] [nvarchar](128) NOT NULL,
	[SetPointTags] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_SetPointCommandConfiguration] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT INTO [dbo].[SetPointCommandConfiguration] 
	([Id] ,[Type], [Description], [InsightName], [PointTags], [SetPointTags])
VALUES (1 , 0, 'Temperature', 'temperature', 'temp,sensor', 'temp,sp')
GO
