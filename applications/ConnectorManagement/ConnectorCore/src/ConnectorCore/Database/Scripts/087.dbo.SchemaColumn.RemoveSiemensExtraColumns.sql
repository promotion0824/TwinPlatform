-- Remove non-interval Columns from DefaultSiemensConnectorConfiguration  
BEGIN 
    DELETE FROM [dbo].[SchemaColumn]
    WHERE SchemaColumn.[SchemaId] = 'e73e8e7a-bd79-4813-acff-51c8e51500a3'
      AND SchemaColumn.[Name] != 'Interval'
END
GO