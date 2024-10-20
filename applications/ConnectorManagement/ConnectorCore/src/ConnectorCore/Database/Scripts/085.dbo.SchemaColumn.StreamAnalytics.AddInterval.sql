BEGIN
    IF NOT EXISTS (SELECT 1 FROM SchemaColumn
                    WHERE Id = 'fbcdc821-417f-4504-a4ee-34570ebe5023')
    BEGIN
        INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId)
        VALUES ('fbcdc821-417f-4504-a4ee-34570ebe5023', 'Interval', 0, 'Number', '5435c70d-4706-4a06-90d8-7198c215aeb9')
    END
END
GO
