IF NOT EXISTS (SELECT * FROM [dbo].[SchemaColumn] WHERE Id = 'EB164B27-D743-4C36-B051-3B43F969F94A')
    BEGIN
        INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType], [SchemaId]) VALUES ('EB164B27-D743-4C36-B051-3B43F969F94A', 'SAJResourceId', 1, 'String', '5435C70D-4706-4A06-90D8-7198C215AEB9')
    END 
