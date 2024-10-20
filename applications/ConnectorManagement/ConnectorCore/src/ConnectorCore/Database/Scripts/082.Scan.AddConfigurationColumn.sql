-- Add Configuration column

ALTER TABLE [dbo].[Scan]
ADD Configuration nvarchar(max);
GO