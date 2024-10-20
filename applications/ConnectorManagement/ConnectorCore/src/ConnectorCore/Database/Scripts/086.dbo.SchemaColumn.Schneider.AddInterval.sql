-- Note that the Schneider Schema Id is different in UAT (2ede970c-416b-40b0-aa7f-745043b4f726) and PROD (678bfcf5-cc8f-4a50-9160-7e935c577e4c)

BEGIN
    IF EXISTS (SELECT 1 FROM [Schema]
                    WHERE Id = '678bfcf5-cc8f-4a50-9160-7e935c577e4c')
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM SchemaColumn
                        WHERE Id = '789c4d5c-456c-4014-bbb1-842568fab6e0')
        BEGIN
            INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId)
            VALUES ('789c4d5c-456c-4014-bbb1-842568fab6e0', 'Interval', 0, 'Number', '678bfcf5-cc8f-4a50-9160-7e935c577e4c')
        END
    END

    IF EXISTS (SELECT 1 FROM [Schema]
                    WHERE Id = '2ede970c-416b-40b0-aa7f-745043b4f726')
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM SchemaColumn
                        WHERE Id = '789c4d5c-456c-4014-bbb1-842568fab6e0')
        BEGIN
            INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId)
            VALUES ('789c4d5c-456c-4014-bbb1-842568fab6e0', 'Interval', 0, 'Number', '2ede970c-416b-40b0-aa7f-745043b4f726')
        END
    END
END
GO
