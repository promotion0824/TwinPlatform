ALTER VIEW [dbo].[vRoleAssignments]
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

	SELECT null as CustomerId, null as PortfolioId, a.*
	FROM dbo.[Assignments] a
	WHERE a.ResourceType = 3
GO

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='FK_ContractorAssignments_Sites_SiteId')
BEGIN
	ALTER TABLE [dbo].[ContractorAssignments] DROP CONSTRAINT [FK_ContractorAssignments_Sites_SiteId]
END
GO

IF EXISTS (SELECT 1 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'Sites')
BEGIN
    DROP TABLE [dbo].[Sites]
END
GO
