ALTER TABLE dbo.Metrics ADD
	Tooltip nvarchar(1024) NOT NULL CONSTRAINT DF_Metrics_Tooltip DEFAULT ''
GO

ALTER TABLE dbo.Metrics
	DROP DF_Metrics_Tooltip
GO
