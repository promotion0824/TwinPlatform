DECLARE @NewFieldDataType VARCHAR(30), @NewFieldBooleanDataType VARCHAR(30), @ChipkinScanConfigId uniqueidentifier;
SET @NewFieldBooleanDataType = 'Boolean';  
SET @NewFieldDataType = 'Number';  
SET @ChipkinScanConfigId = '7c2807f4-8868-4327-a960-4805edd775e9';  
BEGIN
    --ChipkinBACnet
    INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId) VALUES ('f34f1404-2e27-4c3e-87a0-10e5535f1e22', 'WhoisSegmentSize', 1, @NewFieldDataType, @ChipkinScanConfigId)
    INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId) VALUES ('69b58dbe-694d-4ae7-a6ad-8dc9fe79c9b6', 'TimeInterval', 1, @NewFieldDataType, @ChipkinScanConfigId)
    INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId) VALUES ('7bad44b7-0679-4727-81b1-c79fbac4f94a', 'MinScanTime', 1, @NewFieldDataType, @ChipkinScanConfigId)
    INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId) VALUES ('2b83c9dd-1901-4b75-a8c8-2f1e3507f5ac', 'InRangeOnly', 1, @NewFieldBooleanDataType, @ChipkinScanConfigId)
END
GO