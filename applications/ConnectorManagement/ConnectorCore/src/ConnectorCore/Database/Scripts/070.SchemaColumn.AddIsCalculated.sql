BEGIN
    --BACnet
    INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId) VALUES ('B1D58F82-1E62-4F05-BE72-15015D4393F7', 'IsCalculated', 0, 'String', '86CB421A-BD47-4D09-B78E-3ED88976D9B9')
    --OPC-UA
    INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId) VALUES ('A123A4A7-77C3-4CBD-9659-E2F2E1FE9BBB', 'IsCalculated', 0, 'String', '203C392F-BB84-456C-ACA9-2AB7AF7F6595')
    --OPC-DA
    INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId) VALUES ('25900E8F-5C48-4861-9F28-D5A59831A615', 'IsCalculated', 0, 'String', 'B6270AA7-294C-411C-94C8-FF2CD1C8268F')
    --Modbus
    INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId) VALUES ('A7B8502B-B470-4B55-885B-C975D581BD2B', 'IsCalculated', 0, 'String', '3D0DFD33-2954-42AB-B8A4-2FADDD185C22')
END
GO