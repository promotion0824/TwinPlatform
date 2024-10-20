ALTER TABLE dbo.Connector ADD
	LastUpdatedAt datetime2(7) NOT NULL CONSTRAINT DF_Connector_LastUpdatedAt DEFAULT SYSUTCDATETIME()
GO
