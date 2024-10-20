UPDATE i  SET i.Priority = Floor(5 - (s.Value / 25))
FROM [dbo].[Insights] i
JOIN ImpactScores s ON i.Id = s.InsightId
WHERE s.Name = 'Priority' AND s.Value <= 100
