IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='UX_WF_Ticket_SourceId_ExternalId_SourceType' AND object_id = OBJECT_ID(N'dbo.WF_Ticket'))
BEGIN
    CREATE UNIQUE INDEX  UX_WF_Ticket_SourceId_ExternalId_SourceType 
	ON WF_Ticket (SourceId, ExternalId, SourceType) 
	WHERE SourceId IS NOT NULL AND ExternalId IS NOT NULL and SourceType = 3
	WITH (ONLINE = ON);
END
