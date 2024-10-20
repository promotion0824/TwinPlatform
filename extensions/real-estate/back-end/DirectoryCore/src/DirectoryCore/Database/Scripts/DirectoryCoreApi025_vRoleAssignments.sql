CREATE VIEW [dbo].[vRoleAssignments]
  AS

	SELECT ResourceId as CustomerId, null as PortfolioId, *
	FROM dbo.[Assignments]
	WHERE ResourceType = 1

	UNION 

	SELECT p.CustomerId as CustomerId, a.ResourceId as PortfolioId, a.*
	FROM dbo.[Assignments] a
	INNER JOIN [dbo].[Portfolios] p
	ON p.Id = a.ResourceId 
	WHERE a.ResourceType = 2

	UNION 

	SELECT s.CustomerId as CustomerId, s.PortfolioId as PortfolioId, a.*
	FROM dbo.[Assignments] a
	INNER JOIN [dbo].[Sites] s
	ON s.Id = a.ResourceId 
	WHERE a.ResourceType = 3

GO