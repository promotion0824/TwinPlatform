﻿IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'SigmaConnectionId'
          AND Object_ID = Object_ID(N'dbo.Customers'))
BEGIN
    ALTER TABLE [dbo].Customers ALTER COLUMN SigmaConnectionId NVARCHAR(128) NULL 
END
