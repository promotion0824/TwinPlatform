BEGIN
    IF NOT EXISTS (SELECT 1 FROM SchemaColumn
                    WHERE Id = '56f6a8b3-4c60-41bc-814e-9ce6f1389557')
    BEGIN
        INSERT INTO SchemaColumn (Id, Name, IsRequired, DataType, SchemaId)
        VALUES ('56f6a8b3-4c60-41bc-814e-9ce6f1389557', 'Interval', 0, 'Number', 'e73e8e7a-bd79-4813-acff-51c8e51500a3')
    END
END
GO
