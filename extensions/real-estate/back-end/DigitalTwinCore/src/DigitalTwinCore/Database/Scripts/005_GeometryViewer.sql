SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'DT_GeometryViewerModels'))
BEGIN
    CREATE TABLE [dbo].[DT_GeometryViewerModels](
      [Id] [uniqueidentifier] NOT NULL,
      [TwinId] NVARCHAR(100) NOT NULL,
      [Is3D] BIT NULL DEFAULT (1),
      [Urn] NVARCHAR(1024) NOT NULL,
      CONSTRAINT [PK_DT_GeometryViewerModels] PRIMARY KEY CLUSTERED ( [Id] ASC ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
      ) ON [PRIMARY]
END

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'DT_GeometryViewerReferences'))
BEGIN
    CREATE TABLE [dbo].[DT_GeometryViewerReferences](
      [GeometryViewerId] [uniqueidentifier] NOT NULL,
      [GeometryViewerModelId] [uniqueidentifier] NOT NULL,
      CONSTRAINT [FK_DT_GeometryViewerReferences_GeometryViewerModels] FOREIGN KEY (GeometryViewerModelId) REFERENCES DT_GeometryViewerModels(Id),
      CONSTRAINT [PK_DT_GeometryViewerReferences] PRIMARY KEY CLUSTERED ( [GeometryViewerId] ASC ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
      ) ON [PRIMARY]
END
