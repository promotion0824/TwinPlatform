BEGIN
    IF EXISTS (SELECT 1 FROM [Schema]
                    WHERE Id = '41cfe979-4b56-40f4-8aab-0292d2f96bb2')
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM SchemaColumn
                        WHERE Id = 'a3a84729-8e2e-4c33-8cf6-1655cf7e8bac')
        BEGIN
            INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId)
            VALUES ('a3a84729-8e2e-4c33-8cf6-1655cf7e8bac', 'PointGroupSize', 0, 'Number', '41cfe979-4b56-40f4-8aab-0292d2f96bb2')
        END
        IF NOT EXISTS (SELECT 1 FROM SchemaColumn
                        WHERE Id = 'cd6e1881-add7-40fe-b8f1-15eb17c8d6c7')
        BEGIN
            INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId)
            VALUES ('cd6e1881-add7-40fe-b8f1-15eb17c8d6c7', 'InterReadDelay', 0, 'Number', '41cfe979-4b56-40f4-8aab-0292d2f96bb2')
        END
    END
END
GO