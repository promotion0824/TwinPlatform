


IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = N'Insights' AND COLUMN_NAME = N'NewOccurrence')
BEGIN
    ALTER TABLE Insights
    ADD NewOccurrence [bit] NOT NULL Default ( 0 )
END
