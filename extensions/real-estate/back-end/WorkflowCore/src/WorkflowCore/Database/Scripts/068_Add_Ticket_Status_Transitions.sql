
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = N'WF_TicketStatusTransitions')
BEGIN
   CREATE TABLE WF_TicketStatusTransitions (
    Id uniqueidentifier NOT NULL,
	FromStatus int NOT NULL,
	ToStatus int NOT NULL,
    PRIMARY KEY (Id),
	CONSTRAINT UX_WF_TicketStatusTransitions_FromStatus_ToStatus UNIQUE (FromStatus, ToStatus)
);
END
