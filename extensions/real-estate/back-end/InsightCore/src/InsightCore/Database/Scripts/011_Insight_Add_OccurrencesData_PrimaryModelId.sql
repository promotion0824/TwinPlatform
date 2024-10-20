
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'InsightOccurrences' AND TABLE_SCHEMA='dbo')
BEGIN
     CREATE TABLE dbo.InsightOccurrences (
        Id uniqueidentifier NOT NULL,
		InsightId uniqueidentifier NOT NULL,
        OccurrenceId nvarchar(36) NOT NULL,
		IsFaulted  bit NOT NULL,
        IsValid   bit NOT NULL,
		Started  datetime2 NOT NULL,
		Ended   datetime2 NOT NULL,
		Text nvarchar(max) NULL,
		CONSTRAINT FK_Occurrences_Insights FOREIGN KEY (InsightId) REFERENCES dbo.Insights(Id),
		CONSTRAINT PK_InsightOccurrences PRIMARY KEY CLUSTERED ( [Id] ASC ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
      ) ON [PRIMARY]
    
END

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = N'Insights' AND COLUMN_NAME = N'PrimaryModelId')
BEGIN
    ALTER TABLE Insights
    ADD PrimaryModelId nvarchar(MAX) NULL
END
