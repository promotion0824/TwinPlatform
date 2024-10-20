BEGIN
    IF NOT EXISTS (SELECT 1 FROM SchemaColumn
                    WHERE Id = 'a2b0c5ed-192c-4fca-8b3b-321d294f85ca')
    BEGIN
        INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId)
        VALUES ('a2b0c5ed-192c-4fca-8b3b-321d294f85ca', 'Interval', 0, 'Number', '30b72f42-e586-4ce1-a5f0-6f5c4a6d79e6')
    END
END
GO
