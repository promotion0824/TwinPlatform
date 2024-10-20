IF NOT EXISTS (SELECT 1 FROM SYS.COLUMNS C WHERE C.OBJECT_ID=OBJECT_ID('[dbo].[Connector]') AND C.NAME = 'IsArchived')
BEGIN
	ALTER TABLE [dbo].[Connector]
	ADD [IsArchived] BIT 

	ALTER TABLE [dbo].[Connector] ADD  CONSTRAINT [DF_Connector_IsArchived]  DEFAULT ((0)) FOR [IsArchived]
END