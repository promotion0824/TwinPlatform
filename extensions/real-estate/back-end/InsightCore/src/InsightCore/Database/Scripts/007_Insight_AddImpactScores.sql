IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Recommendation'
          AND Object_ID = Object_ID(N'dbo.Insights'))
BEGIN
    ALTER TABLE dbo.Insights ADD Recommendation [nvarchar](MAX) NULL;
END
GO	

IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'ImpactScores'
          AND Object_ID = Object_ID(N'dbo.Insights'))
BEGIN
    ALTER TABLE dbo.Insights DROP COLUMN ImpactScores;
END
GO

IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'ImpactScores'))
BEGIN
  DROP TABLE ImpactScores
END

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'ImpactScores'))
BEGIN
    CREATE TABLE [dbo].[ImpactScores](
      [Id] [uniqueidentifier] NOT NULL,
      [InsightId] [uniqueidentifier] NOT NULL,
      [Name] NVARCHAR(256) NOT NULL,
      [Value] FLOAT NOT NULL,
      [Unit] NVARCHAR(100) NOT NULL,
      CONSTRAINT [FK_ImpactScores_Insights] FOREIGN KEY (InsightId) REFERENCES Insights(Id),
      CONSTRAINT [PK_ImpactScores] PRIMARY KEY CLUSTERED ( [Id] ASC ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
      ) ON [PRIMARY]
END
