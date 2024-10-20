BEGIN
    --Modbus
    -- Existing EndianBig column maybe used by other services. This is only for non-ADT point metadata to be same as ADT.
    INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId) VALUES ('882FD7FE-7D17-417D-8DC1-BA4C49AB856E', 'Endian', 1, 'String', '3D0DFD33-2954-42AB-B8A4-2FADDD185C22')
END
GO