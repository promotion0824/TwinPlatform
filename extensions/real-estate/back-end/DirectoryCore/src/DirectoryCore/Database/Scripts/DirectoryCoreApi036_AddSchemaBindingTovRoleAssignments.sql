SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[vRoleAssignments] WITH SCHEMABINDING
  AS

	SELECT ResourceId as CustomerId, null as PortfolioId, PrincipalId, RoleId, ResourceId, ResourceType
	FROM dbo.[Assignments]
	WHERE ResourceType = 1

	UNION

	SELECT p.CustomerId as CustomerId, a.ResourceId as PortfolioId, a.PrincipalId, a.RoleId, a.ResourceId, a.ResourceType
	FROM dbo.[Assignments] a
	INNER JOIN [dbo].[Portfolios] p
	ON p.Id = a.ResourceId
	WHERE a.ResourceType = 2

	UNION

	SELECT null as CustomerId, null as PortfolioId, a.PrincipalId, a.RoleId, a.ResourceId, a.ResourceType
	FROM dbo.[Assignments] a
	WHERE a.ResourceType = 3
GO
