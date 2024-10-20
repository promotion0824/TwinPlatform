BEGIN
    IF NOT EXISTS (SELECT * FROM SchemaColumn
                    WHERE Id = '98a40ca7-1319-49c4-ab7f-3f9864b949cf')
    BEGIN
        INSERT INTO SchemaColumn
        VALUES ('98a40ca7-1319-49c4-ab7f-3f9864b949cf', 'Interval', 1, 'Number', '21e52a57-1dcf-4c13-a0f8-c65b2f2b99d2')
    END
END
GO