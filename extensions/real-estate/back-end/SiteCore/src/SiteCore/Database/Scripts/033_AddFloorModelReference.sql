IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'ModelReference'
          AND Object_ID = Object_ID(N'dbo.Floors'))
BEGIN
    ALTER TABLE [dbo].Floors ADD ModelReference UNIQUEIDENTIFIER NULL
END