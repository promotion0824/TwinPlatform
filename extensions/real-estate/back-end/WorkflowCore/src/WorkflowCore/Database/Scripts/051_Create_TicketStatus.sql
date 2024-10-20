SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF OBJECT_ID('dbo.WF_TicketStatus') IS NULL
   BEGIN      
		CREATE TABLE [dbo].[WF_TicketStatus]
		(
			[CustomerId]		[uniqueidentifier] NOT NULL,
			[StatusCode]		[int] NOT NULL,
			[Status]			[nvarchar](64) NOT NULL,
			[Tab]				[nvarchar](16) NOT NULL,
			[Color]				[nvarchar](32) NOT NULL,

			CONSTRAINT [PK_WF_TicketStatus] PRIMARY KEY CLUSTERED 
			(
				[CustomerId] ASC,
				[StatusCode] ASC
			)
			WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]
   END;
GO