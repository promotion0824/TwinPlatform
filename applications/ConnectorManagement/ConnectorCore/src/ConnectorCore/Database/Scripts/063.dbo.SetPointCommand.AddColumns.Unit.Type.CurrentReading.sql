BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.SetPointCommand ADD
	Unit nvarchar(64) NOT NULL CONSTRAINT DF_SetPointCommand_Unit DEFAULT '',
	Type int NOT NULL CONSTRAINT DF_SetPointCommand_Type DEFAULT 0,
	CurrentReading decimal(18, 6) NOT NULL CONSTRAINT DF_SetPointCommand_CurrentReading DEFAULT 0,
	DesiredDurationMinutes int NOT NULL CONSTRAINT DF_SetPointCommand_DesiredDurationMinutes DEFAULT 0
GO

ALTER TABLE dbo.SetPointCommand
	DROP CONSTRAINT DF_SetPointCommand_Unit
GO
ALTER TABLE dbo.SetPointCommand
	DROP CONSTRAINT DF_SetPointCommand_Type
GO
ALTER TABLE dbo.SetPointCommand
	DROP CONSTRAINT DF_SetPointCommand_CurrentReading
GO

UPDATE [dbo].[SetPointCommand] SET DesiredDurationMinutes = DesiredDurationSeconds / 60
GO

ALTER TABLE [dbo].[SetPointCommand] DROP COLUMN [DesiredDurationSeconds]
GO

ALTER TABLE dbo.SetPointCommand
	DROP CONSTRAINT DF_SetPointCommand_DesiredDurationMinutes
GO

ALTER TABLE dbo.SetPointCommand SET (LOCK_ESCALATION = TABLE)
GO
COMMIT