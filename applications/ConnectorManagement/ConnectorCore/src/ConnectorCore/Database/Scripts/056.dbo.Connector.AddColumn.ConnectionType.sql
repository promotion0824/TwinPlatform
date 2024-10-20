ALTER TABLE Connector
ADD ConnectionType NVARCHAR(64) NULL;
GO
UPDATE Connector SET ConnectionType = ''
GO
ALTER TABLE Connector
ALTER COLUMN ConnectionType NVARCHAR(64) NOT NULL
GO
