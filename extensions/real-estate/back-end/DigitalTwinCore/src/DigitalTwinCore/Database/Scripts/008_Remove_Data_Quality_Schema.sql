IF EXISTS (SELECT  object_id FROM sys.tables WHERE name = 'DataQualityData') 
BEGIN
DROP TABLE [dbo].[DataQualityData]
END



IF EXISTS (SELECT  object_id FROM sys.tables WHERE name = 'FloorDataQuality') 
BEGIN
DROP TABLE [dbo].[FloorDataQuality]
END



IF EXISTS (SELECT  object_id FROM sys.tables WHERE name = 'SiteDataQuality') 
BEGIN

DROP TABLE [dbo].[SiteDataQuality]

END



IF EXISTS (SELECT  object_id FROM sys.tables WHERE name = 'TwinDataQuality') 
BEGIN

DROP TABLE [dbo].[TwinDataQuality]

END
