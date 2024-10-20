UPDATE WF_Ticket SET AssigneeName = 'Unassigned' WHERE AssigneeId IS NULL
UPDATE WF_Ticket SET SourceName = 'Platform' WHERE SourceType = 0
UPDATE WF_Ticket SET SourceName = 'Dynamics' WHERE SourceType = 2