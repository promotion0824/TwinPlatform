ALTER TABLE [dbo].[Sites] 
ALTER COLUMN [Area] NVARCHAR(50) NOT NULL;

ALTER TABLE [dbo].[Sites] 
ADD [Type] int NOT NULL CONSTRAINT TypeDefault DEFAULT '1';
ALTER TABLE [dbo].[Sites]
    DROP TypeDefault
GO

ALTER TABLE [dbo].[Sites] 
DROP COLUMN [NetLettableArea], [Contact], [ContactNumber], [ContactEmail], [ImageUrl], [Introduction], [CreatedOn], [UpdatedOn];