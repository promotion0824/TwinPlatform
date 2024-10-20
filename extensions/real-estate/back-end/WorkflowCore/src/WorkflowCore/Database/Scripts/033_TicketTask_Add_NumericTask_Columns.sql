ALTER TABLE [dbo].[WF_TicketTask]
    DROP COLUMN [Value];
GO

ALTER TABLE [dbo].[WF_TicketTask]
    ADD [Type] [int] NOT NULL CONSTRAINT TypeDefault DEFAULT 1,
	    [DecimalPlaces] [int] NULL,
	    [MinValue] [float] NULL,
	    [MaxValue] [float] NULL, 
	    [Unit] [nvarchar](64) NULL,
	    [NumberValue] [float] NULL;
GO

ALTER TABLE [dbo].[WF_TicketTask]
    DROP TypeDefault
GO