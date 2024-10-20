--drop constraint
DECLARE @ConstraintName nvarchar(200)
SELECT @ConstraintName = Name FROM SYS.DEFAULT_CONSTRAINTS
WHERE PARENT_OBJECT_ID = OBJECT_ID('Users')
AND PARENT_COLUMN_ID = (SELECT column_id FROM sys.columns
                        WHERE NAME = N'AccountExternalId'
                        AND object_id = OBJECT_ID(N'Users'))
IF @ConstraintName IS NOT NULL
EXEC('ALTER TABLE Users DROP CONSTRAINT ' + @ConstraintName)

--drop AccountExternalId column from users
ALTER TABLE [dbo].[Users] DROP COLUMN [AccountExternalId]
GO

--add AccountExternalId column to customers
ALTER TABLE [dbo].[Customers] ADD [AccountExternalId] NVARCHAR(50) NOT NULL DEFAULT ''
GO
 
-- DEV
UPDATE [dbo].[Customers] SET AccountExternalId = '001f400000gIEWillow' WHERE Id = '3FC260F3-3E91-470B-8285-15A11C799491' -- Willow AU
GO

-- UAT
UPDATE [dbo].[Customers] SET AccountExternalId = '001f400000gIEWillow' WHERE Id = '159D12D0-7491-490A-8C61-54A21E94924D' -- Willow AU
GO

-- PROD
UPDATE [dbo].[Customers] SET AccountExternalId = '001f400000gIEi9AAG' WHERE Id = 'c540e54e-ab94-43d2-b6cf-48632f8682a4'; -- Brookfield
GO
UPDATE [dbo].[Customers] SET AccountExternalId = '001f400001CAPM7AAP' WHERE Id = 'c182bab8-75a3-4097-b18d-01415964066d'; -- Sofi
GO