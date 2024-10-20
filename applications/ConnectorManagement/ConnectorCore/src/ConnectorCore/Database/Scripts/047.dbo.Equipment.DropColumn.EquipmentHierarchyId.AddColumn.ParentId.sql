/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Equipment ADD
	ParentId uniqueidentifier NULL
GO

ALTER TABLE dbo.Equipment ADD CONSTRAINT
	FK_Equipment_Equipment FOREIGN KEY
	(
	ParentId
	) REFERENCES dbo.Equipment
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION
	
GO

ALTER TABLE dbo.Equipment SET (LOCK_ESCALATION = TABLE)
GO

COMMIT

BEGIN TRANSACTION
GO

UPDATE e
SET e.[ParentId] = e2.[Id]
FROM	[dbo].[Equipment] AS e INNER JOIN
		dbo.Equipment AS e2 ON e2.SiteId = e.SiteId AND e2.EquipmentHierarchyId = e.EquipmentHierarchyId.GetAncestor(1)
WHERE e2.EquipmentHierarchyId != hierarchyid::GetRoot()
GO

COMMIT
GO

ALTER VIEW [dbo].[vEquipmentWithParent]
AS
SELECT e.Id, e.Name, e.ClientId, e.SiteId, e.FloorId, e.ExternalEquipmentId, e.Category, e.CreatedAt, e.CreatedBy, e.LastUpdatedAt, e.LastUpdatedBy, hierarchyid::GetRoot() AS EquipmentHierarchyId, e.ParentId AS ParentEquipmentId
FROM dbo.Equipment AS e
GO

BEGIN TRANSACTION
GO

DROP INDEX [IDX_Equipment_SiteId_EquipmentHierarchyId] ON [dbo].[Equipment]
GO

ALTER TABLE dbo.Equipment 
DROP COLUMN	[EquipmentHierarchyId]
GO

CREATE NONCLUSTERED INDEX [IDX_Equipment_SiteId_ParentId] ON [dbo].[Equipment]
(
	[SiteId] ASC,
	[ParentId] ASC
)
INCLUDE([Id]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

ALTER TABLE dbo.Equipment SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
