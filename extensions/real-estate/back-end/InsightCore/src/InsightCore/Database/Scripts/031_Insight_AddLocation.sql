IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'InsightLocations'))
BEGIN
CREATE TABLE [dbo].[InsightLocations] (
    [InsightId] [uniqueidentifier] NOT NULL,
    [LocationId] NVARCHAR(250) NOT NULL,
    CONSTRAINT [FK_Location_Insight] FOREIGN KEY (InsightId) REFERENCES Insights(Id),
    CONSTRAINT [PK_InsightLocation] PRIMARY KEY CLUSTERED ([InsightId],[LocationId]) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
