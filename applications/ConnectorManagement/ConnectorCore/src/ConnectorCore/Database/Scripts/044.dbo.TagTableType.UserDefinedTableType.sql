CREATE TYPE [dbo].[TagTableType] AS TABLE(
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](64) NOT NULL,
	[Description] [nvarchar](128) NULL,
	[ClientId] [uniqueidentifier] NULL,
	[Feature] [nvarchar](128) NULL,
 PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO
