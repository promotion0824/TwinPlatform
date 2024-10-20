ALTER TABLE [dbo].[Floors] 
ADD [IsDecomissioned] [bit] NOT NULL CONSTRAINT IsDecomissionedDefault DEFAULT 0;