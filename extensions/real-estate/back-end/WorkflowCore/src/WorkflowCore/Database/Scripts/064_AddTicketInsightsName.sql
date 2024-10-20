IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'InsightName'
          AND Object_ID = Object_ID(N'dbo.WF_TicketInsights'))
BEGIN
    ALTER TABLE dbo.WF_TicketInsights
    ADD InsightName NVARCHAR(256) NOT NULL
END
