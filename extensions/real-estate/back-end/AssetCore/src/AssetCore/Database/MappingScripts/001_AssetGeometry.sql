CREATE TABLE [dbo].[AssetGeometry](
	[AssetRegisterId] [int] NOT NULL,
	[Geometry] [nvarchar](128) NOT NULL,	
 CONSTRAINT [PK_AssetGeometry] PRIMARY KEY CLUSTERED 
(
	[AssetRegisterId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO