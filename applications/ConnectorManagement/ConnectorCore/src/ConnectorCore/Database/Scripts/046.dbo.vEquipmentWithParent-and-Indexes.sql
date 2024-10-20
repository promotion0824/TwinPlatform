SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP INDEX [Idx_Equipment_SiteId] ON [dbo].[Equipment]
GO

CREATE NONCLUSTERED INDEX [IDX_Equipment_SiteId_EquipmentHierarchyId] ON [dbo].[Equipment]
(
	[SiteId] ASC,
	[EquipmentHierarchyId] ASC
)
INCLUDE([Id]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE VIEW [dbo].[vEquipmentWithParent]
AS
SELECT e.Id, e.Name, e.ClientId, e.SiteId, e.FloorId, e.ExternalEquipmentId, e.Category, e.CreatedAt, e.CreatedBy, e.LastUpdatedAt, e.LastUpdatedBy, e.EquipmentHierarchyId, e2.Id AS ParentEquipmentId
FROM	dbo.Equipment AS e LEFT OUTER JOIN
		dbo.Equipment AS e2 ON e2.SiteId = e.SiteId AND e2.EquipmentHierarchyId = e.EquipmentHierarchyId.GetAncestor(1) AND e2.EquipmentHierarchyId != hierarchyid::GetRoot()
GO
