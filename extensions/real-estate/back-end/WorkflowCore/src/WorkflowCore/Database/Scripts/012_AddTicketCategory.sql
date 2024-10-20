﻿-- Add CategoryId
ALTER TABLE [dbo].[WF_Ticket]
  ADD [CategoryId] UNIQUEIDENTIFIER  NULL
GO

-- Add WF_TcketCategory table
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WF_TicketCategory](
	[Id] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](80) NOT NULL,
 CONSTRAINT [PK_WF_TicketCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[WF_Ticket]  WITH CHECK ADD  CONSTRAINT [FK_WF_Ticket_WF_TicketCategory] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[WF_TicketCategory] ([Id])
GO
ALTER TABLE [dbo].[WF_Ticket] CHECK CONSTRAINT [FK_WF_Ticket_WF_TicketCategory]
GO