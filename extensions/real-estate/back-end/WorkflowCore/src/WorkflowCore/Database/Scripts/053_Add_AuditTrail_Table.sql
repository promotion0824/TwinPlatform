IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = N'WF_AuditTrail')
BEGIN
   CREATE TABLE WF_AuditTrail (
    Id uniqueidentifier NOT NULL,
	RecordID uniqueidentifier NOT NULL,
	OperationType nvarchar(50) NOT NULL,
	Timestamp datetime2(7) NOT NULL,
	TableName nvarchar(255) NOT NULL,
	ColumnName nvarchar(255) NOT NULL,
	SourceType int  NULL,
	SourceId uniqueidentifier NULL,
	OldValue nvarchar(max) NULL,
	NewValue nvarchar(max) NOT NULL,
	
    PRIMARY KEY (Id)
);
END
