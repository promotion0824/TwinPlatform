CREATE TYPE [dbo].[PointTagType] AS TABLE(
	[PointEntityId] [uniqueidentifier] NOT NULL,
	[TagId] [uniqueidentifier] NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[PointEntityId] ASC,
	[TagId] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO
