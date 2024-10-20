IF NOT EXISTS(SELECT 1 FROM sys.columns
          WHERE Name = N'SingleTenantUrl'
          AND Object_ID = Object_ID(N'dbo.Customers'))
BEGIN
    ALTER TABLE [dbo].[Customers]
    ADD [SingleTenantUrl] NVARCHAR(255) NULL
END
