SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
INSERT INTO [dbo].[WF_TicketStatus] 
(CustomerId, StatusCode, Status, Tab, Color)
SELECT DISTINCT CustomerId, Statuscode, Statuses_Tobe_Inserted.Status, Tab, Color 
FROM [dbo].[WF_Ticket]
    CROSS JOIN 
    (VALUES
    (0, 'Open', 'Open', 'yellow'),
    (5, 'Reassign', 'Open', 'yellow'),
    (10, 'InProgress', 'Open', 'green'),
    (15, 'LimitedAvailability', 'Open', 'yellow'),
    (20, 'Resolved', 'Resolved', 'green'),
    (30, 'Closed', 'Closed', 'green'))
	AS Statuses_Tobe_Inserted(Statuscode, Status, Tab, Color)
GO
