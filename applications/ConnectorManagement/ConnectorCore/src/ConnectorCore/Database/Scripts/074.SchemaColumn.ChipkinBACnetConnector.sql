-- Schema Columns for new Chipkin BACnet Connector

IF NOT EXISTS (SELECT * FROM [dbo].[SchemaColumn] WHERE Id = '14F2A3CF-DDA7-4523-987A-81B6EE22CADA')
    BEGIN
        INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType], [SchemaId]) VALUES ('14F2A3CF-DDA7-4523-987A-81B6EE22CADA', 'DeviceId', 1, 'Number', '8E3DE645-3C5C-443F-AFD8-27E90F64C9A7')
    END

IF NOT EXISTS (SELECT * FROM [dbo].[SchemaColumn] WHERE Id = 'E0671F5B-5CAD-4AA8-A14F-094DB13E2914')
    BEGIN
        INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType], [SchemaId]) VALUES ('E0671F5B-5CAD-4AA8-A14F-094DB13E2914', 'NetworkSettingInterfaceName', 1, 'String', '8E3DE645-3C5C-443F-AFD8-27E90F64C9A7')
    END

IF NOT EXISTS (SELECT * FROM [dbo].[SchemaColumn] WHERE Id = 'AEBA86BE-053D-4489-B698-CED524C2FED6')
    BEGIN
        INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType], [SchemaId]) VALUES ('AEBA86BE-053D-4489-B698-CED524C2FED6', 'Port', 0, 'Number', '8E3DE645-3C5C-443F-AFD8-27E90F64C9A7')
    END

IF NOT EXISTS (SELECT * FROM [dbo].[SchemaColumn] WHERE Id = '2943EC90-A97C-4EC3-BF74-1F88B449A498')
    BEGIN
        INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType], [SchemaId]) VALUES ('2943EC90-A97C-4EC3-BF74-1F88B449A498', 'BbmdConnection', 0, 'Boolean', '8E3DE645-3C5C-443F-AFD8-27E90F64C9A7')
    END

IF NOT EXISTS (SELECT * FROM [dbo].[SchemaColumn] WHERE Id = '9CFA3B16-4874-4497-A9FA-3F359E9BC20F')
    BEGIN
        INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType], [SchemaId]) VALUES ('9CFA3B16-4874-4497-A9FA-3F359E9BC20F', 'BbmdAddress', 0, 'String', '8E3DE645-3C5C-443F-AFD8-27E90F64C9A7')
    END

IF NOT EXISTS (SELECT * FROM [dbo].[SchemaColumn] WHERE Id = 'F2BF5093-15D8-4FDB-A3C6-58DFA2936115')
    BEGIN
        INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType], [SchemaId]) VALUES ('F2BF5093-15D8-4FDB-A3C6-58DFA2936115', 'DeviceInstanceRangeLowLimit', 1, 'Number', '8E3DE645-3C5C-443F-AFD8-27E90F64C9A7')
    END

IF NOT EXISTS (SELECT * FROM [dbo].[SchemaColumn] WHERE Id = '1374825F-183D-4358-B4BC-AB9B9C213213')
    BEGIN
        INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType], [SchemaId]) VALUES ('1374825F-183D-4358-B4BC-AB9B9C213213', 'DeviceInstanceRangeHighLimit', 1, 'Number', '8E3DE645-3C5C-443F-AFD8-27E90F64C9A7')
    END

IF NOT EXISTS (SELECT * FROM [dbo].[SchemaColumn] WHERE Id = 'B99B4D45-F62A-42CC-9CEE-E871CBC680AA')
    BEGIN
        INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType], [SchemaId]) VALUES ('B99B4D45-F62A-42CC-9CEE-E871CBC680AA', 'ScannerMaxResponseGap', 1, 'Number', '8E3DE645-3C5C-443F-AFD8-27E90F64C9A7')
    END

IF NOT EXISTS (SELECT * FROM [dbo].[SchemaColumn] WHERE Id = '3D2288EA-F386-4BE8-B0E3-DAF3B95450E5')
    BEGIN
        INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType], [SchemaId]) VALUES ('3D2288EA-F386-4BE8-B0E3-DAF3B95450E5', 'MaxRetry', 1, 'Number', '8E3DE645-3C5C-443F-AFD8-27E90F64C9A7')
    END

IF NOT EXISTS (SELECT * FROM [dbo].[SchemaColumn] WHERE Id = 'B5C01826-DC74-43C6-A480-69DBF476C893')
    BEGIN
        INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType], [SchemaId]) VALUES ('B5C01826-DC74-43C6-A480-69DBF476C893', 'Interval', 1, 'Number', '8E3DE645-3C5C-443F-AFD8-27E90F64C9A7')
    END 
