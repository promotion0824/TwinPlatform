
IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'DT_GeometryViewerReferences'))
BEGIN
    DROP TABLE DT_GeometryViewerReferences
END

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'DT_GeometryViewerReferences'))
BEGIN
    CREATE TABLE [dbo].[DT_GeometryViewerReferences](
      [Id] [uniqueidentifier] NOT NULL,
      [GeometryViewerModelId] [uniqueidentifier] NOT NULL,
      [GeometryViewerId] NVARCHAR(100) NOT NULL,
      CONSTRAINT [FK_DT_GeometryViewerReferences_GeometryViewerModels] FOREIGN KEY (GeometryViewerModelId) REFERENCES DT_GeometryViewerModels(Id),
      CONSTRAINT [PK_DT_GeometryViewerReferences] PRIMARY KEY CLUSTERED ( [Id] ASC ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
      ) ON [PRIMARY]
END

