
IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'StatusLogs'))
BEGIN
DROP TABLE [dbo].[StatusLogs]
END

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'StatusLog'))
BEGIN
    CREATE TABLE [dbo].[StatusLog](
      [Id] [uniqueidentifier] NOT NULL,
      [InsightId] [uniqueidentifier] NOT NULL,
      [UserId] [uniqueidentifier] NULL,
      [SourceType] int NULL,
      [SourceId] [uniqueidentifier] NULL,
      [Status] int NOT NULL,
      [Priority] [int] NOT NULL,
      [OccurrenceCount] int NOT NULL,
      [ImpactScores]  NVARCHAR(MAX) NULL,
      [CreatedDateTime] [datetime2] NOT NULL,
      [Reason] NVARCHAR(2048) NULL,
      CONSTRAINT [FK_StatusLog_Insights] FOREIGN KEY (InsightId) REFERENCES Insights(Id),
      CONSTRAINT [PK_StatusLog] PRIMARY KEY CLUSTERED ( [Id] ASC ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
      ) ON [PRIMARY]
END

