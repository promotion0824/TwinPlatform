IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'WF_TicketInsights'))
BEGIN
    CREATE TABLE [dbo].WF_TicketInsights(
      [Id] [uniqueidentifier] NOT NULL,
      [TicketId] [uniqueidentifier] NOT NULL,
      [InsightId] [uniqueidentifier] NOT NULL,
      CONSTRAINT [FK_TicketInsights_Insights] FOREIGN KEY (TicketId) REFERENCES WF_Ticket(Id),
      CONSTRAINT [PK_TicketInsights] PRIMARY KEY CLUSTERED ( [Id] ASC ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
      ) ON [PRIMARY]
END
