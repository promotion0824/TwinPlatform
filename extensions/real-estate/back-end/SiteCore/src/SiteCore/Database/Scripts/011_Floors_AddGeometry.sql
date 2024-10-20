ALTER TABLE [dbo].[Floors] 
ADD [Geometry] [nvarchar](256) NOT NULL CONSTRAINT FloorsGeometryDefault DEFAULT '';

ALTER TABLE [dbo].[Floors]
    DROP FloorsGeometryDefault
GO