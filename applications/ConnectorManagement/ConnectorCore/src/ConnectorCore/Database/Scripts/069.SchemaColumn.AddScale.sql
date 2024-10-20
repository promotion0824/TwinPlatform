BEGIN
    --BACnet
    INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId) VALUES ('7D34EEC4-91E2-4426-B8E7-E5678E8E6B2B', 'Scale', 0, 'String', '86CB421A-BD47-4D09-B78E-3ED88976D9B9')
    --OPC-UA
    INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId) VALUES ('F196DA35-F412-43EF-829C-EF7C8AFF2658', 'Scale', 0, 'String', '203C392F-BB84-456C-ACA9-2AB7AF7F6595')
    --OPC-DA
    INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId) VALUES ('9FE3DE15-CCF4-4250-9D34-021C7E4BA869', 'Scale', 0, 'String', 'B6270AA7-294C-411C-94C8-FF2CD1C8268F')
    --Modbus already has Scale property defined
END
GO