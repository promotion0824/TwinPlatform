-- SET Inspection SortOrder
WITH cte AS
  (SELECT ZoneId,
          name,
          SortOrder,
          ROW_NUMBER() OVER (PARTITION BY zoneid
                             ORDER BY Name DESC, zoneid) rn
   FROM WF_Inspections)
UPDATE cte
SET SortOrder = rn