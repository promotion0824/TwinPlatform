IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'Idx_Scan_ConnectorId')
CREATE INDEX [Idx_Scan_ConnectorId] ON [Scan](ConnectorId);
GO