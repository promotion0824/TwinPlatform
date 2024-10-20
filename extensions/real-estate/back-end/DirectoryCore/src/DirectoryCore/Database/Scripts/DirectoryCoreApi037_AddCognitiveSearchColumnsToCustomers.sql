IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'CognitiveSearchUri'
          AND Object_ID = Object_ID(N'dbo.Customers'))
BEGIN
    ALTER TABLE [dbo].[Customers]
    ADD [CognitiveSearchUri] NVARCHAR(100) NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'CognitiveSearchIndex'
          AND Object_ID = Object_ID(N'dbo.Customers'))
BEGIN
    ALTER TABLE [dbo].[Customers]
    ADD [CognitiveSearchIndex] NVARCHAR(100) NULL
END
