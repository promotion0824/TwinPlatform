DECLARE @TableName nvarchar(128) = N'Insights'
DECLARE @ColumnName nvarchar(128) = N'IsDeleted'
DECLARE @IsDeletedConstraintName nvarchar(200)
SELECT @IsDeletedConstraintName = Name 
FROM SYS.DEFAULT_CONSTRAINTS 
WHERE PARENT_OBJECT_ID = OBJECT_ID(@TableName)
AND PARENT_COLUMN_ID = (
    SELECT column_id 
    FROM sys.columns 
    WHERE NAME = @ColumnName
    AND object_id = OBJECT_ID(@TableName)
)
IF @IsDeletedConstraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE ' + @TableName + ' DROP CONSTRAINT ' + @IsDeletedConstraintName)
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = @TableName AND COLUMN_NAME =  @ColumnName)
BEGIN
    EXEC('ALTER TABLE ' + @TableName + ' DROP COLUMN ' + @ColumnName)
END
