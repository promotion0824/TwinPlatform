SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WF_NotificationReceiver](
    [SiteId] [uniqueidentifier] NOT NULL,
    [UserId] [uniqueidentifier] NOT NULL,
CONSTRAINT [PK_WF_NotificationReceiver] PRIMARY KEY CLUSTERED 
(
    [SiteId] ASC,
    [UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
