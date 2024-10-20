BEGIN
    IF NOT EXISTS (SELECT * FROM SchemaColumn
                    WHERE Id = '0b8b8e58-9ceb-476f-be09-93a5a56c62e9')
    BEGIN
        INSERT INTO SchemaColumn
        VALUES ('0b8b8e58-9ceb-476f-be09-93a5a56c62e9', 'Interval', 1, 'Number', '41cfe979-4b56-40f4-8aab-0292d2f96bb2')
    END
END
GO