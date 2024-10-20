IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'RuleId' AND Object_ID = Object_ID(N'dbo.Insights'))
BEGIN
    ALTER TABLE dbo.Insights ADD RuleId nvarchar(450) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'RuleName' AND Object_ID = Object_ID(N'dbo.Insights'))
BEGIN
    ALTER TABLE dbo.Insights ADD RuleName nvarchar(max) NULL;
END;

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ExternalName' AND Object_ID = Object_ID(N'dbo.Insights'))
BEGIN
    ALTER TABLE dbo.Insights DROP COLUMN ExternalName;
END
