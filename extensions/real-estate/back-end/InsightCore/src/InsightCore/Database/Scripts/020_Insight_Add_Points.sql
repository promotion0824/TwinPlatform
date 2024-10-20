IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'PointsJson' AND Object_ID = Object_ID(N'dbo.Insights'))
BEGIN
    ALTER TABLE dbo.Insights ADD PointsJson nvarchar(max) NULL;
END;
