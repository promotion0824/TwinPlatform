SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SchemaColumn](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](64) NOT NULL,
	[IsRequired] [bit] NOT NULL,
	[DataType] [nvarchar](64) NOT NULL,
	[SchemaId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_SchemaColumn] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[SchemaColumn]  WITH CHECK ADD  CONSTRAINT [FK_SchemaColumn_Schema] FOREIGN KEY([SchemaId])
REFERENCES [dbo].[Schema] ([Id])
GO
ALTER TABLE [dbo].[SchemaColumn] CHECK CONSTRAINT [FK_SchemaColumn_Schema]
GO
