﻿ALTER TABLE [dbo].[Sites]
  ADD WebMapId NVARCHAR(40) NOT NULL CONSTRAINT WebMapIdDefault DEFAULT '';
GO

ALTER TABLE [dbo].[Sites]
  DROP WebMapIdDefault
GO

UPDATE [dbo].[Sites]
SET WebMapId = 'c56d09d737464c288852cb16f80680b1'
WHERE Id = '3b1f27d9-a295-4d54-9839-fc5f5c2460fc'