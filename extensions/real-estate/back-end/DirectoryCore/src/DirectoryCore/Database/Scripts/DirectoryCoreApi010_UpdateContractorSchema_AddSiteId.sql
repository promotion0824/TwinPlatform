ALTER TABLE dbo.ContractorAssignments
ADD SiteId UNIQUEIDENTIFIER NULL;
GO
UPDATE CA SET CA.SiteId = S.Id from ContractorAssignments CA INNER JOIN Sites S on CA.CustomerId = S.CustomerId
GO
ALTER TABLE dbo.ContractorAssignments
ALTER COLUMN SiteId UNIQUEIDENTIFIER NOT NULL;
GO

ALTER TABLE dbo.ContractorAssignments
DROP CONSTRAINT PK_ContractorAssignments
GO
ALTER TABLE [dbo].[ContractorAssignments] ADD  CONSTRAINT [PK_ContractorAssignments] PRIMARY KEY CLUSTERED 
(
	[ContractorId] ASC,
	[CustomerId] ASC,
	[SiteId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ContractorAssignments]  WITH CHECK ADD  CONSTRAINT [FK_ContractorAssignments_Sites_SiteId] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([Id])
GO
ALTER TABLE [dbo].[ContractorAssignments] CHECK CONSTRAINT [FK_ContractorAssignments_Sites_SiteId]
GO