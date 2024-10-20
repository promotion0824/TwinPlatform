IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Twins'
          AND Object_ID = Object_ID(N'dbo.WF_TicketTemplate'))
BEGIN
    ALTER TABLE dbo.WF_TicketTemplate ADD Twins [nvarchar](MAX) NULL;
END
GO
