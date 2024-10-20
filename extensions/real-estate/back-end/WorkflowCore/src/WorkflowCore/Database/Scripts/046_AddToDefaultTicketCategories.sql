
DECLARE @Cat VARCHAR(100) = 'Audio/Visual'
IF NOT EXISTS(SELECT 1 FROM WF_TicketCategory WHERE Name = @Cat)
  INSERT INTO WF_TicketCategory (Id, Name) VALUES (NEWID(), @Cat)

SET @Cat = 'IT'
IF NOT EXISTS(SELECT 1 FROM WF_TicketCategory WHERE Name = @Cat)
  INSERT INTO WF_TicketCategory (Id, Name) VALUES (NEWID(), @Cat)

SET @Cat = 'Food Service'
IF NOT EXISTS(SELECT 1 FROM WF_TicketCategory WHERE Name = @Cat)
  INSERT INTO WF_TicketCategory (Id, Name) VALUES (NEWID(), @Cat)