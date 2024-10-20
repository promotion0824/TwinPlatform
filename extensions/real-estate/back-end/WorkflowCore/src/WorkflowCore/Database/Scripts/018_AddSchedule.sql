SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Schedule]
(
	[Id]                    uniqueidentifier NOT NULL,
    [Active]                bit NOT NULL DEFAULT ((1)), 
    [OwnerId]               uniqueidentifier NOT NULL,      
    [Recurrence]            varchar(MAX) NOT NULL,          
    [RecipientClient]       varchar(MAX) NOT NULL,          
    [Recipient]             varchar(MAX) NOT NULL            

 CONSTRAINT [PK_Schedule] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
