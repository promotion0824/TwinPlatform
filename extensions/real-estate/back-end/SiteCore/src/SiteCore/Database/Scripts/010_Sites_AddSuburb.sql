ALTER TABLE [dbo].[Sites] 
ADD Suburb NVARCHAR(100) NOT NULL CONSTRAINT SuburbDefault DEFAULT '';
ALTER TABLE [dbo].[Sites]
    DROP SuburbDefault
GO