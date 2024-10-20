-- Remove Schema Columns from Chipkin BACnet Connector

DELETE FROM [dbo].[SchemaColumn]
  WHERE SchemaColumn.[SchemaId] = '8e3de645-3c5c-443f-afd8-27e90f64c9a7'
    AND SchemaColumn.[Name] IN ('ScannerMaxResponseGap', 'DeviceInstanceRangeHighLimit', 'DeviceInstanceRangeLowLimit');
GO