
IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'Dependencies'))
BEGIN
    CREATE TABLE [dbo].[Dependencies](
      [Id] [uniqueidentifier] NOT NULL,
      [FromInsightId] [uniqueidentifier] NOT NULL,
      [Relationship] NVARCHAR(500) NOT NULL,
      [ToInsightId] [uniqueidentifier] NOT NULL,

      CONSTRAINT [FK_Dependencies_Insights] FOREIGN KEY (FromInsightId) REFERENCES Insights(Id),
      CONSTRAINT [PK_Dependencies] PRIMARY KEY CLUSTERED ( [Id] ASC ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
      ) ON [PRIMARY]
END
