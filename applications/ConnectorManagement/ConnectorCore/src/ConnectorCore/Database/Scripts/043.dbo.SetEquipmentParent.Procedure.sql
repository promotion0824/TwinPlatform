create or alter procedure [dbo].[SetEquipmentParent] (@SiteId uniqueidentifier, @EquipmentName nvarchar(128), @EquipmentParentName nvarchar(128))
as
begin
  declare @EquipmentId uniqueidentifier;
  declare @ParentId uniqueidentifier;

  select top 1 @EquipmentId = Id from Equipment where SiteId = @SiteId and Name = @EquipmentName;

  if @EquipmentParentName is null
    set @ParentId = 'dfc0bbce-92a1-4a12-93b5-90355b95965c';
  else
    select top 1 @ParentId = Id from Equipment where SiteId = @SiteId and Name = @EquipmentParentName;

  if @EquipmentId is null or @ParentId is null
    THROW 51000, 'Equipment does not exist', 1;  

  declare @parent hierarchyid;
  declare @maxchild hierarchyid;
  select @parent = [EquipmentHierarchyId] from [dbo].[Equipment] where [Id] = @ParentId;
  select @maxchild = max([EquipmentHierarchyId]) from [dbo].[Equipment] where [EquipmentHierarchyId].GetAncestor(1) = @parent;

  update [dbo].[Equipment] set [EquipmentHierarchyId] = @parent.GetDescendant (@maxchild , null) where [Id] = @EquipmentId;

  return 0;
end