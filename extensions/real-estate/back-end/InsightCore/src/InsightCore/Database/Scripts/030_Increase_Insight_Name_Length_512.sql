DECLARE @NewSize INT = 512
DECLARE @CurrentSize INT
SELECT  @CurrentSize =  CHARACTER_MAXIMUM_LENGTH 
					    FROM INFORMATION_SCHEMA.COLUMNS 
						WHERE TABLE_NAME = N'Insights'
						AND COLUMN_NAME = N'Name'
						AND TABLE_SCHEMA = N'dbo'

IF @CurrentSize IS NOT NULL AND @CurrentSize < @NewSize
BEGIN
   ALTER TABLE [dbo].[Insights]
   ALTER COLUMN Name [nvarchar](512) NOT NULL
   WITH(ONLINE = ON)
END
GO	
