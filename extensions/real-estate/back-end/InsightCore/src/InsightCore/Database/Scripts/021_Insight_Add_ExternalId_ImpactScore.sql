IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ExternalId' AND Object_ID = Object_ID(N'dbo.ImpactScores'))
BEGIN
    ALTER TABLE dbo.ImpactScores ADD ExternalId nvarchar(max) NULL;
END;
